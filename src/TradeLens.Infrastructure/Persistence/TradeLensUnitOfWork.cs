using Microsoft.EntityFrameworkCore;
using Npgsql;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Infrastructure.Persistence;

public sealed class TradeLensUnitOfWork : IUnitOfWork
{
    private readonly TradeLensDbContext _dbContext;

    public TradeLensUnitOfWork(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();

            throw new PositionConcurrencyException();
        }
    }
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var result = await operation(cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return result;
        }
        catch (DbUpdateException ex)
            when (IsIdempotencyUniqueViolation(ex))
        {
            await transaction.RollbackAsync(
                cancellationToken);

            _dbContext.ChangeTracker.Clear();

            throw new IdempotencyConcurrencyException();
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static bool IsIdempotencyUniqueViolation(
        DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName
                == "IX_idempotency_records_UserId_IdempotencyKey";
    }
}