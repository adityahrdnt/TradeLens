using TradeLens.Application.Interfaces;
using TradeLens.Domain.Enums;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeTransactionRepository : ITransactionRepository
{
    public List<DomainTransaction> Transactions { get; } = [];

    public Task AddAsync(
        DomainTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<DomainTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var transaction = Transactions
            .FirstOrDefault(x => x.Id == id);

        return Task.FromResult(transaction);
    }

    public Task<IReadOnlyList<DomainTransaction>> GetEffectiveTransactionsAsync(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DomainTransaction> result = Transactions
            .Where(x =>
                x.PortfolioId == portfolioId &&
                x.InstrumentId == instrumentId &&
                x.Status == TransactionStatus.Active)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Sequence)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DomainTransaction>> GetByPortfolioAsync(
        Guid portfolioId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DomainTransaction> result = Transactions
            .Where(x => x.PortfolioId == portfolioId)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Sequence)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<int> CountByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var count = Transactions
            .Count(x => x.PortfolioId == portfolioId);

        return Task.FromResult(count);
    }
}