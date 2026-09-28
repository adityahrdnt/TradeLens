using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface IPositionRepository
{
    Task<Position?> GetByPortfolioAndInstrumentAsync(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Position>> GetByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Position>> GetPagedByPortfolioAsync(
        Guid portfolioId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountByPortfolioAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Position position,
        CancellationToken cancellationToken = default);

    void Update(Position position);
}