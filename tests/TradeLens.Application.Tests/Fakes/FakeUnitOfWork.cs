using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public int TransactionCallCount { get; private set; }

    public Exception? SaveChangesException { get; set; }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (SaveChangesException is not null)
        {
            return Task.FromException<int>(SaveChangesException);
        }

        return Task.FromResult(1);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        TransactionCallCount++;

        return await operation(cancellationToken);
    }
}
