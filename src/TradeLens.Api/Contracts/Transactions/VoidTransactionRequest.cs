namespace TradeLens.Api.Contracts.Transactions;

public sealed record VoidTransactionRequest(
    string Reason);