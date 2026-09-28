using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakePositionRepository : IPositionRepository
    {
        public List<Position> Positions { get; } = [];

        public Task<Position?> GetByPortfolioAndInstrumentAsync(
            Guid portfolioId,
            Guid instrumentId,
            CancellationToken cancellationToken = default)
        {
            var position = Positions.FirstOrDefault(x =>
                x.PortfolioId == portfolioId &&
                x.InstrumentId == instrumentId);

            return Task.FromResult(position);
        }

        public Task<IReadOnlyCollection<Position>> GetByPortfolioAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            var positions = Positions
                .Where(x => x.PortfolioId == portfolioId)
                .ToList();

            return Task.FromResult<IReadOnlyCollection<Position>>(
                positions);
        }

        public Task<IReadOnlyList<Position>> GetPagedByPortfolioAsync(
            Guid portfolioId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var positions = Positions
                .Where(x => x.PortfolioId == portfolioId)
                .OrderBy(x => x.InstrumentId)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToList();

            return Task.FromResult<IReadOnlyList<Position>>(
                positions);
        }

        public Task<int> CountByPortfolioAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            var count = Positions.Count(
                x => x.PortfolioId == portfolioId);

            return Task.FromResult(count);
        }

        public Task AddAsync(
            Position position,
            CancellationToken cancellationToken = default)
        {
            Positions.Add(position);
            return Task.CompletedTask;
        }

        public void Update(Position position)
        {
            // Fake repository keeps the same tracked object.
        }
    }  