using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IInstrumentRepository
{
    Task<Instrument?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}