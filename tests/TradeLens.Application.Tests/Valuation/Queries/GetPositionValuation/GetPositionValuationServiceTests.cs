using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Valuation.Queries.GetPositionValuation;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;
using Xunit;

namespace TradeLens.Application.Tests.Valuation.Queries.GetPositionValuation;

public sealed class GetPositionValuationServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPositionExists_ShouldReturnValuation()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
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

        var repository = new FakePositionRepository();
        repository.Positions.Add(position);

        var marketPriceRepository = new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            repository,
            marketPriceRepository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        // Act
        var result = await service.ExecuteAsync(query);

        // Assert
        result.PortfolioId.Should().Be(portfolioId);
        result.InstrumentId.Should().Be(instrumentId);
        result.Quantity.Should().Be(100);
        result.CostBasis.Should().Be(1_000_000m);
        result.AveragePrice.Should().Be(10_000m);
        result.MarketPrice.Should().Be(12_000m);
        result.MarketValue.Should().Be(1_200_000m);
        result.UnrealizedPnl.Should().Be(200_000m);
        result.UnrealizedPnlPercentage.Should().Be(20m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionDoesNotExist_ShouldThrowPositionNotFoundException()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var repository = new FakePositionRepository();

        var marketPriceRepository = new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var service = CreateService(
            portfolioId,
            userId,
            repository,
            marketPriceRepository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        // Act
        var act = () => service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PositionNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotModifyPosition()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
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

        var repository = new FakePositionRepository();
        repository.Positions.Add(position);

        var marketPriceRepository = new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            repository,
            marketPriceRepository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        // Act
        await service.ExecuteAsync(query);

        // Assert
        position.Quantity.Should().Be(100);
        position.CostBasis.Should().Be(1_000_000m);
        position.AveragePrice.Should().Be(10_000m);
        position.Version.Should().Be(0);
    }
    
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToAnotherUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var repository = new FakePositionRepository();
        repository.Positions.Add(position);

        var marketPriceRepository = new FakeMarketPriceRepository();

        var portfolio = new Portfolio(
            portfolioId,
            ownerId,
            "Owner Portfolio");

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(anotherUserId);

        var marketPriceFreshnessPolicy =
            new FakeMarketPriceFreshnessPolicy();

        var service = new GetPositionValuationService(
            repository,
            marketPriceRepository,
            marketPriceFreshnessPolicy,
            portfolioAccessService,
            new ValuationCalculator(),
            currentUserService);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        // Act
        var act = () => service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleMarketPricesExist_ShouldUseLatestPrice()
    {
        var portfolioId = Guid.NewGuid();
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

        var positionRepository = new FakePositionRepository();
        positionRepository.Positions.Add(position);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                11_000m,
                DateTimeOffset.UtcNow.AddMinutes(-10),
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                12_000m,
                DateTimeOffset.UtcNow.AddMinutes(-5),
                "TEST"));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                13_000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        var result = await service.ExecuteAsync(query);

        result.MarketPrice.Should().Be(13_000m);
        result.MarketValue.Should().Be(1_300_000m);
        result.UnrealizedPnl.Should().Be(300_000m);
        result.UnrealizedPnlPercentage.Should().Be(30m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMarketPriceIsNotAvailable_ShouldThrowMarketPriceNotAvailableException()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var positionRepository = new FakePositionRepository();
        positionRepository.Positions.Add(position);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var service = CreateService(
            portfolioId,
            userId,
            positionRepository,
            marketPriceRepository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        var action = () => service.ExecuteAsync(query);

        await action.Should()
            .ThrowAsync<MarketPriceNotAvailableException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMarketPriceIsStale_ShouldThrowMarketPriceStaleException()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var position = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            100,
            1_000_000m,
            10_000m,
            0,
            DateTimeOffset.UtcNow);

        var positionRepository =
            new FakePositionRepository();

        positionRepository.Positions.Add(position);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var marketPrice =
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                12_000m,
                DateTimeOffset.UtcNow,
                "TEST");

        marketPriceRepository.MarketPrices.Add(
            marketPrice);

        var freshnessPolicy =
            new FakeMarketPriceFreshnessPolicy
            {
                IsFreshResult = false
            };

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

        var service =
            new GetPositionValuationService(
                positionRepository,
                marketPriceRepository,
                freshnessPolicy,
                portfolioAccessService,
                new ValuationCalculator(),
                currentUserService);

        var query =
            new GetPositionValuationQuery(
                portfolioId,
                instrumentId);

        var action =
            () => service.ExecuteAsync(query);

        await action.Should()
            .ThrowAsync<MarketPriceStaleException>();
    }

    private static GetPositionValuationService CreateService(
        Guid portfolioId,
        Guid userId,
        FakePositionRepository positionRepository,
        FakeMarketPriceRepository marketPriceRepository)
    {
        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        var marketPriceFreshnessPolicy =
            new FakeMarketPriceFreshnessPolicy();

        return new GetPositionValuationService(
            positionRepository,
            marketPriceRepository,
            marketPriceFreshnessPolicy,
            portfolioAccessService,
            new ValuationCalculator(),
            currentUserService);
    }
}