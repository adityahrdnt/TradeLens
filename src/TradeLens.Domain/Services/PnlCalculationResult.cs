namespace TradeLens.Domain.Services;

public sealed record PnlCalculationResult(
    decimal MarketValue,
    decimal UnrealizedPnl,
    decimal TotalPnl);