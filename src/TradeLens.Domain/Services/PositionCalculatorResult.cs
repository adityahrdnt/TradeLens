namespace TradeLens.Domain.Services;

public sealed record PositionCalculationResult(
    long Quantity,
    decimal CostBasis,
    decimal AveragePrice,
    decimal RealizedPnl);