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
}