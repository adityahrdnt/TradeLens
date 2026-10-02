using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeCorporateActionRepository
    : ICorporateActionRepository
{
    public List<CorporateAction> CorporateActions { get; } = [];

    public Task AddAsync(
        CorporateAction corporateAction,
        CancellationToken cancellationToken = default)
    {
        CorporateActions.Add(corporateAction);
        return Task.CompletedTask;
    }

    public Task<CorporateAction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var corporateAction =
            CorporateActions.FirstOrDefault(x => x.Id == id);

        return Task.FromResult(corporateAction);
    }

    public Task<IReadOnlyList<CorporateAction>> GetByInstrumentAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CorporateAction> result =
            CorporateActions
                .Where(x => x.InstrumentId == instrumentId)
                .OrderBy(x => x.EffectiveDate)
                .ThenBy(x => x.Id)
                .ToList();

        return Task.FromResult(result);
    }
}