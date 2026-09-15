using FluentAssertions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Tests.Transaction.Commands.AddTransaction;

public class AddTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAddingFirstBuy_ShouldCreateTransactionAndPosition()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var positionCalculator = new PositionCalculator();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            unitOfWork,
            positionCalculator);

        var command = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            createdBy);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.TransactionId.Should().NotBeEmpty();

        result.PositionId.Should().NotBeEmpty();
        result.PositionQuantity.Should().Be(100);
        result.PositionCostBasis.Should().Be(1_001_000m);
        result.PositionAveragePrice.Should().Be(10_010m);

        transactionRepository.Transactions.Should().ContainSingle();

        var transaction = transactionRepository.Transactions.Single();

        transaction.PortfolioId.Should().Be(portfolioId);
        transaction.BrokerAccountId.Should().Be(brokerAccountId);
        transaction.InstrumentId.Should().Be(instrumentId);
        transaction.Type.Should().Be(TransactionType.Buy);
        transaction.Quantity.Should().Be(100);
        transaction.Price.Should().Be(10_000m);
        transaction.Fee.Should().Be(1_000m);

        positionRepository.Positions.Should().ContainSingle();

        var position = positionRepository.Positions.Single();

        position.PortfolioId.Should().Be(portfolioId);
        position.InstrumentId.Should().Be(instrumentId);
        position.Quantity.Should().Be(100);
        position.CostBasis.Should().Be(1_001_000m);
        position.AveragePrice.Should().Be(10_010m);

        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAddingSecondBuy_ShouldRecalculatePositionFromTransactionHistory()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var positionCalculator = new PositionCalculator();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            unitOfWork,
            positionCalculator);

        var firstCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            createdBy);

        await service.ExecuteAsync(firstCommand);

        var secondCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            200,
            12_000m,
            2_000m,
            new DateOnly(2026, 9, 16),
            2,
            createdBy);

        // Act
        var result = await service.ExecuteAsync(secondCommand);

        // Assert
        result.PositionQuantity.Should().Be(300);

        result.PositionCostBasis.Should().Be(3_403_000m);

        result.PositionAveragePrice.Should()
            .Be(3_403_000m / 300m);

        transactionRepository.Transactions.Should().HaveCount(2);

        positionRepository.Positions.Should().ContainSingle();

        var position = positionRepository.Positions.Single();

        position.Quantity.Should().Be(300);
        position.CostBasis.Should().Be(3_403_000m);
        position.AveragePrice.Should().Be(3_403_000m / 300m);

        unitOfWork.SaveChangesCallCount.Should().Be(2);
    }

    private sealed class FakeTransactionRepository : ITransactionRepository
    {
        public List<DomainTransaction> Transactions { get; } = [];

        public Task AddAsync(
            DomainTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<DomainTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var transaction = Transactions
                .FirstOrDefault(x => x.Id == id);

            return Task.FromResult(transaction);
        }

        public Task<IReadOnlyList<DomainTransaction>> GetEffectiveTransactionsAsync(
            Guid portfolioId,
            Guid instrumentId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<DomainTransaction> result = Transactions
                .Where(x =>
                    x.PortfolioId == portfolioId &&
                    x.InstrumentId == instrumentId &&
                    x.Status == TransactionStatus.Active)
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Sequence)
                .ToList();

            return Task.FromResult(result);
        }
    }

    private sealed class FakePositionRepository : IPositionRepository
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

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}