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
}