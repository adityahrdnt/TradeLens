using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);
}