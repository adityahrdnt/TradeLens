namespace TradeLens.Api.Contracts.Transactions;

public sealed record CorrectTransactionRequest(
    long Quantity,
    decimal Price,
    decimal Fee,
    DateOnly TransactionDate,
    long Sequence,
    string Reason);