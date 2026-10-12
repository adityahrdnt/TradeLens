using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Services;

public class CorporateActionPositionCalculator
{
    private readonly PositionCalculator _positionCalculator;
    private readonly CorporateActionCalculator _corporateActionCalculator;

    public CorporateActionPositionCalculator(
        PositionCalculator positionCalculator,
        CorporateActionCalculator corporateActionCalculator)
    {
        _positionCalculator = positionCalculator;
        _corporateActionCalculator = corporateActionCalculator;
    }

    public PositionCalculationResult Calculate(
        CorporateAction corporateAction,
        IEnumerable<Transaction> transactions)
    {
        if (corporateAction.Status != CorporateActionStatus.Applied)
            throw new DomainException(
                "Only an applied corporate action can be calculated.");

        var orderedTransactions = transactions
            .Where(x => x.Status == TransactionStatus.Active)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Sequence)
            .ToList();

        var transactionsThroughEligibilityDate = orderedTransactions
            .Where(x => x.TransactionDate <= corporateAction.EligibilityDate)
            .ToList();

        var transactionsBetweenEligibilityAndEffectiveDate = orderedTransactions
            .Where(x =>
                x.TransactionDate > corporateAction.EligibilityDate &&
                x.TransactionDate <= corporateAction.EffectiveDate)
            .ToList();

        var transactionsAfterEffectiveDate = orderedTransactions
            .Where(x => x.TransactionDate > corporateAction.EffectiveDate)
            .ToList();

        var positionAtEligibilityDate =
            _positionCalculator.Calculate(
                transactionsThroughEligibilityDate);

        var positionBeforeCorporateAction =
            _positionCalculator.Calculate(
                transactionsBetweenEligibilityAndEffectiveDate,
                positionAtEligibilityDate);

        var resultingEligibleQuantity =
            _corporateActionCalculator.CalculateResultingQuantity(
                corporateAction,
                positionAtEligibilityDate.Quantity);

        var corporateActionQuantityAdjustment =
            resultingEligibleQuantity -
            positionAtEligibilityDate.Quantity;

        var adjustedQuantity =
            positionBeforeCorporateAction.Quantity +
            corporateActionQuantityAdjustment;

        var adjustedAveragePrice =
            adjustedQuantity == 0
                ? 0m
                : positionBeforeCorporateAction.CostBasis / adjustedQuantity;

        var adjustedPosition =
            new PositionCalculationResult(
                adjustedQuantity,
                positionBeforeCorporateAction.CostBasis,
                adjustedAveragePrice,
                positionBeforeCorporateAction.RealizedPnl);

        if (transactionsAfterEffectiveDate.Count == 0)
            return adjustedPosition;

        return _positionCalculator.Calculate(
            transactionsAfterEffectiveDate,
            adjustedPosition);
    }
}
