using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeMarketPriceProvider : IMarketPriceProvider
{
    public List<MarketPriceQuote> Quotes { get; } = new();
    public CancellationToken LastCancellationToken { get; private set; }

    public Task<IReadOnlyCollection<MarketPriceQuote>> GetLatestPricesAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        
        var result = Quotes
            .Where(x => symbols.Contains(x.Symbol))
            .ToList();

        return Task.FromResult<IReadOnlyCollection<MarketPriceQuote>>(result);
    }
}