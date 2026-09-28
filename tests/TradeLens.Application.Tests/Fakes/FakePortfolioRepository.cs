using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakePortfolioRepository : IPortfolioRepository
{
    public List<Portfolio> Portfolios { get; } = new();

    public FakePortfolioRepository()
    {
    }

    public FakePortfolioRepository(Portfolio portfolio)
    {
        Portfolios.Add(portfolio);
    }

    public Task<Portfolio?> GetByIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = Portfolios.FirstOrDefault(
            x => x.Id == portfolioId);

        return Task.FromResult(portfolio);
    }

    public Task<IReadOnlyList<Portfolio>> GetPagedByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var portfolios = Portfolios
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<Portfolio>>(portfolios);
    }

    public Task<int> CountByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var count = Portfolios.Count(
            x => x.UserId == userId);

        return Task.FromResult(count);
    }
}