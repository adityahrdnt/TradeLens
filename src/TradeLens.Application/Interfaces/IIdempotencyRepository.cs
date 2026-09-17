using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default);
}