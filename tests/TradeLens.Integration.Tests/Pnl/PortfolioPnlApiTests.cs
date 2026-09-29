using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Pnl;

public sealed class PortfolioPnlApiTests
{
    [Fact]
    public async Task GetPnl_WhenPricesAreFresh_ShouldReturnCompletePnl()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();

        await SeedPortfolioAsync(
            factory,
            portfolioId,
            CustomWebApplicationFactory.TestUserId,
            "P&L Complete Test");

        var now = DateTimeOffset.UtcNow;

        var buy = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            100m,
            10m,
            new DateOnly(2026, 1, 1),
            1,
            CustomWebApplicationFactory.TestUserId,
            now.AddMinutes(-2));

        var sell = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            40,
            120m,
            5m,
            new DateOnly(2026, 1, 2),
            1,
            CustomWebApplicationFactory.TestUserId,
            now.AddMinutes(-1));

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            60,
            6_006m,
            100.10m,
            1,
            now);

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        var marketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId,
            120m,
            now,
            "TEST");

        await SeedDataAsync(
            factory,
            instrument,
            marketPrice,
            position,
            buy,
            sell);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/pnl");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<PortfolioPnlResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            portfolioId,
            result!.PortfolioId);

        Assert.Equal(
            "COMPLETE",
            result.Status);

        // Realized:
        // BUY cost = 100 * 100 + 10 = 10,010
        // Average = 100.10
        // Cost sold = 40 * 100.10 = 4,004
        // Net proceeds = 40 * 120 - 5 = 4,795
        // Realized P&L = 791
        Assert.Equal(
            791m,
            result.RealizedPnl);

        // Remaining position:
        // 60 * 120 - 6,006 = 1,194
        Assert.Equal(
            1_194m,
            result.UnrealizedPnl);

        Assert.Equal(
            1_985m,
            result.TotalPnl);
    }

    [Fact]
    public async Task GetPnl_WhenMarketPriceIsNotAvailable_ShouldReturnPartialPnl()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        await SeedPortfolioAsync(
            factory,
            portfolioId,
            CustomWebApplicationFactory.TestUserId,
            "P&L Missing Price Test");

        var now = DateTimeOffset.UtcNow;

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            10_000m,
            100m,
            1,
            now);

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        await SeedDataAsync(
            factory,
            instrument,
            position: position);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/pnl");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<PortfolioPnlResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "PARTIAL",
            result!.Status);

        Assert.Equal(
            0m,
            result.RealizedPnl);

        Assert.Null(result.UnrealizedPnl);
        Assert.Null(result.TotalPnl);
    }

    [Fact]
    public async Task GetPnl_WhenMarketPriceIsStale_ShouldReturnPartialPnl()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        await SeedPortfolioAsync(
            factory,
            portfolioId,
            CustomWebApplicationFactory.TestUserId,
            "P&L Stale Price Test");

        var now = DateTimeOffset.UtcNow;

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            10_000m,
            100m,
            1,
            now);

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        var staleMarketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId,
            120m,
            now.AddMinutes(-31),
            "TEST");

        await SeedDataAsync(
            factory,
            instrument,
            staleMarketPrice,
            position : position);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/pnl");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<PortfolioPnlResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "PARTIAL",
            result!.Status);

        Assert.Equal(
            0m,
            result.RealizedPnl);

        Assert.Null(result.UnrealizedPnl);
        Assert.Null(result.TotalPnl);
    }

    [Fact]
    public async Task GetPnl_WhenPortfolioDoesNotBelongToCurrentUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        await SeedPortfolioAsync(
            factory,
            portfolioId,
            ownerUserId,
            "Unauthorized P&L Test");

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/pnl");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            403,
            result!.Status);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            result.Code);

        Assert.Equal(
            "Portfolio Access Denied",
            result.Title);
    }

    private static async Task SeedPortfolioAsync(
        CustomWebApplicationFactory factory,
        Guid portfolioId,
        Guid userId,
        string name)
    {
        await using var scope =
            factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

        dbContext.Portfolios.Add(
            new Portfolio(
                portfolioId,
                userId,
                name));

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDataAsync(
        CustomWebApplicationFactory factory,
        Instrument instrument,
        MarketPrice? marketPrice = null,
        Position? position = null,
        params Transaction[] transactions)
    {
        await using var scope =
            factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

        dbContext.Instruments.Add(instrument);

        if (marketPrice is not null)
            dbContext.MarketPrices.Add(marketPrice);

        if (position is not null)
            dbContext.Positions.Add(position);

        if (transactions.Length > 0)
            dbContext.Transactions.AddRange(transactions);

        await dbContext.SaveChangesAsync();
    }

    private sealed record PortfolioPnlResponse(
        Guid PortfolioId,
        DateTimeOffset AsOf,
        string Status,
        decimal RealizedPnl,
        decimal? UnrealizedPnl,
        decimal? TotalPnl);

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);
}