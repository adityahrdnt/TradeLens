namespace TradeLens.Application.Portfolios.Queries.GetPortfolios;

public sealed record GetPortfoliosItem(
    Guid PortfolioId,
    string Name);