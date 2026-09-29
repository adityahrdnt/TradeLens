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

    Task<IReadOnlyList<Transaction>> GetEffectiveByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByPortfolioAsync(
        Guid portfolioId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);
}