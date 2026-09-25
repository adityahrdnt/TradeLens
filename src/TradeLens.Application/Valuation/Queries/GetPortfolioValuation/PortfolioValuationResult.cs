namespace TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

public sealed record PortfolioValuationResult(
    Guid PortfolioId,
    string Status,
    decimal TotalCostBasis,
    decimal? TotalMarketValue,
    decimal? TotalUnrealizedPnl,
    decimal? TotalUnrealizedPnlPercentage,
    IReadOnlyCollection<PortfolioPositionValuationResult> Positions);