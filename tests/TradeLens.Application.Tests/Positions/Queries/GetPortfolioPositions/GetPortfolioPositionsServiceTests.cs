using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Positions.Queries.GetPortfolioPositions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Positions.Queries.GetPortfolioPositions;

public sealed class GetPortfolioPositionsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToCurrentUser_ShouldReturnPositions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var instrumentId1 =
            Guid.Parse("00000000-0000-0000-0000-000000000001");

        var instrumentId2 =
            Guid.Parse("00000000-0000-0000-0000-000000000002");

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "My Portfolio");

        var position1 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId1,
            100,
            1_000_000m,
            10_000m,
            1,
            DateTimeOffset.UtcNow);

        var position2 = new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId2,
            50,
            600_000m,
            12_000m,
            1,
            DateTimeOffset.UtcNow);

        var positionRepository =
            new FakePositionRepository();

        var portfolioRepository =
            new FakePortfolioRepository();

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        portfolioRepository.Portfolios.Add(portfolio);

        positionRepository.Positions.Add(position1);
        positionRepository.Positions.Add(position2);

        var service = new GetPortfolioPositionsService(
            positionRepository,
            portfolioAccessService,
            currentUserService);

        var query = new GetPortfolioPositionsQuery(
            portfolioId,
            1,
            20);

        // Act
        var result = await service.ExecuteAsync(query);

        // Assert
        result.PortfolioId.Should().Be(portfolioId);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        result.Items[0].PositionId
            .Should().Be(position1.Id);

        result.Items[0].InstrumentId
            .Should().Be(instrumentId1);

        result.Items[0].Quantity
            .Should().Be(100);

        result.Items[0].CostBasis
            .Should().Be(1_000_000m);

        result.Items[0].AveragePrice
            .Should().Be(10_000m);

        result.Items[1].PositionId
            .Should().Be(position2.Id);

        result.Items[1].InstrumentId
            .Should().Be(instrumentId2);

        result.Items[1].Quantity
            .Should().Be(50);

        result.Items[1].CostBasis
            .Should().Be(600_000m);

        result.Items[1].AveragePrice
            .Should().Be(12_000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestingSecondPage_ShouldReturnCorrectPositions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var instrumentId1 =
            Guid.Parse("00000000-0000-0000-0000-000000000001");

        var instrumentId2 =
            Guid.Parse("00000000-0000-0000-0000-000000000002");

        var instrumentId3 =
            Guid.Parse("00000000-0000-0000-0000-000000000003");

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "My Portfolio");

        var position1 = CreatePosition(
            portfolioId,
            instrumentId1,
            100,
            1_000_000m,
            10_000m);

        var position2 = CreatePosition(
            portfolioId,
            instrumentId2,
            50,
            600_000m,
            12_000m);

        var position3 = CreatePosition(
            portfolioId,
            instrumentId3,
            200,
            3_000_000m,
            15_000m);

        var positionRepository =
            new FakePositionRepository();

        var portfolioRepository =
            new FakePortfolioRepository();

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        portfolioRepository.Portfolios.Add(portfolio);

        positionRepository.Positions.Add(position1);
        positionRepository.Positions.Add(position2);
        positionRepository.Positions.Add(position3);

        var service = new GetPortfolioPositionsService(
            positionRepository,
            portfolioAccessService,
            currentUserService);

        var query = new GetPortfolioPositionsQuery(
            portfolioId,
            2,
            2);

        // Act
        var result = await service.ExecuteAsync(query);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);

        result.Items.Should().HaveCount(1);

        result.Items[0].PositionId
            .Should().Be(position3.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToAnotherUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var portfolioOwnerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            portfolioOwnerId,
            "Another User Portfolio");

        var positionRepository =
            new FakePositionRepository();

        var portfolioRepository =
            new FakePortfolioRepository();

        portfolioRepository.Portfolios.Add(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var service =
            new GetPortfolioPositionsService(
                positionRepository,
                portfolioAccessService,
                new FakeCurrentUserService(currentUserId));

        var query = new GetPortfolioPositionsQuery(
            portfolioId,
            1,
            20);

        // Act
        var act = async () =>
            await service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>()
            .WithMessage("You do not have access to this portfolio.");
    }

    private static Position CreatePosition(
        Guid portfolioId,
        Guid instrumentId,
        long quantity,
        decimal costBasis,
        decimal averagePrice)
    {
        return new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            quantity,
            costBasis,
            averagePrice,
            1,
            DateTimeOffset.UtcNow);
    }
}