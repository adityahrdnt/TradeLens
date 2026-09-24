using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeMarketPriceRepository : IMarketPriceRepository
{
    public List<MarketPrice> MarketPrices { get; } = new();
    public CancellationToken LastCancellationToken { get; private set; }

    public Task<MarketPrice?> GetLatestAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        var result = MarketPrices
            .Where(x => x.InstrumentId == instrumentId)
            .OrderByDescending(x => x.PriceTimestamp)
            .FirstOrDefault();

        return Task.FromResult(result);
    }

    public Task AddAsync(
        MarketPrice marketPrice,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;

        MarketPrices.Add(marketPrice);

        return Task.CompletedTask;
    }
}