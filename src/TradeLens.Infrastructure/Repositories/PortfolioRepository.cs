using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Repositories;

public sealed class PortfolioRepository : IPortfolioRepository
{
    private readonly TradeLensDbContext _dbContext;

    public PortfolioRepository(
        TradeLensDbContext dbContext)
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
}