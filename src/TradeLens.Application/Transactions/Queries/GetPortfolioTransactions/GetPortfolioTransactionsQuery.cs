namespace TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

public sealed record GetPortfolioTransactionsQuery(
    Guid PortfolioId,
    int Page,
    int PageSize);