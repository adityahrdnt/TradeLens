namespace TradeLens.Application.Transactions.Queries.GetTransaction;

public sealed record GetTransactionResult(
    Guid Id,
    Guid PortfolioId,
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
    DateTimeOffset CreatedAt);