using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public sealed class MarketPriceRepository : IMarketPriceRepository
{
    private readonly TradeLensDbContext _dbContext;

    public MarketPriceRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MarketPrice?> GetLatestAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MarketPrices
            .Where(x => x.InstrumentId == instrumentId)
            .OrderByDescending(x => x.PriceTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}