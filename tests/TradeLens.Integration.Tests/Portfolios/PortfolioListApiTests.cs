using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Portfolios;

public sealed class PortfolioListApiTests
{
    [Fact]
    public async Task GetPortfolios_ShouldReturnCurrentUserPortfolios()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var userId = Guid.NewGuid();

        var portfolio1 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Long Term");

        var portfolio2 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Trading");

        var otherUserPortfolio = new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Another User Portfolio");

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Portfolios.AddRange(
                portfolio1,
                portfolio2,
                otherUserPortfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-User-Id",
            userId.ToString());

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/portfolios");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioListResponse>();

        Assert.NotNull(result);

        Assert.Equal(1, result!.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        Assert.Equal(2, result.Items.Count);

        Assert.Equal(
            portfolio1.Id,
            result.Items[0].PortfolioId);

        Assert.Equal(
            "Long Term",
            result.Items[0].Name);

        Assert.Equal(
            portfolio2.Id,
            result.Items[1].PortfolioId);

        Assert.Equal(
            "Trading",
            result.Items[1].Name);
    }

    [Fact]
    public async Task GetPortfolios_WhenRequestingSecondPage_ShouldReturnSecondPage()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var userId = Guid.NewGuid();

        var portfolio1 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Alpha");

        var portfolio2 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Beta");

        var portfolio3 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Gamma");

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Portfolios.AddRange(
                portfolio1,
                portfolio2,
                portfolio3);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-User-Id",
            userId.ToString());

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/portfolios?page=2&pageSize=2");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioListResponse>();

        Assert.NotNull(result);

        Assert.Equal(2, result!.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        Assert.Single(result.Items);

        Assert.Equal(
            portfolio3.Id,
            result.Items[0].PortfolioId);

        Assert.Equal(
            "Gamma",
            result.Items[0].Name);
    }

    [Fact]
    public async Task GetPortfolios_WhenCurrentUserHasNoPortfolios_ShouldReturnEmptyResult()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var userId = Guid.NewGuid();

        var otherUserPortfolio = new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Another User Portfolio");

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Portfolios.Add(
                otherUserPortfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-User-Id",
            userId.ToString());

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/portfolios");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioListResponse>();

        Assert.NotNull(result);

        Assert.Equal(1, result!.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetPortfolios_WhenPageIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var userId = Guid.NewGuid();

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-User-Id",
            userId.ToString());

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/portfolios?page=0");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(problem);

        Assert.Equal(
            (int)HttpStatusCode.BadRequest,
            problem!.Status);
    }

    [Fact]
    public async Task GetPortfolios_WhenPageSizeExceedsMaximum_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var userId = Guid.NewGuid();

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-User-Id",
            userId.ToString());

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/portfolios?pageSize=101");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(problem);

        Assert.Equal(
            (int)HttpStatusCode.BadRequest,
            problem!.Status);
    }

    private sealed record PortfolioListResponse(
        IReadOnlyList<PortfolioResponse> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    private sealed record PortfolioResponse(
        Guid PortfolioId,
        string Name);

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);
}