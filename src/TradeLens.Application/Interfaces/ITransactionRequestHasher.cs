namespace TradeLens.Application.Interfaces;

public interface ITransactionRequestHasher
{
    string ComputeHash(
        Guid portfolioId,
        Guid brokerAccountId,
        Guid instrumentId,
        string type,
        long quantity,
        decimal price,
        decimal fee,
        DateOnly transactionDate,
        long sequence);
}