using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;

namespace TradeLens.Domain.Tests.Services;

public class CorporateActionPositionCalculatorTests
{
    [Fact]
    public void StockSplit_ShouldAdjustPositionQuantity_AndPreserveCostBasis()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

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
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            transactions);

        result.Quantity.Should().Be(200);
        result.CostBasis.Should().Be(1_000_000m);
        result.AveragePrice.Should().Be(5_000m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void ReverseSplit_ShouldAdjustPositionQuantity_AndPreserveCostBasis()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.ReverseSplit,
            1,
            2,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

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
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            transactions);

        result.Quantity.Should().Be(50);
        result.CostBasis.Should().Be(1_000_000m);
        result.AveragePrice.Should().Be(20_000m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void StockSplit_ShouldUseHoldingAtRecordDate_AsEligibleQuantity()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

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
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                50,
                12_000m,
                0m,
                new DateOnly(2026, 9, 11),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            transactions);

        result.Quantity.Should().Be(250);
        result.CostBasis.Should().Be(1_600_000m);
        result.AveragePrice.Should().Be(6_400m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void TransactionAfterEffectiveDate_ShouldBeReplayed_FromAdjustedPosition()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

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
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow),

            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Sell,
                50,
                8_000m,
                0m,
                new DateOnly(2026, 9, 13),
                2,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            transactions);

        result.Quantity.Should().Be(150);
        result.CostBasis.Should().Be(750_000m);
        result.AveragePrice.Should().Be(5_000m);
        result.RealizedPnl.Should().Be(150_000m);
    }

    [Fact]
    public void StockSplit_ShouldFloorFractionalResult()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            3,
            2,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

        var transactions = new[]
        {
            new Transaction(
                Guid.NewGuid(),
                portfolioId,
                Guid.NewGuid(),
                instrumentId,
                TransactionType.Buy,
                101,
                10_000m,
                0m,
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            transactions);

        result.Quantity.Should().Be(151);
        result.CostBasis.Should().Be(1_010_000m);
        result.AveragePrice.Should().Be(
            1_010_000m / 151m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void VoidedTransaction_ShouldBeIgnored()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

        var activeTransaction = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var voidedTransaction = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            500,
            5_000m,
            0m,
            new DateOnly(2026, 9, 6),
            2,
            userId,
            DateTimeOffset.UtcNow);

        voidedTransaction.Void("Test void");

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            new[] { activeTransaction, voidedTransaction });

        result.Quantity.Should().Be(200);
        result.CostBasis.Should().Be(1_000_000m);
        result.AveragePrice.Should().Be(5_000m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void CorrectedTransaction_ShouldIgnoreOriginalTransaction()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

        var originalTransaction = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var correctedTransaction = Transaction.CreateCorrection(
            Guid.NewGuid(),
            originalTransaction,
            120,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow,
            "Incorrect quantity");

        originalTransaction.Supersede(
            DateTimeOffset.UtcNow,
            userId,
            "Incorrect quantity");

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var result = calculator.Calculate(
            corporateAction,
            new[] { originalTransaction, correctedTransaction });

        result.Quantity.Should().Be(240);
        result.CostBasis.Should().Be(1_200_000m);
        result.AveragePrice.Should().Be(5_000m);
        result.RealizedPnl.Should().Be(0m);
    }

    [Fact]
    public void Calculate_ShouldFail_WhenCorporateActionIsNotApplied()
    {
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

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
                new DateOnly(2026, 9, 5),
                1,
                userId,
                DateTimeOffset.UtcNow)
        };

        var calculator = new CorporateActionPositionCalculator(
            new PositionCalculator(),
            new CorporateActionCalculator());

        var action = () => calculator.Calculate(
            corporateAction,
            transactions);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Only an applied corporate action can be calculated.");
    }
}
