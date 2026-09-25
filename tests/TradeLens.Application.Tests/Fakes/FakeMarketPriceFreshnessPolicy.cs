using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeMarketPriceFreshnessPolicy
    : IMarketPriceFreshnessPolicy
{
    public bool IsFreshResult { get; set; } = true;

    public Func<MarketPrice, bool>? IsFreshOverride { get; set; }

    public MarketPrice? LastMarketPrice { get; private set; }

    public DateTimeOffset? LastNow { get; private set; }

    public List<DateTimeOffset> ReceivedTimes { get; } = [];

    public bool IsFresh(
        MarketPrice marketPrice,
        DateTimeOffset now)
    {
        LastMarketPrice = marketPrice;
        LastNow = now;

        ReceivedTimes.Add(now);

        if (IsFreshOverride is not null)
        {
            return IsFreshOverride(marketPrice);
        }

        return IsFreshResult;
    }
}