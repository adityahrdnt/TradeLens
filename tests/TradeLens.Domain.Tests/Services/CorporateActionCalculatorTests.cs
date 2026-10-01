using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Services;

namespace TradeLens.Domain.Tests.Services;

public class CorporateActionCalculatorTests
{
    [Fact]
    public void CalculateResultingQuantity_ShouldApplyStockSplitRatio()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.StockSplit,
            numerator: 2,
            denominator: 1);

        var calculator = new CorporateActionCalculator();

        var result = calculator.CalculateResultingQuantity(
            corporateAction,
            eligibleQuantity: 100);

        result.Should().Be(200);
    }

    [Fact]
    public void CalculateResultingQuantity_ShouldApplyFractionalRatioAndRoundDown()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.StockSplit,
            numerator: 3,
            denominator: 2);

        var calculator = new CorporateActionCalculator();

        var result = calculator.CalculateResultingQuantity(
            corporateAction,
            eligibleQuantity: 101);

        result.Should().Be(151);
    }

    [Fact]
    public void CalculateResultingQuantity_ShouldApplyReverseSplitRatio()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.ReverseSplit,
            numerator: 1,
            denominator: 2);

        var calculator = new CorporateActionCalculator();

        var result = calculator.CalculateResultingQuantity(
            corporateAction,
            eligibleQuantity: 101);

        result.Should().Be(50);
    }

    [Fact]
    public void CalculateResultingQuantity_ShouldAllowZeroResult()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.ReverseSplit,
            numerator: 1,
            denominator: 10);

        var calculator = new CorporateActionCalculator();

        var result = calculator.CalculateResultingQuantity(
            corporateAction,
            eligibleQuantity: 5);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateResultingQuantity_ShouldRejectNegativeEligibleQuantity()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.StockSplit,
            numerator: 2,
            denominator: 1);

        var calculator = new CorporateActionCalculator();

        var action = () =>
            calculator.CalculateResultingQuantity(
                corporateAction,
                eligibleQuantity: -1);

        action.Should()
            .Throw<TradeLens.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public void CalculateResultingQuantity_ShouldRejectScheduledCorporateAction()
    {
        var corporateAction = CreateCorporateAction(
            CorporateActionType.StockSplit,
            numerator: 2,
            denominator: 1,
            apply: false);

        var calculator = new CorporateActionCalculator();

        var action = () =>
            calculator.CalculateResultingQuantity(
                corporateAction,
                eligibleQuantity: 100);

        action.Should()
            .Throw<TradeLens.Domain.Exceptions.DomainException>();
    }

    private static CorporateAction CreateCorporateAction(
        CorporateActionType type,
        int numerator,
        int denominator,
        bool apply = true)
    {
        var action = new CorporateAction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            type,
            numerator,
            denominator,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 2),
            new DateOnly(2026, 9, 3),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        if (apply)
        {
            action.Apply(
                DateTimeOffset.UtcNow,
                Guid.NewGuid());
        }

        return action;
    }
}