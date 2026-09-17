using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly TradeLensDbContext _dbContext;

    public IdempotencyRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyRecord?> GetAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IdempotencyRecords
            .FirstOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public async Task AddAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IdempotencyRecords.AddAsync(
            record,
            cancellationToken);
    }
}