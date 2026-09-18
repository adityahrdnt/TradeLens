using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;

namespace TradeLens.Domain.Tests.Services;

public sealed class ValuationCalculatorTests
{
    private readonly ValuationCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldReturnMarketValueAndUnrealizedPnl()
    {
        // Arrange
        const long quantity = 100;
        const decimal costBasis = 1_000_000m;
        const decimal marketPrice = 12_000m;

        // Act
        var result = _calculator.Calculate(
            quantity,
            costBasis,
            marketPrice);

        // Assert
        Assert.Equal(
            1_200_000m,
            result.MarketValue);

        Assert.Equal(
            200_000m,
            result.UnrealizedPnl);

        Assert.Equal(
            20m,
            result.UnrealizedPnlPercentage);
    }

    [Fact]
    public void Calculate_WhenMarketPriceIsLower_ShouldReturnNegativeUnrealizedPnl()
    {
        // Arrange
        const long quantity = 100;
        const decimal costBasis = 1_000_000m;
        const decimal marketPrice = 8_000m;

        // Act
        var result = _calculator.Calculate(
            quantity,
            costBasis,
            marketPrice);

        // Assert
        Assert.Equal(
            800_000m,
            result.MarketValue);

        Assert.Equal(
            -200_000m,
            result.UnrealizedPnl);

        Assert.Equal(
            -20m,
            result.UnrealizedPnlPercentage);
    }

    [Fact]
    public void Calculate_WhenMarketPriceEqualsCostBasis_ShouldReturnZeroUnrealizedPnl()
    {
        // Arrange
        const long quantity = 100;
        const decimal costBasis = 1_000_000m;
        const decimal marketPrice = 10_000m;

        // Act
        var result = _calculator.Calculate(
            quantity,
            costBasis,
            marketPrice);

        // Assert
        Assert.Equal(
            1_000_000m,
            result.MarketValue);

        Assert.Equal(
            0m,
            result.UnrealizedPnl);

        Assert.Equal(
            0m,
            result.UnrealizedPnlPercentage);
    }

    [Fact]
    public void Calculate_WhenQuantityIsZero_ShouldReturnZeroMarketValue()
    {
        // Arrange
        const long quantity = 0;
        const decimal costBasis = 0m;
        const decimal marketPrice = 10_000m;

        // Act
        var result = _calculator.Calculate(
            quantity,
            costBasis,
            marketPrice);

        // Assert
        Assert.Equal(
            0m,
            result.MarketValue);

        Assert.Equal(
            0m,
            result.UnrealizedPnl);

        Assert.Equal(
            0m,
            result.UnrealizedPnlPercentage);
    }

    [Fact]
    public void Calculate_WhenQuantityIsNegative_ShouldThrowDomainException()
    {
        // Act
        var action = () => _calculator.Calculate(
            -1,
            1_000_000m,
            10_000m);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Calculate_WhenCostBasisIsNegative_ShouldThrowDomainException()
    {
        // Act
        var action = () => _calculator.Calculate(
            100,
            -1m,
            10_000m);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Calculate_WhenMarketPriceIsNegative_ShouldThrowDomainException()
    {
        // Act
        var action = () => _calculator.Calculate(
            100,
            1_000_000m,
            -1m);

        // Assert
        Assert.Throws<DomainException>(action);
    }
}