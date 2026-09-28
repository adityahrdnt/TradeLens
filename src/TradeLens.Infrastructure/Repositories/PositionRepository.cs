using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly TradeLensDbContext _dbContext;

    public PositionRepository(TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Position?> GetByPortfolioAndInstrumentAsync(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .FirstOrDefaultAsync(
                x =>
                    x.PortfolioId == portfolioId &&
                    x.InstrumentId == instrumentId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Position>> GetByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .Where(x => x.PortfolioId == portfolioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Position>> GetPagedByPortfolioAsync(
        Guid portfolioId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .Where(x => x.PortfolioId == portfolioId)
            .OrderBy(x => x.InstrumentId)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .CountAsync(
                x => x.PortfolioId == portfolioId,
                cancellationToken);
    }

    public async Task AddAsync(
        Position position,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Positions.AddAsync(
            position,
            cancellationToken);
    }

    public void Update(Position position)
    {
        _dbContext.Positions.Update(position);
    }
}