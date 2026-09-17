using TradeLens.Domain.Enums;

namespace TradeLens.Application.Transactions.Commands.CorrectTransaction;

public sealed record CorrectTransactionCommand(
    Guid TransactionId,
    long Quantity,
    decimal Price,
    decimal Fee,
    DateOnly TransactionDate,
    long Sequence,
    Guid CorrectedBy,
    string Reason);