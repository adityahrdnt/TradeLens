using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeCorporateActionApplicationRepository
    : ICorporateActionApplicationRepository
{
    public List<CorporateActionApplication> Applications { get; } = [];

    public Task AddAsync(
        CorporateActionApplication application,
        CancellationToken cancellationToken = default)
    {
        Applications.Add(application);
        return Task.CompletedTask;
    }

    public Task<CorporateActionApplication?>
        GetByCorporateActionAndPortfolioAsync(
            Guid corporateActionId,
            Guid portfolioId,
            CancellationToken cancellationToken = default)
    {
        var application =
            Applications.FirstOrDefault(x =>
                x.CorporateActionId == corporateActionId &&
                x.PortfolioId == portfolioId);

        return Task.FromResult(application);
    }

    public Task<IReadOnlyList<CorporateActionApplication>>
        GetByPortfolioAndInstrumentAsync(
            Guid portfolioId,
            Guid instrumentId,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CorporateActionApplication> result =
            Applications
                .Where(x =>
                    x.PortfolioId == portfolioId &&
                    x.InstrumentId == instrumentId)
                .OrderBy(x => x.AppliedAt)
                .ThenBy(x => x.Id)
                .ToList();

        return Task.FromResult(result);
    }
}