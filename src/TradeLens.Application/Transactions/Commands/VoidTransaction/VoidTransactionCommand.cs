namespace TradeLens.Application.Transactions.Commands.VoidTransaction;

public sealed record VoidTransactionCommand(
    Guid TransactionId,
    Guid VoidedBy,
    string Reason);