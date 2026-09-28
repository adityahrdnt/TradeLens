using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Repositories;

public sealed class PortfolioRepository : IPortfolioRepository
{
    private readonly TradeLensDbContext _dbContext;

    public PortfolioRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Portfolio?> GetByIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .FirstOrDefaultAsync(
                x => x.Id == portfolioId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Portfolio>> GetPagedByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Portfolios
            .CountAsync(
                x => x.UserId == userId,
                cancellationToken);
    }
}