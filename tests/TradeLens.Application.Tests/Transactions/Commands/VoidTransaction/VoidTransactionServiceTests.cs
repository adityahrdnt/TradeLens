using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Transactions.Commands.VoidTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Transactions.Commands.VoidTransaction;

public class VoidTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenVoidingBuy_ShouldRecalculatePosition()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var buy1 = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var buy2 = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            50,
            12_000m,
            50_000m,
            new DateOnly(2026, 9, 16),
            2,
            userId,
            DateTimeOffset.UtcNow);

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(buy1);
        transactionRepository.Transactions.Add(buy2);

        var positionRepository = new FakePositionRepository();

        var position = Position.Empty(
            portfolioId,
            instrumentId,
            DateTimeOffset.UtcNow);

        position.Apply(
            150,
            1_750_000m,
            1_750_000m / 150m,
            DateTimeOffset.UtcNow);

        positionRepository.Positions.Add(position);

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(
            transactionRepository,
            positionRepository,
            portfolio,
            unitOfWork);

        var command = new VoidTransactionCommand(
            buy2.Id,
            userId,
            "Duplicate transaction");

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        buy2.Status.Should().Be(TransactionStatus.Voided);
        buy2.VoidReason.Should().Be("Duplicate transaction");

        result.TransactionId.Should().Be(buy2.Id);
        result.PositionId.Should().Be(position.Id);
        result.PositionQuantity.Should().Be(100);
        result.PositionCostBasis.Should().Be(1_100_000m);
        result.PositionAveragePrice.Should().Be(11_000m);

        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVoidingSell_ShouldRestorePosition()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var buy = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var sell = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            40,
            12_000m,
            40_000m,
            new DateOnly(2026, 9, 17),
            2,
            userId,
            DateTimeOffset.UtcNow);

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(buy);
        transactionRepository.Transactions.Add(sell);

        var positionRepository = new FakePositionRepository();

        var position = Position.Empty(
            portfolioId,
            instrumentId,
            DateTimeOffset.UtcNow);

        position.Apply(
            60,
            660_000m,
            11_000m,
            DateTimeOffset.UtcNow);

        positionRepository.Positions.Add(position);

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(
            transactionRepository,
            positionRepository,
            portfolio,
            unitOfWork);

        var command = new VoidTransactionCommand(
            sell.Id,
            userId,
            "Sell entered by mistake");

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        sell.Status.Should().Be(TransactionStatus.Voided);
        sell.VoidReason.Should().Be("Sell entered by mistake");

        result.TransactionId.Should().Be(sell.Id);
        result.PositionId.Should().Be(position.Id);
        result.PositionQuantity.Should().Be(100);
        result.PositionCostBasis.Should().Be(1_100_000m);
        result.PositionAveragePrice.Should().Be(11_000m);

        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundException()
    {
        // Arrange
        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var portfolioRepository = new FakePortfolioRepository();
        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);
        var unitOfWork = new FakeUnitOfWork();

        var service = new VoidTransactionService(
            transactionRepository,
            positionRepository,
            portfolioAccessService,
            unitOfWork,
            new PositionCalculator(),
            NullLogger<VoidTransactionService>.Instance);

        var command = new VoidTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Invalid transaction");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should()
            .ThrowAsync<TransactionNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioDoesNotBelongToUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            ownerId,
            "Owner Portfolio");

        var transaction = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            1,
            ownerId,
            DateTimeOffset.UtcNow);

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(transaction);

        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(
            transactionRepository,
            positionRepository,
            portfolio,
            unitOfWork);

        var command = new VoidTransactionCommand(
            transaction.Id,
            anotherUserId,
            "Unauthorized void");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionIsAlreadyVoided_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var transaction = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            100,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            1,
            userId,
            DateTimeOffset.UtcNow);

        transaction.Void("Already voided");

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(transaction);

        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(
            transactionRepository,
            positionRepository,
            portfolio,
            unitOfWork);

        var command = new VoidTransactionCommand(
            transaction.Id,
            userId,
            "Second void");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>();
    }

    private static VoidTransactionService CreateService(
        FakeTransactionRepository transactionRepository,
        FakePositionRepository positionRepository,
        Portfolio portfolio,
        FakeUnitOfWork unitOfWork)
    {
        var portfolioRepository =
            new FakePortfolioRepository(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        return new VoidTransactionService(
            transactionRepository,
            positionRepository,
            portfolioAccessService,
            unitOfWork,
            new PositionCalculator(),
            NullLogger<VoidTransactionService>.Instance);
    }
}
