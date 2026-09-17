using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Services;
using TradeLens.Domain.Exceptions;
using TradeLens.Application.Tests.Fakes;

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
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();
        var unitOfWork = new FakeUnitOfWork();
        var positionCalculator = new PositionCalculator();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            positionCalculator,
            transactionRequestHasher);

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
            createdBy,
            "test-idempotency-key-001");

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
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();
        var unitOfWork = new FakeUnitOfWork();
        var positionCalculator = new PositionCalculator();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            positionCalculator,
            transactionRequestHasher);

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
            createdBy,
            "test-idempotency-key-001");

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
            createdBy,
            "test-idempotency-key-002");

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

    [Fact]
    public async Task ExecuteAsync_BuyThenSell_RecalculatesPositionCorrectly()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            new PositionCalculator(),
            transactionRequestHasher);

        var buyCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            1_000,
            8_000m,
            0m,
            new DateOnly(2026, 9, 15),
            1,
            userId,
            "test-idempotency-key-002");

        var sellCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            400,
            10_000m,
            10_000m,
            new DateOnly(2026, 9, 15),
            2,
            userId,
            "test-idempotency-key-004");

        // Act
        await service.ExecuteAsync(buyCommand);
        var sellResult = await service.ExecuteAsync(sellCommand);

        // Assert
        sellResult.PositionQuantity.Should().Be(600);
        sellResult.PositionCostBasis.Should().Be(4_800_000m);
        sellResult.PositionAveragePrice.Should().Be(8_000m);

        transactionRepository.Transactions.Should().HaveCount(2);
        positionRepository.Positions.Should().HaveCount(1);
        unitOfWork.SaveChangesCallCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSellExceedsPosition_ThrowsDomainException()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            new PositionCalculator(),
            transactionRequestHasher);

        var buyCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            1_000,
            8_000m,
            0m,
            new DateOnly(2026, 9, 15),
            1,
            userId,
            "test-idempotency-key-003");

        var sellCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            1_001,
            10_000m,
            10_000m,
            new DateOnly(2026, 9, 15),
            2,
            userId,
            "test-idempotency-key-005");

        // Act
        await service.ExecuteAsync(buyCommand);

        var act = async () => await service.ExecuteAsync(sellCommand);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Sell quantity cannot exceed current position.");

        transactionRepository.Transactions.Should().HaveCount(1);
        positionRepository.Positions.Should().HaveCount(1);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSameIdempotencyKeyAndSamePayload_ShouldReturnOriginalResult()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();
        var unitOfWork = new FakeUnitOfWork();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            new PositionCalculator(),
            transactionRequestHasher);

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
            userId,
            "same-key");

        // Act
        var firstResult = await service.ExecuteAsync(command);
        var secondResult = await service.ExecuteAsync(command);

        // Assert
        secondResult.TransactionId
            .Should()
            .Be(firstResult.TransactionId);

        secondResult.PositionId
            .Should()
            .Be(firstResult.PositionId);

        secondResult.PositionQuantity
            .Should()
            .Be(firstResult.PositionQuantity);

        secondResult.PositionCostBasis
            .Should()
            .Be(firstResult.PositionCostBasis);

        secondResult.PositionAveragePrice
            .Should()
            .Be(firstResult.PositionAveragePrice);

        transactionRepository.Transactions
            .Should()
            .ContainSingle();

        idempotencyRepository.Records
            .Should()
            .ContainSingle();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSameIdempotencyKeyWithDifferentPayload_ShouldThrowIdempotencyConflictException()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactionRepository = new FakeTransactionRepository();
        var positionRepository = new FakePositionRepository();
        var idempotencyRepository = new FakeIdempotencyRepository();
        var transactionRequestHasher = new FakeTransactionRequestHasher();
        var unitOfWork = new FakeUnitOfWork();

        var service = new AddTransactionService(
            transactionRepository,
            positionRepository,
            idempotencyRepository,
            unitOfWork,
            new PositionCalculator(),
            transactionRequestHasher);

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
            userId,
            "same-key");

        var secondCommand = new AddTransactionCommand(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            200,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            userId,
            "same-key");

        // Act
        await service.ExecuteAsync(firstCommand);

        var act = async () =>
            await service.ExecuteAsync(secondCommand);

        // Assert
        await act.Should()
            .ThrowAsync<IdempotencyConflictException>();

        transactionRepository.Transactions
            .Should()
            .ContainSingle();

        idempotencyRepository.Records
            .Should()
            .ContainSingle();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }
}