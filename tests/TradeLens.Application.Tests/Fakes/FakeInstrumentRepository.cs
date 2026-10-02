using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeInstrumentRepository : IInstrumentRepository
{
    public List<Instrument> Instruments { get; } = new();

    public Task<Instrument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = Instruments
            .FirstOrDefault(x => x.Id == id);

        return Task.FromResult(result);
    }

    public Task<Instrument?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var result = Instruments
            .FirstOrDefault(x => x.Symbol == symbol);

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<Instrument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Instrument> result = Instruments;

        return Task.FromResult(result);
    }
}