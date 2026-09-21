using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TradeLens.Api.Errors;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;
using Xunit;

namespace TradeLens.Integration.Tests.Authentication;

public sealed class JwtAuthenticationTests
{
    [Fact]
    public async Task ValidJwtToken_ShouldAuthenticateRequest()
    {
        await using var factory = new JwtWebApplicationFactory();

        using var client = factory.CreateClient();

        var token = CreateToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync(
            "/api/v1/transactions/00000000-0000-0000-0000-000000000000");

        Assert.NotEqual(
            System.Net.HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private static string CreateToken()
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                CustomWebApplicationFactory.TestUserId.ToString()),
            new Claim(
                ClaimTypes.Name,
                "integration-test-user")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "TradeLens-Integration-Test-Secret-Key-At-Least-32"));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TradeLens",
            audience: "TradeLens.Api",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    [Fact]
    public async Task InvalidJwtToken_ShouldReturnUnauthorized()
    {
        await using var factory = new JwtWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                "this-is-not-a-valid-jwt");

        var response = await client.GetAsync(
            "/api/v1/transactions/00000000-0000-0000-0000-000000000000");

        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ValidJwtToken_WhenPortfolioDoesNotBelongToUser_ShouldReturnForbidden()
    {
        await using var factory = new JwtWebApplicationFactory();

        using var client = factory.CreateClient();

        var otherUserId = Guid.Parse(
            "99999999-9999-9999-9999-999999999999");

        var otherPortfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                otherPortfolioId,
                otherUserId,
                "Other User Portfolio");

            dbContext.Portfolios.Add(portfolio);

            var transaction = new Transaction(
                transactionId,
                otherPortfolioId,
                brokerAccountId,
                instrumentId,
                TradeLens.Domain.Enums.TransactionType.Buy,
                100,
                10_000m,
                100_000m,
                DateOnly.FromDateTime(DateTime.UtcNow),
                1,
                otherUserId,
                DateTimeOffset.UtcNow);

            dbContext.Transactions.Add(transaction);

            await dbContext.SaveChangesAsync();
        }

        var token = CreateToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync(
            $"/api/v1/transactions/{transactionId}");

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        Assert.Equal(
            System.Net.HttpStatusCode.Forbidden,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            problem!.Code);
    }
}