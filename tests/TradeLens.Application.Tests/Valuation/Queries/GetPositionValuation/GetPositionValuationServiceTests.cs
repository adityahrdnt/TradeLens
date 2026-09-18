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

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            repository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId,
            12_000m);

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

        var service = CreateService(
            portfolioId,
            userId,
            repository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId,
            12_000m);

        // Act
        var act = () => service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PositionNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMarketPriceIsNegative_ShouldThrowDomainException()
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

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            repository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId,
            -1m);

        // Act
        var act = () => service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>();
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

        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            repository);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId,
            12_000m);

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

        var service = new GetPositionValuationService(
            repository,
            portfolioAccessService,
            new ValuationCalculator(),
            currentUserService);

        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId,
            12_000m);

        // Act
        var act = () => service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>();
    }

    private static GetPositionValuationService CreateService(
        Guid portfolioId,
        Guid userId,
        FakePositionRepository positionRepository)
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

        return new GetPositionValuationService(
            positionRepository,
            portfolioAccessService,
            new ValuationCalculator(),
            currentUserService);
    }
}