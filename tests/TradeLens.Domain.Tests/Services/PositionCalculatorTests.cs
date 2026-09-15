using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;

namespace TradeLens.Domain.Tests.Services;

public class PositionCalculatorTests
{
    [Fact]
    public void Buy_ShouldCreatePosition_WhenCurrentPositionIsEmpty()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                100,
                10_000m,
                1_000m,
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new PositionCalculator();

        var result = calculator.Calculate(transactions);

        result.Quantity.Should().Be(100);
        result.CostBasis.Should().Be(1_001_000m);
        result.AveragePrice.Should().Be(10_010m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void Sell_ShouldCalculateRealizedPnl()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                1_000,
                8_000m,
                0m,
                new DateOnly(2026, 9, 1),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Sell,
                400,
                10_000m,
                10_000m,
                new DateOnly(2026, 9, 5),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new PositionCalculator();

        var result = calculator.Calculate(transactions);

        result.Quantity.Should().Be(600);
        result.CostBasis.Should().Be(4_800_000m);
        result.AveragePrice.Should().Be(8_000m);
        result.RealizedPnl.Should().Be(790_000m);
    }

    [Fact]
    public void Sell_ShouldFail_WhenQuantityExceedsPosition()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                100,
                10_000m,
                0m,
                new DateOnly(2026, 9, 1),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Sell,
                200,
                10_000m,
                0m,
                new DateOnly(2026, 9, 5),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new PositionCalculator();

        var action = () => calculator.Calculate(transactions);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Sell quantity cannot exceed current position.");
    }

    [Fact]
    public void Buy_ShouldCalculateWeightedAverageCost()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                100,
                10_000m,
                0m,
                new DateOnly(2026, 9, 1),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                200,
                12_000m,
                0m,
                new DateOnly(2026, 9, 2),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new PositionCalculator();

        var result = calculator.Calculate(transactions);

        result.Quantity.Should().Be(300);
        result.CostBasis.Should().Be(3_400_000m);
        result.AveragePrice.Should().Be(3_400_000m / 300m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void FullSell_ShouldCreateZeroPosition()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                1_000,
                10_000m,
                0m,
                new DateOnly(2026, 9, 1),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Sell,
                1_000,
                12_000m,
                0m,
                new DateOnly(2026, 9, 5),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new PositionCalculator();

        var result = calculator.Calculate(transactions);

        result.Quantity.Should().Be(0);
        result.CostBasis.Should().Be(0m);
        result.AveragePrice.Should().Be(0m);
        result.RealizedPnl.Should().Be(2_000_000m);
    }
}