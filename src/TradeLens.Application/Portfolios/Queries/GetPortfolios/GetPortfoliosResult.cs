namespace TradeLens.Application.Portfolios.Queries.GetPortfolios;

public sealed record GetPortfoliosResult(
    IReadOnlyList<GetPortfoliosItem> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        (int)Math.Ceiling(
            TotalCount / (double)PageSize);
}