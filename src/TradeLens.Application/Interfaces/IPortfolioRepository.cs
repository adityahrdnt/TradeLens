using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Portfolio>> GetPagedByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}