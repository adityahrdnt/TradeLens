using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Integration.Tests.Infrastructure;
using TradeLens.Infrastructure.Persistence;
using Xunit;

namespace TradeLens.Integration.Tests.Valuation;

public sealed class PositionValuationApiTests
{
    [Fact]
    public async Task GetValuation_WhenPositionExists_ShouldReturnValuation()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = CustomWebApplicationFactory.TestPortfolioId;
        var instrumentId = Guid.NewGuid();

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        var marketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId,
            12_000m,
            DateTimeOffset.UtcNow,
            "TEST");

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.Add(instrument);
            dbContext.MarketPrices.Add(marketPrice);
            dbContext.Positions.Add(position);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ValuationResponse>();

        Assert.NotNull(result);
        Assert.Equal(portfolioId, result!.PortfolioId);
        Assert.Equal(instrumentId, result.InstrumentId);
        Assert.Equal(100, result.Quantity);
        Assert.Equal(1_000_000m, result.CostBasis);
        Assert.Equal(10_000m, result.AveragePrice);
        Assert.Equal(12_000m, result.MarketPrice);
        Assert.Equal(1_200_000m, result.MarketValue);
        Assert.Equal(200_000m, result.UnrealizedPnl);
        Assert.Equal(20m, result.UnrealizedPnlPercentage);
    }

    [Fact]
    public async Task GetValuation_WhenPositionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = CustomWebApplicationFactory.TestPortfolioId;
        var instrumentId = Guid.NewGuid();

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetValuation_WhenMarketPriceIsNotAvailable_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = CustomWebApplicationFactory.TestPortfolioId;
        var instrumentId = Guid.NewGuid();

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.Add(instrument);
            dbContext.Positions.Add(position);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);
        Assert.Equal(400, result!.Status);
        Assert.Equal(
            "MARKET_PRICE_NOT_AVAILABLE",
            result.Code);
        Assert.Equal(
            "Market Price Not Available",
            result.Title);
    }

    [Fact]
    public async Task GetValuation_WhenMarketPriceIsStale_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = CustomWebApplicationFactory.TestPortfolioId;
        var instrumentId = Guid.NewGuid();

        var instrument = new Instrument(
            instrumentId,
            $"T{instrumentId.ToString("N")[..10]}",
            "Test Instrument",
            "IDR");

        var staleMarketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId,
            12_000m,
            DateTimeOffset.UtcNow.AddMinutes(-31),
            "TEST");

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.Add(instrument);
            dbContext.MarketPrices.Add(staleMarketPrice);
            dbContext.Positions.Add(position);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);
        Assert.Equal(400, result!.Status);
        Assert.Equal(
            "MARKET_PRICE_STALE",
            result.Code);
        Assert.Equal(
            "Market Price Stale",
            result.Title);
    }

    private sealed record ValuationResponse(
        Guid PortfolioId,
        Guid InstrumentId,
        long Quantity,
        decimal CostBasis,
        decimal AveragePrice,
        decimal MarketPrice,
        decimal MarketValue,
        decimal UnrealizedPnl,
        decimal UnrealizedPnlPercentage);        

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);
}