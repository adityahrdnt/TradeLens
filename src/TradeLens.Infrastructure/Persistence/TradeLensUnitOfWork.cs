using TradeLens.Application.Interfaces;

namespace TradeLens.Infrastructure.Persistence;

public sealed class TradeLensUnitOfWork : IUnitOfWork
{
    private readonly TradeLensDbContext _dbContext;

    public TradeLensUnitOfWork(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}