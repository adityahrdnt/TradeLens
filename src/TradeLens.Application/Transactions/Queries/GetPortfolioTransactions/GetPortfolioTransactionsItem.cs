namespace TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

public sealed record GetPortfolioTransactionsItem(
    Guid TransactionId,
    Guid BrokerAccountId,
    Guid InstrumentId,
    string Type,
    long Quantity,
    decimal Price,
    decimal Fee,
    DateOnly TransactionDate,
    long Sequence,
    string Status,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? SupersedesTransactionId,
    string? CorrectionReason);