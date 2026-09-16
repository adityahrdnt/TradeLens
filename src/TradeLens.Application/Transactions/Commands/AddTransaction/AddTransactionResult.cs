namespace TradeLens.Application.Transactions.Commands.AddTransaction;

public sealed record AddTransactionResult(
    Guid TransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);