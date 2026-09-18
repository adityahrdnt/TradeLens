namespace TradeLens.Application.Valuation.Queries.GetPositionValuation;

public sealed record GetPositionValuationResult(
    Guid PortfolioId,
    Guid InstrumentId,
    long Quantity,
    decimal CostBasis,
    decimal AveragePrice,
    decimal MarketPrice,
    decimal MarketValue,
    decimal UnrealizedPnl,
    decimal UnrealizedPnlPercentage);