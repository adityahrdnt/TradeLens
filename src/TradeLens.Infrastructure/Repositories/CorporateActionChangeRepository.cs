using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Infrastructure.Repositories;

public class CorporateActionChangeRepository
    : ICorporateActionChangeRepository
{
    private readonly TradeLensDbContext _dbContext;

    public CorporateActionChangeRepository(
        TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        CorporateActionChange change,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CorporateActionChanges.AddAsync(
            change,
            cancellationToken);
    }
}
