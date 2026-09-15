using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetEffectiveTransactionsAsync(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken = default);
}