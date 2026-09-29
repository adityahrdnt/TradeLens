namespace TradeLens.Application.Pnl.Queries.GetPortfolioPnl;

public sealed record GetPortfolioPnlResult(
    Guid PortfolioId,
    DateTimeOffset AsOf,
    string Status,
    decimal RealizedPnl,
    decimal? UnrealizedPnl,
    decimal? TotalPnl);