using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class MarketPrice
{
    public Guid Id { get; private set; }

    public Guid InstrumentId { get; private set; }

    public decimal Price { get; private set; }

    public DateTimeOffset PriceTimestamp { get; private set; }

    public string Source { get; private set; }

    private MarketPrice()
    {
        Source = null!;
    }

    public MarketPrice(
        Guid id,
        Guid instrumentId,
        decimal price,
        DateTimeOffset priceTimestamp,
        string source)
    {
        if (id == Guid.Empty)
            throw new DomainException(
                "Market price id is required.");

        if (instrumentId == Guid.Empty)
            throw new DomainException(
                "Instrument id is required.");

        if (price <= 0)
            throw new DomainException(
                "Market price must be greater than zero.");

        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException(
                "Market price source is required.");

        Id = id;
        InstrumentId = instrumentId;
        Price = price;
        PriceTimestamp = priceTimestamp;
        Source = source.Trim();
    }
}