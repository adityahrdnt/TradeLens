using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IMarketPriceFreshnessPolicy
{
    bool IsFresh(
        MarketPrice marketPrice,
        DateTimeOffset now);
}