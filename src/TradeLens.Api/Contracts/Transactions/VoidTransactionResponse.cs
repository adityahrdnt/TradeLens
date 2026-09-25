namespace TradeLens.Api.Contracts.Transactions;

public sealed record VoidTransactionResponse(
    Guid TransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);