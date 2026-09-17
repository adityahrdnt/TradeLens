namespace TradeLens.Api.Contracts.Transactions;

public sealed record CorrectTransactionResponse(
    Guid OriginalTransactionId,
    Guid CorrectedTransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);