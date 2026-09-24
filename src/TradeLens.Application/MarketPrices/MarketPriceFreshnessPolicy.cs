using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.MarketPrices;

public sealed class MarketPriceFreshnessPolicy
    : IMarketPriceFreshnessPolicy
{
    private readonly TimeSpan _maxAge;

    public MarketPriceFreshnessPolicy(TimeSpan maxAge)
    {
        if (maxAge <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(maxAge),
                "Maximum market price age must be greater than zero.");

        _maxAge = maxAge;
    }

    public bool IsFresh(
        MarketPrice marketPrice,
        DateTimeOffset now)
    {
        if (marketPrice.PriceTimestamp > now)
            return false;

        return now - marketPrice.PriceTimestamp <= _maxAge;
    }
}