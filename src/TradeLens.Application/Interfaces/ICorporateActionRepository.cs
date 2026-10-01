using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface ICorporateActionRepository
{
    Task AddAsync(
        CorporateAction corporateAction,
        CancellationToken cancellationToken = default);

    Task<CorporateAction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CorporateAction>> GetByInstrumentAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default);
}