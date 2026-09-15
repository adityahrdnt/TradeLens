using TradeLens.Domain.Entities;

namespace TradeLens.Domain.Services;

public class PnlCalculator
{
    public PnlCalculationResult Calculate(
        Position position,
        decimal marketPrice,
        decimal realizedPnl)
    {
        if (marketPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(marketPrice),
                "Market price cannot be negative.");

        var marketValue =
            position.Quantity * marketPrice;

        var unrealizedPnl =
            marketValue - position.CostBasis;

        var totalPnl =
            realizedPnl + unrealizedPnl;

        return new PnlCalculationResult(
            marketValue,
            unrealizedPnl,
            totalPnl);
    }
}