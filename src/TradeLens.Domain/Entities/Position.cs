using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid InstrumentId { get; private set; }

    public long Quantity { get; private set; }

    public decimal CostBasis { get; private set; }

    public decimal AveragePrice { get; private set; }

    public long Version { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Position()
    {
    }

    public Position(
        Guid id,
        Guid portfolioId,
        Guid instrumentId,
        long quantity,
        decimal costBasis,
        decimal averagePrice,
        long version,
        DateTimeOffset updatedAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("Position id is required.");

        if (portfolioId == Guid.Empty)
            throw new DomainException("Portfolio id is required.");

        if (instrumentId == Guid.Empty)
            throw new DomainException("Instrument id is required.");

        if (quantity < 0)
            throw new DomainException("Position quantity cannot be negative.");

        if (costBasis < 0)
            throw new DomainException("Position cost basis cannot be negative.");

        if (averagePrice < 0)
            throw new DomainException("Position average price cannot be negative.");

        if (version < 0)
            throw new DomainException("Position version cannot be negative.");

        Id = id;
        PortfolioId = portfolioId;
        InstrumentId = instrumentId;

        Quantity = quantity;
        CostBasis = costBasis;
        AveragePrice = averagePrice;

        Version = version;
        UpdatedAt = updatedAt;
    }

    public static Position Empty(
        Guid portfolioId,
        Guid instrumentId,
        DateTimeOffset now)
    {
        return new Position(
            Guid.NewGuid(),
            portfolioId,
            instrumentId,
            0,
            0m,
            0m,
            0,
            now);
    }

    public void Apply(
        long quantity,
        decimal costBasis,
        decimal averagePrice,
        DateTimeOffset updatedAt)
    {
        if (quantity < 0)
            throw new DomainException("Position quantity cannot be negative.");

        if (costBasis < 0)
            throw new DomainException("Position cost basis cannot be negative.");

        if (averagePrice < 0)
            throw new DomainException("Position average price cannot be negative.");

        Quantity = quantity;
        CostBasis = costBasis;
        AveragePrice = averagePrice;

        Version++;
        UpdatedAt = updatedAt;
    }
}