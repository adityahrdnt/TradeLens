using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public sealed class InstrumentRepository : IInstrumentRepository
{
    private readonly TradeLensDbContext _dbContext;

    public InstrumentRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Instrument?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Instruments
            .FirstOrDefaultAsync(
                x => x.Symbol == symbol,
                cancellationToken);
    }
}