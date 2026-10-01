using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public class CorporateActionApplicationRepository
    : ICorporateActionApplicationRepository
{
    private readonly TradeLensDbContext _dbContext;

    public CorporateActionApplicationRepository(
        TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        CorporateActionApplication application,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CorporateActionApplications.AddAsync(
            application,
            cancellationToken);
    }

    public async Task<CorporateActionApplication?>
        GetByCorporateActionAndPortfolioAsync(
            Guid corporateActionId,
            Guid portfolioId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.CorporateActionApplications
            .FirstOrDefaultAsync(
                x =>
                    x.CorporateActionId == corporateActionId &&
                    x.PortfolioId == portfolioId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CorporateActionApplication>>
        GetByPortfolioAndInstrumentAsync(
            Guid portfolioId,
            Guid instrumentId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.CorporateActionApplications
            .Where(x =>
                x.PortfolioId == portfolioId &&
                x.InstrumentId == instrumentId)
            .OrderBy(x => x.AppliedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}