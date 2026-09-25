namespace TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

public sealed record PortfolioPositionValuationResult(
    Guid InstrumentId,
    long Quantity,
    decimal CostBasis,
    decimal AveragePrice,
    decimal? MarketPrice,
    decimal? MarketValue,
    decimal? UnrealizedPnl,
    decimal? UnrealizedPnlPercentage,
    string PriceStatus);