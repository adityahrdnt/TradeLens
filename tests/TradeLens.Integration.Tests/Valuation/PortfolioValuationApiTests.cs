using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Integration.Tests.Infrastructure;
using TradeLens.Infrastructure.Persistence;
using Xunit;

namespace TradeLens.Integration.Tests.Valuation;

public sealed class PortfolioValuationApiTests
{
    [Fact]
    public async Task GetValuation_WhenAllMarketPricesAreFresh_ShouldReturnCompleteValuation()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Valuation Integration Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        var instrument1 = new Instrument(
            instrumentId1,
            $"T{instrumentId1.ToString("N")[..10]}",
            "Test Instrument 1",
            "IDR");

        var instrument2 = new Instrument(
            instrumentId2,
            $"T{instrumentId2.ToString("N")[..10]}",
            "Test Instrument 2",
            "IDR");

        var marketPrice1 = new MarketPrice(
            Guid.NewGuid(),
            instrumentId1,
            12_000m,
            DateTimeOffset.UtcNow,
            "TEST");

        var marketPrice2 = new MarketPrice(
            Guid.NewGuid(),
            instrumentId2,
            22_000m,
            DateTimeOffset.UtcNow,
            "TEST");

        var position1 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId1,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var position2 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId2,
            200,
            4_000_000m,
            20_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.AddRange(
                instrument1,
                instrument2);

            dbContext.MarketPrices.AddRange(
                marketPrice1,
                marketPrice2);

