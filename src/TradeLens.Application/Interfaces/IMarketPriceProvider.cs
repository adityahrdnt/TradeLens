using TradeLens.Application.MarketPrices;

namespace TradeLens.Application.Interfaces;

public interface IMarketPriceProvider
{
    Task<IReadOnlyCollection<MarketPriceQuote>> GetLatestPricesAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default);
}