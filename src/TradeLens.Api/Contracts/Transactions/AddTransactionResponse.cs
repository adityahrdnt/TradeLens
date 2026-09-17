namespace TradeLens.Api.Contracts.Transactions;

public sealed record AddTransactionResponse(
    Guid TransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);