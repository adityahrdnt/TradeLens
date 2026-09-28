namespace TradeLens.Application.Positions.Queries.GetPortfolioPositions;

public sealed record GetPortfolioPositionsResult(
    Guid PortfolioId,
    IReadOnlyList<GetPortfolioPositionsItem> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        (int)Math.Ceiling(
            TotalCount / (double)PageSize);
}