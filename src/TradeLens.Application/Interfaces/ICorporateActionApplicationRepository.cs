using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface ICorporateActionApplicationRepository
{
    Task AddAsync(
        CorporateActionApplication application,
        CancellationToken cancellationToken = default);

    Task<CorporateActionApplication?> GetByCorporateActionAndPortfolioAsync(
        Guid corporateActionId,
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CorporateActionApplication>> GetByPortfolioAndInstrumentAsync(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken = default);
}