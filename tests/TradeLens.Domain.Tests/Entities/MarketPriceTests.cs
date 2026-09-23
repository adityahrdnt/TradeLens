using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public sealed class MarketPriceTests
{
    [Fact]
    public void Constructor_WhenPriceIsValid_ShouldCreateMarketPrice()
    {
        var id = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var result = new MarketPrice(
            id,
            instrumentId,
            12_000m,
            timestamp,
            "TEST");

        result.Id.Should().Be(id);
        result.InstrumentId.Should().Be(instrumentId);
        result.Price.Should().Be(12_000m);
        result.PriceTimestamp.Should().Be(timestamp);
        result.Source.Should().Be("TEST");
    }

    [Fact]
    public void Constructor_WhenPriceIsZero_ShouldThrowDomainException()
    {
        var action = () => new MarketPrice(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0m,
            DateTimeOffset.UtcNow,
            "TEST");

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Market price must be greater than zero.");
    }

    [Fact]
    public void Constructor_WhenPriceIsNegative_ShouldThrowDomainException()
    {
        var action = () => new MarketPrice(
            Guid.NewGuid(),
            Guid.NewGuid(),
            -100m,
            DateTimeOffset.UtcNow,
            "TEST");

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Market price must be greater than zero.");
    }
}