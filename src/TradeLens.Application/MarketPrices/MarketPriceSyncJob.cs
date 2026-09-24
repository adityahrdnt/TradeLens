using TradeLens.Application.Interfaces;

namespace TradeLens.Application.MarketPrices;

public sealed class MarketPriceSyncJob : IMarketPriceSyncJob
{
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly MarketPriceSyncService _marketPriceSyncService;

    public MarketPriceSyncJob(
        IInstrumentRepository instrumentRepository,
        MarketPriceSyncService marketPriceSyncService)
    {
        _instrumentRepository = instrumentRepository;
        _marketPriceSyncService = marketPriceSyncService;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var instruments =
            await _instrumentRepository.GetAllAsync(
                cancellationToken);

        var symbols =
            instruments
                .Select(x => x.Symbol)
                .ToArray();

        if (symbols.Length == 0)
            return;

        await _marketPriceSyncService.SyncAsync(
            symbols,
            cancellationToken);
    }
}