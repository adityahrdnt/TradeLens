using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Transaction.Queries.GetTransaction;

public class GetTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTransactionBelongsToCurrentUser_ShouldReturnTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var transaction = new DomainTransaction(
            transactionId,
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "My Portfolio");

        var transactionRepository = new FakeTransactionRepository();
        var portfolioRepository = new FakePortfolioRepository();
        var currentUserService = new FakeCurrentUserService(userId);

        transactionRepository.Transactions.Add(transaction);
        portfolioRepository.Portfolios.Add(portfolio);

        var service = new GetTransactionService(
            transactionRepository,
            portfolioRepository,
            currentUserService);

        // Act
        var result = await service.ExecuteAsync(transactionId);

        // Assert
        result.Id.Should().Be(transactionId);
        result.PortfolioId.Should().Be(portfolioId);
        result.BrokerAccountId.Should().Be(brokerAccountId);
        result.InstrumentId.Should().Be(instrumentId);
        result.Type.Should().Be("Buy");
        result.Quantity.Should().Be(100);
        result.Price.Should().Be(10_000m);
        result.Fee.Should().Be(1_000m);
        result.TransactionDate.Should().Be(new DateOnly(2026, 9, 15));
        result.Sequence.Should().Be(1);
        result.Status.Should().Be("Active");
        result.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var service = new GetTransactionService(
            new FakeTransactionRepository(),
            new FakePortfolioRepository(),
            new FakeCurrentUserService(userId));

        var transactionId = Guid.NewGuid();

        // Act
        var act = async () =>
            await service.ExecuteAsync(transactionId);

        // Assert
        await act.Should()
            .ThrowAsync<TransactionNotFoundException>()
            .WithMessage($"Transaction '{transactionId}' was not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotExist_ShouldThrowPortfolioNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var transaction = CreateTransaction(
            transactionId,
            portfolioId,
            userId);

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(transaction);

        var service = new GetTransactionService(
            transactionRepository,
            new FakePortfolioRepository(),
            new FakeCurrentUserService(userId));

        // Act
        var act = async () =>
            await service.ExecuteAsync(transactionId);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioNotFoundException>()
            .WithMessage($"Portfolio '{portfolioId}' was not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToAnotherUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var portfolioOwnerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var transaction = CreateTransaction(
            transactionId,
            portfolioId,
            portfolioOwnerId);

        var portfolio = new Portfolio(
            portfolioId,
            portfolioOwnerId,
            "Another User Portfolio");

        var transactionRepository = new FakeTransactionRepository();
        var portfolioRepository = new FakePortfolioRepository();

        transactionRepository.Transactions.Add(transaction);
        portfolioRepository.Portfolios.Add(portfolio);

        var service = new GetTransactionService(
            transactionRepository,
            portfolioRepository,
            new FakeCurrentUserService(currentUserId));

        // Act
        var act = async () =>
            await service.ExecuteAsync(transactionId);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>()
            .WithMessage("You do not have access to this portfolio.");
    }

    private static DomainTransaction CreateTransaction(
        Guid transactionId,
        Guid portfolioId,
        Guid createdBy)
    {
        return new DomainTransaction(
            transactionId,
            portfolioId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 15),
            1,
            createdBy,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeTransactionRepository
        : ITransactionRepository
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

        public Task<IReadOnlyList<DomainTransaction>>
            GetEffectiveTransactionsAsync(
                Guid portfolioId,
                Guid instrumentId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<DomainTransaction> result = Transactions
                .Where(x =>
                    x.PortfolioId == portfolioId &&
                    x.InstrumentId == instrumentId)
                .ToList();

            return Task.FromResult(result);
        }
    }

    private sealed class FakePortfolioRepository
        : IPortfolioRepository
    {
        public List<Portfolio> Portfolios { get; } = [];

        public Task<Portfolio?> GetByIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            var portfolio = Portfolios
                .FirstOrDefault(x => x.Id == portfolioId);

            return Task.FromResult(portfolio);
        }
    }

    private sealed class FakeCurrentUserService
        : ICurrentUserService
    {
        public FakeCurrentUserService(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}