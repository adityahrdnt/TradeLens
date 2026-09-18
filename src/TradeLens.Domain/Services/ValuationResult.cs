namespace TradeLens.Domain.Services;

public sealed record ValuationResult(
    decimal MarketValue,
    decimal UnrealizedPnl,
    decimal UnrealizedPnlPercentage);