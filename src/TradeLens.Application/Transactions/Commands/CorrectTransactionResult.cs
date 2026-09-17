namespace TradeLens.Application.Transactions.Commands;

public sealed record CorrectTransactionResult(
    Guid OriginalTransactionId,
    Guid CorrectedTransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);