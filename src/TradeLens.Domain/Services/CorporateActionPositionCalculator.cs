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

        var transactionsThroughRecordDate = orderedTransactions
            .Where(x => x.TransactionDate <= corporateAction.RecordDate)
            .ToList();

        var transactionsBetweenRecordAndEffectiveDate = orderedTransactions
            .Where(x =>
                x.TransactionDate > corporateAction.RecordDate &&
                x.TransactionDate <= corporateAction.EffectiveDate)
            .ToList();

        var transactionsAfterEffectiveDate = orderedTransactions
            .Where(x => x.TransactionDate > corporateAction.EffectiveDate)
            .ToList();

        var positionAtRecordDate =
            _positionCalculator.Calculate(
                transactionsThroughRecordDate);

        var positionBeforeCorporateAction =
            _positionCalculator.Calculate(
                transactionsBetweenRecordAndEffectiveDate,
                positionAtRecordDate);

        var resultingEligibleQuantity =
            _corporateActionCalculator.CalculateResultingQuantity(
                corporateAction,
                positionAtRecordDate.Quantity);

        var corporateActionQuantityAdjustment =
            resultingEligibleQuantity -
            positionAtRecordDate.Quantity;

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