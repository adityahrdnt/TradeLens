using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IInstrumentRepository
{
    Task<Instrument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Instrument?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Instrument>> GetAllAsync(
        CancellationToken cancellationToken = default);
}