using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Api.Errors;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public class GetTransactionApiTests
{
    [Fact]
    public async Task GetTransaction_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var transactionId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/transactions/{transactionId}");

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "TRANSACTION_NOT_FOUND",
            problem!.Code);
    }

    [Fact]
    public async Task GetTransaction_WhenPortfolioDoesNotBelongToUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var otherUserId = Guid.Parse(
            "99999999-9999-9999-9999-999999999999");

        var otherPortfolioId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                otherPortfolioId,
                otherUserId,
                "Other User Portfolio");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            portfolioId = otherPortfolioId,
            brokerAccountId = Guid.NewGuid(),
            instrumentId = Guid.NewGuid(),
            type = "Buy",
            quantity = 100,
            price = 10000m,
            fee = 100000m,
            transactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            sequence = 1
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Add(
            "Idempotency-Key",
            Guid.NewGuid().ToString());

        var postResponse = await client.SendAsync(httpRequest);

        var result = await postResponse.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(result);

        // Act
        var response = await client.GetAsync(
            $"/api/v1/transactions/{result!.TransactionId}");

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            problem!.Code);
    }

}