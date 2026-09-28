namespace TradeLens.Application.Positions.Queries.GetPortfolioPositions;

public sealed record GetPortfolioPositionsQuery(
    Guid PortfolioId,
    int Page,
    int PageSize);