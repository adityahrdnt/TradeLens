using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class Instrument
{
    public Guid Id { get; private set; }

    public string Symbol { get; private set; }

    public string Name { get; private set; }

    public string Currency { get; private set; }

    private Instrument()
    {
        Symbol = null!;
        Name = null!;
        Currency = null!;
    }

    public Instrument(
        Guid id,
        string symbol,
        string name,
        string currency)
    {
        if (id == Guid.Empty)
            throw new DomainException("Instrument id is required.");

        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Instrument symbol is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Instrument name is required.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Instrument currency is required.");

        Id = id;
        Symbol = symbol.Trim().ToUpperInvariant();
        Name = name.Trim();
        Currency = currency.Trim().ToUpperInvariant();
    }
}