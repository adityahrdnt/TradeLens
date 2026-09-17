using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeIdempotencyRepository
    : IIdempotencyRepository
{
    private readonly List<IdempotencyRecord> _records = new();

     public IReadOnlyList<IdempotencyRecord> Records => _records;

    public Task<IdempotencyRecord?> GetAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var record = _records.FirstOrDefault(
            x =>
                x.UserId == userId &&
                x.IdempotencyKey == idempotencyKey);

        return Task.FromResult(record);
    }

    public Task AddAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default)
    {
        _records.Add(record);

        return Task.CompletedTask;
    }
}