            dbContext.Positions.AddRange(
                position1,
                position2);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/valuation");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioValuationResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            portfolioId,
            result!.PortfolioId);

        foreach (var position in result.Positions)
        {
            Assert.True(
                position.PriceStatus == "FRESH",
                $"Instrument {position.InstrumentId} " +
                $"has PriceStatus={position.PriceStatus}, " +
                $"MarketPrice={position.MarketPrice}");
        }

        Assert.Equal(
            "COMPLETE",
            result.Status);

        Assert.Equal(
            5_000_000m,
            result.TotalCostBasis);

        Assert.Equal(
            5_600_000m,
            result.TotalMarketValue);

        Assert.Equal(
            600_000m,
            result.TotalUnrealizedPnl);

        Assert.Equal(
            12m,
            result.TotalUnrealizedPnlPercentage);

        Assert.Equal(
            2,
            result.Positions.Count);

        var positionResult1 =
            result.Positions
                .Single(x =>
                    x.InstrumentId == instrumentId1);

        Assert.Equal(
            100,
            positionResult1.Quantity);

        Assert.Equal(
            1_000_000m,
            positionResult1.CostBasis);

        Assert.Equal(
            10_000m,
            positionResult1.AveragePrice);

        Assert.Equal(
            12_000m,
            positionResult1.MarketPrice);

        Assert.Equal(
            1_200_000m,
            positionResult1.MarketValue);

        Assert.Equal(
            200_000m,
            positionResult1.UnrealizedPnl);

        Assert.Equal(
            "FRESH",
            positionResult1.PriceStatus);

        var positionResult2 =
            result.Positions
                .Single(x =>
                    x.InstrumentId == instrumentId2);

        Assert.Equal(
            200,
            positionResult2.Quantity);

        Assert.Equal(
            4_000_000m,
            positionResult2.CostBasis);

        Assert.Equal(
            20_000m,
            positionResult2.AveragePrice);

        Assert.Equal(
            22_000m,
            positionResult2.MarketPrice);

        Assert.Equal(
            4_400_000m,
            positionResult2.MarketValue);

        Assert.Equal(
            400_000m,
            positionResult2.UnrealizedPnl);

        Assert.Equal(
            "FRESH",
            positionResult2.PriceStatus);
    }

    [Fact]
    public async Task GetValuation_WhenOneMarketPriceIsNotAvailable_ShouldReturnPartialValuation()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Valuation Partial Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        var instrument1 = new Instrument(
            instrumentId1,
            $"T{instrumentId1.ToString("N")[..10]}",
            "Test Instrument 1",
            "IDR");

        var instrument2 = new Instrument(
            instrumentId2,
            $"T{instrumentId2.ToString("N")[..10]}",
            "Test Instrument 2",
            "IDR");

        var marketPrice1 = new MarketPrice(
            Guid.NewGuid(),
            instrumentId1,
            12_000m,
            DateTimeOffset.UtcNow,
            "TEST");

        var position1 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId1,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var position2 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId2,
            200,
            4_000_000m,
            20_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.AddRange(
                instrument1,
                instrument2);

            dbContext.MarketPrices.Add(marketPrice1);

            dbContext.Positions.AddRange(
                position1,
                position2);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/valuation");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioValuationResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "PARTIAL",
            result!.Status);

        Assert.Equal(
            5_000_000m,
            result.TotalCostBasis);

        Assert.Null(result.TotalMarketValue);
        Assert.Null(result.TotalUnrealizedPnl);
        Assert.Null(result.TotalUnrealizedPnlPercentage);

        Assert.Equal(
            2,
            result.Positions.Count);

        var freshPosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId1);

        Assert.Equal(
            "FRESH",
            freshPosition.PriceStatus);

        Assert.Equal(
            12_000m,
            freshPosition.MarketPrice);

        Assert.Equal(
            1_200_000m,
            freshPosition.MarketValue);

        Assert.Equal(
            200_000m,
            freshPosition.UnrealizedPnl);

        var unavailablePosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId2);

        Assert.Equal(
            "NOT_AVAILABLE",
            unavailablePosition.PriceStatus);

        Assert.Null(unavailablePosition.MarketPrice);
        Assert.Null(unavailablePosition.MarketValue);
        Assert.Null(unavailablePosition.UnrealizedPnl);
    }

    [Fact]
    public async Task GetValuation_WhenOneMarketPriceIsStale_ShouldReturnPartialValuation()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Valuation Stale Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        var instrument1 = new Instrument(
            instrumentId1,
            $"T{instrumentId1.ToString("N")[..10]}",
            "Test Instrument 1",
            "IDR");

        var instrument2 = new Instrument(
            instrumentId2,
            $"T{instrumentId2.ToString("N")[..10]}",
            "Test Instrument 2",
            "IDR");

        var freshMarketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId1,
            12_000m,
            DateTimeOffset.UtcNow,
            "TEST");

        var staleMarketPrice = new MarketPrice(
            Guid.NewGuid(),
            instrumentId2,
            22_000m,
            DateTimeOffset.UtcNow.AddMinutes(-31),
            "TEST");

        var position1 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId1,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var position2 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId2,
            200,
            4_000_000m,
            20_000m,
            0,
            DateTimeOffset.UtcNow);

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Instruments.AddRange(
                instrument1,
                instrument2);

            dbContext.MarketPrices.AddRange(
                freshMarketPrice,
                staleMarketPrice);

            dbContext.Positions.AddRange(
                position1,
                position2);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/valuation");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioValuationResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            "PARTIAL",
            result!.Status);

        Assert.Equal(
            5_000_000m,
            result.TotalCostBasis);

        Assert.Null(result.TotalMarketValue);
        Assert.Null(result.TotalUnrealizedPnl);
        Assert.Null(result.TotalUnrealizedPnlPercentage);

        Assert.Equal(
            2,
            result.Positions.Count);

        var freshPosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId1);

        Assert.Equal(
            "FRESH",
            freshPosition.PriceStatus);

        Assert.Equal(
            12_000m,
            freshPosition.MarketPrice);

        Assert.Equal(
            1_200_000m,
            freshPosition.MarketValue);

        Assert.Equal(
            200_000m,
            freshPosition.UnrealizedPnl);

        var stalePosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId2);

        Assert.Equal(
            "STALE",
            stalePosition.PriceStatus);

        Assert.Null(stalePosition.MarketPrice);
        Assert.Null(stalePosition.MarketValue);
        Assert.Null(stalePosition.UnrealizedPnl);
    }
    
    [Fact]
    public async Task GetValuation_WhenPortfolioDoesNotBelongToCurrentUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                ownerUserId,
                "Unauthorized Portfolio Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/valuation");

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

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);

    private sealed record PortfolioValuationResponse(
        Guid PortfolioId,
        string Status,
        decimal TotalCostBasis,
        decimal? TotalMarketValue,
        decimal? TotalUnrealizedPnl,
        decimal? TotalUnrealizedPnlPercentage,
        IReadOnlyCollection<PositionValuationResponse> Positions);

    private sealed record PositionValuationResponse(
        Guid InstrumentId,
        long Quantity,
        decimal CostBasis,
        decimal AveragePrice,
        decimal? MarketPrice,
        decimal? MarketValue,
        decimal? UnrealizedPnl,
        decimal? UnrealizedPnlPercentage,
        string PriceStatus);
}