using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public class CorporateActionRepository : ICorporateActionRepository
{
    private readonly TradeLensDbContext _dbContext;

    public CorporateActionRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        CorporateAction corporateAction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CorporateActions.AddAsync(
            corporateAction,
            cancellationToken);
    }

    public async Task<CorporateAction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CorporateActions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CorporateAction>> GetByInstrumentAsync(
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CorporateActions
            .Where(x => x.InstrumentId == instrumentId)
            .OrderBy(x => x.EffectiveDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}