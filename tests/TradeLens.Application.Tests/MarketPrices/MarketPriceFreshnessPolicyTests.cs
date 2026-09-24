using FluentAssertions;
using TradeLens.Application.MarketPrices;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.MarketPrices;

public class MarketPriceFreshnessPolicyTests
{
    private static readonly TimeSpan MaxAge =
        TimeSpan.FromMinutes(30);

    private static readonly DateTimeOffset Now =
        new(2026, 9, 24, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsFresh_ShouldReturnTrue_WhenPriceIsWithinMaxAge()
    {
        var policy =
            new MarketPriceFreshnessPolicy(MaxAge);

        var marketPrice =
            CreateMarketPrice(
                Now.AddMinutes(-29));

        var result =
            policy.IsFresh(
                marketPrice,
                Now);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsFresh_ShouldReturnTrue_WhenPriceAgeEqualsMaxAge()
    {
        var policy =
            new MarketPriceFreshnessPolicy(MaxAge);

        var marketPrice =
            CreateMarketPrice(
                Now.AddMinutes(-30));

        var result =
            policy.IsFresh(
                marketPrice,
                Now);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsFresh_ShouldReturnFalse_WhenPriceIsOlderThanMaxAge()
    {
        var policy =
            new MarketPriceFreshnessPolicy(MaxAge);

        var marketPrice =
            CreateMarketPrice(
                Now.AddMinutes(-31));

        var result =
            policy.IsFresh(
                marketPrice,
                Now);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsFresh_ShouldReturnFalse_WhenPriceTimestampIsInTheFuture()
    {
        var policy =
            new MarketPriceFreshnessPolicy(MaxAge);

        var marketPrice =
            CreateMarketPrice(
                Now.AddMinutes(1));

        var result =
            policy.IsFresh(
                marketPrice,
                Now);

        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMaxAgeIsZero()
    {
        var action = () =>
            new MarketPriceFreshnessPolicy(
                TimeSpan.Zero);

        action.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMaxAgeIsNegative()
    {
        var action = () =>
            new MarketPriceFreshnessPolicy(
                TimeSpan.FromMinutes(-1));

        action.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    private static MarketPrice CreateMarketPrice(
        DateTimeOffset timestamp)
    {
        return new MarketPrice(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m,
            timestamp,
            "Test");
    }
}