using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.MarketPrices;

public sealed class MarketPriceSyncService
{
    private readonly IMarketPriceProvider _marketPriceProvider;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IMarketPriceRepository _marketPriceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarketPriceSyncService(
        IMarketPriceProvider marketPriceProvider,
        IInstrumentRepository instrumentRepository,
        IMarketPriceRepository marketPriceRepository,
        IUnitOfWork unitOfWork)
    {
        _marketPriceProvider = marketPriceProvider;
        _instrumentRepository = instrumentRepository;
        _marketPriceRepository = marketPriceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SyncAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default)
    {
        if (symbols.Count == 0)
            return;

        var quotes =
            await _marketPriceProvider.GetLatestPricesAsync(
                symbols,
                cancellationToken);

        var addedCount = 0;

        foreach (var quote in quotes)
        {
            var instrument =
                await _instrumentRepository.GetBySymbolAsync(
                    quote.Symbol,
                    cancellationToken);

            if (instrument is null)
                continue;

            var marketPrice = new MarketPrice(
                Guid.NewGuid(),
                instrument.Id,
                quote.Price,
                quote.PriceTimestamp,
                quote.Source);

            await _marketPriceRepository.AddAsync(
                marketPrice,
                cancellationToken);

            addedCount++;
        }

        if (addedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}