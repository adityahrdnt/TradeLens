using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public class FakeCorporateActionChangeRepository
    : ICorporateActionChangeRepository
{
    public List<CorporateActionChange> Changes { get; } = new();

    public Task AddAsync(
        CorporateActionChange change,
        CancellationToken cancellationToken = default)
    {
        Changes.Add(change);

        return Task.CompletedTask;
    }
}
