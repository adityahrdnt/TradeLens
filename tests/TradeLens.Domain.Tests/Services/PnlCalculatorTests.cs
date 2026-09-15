using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;

namespace TradeLens.Domain.Tests.Services;

public class PnlCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnUnrealizedProfit()
    {
        var position = new Position(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1_000,
            10_000_000m,
            10_000m,
            1,
            DateTimeOffset.UtcNow);

        var calculator = new PnlCalculator();

        var result = calculator.Calculate(
            position,
            12_000m,
            500_000m);

        result.MarketValue.Should().Be(
            12_000_000m);

        result.UnrealizedPnl.Should().Be(
            2_000_000m);

        result.TotalPnl.Should().Be(
            2_500_000m);
    }

    [Fact]
    public void Calculate_ShouldReturnUnrealizedLoss()
    {
        var position = new Position(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1_000,
            10_000_000m,
            10_000m,
            1,
            DateTimeOffset.UtcNow);

        var calculator = new PnlCalculator();

        var result = calculator.Calculate(
            position,
            8_000m,
            500_000m);

        result.MarketValue.Should().Be(
            8_000_000m);

        result.UnrealizedPnl.Should().Be(
            -2_000_000m);

        result.TotalPnl.Should().Be(
            -1_500_000m);
    }

    [Fact]
    public void Calculate_ShouldReturnZeroForEmptyPosition()
    {
        var position = Position.Empty(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        var calculator = new PnlCalculator();

        var result =
            calculator.Calculate(
                position,
                10_000m,
                500_000m);

        result.MarketValue.Should().Be(0m);
        result.UnrealizedPnl.Should().Be(0m);
        result.TotalPnl.Should().Be(500_000m);
    }

    [Fact]
    public void Calculate_ShouldRejectNegativeMarketPrice()
    {
        var position = Position.Empty(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        var calculator = new PnlCalculator();

        var action = () =>
            calculator.Calculate(
                position,
                -1m,
                0m);

        action.Should()
            .Throw<ArgumentOutOfRangeException>();
    }


}