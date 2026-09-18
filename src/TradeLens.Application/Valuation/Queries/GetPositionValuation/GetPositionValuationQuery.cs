namespace TradeLens.Application.Valuation.Queries.GetPositionValuation;

public sealed record GetPositionValuationQuery(
    Guid PortfolioId,
    Guid InstrumentId,
    decimal MarketPrice);