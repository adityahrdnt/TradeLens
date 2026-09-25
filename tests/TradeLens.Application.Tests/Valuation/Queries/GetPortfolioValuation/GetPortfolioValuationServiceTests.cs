using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Valuation.Queries.GetPortfolioValuation;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;
using Xunit;

namespace TradeLens.Application.Tests.Valuation.Queries.GetPortfolioValuation;

public sealed class GetPortfolioValuationServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAllMarketPricesAreFresh_ShouldReturnCompleteValuation()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

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

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(position1);
        positionRepository.Positions.Add(position2);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId1,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId2,
                22_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.PortfolioId
            .Should()
            .Be(portfolioId);

        result.Status
            .Should()
            .Be("COMPLETE");

        result.TotalCostBasis
            .Should()
            .Be(5_000_000m);

        result.TotalMarketValue
            .Should()
            .Be(5_600_000m);

        result.TotalUnrealizedPnl
            .Should()
            .Be(600_000m);

        result.TotalUnrealizedPnlPercentage
            .Should()
            .Be(12m);

        result.Positions
            .Should()
            .HaveCount(2);

        result.Positions
            .Should()
            .AllSatisfy(position =>
            {
                position.PriceStatus
                    .Should()
                    .Be("FRESH");

                position.MarketPrice
                    .Should()
                    .NotBeNull();

                position.MarketValue
                    .Should()
                    .NotBeNull();

                position.UnrealizedPnl
                    .Should()
                    .NotBeNull();
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneMarketPriceIsStale_ShouldReturnPartialValuation()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

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

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(position1);
        positionRepository.Positions.Add(position2);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId1,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId2,
                22_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var freshnessPolicy =
            new FakeMarketPriceFreshnessPolicy
            {
                IsFreshResult = true
            };

        // Make the second price stale.
        freshnessPolicy.IsFreshOverride =
            marketPrice => marketPrice.InstrumentId != instrumentId2;

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository,
            freshnessPolicy);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Status
            .Should()
            .Be("PARTIAL");

        result.TotalCostBasis
            .Should()
            .Be(5_000_000m);

        result.TotalMarketValue
            .Should()
            .BeNull();

        result.TotalUnrealizedPnl
            .Should()
            .BeNull();

        result.TotalUnrealizedPnlPercentage
            .Should()
            .BeNull();

        result.Positions
            .Should()
            .HaveCount(2);

        var freshPosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId1);

        freshPosition.PriceStatus
            .Should()
            .Be("FRESH");

        freshPosition.MarketValue
            .Should()
            .Be(1_200_000m);

        freshPosition.UnrealizedPnl
            .Should()
            .Be(200_000m);

        var stalePosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId2);

        stalePosition.PriceStatus
            .Should()
            .Be("STALE");

        stalePosition.MarketPrice
            .Should()
            .BeNull();

        stalePosition.MarketValue
            .Should()
            .BeNull();

        stalePosition.UnrealizedPnl
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneMarketPriceIsNotAvailable_ShouldReturnPartialValuation()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

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

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(position1);
        positionRepository.Positions.Add(position2);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        // Only instrument 1 has a market price.
        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId1,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Status
            .Should()
            .Be("PARTIAL");

        result.TotalCostBasis
            .Should()
            .Be(5_000_000m);

        result.TotalMarketValue
            .Should()
            .BeNull();

        result.TotalUnrealizedPnl
            .Should()
            .BeNull();

        result.TotalUnrealizedPnlPercentage
            .Should()
            .BeNull();

        result.Positions
            .Should()
            .HaveCount(2);

        var freshPosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId1);

        freshPosition.PriceStatus
            .Should()
            .Be("FRESH");

        freshPosition.MarketPrice
            .Should()
            .Be(12_000m);

        freshPosition.MarketValue
            .Should()
            .Be(1_200_000m);

        freshPosition.UnrealizedPnl
            .Should()
            .Be(200_000m);

        var unavailablePosition =
            result.Positions
                .Single(x => x.InstrumentId == instrumentId2);

        unavailablePosition.PriceStatus
            .Should()
            .Be("NOT_AVAILABLE");

        unavailablePosition.MarketPrice
            .Should()
            .BeNull();

        unavailablePosition.MarketValue
            .Should()
            .BeNull();

        unavailablePosition.UnrealizedPnl
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotBelongToCurrentUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var positionRepository =
            new FakePositionRepository();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var portfolio =
            new Portfolio(
                portfolioId,
                ownerUserId,
                "Test Portfolio");

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(
                portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(currentUserId);

        var freshnessPolicy =
            new FakeMarketPriceFreshnessPolicy();

        var service =
            new GetPortfolioValuationService(
                positionRepository,
                marketPriceRepository,
                freshnessPolicy,
                portfolioAccessService,
                new ValuationCalculator(),
                currentUserService);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        var act = () =>
            service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioHasNoPositions_ShouldReturnEmptyCompleteValuation()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var positionRepository =
            new FakePositionRepository();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.PortfolioId
            .Should()
            .Be(portfolioId);

        result.Status
            .Should()
            .Be("COMPLETE");

        result.TotalCostBasis
            .Should()
            .Be(0m);

        result.TotalMarketValue
            .Should()
            .Be(0m);

        result.TotalUnrealizedPnl
            .Should()
            .Be(0m);

        result.TotalUnrealizedPnlPercentage
            .Should()
            .Be(0m);

        result.Positions
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseSameReferenceTimeForAllMarketPriceFreshnessChecks()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId1,
                100,
                1_000_000m,
                10_000m,
                0,
                DateTimeOffset.UtcNow));

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId2,
                200,
                4_000_000m,
                20_000m,
                0,
                DateTimeOffset.UtcNow));

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId1,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId2,
                22_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var freshnessPolicy =
            new FakeMarketPriceFreshnessPolicy();

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository,
            freshnessPolicy);

        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        // Act
        await service.ExecuteAsync(query);

        // Assert
        freshnessPolicy.LastNow
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateTotalUnrealizedPnlPercentageUsingTotalCostBasis()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId1,
                100,
                1_000_000m,
                10_000m,
                0,
                DateTimeOffset.UtcNow));

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId2,
                100,
                9_000_000m,
                90_000m,
                0,
                DateTimeOffset.UtcNow));

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId1,
                11_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId2,
                99_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        // Act
        var result =
            await service.ExecuteAsync(
                new GetPortfolioValuationQuery(portfolioId));

        // Assert
        result.TotalCostBasis
            .Should()
            .Be(10_000_000m);

        result.TotalMarketValue
            .Should()
            .Be(11_000_000m);

        result.TotalUnrealizedPnl
            .Should()
            .Be(1_000_000m);

        result.TotalUnrealizedPnlPercentage
            .Should()
            .Be(10m);
    }

    private static GetPortfolioValuationService CreateService(
        Guid portfolioId,
        Guid userId,
        FakePositionRepository positionRepository,
        FakeMarketPriceRepository marketPriceRepository,
        FakeMarketPriceFreshnessPolicy? marketPriceFreshnessPolicy = null)
    {
        var portfolio =
            new Portfolio(
                portfolioId,
                userId,
                "Test Portfolio");

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(
                portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        marketPriceFreshnessPolicy ??=
            new FakeMarketPriceFreshnessPolicy();

        return new GetPortfolioValuationService(
            positionRepository,
            marketPriceRepository,
            marketPriceFreshnessPolicy,
            portfolioAccessService,
            new ValuationCalculator(),
            currentUserService);
    }
}