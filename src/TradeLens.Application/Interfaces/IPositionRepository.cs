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

    Task AddAsync(
        Position position,
        CancellationToken cancellationToken = default);

    void Update(Position position);
}