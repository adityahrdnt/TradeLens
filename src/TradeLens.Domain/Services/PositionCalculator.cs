using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Services;

public class PositionCalculator
{
    public PositionCalculationResult Calculate(
        IEnumerable<Transaction> transactions)
    {
        long quantity = 0;
        decimal costBasis = 0m;
        decimal realizedPnl = 0m;

        foreach (var transaction in transactions
            .Where(x => x.Status == TransactionStatus.Active)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Sequence))
        {
            switch (transaction.Type)
            {
                case TransactionType.Buy:
                    ApplyBuy(
                        transaction,
                        ref quantity,
                        ref costBasis);

                    break;

                case TransactionType.Sell:
                    ApplySell(
                        transaction,
                        ref quantity,
                        ref costBasis,
                        ref realizedPnl);

                    break;

                default:
                    throw new DomainException(
                        $"Unsupported transaction type: {transaction.Type}");
            }
        }

        var averagePrice =
            quantity == 0
                ? 0m
                : costBasis / quantity;

        return new PositionCalculationResult(
            quantity,
            costBasis,
            averagePrice,
            realizedPnl);
    }

    public PositionCalculationResult Calculate(
        IEnumerable<Transaction> transactions,
        PositionCalculationResult initialPosition)
    {
        long quantity = initialPosition.Quantity;
        decimal costBasis = initialPosition.CostBasis;
        decimal realizedPnl = initialPosition.RealizedPnl;

        foreach (var transaction in transactions
            .Where(x => x.Status == TransactionStatus.Active)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Sequence))
        {
            switch (transaction.Type)
            {
                case TransactionType.Buy:
                    ApplyBuy(
                        transaction,
                        ref quantity,
                        ref costBasis);
                    break;

                case TransactionType.Sell:
                    ApplySell(
                        transaction,
                        ref quantity,
                        ref costBasis,
                        ref realizedPnl);
                    break;

                default:
                    throw new DomainException(
                        $"Unsupported transaction type: {transaction.Type}");
            }
        }

        var averagePrice =
            quantity == 0
                ? 0m
                : costBasis / quantity;

        return new PositionCalculationResult(
            quantity,
            costBasis,
            averagePrice,
            realizedPnl);
    }

    private static void ApplyBuy(
        Transaction transaction,
        ref long quantity,
        ref decimal costBasis)
    {
        var transactionCost =
            transaction.Quantity * transaction.Price
            + transaction.Fee;

        quantity += transaction.Quantity;
        costBasis += transactionCost;
    }

    private static void ApplySell(
        Transaction transaction,
        ref long quantity,
        ref decimal costBasis,
        ref decimal realizedPnl)
    {
        if (transaction.Quantity > quantity)
        {
            throw new PositionInsufficientQuantityException();
        }

        var averagePrice =
            quantity == 0
                ? 0m
                : costBasis / quantity;

        var costOfSold =
            transaction.Quantity * averagePrice;

        var netProceeds =
            transaction.Quantity * transaction.Price
            - transaction.Fee;

        realizedPnl +=
            netProceeds - costOfSold;

        quantity -= transaction.Quantity;
        costBasis -= costOfSold;
    }
}