namespace TradeLens.Application.Portfolios.Queries.GetPortfolios;

public sealed record GetPortfoliosQuery(
    int Page,
    int PageSize);