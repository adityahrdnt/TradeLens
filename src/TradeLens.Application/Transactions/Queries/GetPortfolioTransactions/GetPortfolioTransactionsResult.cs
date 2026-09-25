namespace TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

public sealed record GetPortfolioTransactionsResult(
    Guid PortfolioId,
    IReadOnlyList<GetPortfolioTransactionsItem> Items,
    int Page,
    int PageSize,
    int TotalCount);