namespace TradeLens.Application.Transactions.Commands.VoidTransaction;

public sealed record VoidTransactionResult(
    Guid TransactionId,
    Guid PositionId,
    long PositionQuantity,
    decimal PositionCostBasis,
    decimal PositionAveragePrice);