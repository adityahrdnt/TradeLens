using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeMarketPriceFreshnessPolicy
    : IMarketPriceFreshnessPolicy
{
    public bool IsFreshResult { get; set; } = true;

    public MarketPrice? LastMarketPrice { get; private set; }

    public DateTimeOffset? LastNow { get; private set; }

    public bool IsFresh(
        MarketPrice marketPrice,
        DateTimeOffset now)
    {
        LastMarketPrice = marketPrice;
        LastNow = now;

        return IsFreshResult;
    }
}