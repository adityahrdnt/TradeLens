using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;

namespace TradeLens.Integration.Tests.Fakes;

public sealed class FakeMarketPriceProvider
    : IMarketPriceProvider
{
    public Task<IReadOnlyCollection<MarketPriceQuote>>
        GetLatestPricesAsync(
            IReadOnlyCollection<string> symbols,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<
            IReadOnlyCollection<MarketPriceQuote>>(
                Array.Empty<MarketPriceQuote>());
    }
}