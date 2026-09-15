using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TradeLensDbContext _dbContext;

    public TransactionRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(
            transaction,
            cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>>
        GetEffectiveTransactionsAsync(
            Guid portfolioId,
            Guid instrumentId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Where(x =>
                x.PortfolioId == portfolioId &&
                x.InstrumentId == instrumentId &&
                x.Status == TransactionStatus.Active)
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.Sequence)
            .ToListAsync(cancellationToken);
    }
}