using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Transactions.Commands.CorrectTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Transactions.Commands.CorrectTransaction;

public class CorrectTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldSupersedeOriginalAndCreateCorrectedTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var originalTransactionId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var original = new DomainTransaction(
            originalTransactionId,
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

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(original);

        var positionRepository = new FakePositionRepository();
        var portfolioRepository = new FakePortfolioRepository(portfolio);
        var unitOfWork = new FakeUnitOfWork();

        var service = new CorrectTransactionService(
            transactionRepository,
            positionRepository,
            portfolioRepository,
            unitOfWork,
            new PositionCalculator());

        var command = new CorrectTransactionCommand(
            originalTransactionId,
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            userId,
            "Incorrect quantity");

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.OriginalTransactionId.Should().Be(originalTransactionId);
        result.CorrectedTransactionId.Should().NotBe(originalTransactionId);

        original.Status.Should().Be(TransactionStatus.Superseded);
        original.CorrectionReason.Should().Be("Incorrect quantity");
        original.SupersededBy.Should().Be(userId);
        original.SupersededAt.Should().NotBeNull();

        var corrected = transactionRepository.Transactions
            .Single(x => x.Id == result.CorrectedTransactionId);

        corrected.Status.Should().Be(TransactionStatus.Active);
        corrected.SupersedesTransactionId.Should().Be(originalTransactionId);
        corrected.Quantity.Should().Be(120);
        corrected.Price.Should().Be(10_000m);
        corrected.Fee.Should().Be(100_000m);

        result.PositionQuantity.Should().Be(120);
        result.PositionCostBasis.Should().Be(1_300_000m);
        result.PositionAveragePrice.Should().Be(1_300_000m / 120m);

        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }    

    [Fact]
    public async Task ExecuteAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundException()
    {
        // Arrange
        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var portfolioRepository = new FakePortfolioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CorrectTransactionService(
            transactionRepository,
            positionRepository,
            portfolioRepository,
            unitOfWork,
            new PositionCalculator());

        var command = new CorrectTransactionCommand(
            Guid.NewGuid(),
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            Guid.NewGuid(),
            "Incorrect quantity");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should().ThrowAsync<TransactionNotFoundException>();
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

        var original = new DomainTransaction(
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
        transactionRepository.Transactions.Add(original);

        var positionRepository = new FakePositionRepository();
        var portfolioRepository = new FakePortfolioRepository(portfolio);
        var unitOfWork = new FakeUnitOfWork();

        var service = new CorrectTransactionService(
            transactionRepository,
            positionRepository,
            portfolioRepository,
            unitOfWork,
            new PositionCalculator());

        var command = new CorrectTransactionCommand(
            original.Id,
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            anotherUserId,
            "Incorrect quantity");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should().ThrowAsync<PortfolioAccessDeniedException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionIsAlreadySuperseded_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var original = new DomainTransaction(
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

        original.Supersede(
            DateTimeOffset.UtcNow,
            userId,
            "Already corrected");

        var transactionRepository = new FakeTransactionRepository();
        transactionRepository.Transactions.Add(original);

        var positionRepository = new FakePositionRepository();
        var portfolioRepository = new FakePortfolioRepository(portfolio);
        var unitOfWork = new FakeUnitOfWork();

        var service = new CorrectTransactionService(
            transactionRepository,
            positionRepository,
            portfolioRepository,
            unitOfWork,
            new PositionCalculator());

        var command = new CorrectTransactionCommand(
            original.Id,
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            userId,
            "Second correction");

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}