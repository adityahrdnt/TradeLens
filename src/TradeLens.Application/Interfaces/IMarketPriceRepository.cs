using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IMarketPriceRepository
{
    Task<MarketPrice?> GetLatestAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        MarketPrice marketPrice,
        CancellationToken cancellationToken = default);
}