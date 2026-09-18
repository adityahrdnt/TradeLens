using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IPortfolioAccessService
{
    Task<Portfolio> GetOwnedPortfolioAsync(
        Guid portfolioId,
        Guid userId,
        CancellationToken cancellationToken = default);
}