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

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation?marketPrice=12000");

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
            $"/api/v1/portfolios/{portfolioId}/positions/{instrumentId}/valuation?marketPrice=12000");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
}