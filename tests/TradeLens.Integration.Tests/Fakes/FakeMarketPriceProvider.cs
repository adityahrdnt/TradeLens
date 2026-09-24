using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;

namespace TradeLens.Integration.Tests.Fakes;

public sealed class FakeMarketPriceProvider
    : IMarketPriceProvider
{
    public List<MarketPriceQuote> Quotes { get; } = new();

    public IReadOnlyCollection<string> LastRequestedSymbols { get; private set; }
        = Array.Empty<string>();

    public CancellationToken LastCancellationToken { get; private set; }

    public Task<IReadOnlyCollection<MarketPriceQuote>>
        GetLatestPricesAsync(
            IReadOnlyCollection<string> symbols,
            CancellationToken cancellationToken = default)
    {
        LastRequestedSymbols = symbols.ToArray();
        LastCancellationToken = cancellationToken;

        var result = Quotes
            .Where(x => symbols.Contains(x.Symbol))
            .ToList();

        return Task.FromResult<
            IReadOnlyCollection<MarketPriceQuote>>(result);
    }
}