using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Services;

public sealed class ValuationCalculator
{
    public ValuationResult Calculate(
        long quantity,
        decimal costBasis,
        decimal marketPrice)
    {
        if (quantity < 0)
        {
            throw new DomainException(
                "Position quantity cannot be negative.");
        }

        if (costBasis < 0)
        {
            throw new DomainException(
                "Position cost basis cannot be negative.");
        }

        if (marketPrice < 0)
        {
            throw new DomainException(
                "Market price cannot be negative.");
        }

        var marketValue = quantity * marketPrice;

        var unrealizedPnl = marketValue - costBasis;

        var unrealizedPnlPercentage =
            costBasis == 0
                ? 0
                : unrealizedPnl / costBasis * 100;

        return new ValuationResult(
            marketValue,
            unrealizedPnl,
            unrealizedPnlPercentage);
    }
}