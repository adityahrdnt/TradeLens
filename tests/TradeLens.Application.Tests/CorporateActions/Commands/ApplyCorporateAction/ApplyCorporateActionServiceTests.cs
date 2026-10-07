using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;
using TradeLens.Domain.Services;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.CorporateActions.Commands.ApplyCorporateAction;

public class ApplyCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldApplyCorporateAction_AndUpdatePosition()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        var transaction = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var applicationRepository =
            new FakeCorporateActionApplicationRepository();

        var transactionRepository =
            new FakeTransactionRepository();

        transactionRepository.Transactions.Add(transaction);

        var positionRepository =
            new FakePositionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = CreateService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork);

        var command = new ApplyCorporateActionCommand(
            corporateAction.Id,
            userId);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.CorporateActionId
            .Should()
            .Be(corporateAction.Id);

        result.AppliedPortfolioCount
            .Should()
            .Be(1);

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Applied);

        corporateAction.AppliedBy
            .Should()
            .Be(userId);

        corporateAction.AppliedAt
            .Should()
            .NotBeNull();

        applicationRepository.Applications
            .Should()
            .ContainSingle();

        var application =
            applicationRepository.Applications.Single();

        application.CorporateActionId
            .Should()
            .Be(corporateAction.Id);

        application.PortfolioId
            .Should()
            .Be(portfolioId);

        application.InstrumentId
            .Should()
            .Be(instrumentId);

        application.EligibleQuantity
            .Should()
            .Be(100);

        application.ResultingQuantity
            .Should()
            .Be(200);

        var position =
            positionRepository.Positions.Single();

        position.Quantity
            .Should()
            .Be(200);

        position.CostBasis
            .Should()
            .Be(1_000_000m);

        position.AveragePrice
            .Should()
            .Be(5_000m);

        unitOfWork.TransactionCallCount
            .Should()
            .Be(1);

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorporateActionDoesNotExist_ShouldThrowCorporateActionNotFoundException()
    {
        // Arrange
        var corporateActionRepository =
            new FakeCorporateActionRepository();

        var applicationRepository =
            new FakeCorporateActionApplicationRepository();

        var transactionRepository =
            new FakeTransactionRepository();

        var positionRepository =
            new FakePositionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = CreateService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork);

        var command = new ApplyCorporateActionCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<CorporateActionNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyCorporateActionToMultipleEligiblePortfolios()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var firstPortfolioId = Guid.NewGuid();
        var secondPortfolioId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        var firstTransaction = new DomainTransaction(
            Guid.NewGuid(),
            firstPortfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var secondTransaction = new DomainTransaction(
            Guid.NewGuid(),
            secondPortfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            50,
            12_000m,
            0m,
            new DateOnly(2026, 9, 6),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var applicationRepository =
            new FakeCorporateActionApplicationRepository();

        var transactionRepository =
            new FakeTransactionRepository();

        transactionRepository.Transactions.Add(firstTransaction);
        transactionRepository.Transactions.Add(secondTransaction);

        var positionRepository =
            new FakePositionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = CreateService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork);

        var command = new ApplyCorporateActionCommand(
            corporateAction.Id,
            userId);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.AppliedPortfolioCount
            .Should()
            .Be(2);

        applicationRepository.Applications
            .Should()
            .HaveCount(2);

        applicationRepository.Applications
            .Should()
            .ContainSingle(x =>
                x.PortfolioId == firstPortfolioId &&
                x.EligibleQuantity == 100 &&
                x.ResultingQuantity == 200);

        applicationRepository.Applications
            .Should()
            .ContainSingle(x =>
                x.PortfolioId == secondPortfolioId &&
                x.EligibleQuantity == 50 &&
                x.ResultingQuantity == 100);

        positionRepository.Positions
            .Should()
            .HaveCount(2);

        positionRepository.Positions
            .Should()
            .ContainSingle(x =>
                x.PortfolioId == firstPortfolioId &&
                x.Quantity == 200 &&
                x.CostBasis == 1_000_000m &&
                x.AveragePrice == 5_000m);

        positionRepository.Positions
            .Should()
            .ContainSingle(x =>
                x.PortfolioId == secondPortfolioId &&
                x.Quantity == 100 &&
                x.CostBasis == 600_000m &&
                x.AveragePrice == 6_000m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipPortfolioWithoutHoldingAtRecordDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var eligiblePortfolioId = Guid.NewGuid();
        var soldOutPortfolioId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        var eligibleBuy = new DomainTransaction(
            Guid.NewGuid(),
            eligiblePortfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var soldOutBuy = new DomainTransaction(
            Guid.NewGuid(),
            soldOutPortfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var soldOutSell = new DomainTransaction(
            Guid.NewGuid(),
            soldOutPortfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Sell,
            100,
            11_000m,
            0m,
            new DateOnly(2026, 9, 8),
            2,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var applicationRepository =
            new FakeCorporateActionApplicationRepository();

        var transactionRepository =
            new FakeTransactionRepository();

        transactionRepository.Transactions.Add(eligibleBuy);
        transactionRepository.Transactions.Add(soldOutBuy);
        transactionRepository.Transactions.Add(soldOutSell);

        var positionRepository =
            new FakePositionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = CreateService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork);

        var command = new ApplyCorporateActionCommand(
            corporateAction.Id,
            userId);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.AppliedPortfolioCount
            .Should()
            .Be(1);

        applicationRepository.Applications
            .Should()
            .ContainSingle();

        var application =
            applicationRepository.Applications.Single();

        application.PortfolioId
            .Should()
            .Be(eligiblePortfolioId);

        application.EligibleQuantity
            .Should()
            .Be(100);

        application.ResultingQuantity
            .Should()
            .Be(200);

        positionRepository.Positions
            .Should()
            .ContainSingle();

        positionRepository.Positions.Single().PortfolioId
            .Should()
            .Be(eligiblePortfolioId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecalculatePositionWithTransactionsAfterEffectiveDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        var initialBuy = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var postEffectiveBuy = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            50,
            6_000m,
            0m,
            new DateOnly(2026, 9, 15),
            2,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var applicationRepository =
            new FakeCorporateActionApplicationRepository();

        var transactionRepository =
            new FakeTransactionRepository();

        transactionRepository.Transactions.Add(initialBuy);
        transactionRepository.Transactions.Add(postEffectiveBuy);

        var positionRepository =
            new FakePositionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = CreateService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork);

        var command = new ApplyCorporateActionCommand(
            corporateAction.Id,
            userId);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.AppliedPortfolioCount
            .Should()
            .Be(1);

        var application =
            applicationRepository.Applications.Single();

        application.EligibleQuantity
            .Should()
            .Be(100);

        application.ResultingQuantity
            .Should()
            .Be(200);

        var position =
            positionRepository.Positions.Single();

        position.Quantity
            .Should()
            .Be(250);

        position.CostBasis
            .Should()
            .Be(1_300_000m);

        position.AveragePrice
            .Should()
            .Be(1_300_000m / 250m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorporateActionIsAlreadyApplied_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            Guid.NewGuid(),
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Apply(
            DateTimeOffset.UtcNow,
            userId);

        var transaction = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            Guid.NewGuid(),
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 5),
            1,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var service = CreateService(
            corporateActionRepository,
            new FakeCorporateActionApplicationRepository(),
            new FakeTransactionRepository
            {
                Transactions = { transaction }
            },
            new FakePositionRepository(),
            new FakeUnitOfWork());

        var command = new ApplyCorporateActionCommand(
            corporateAction.Id,
            userId);

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorporateActionIsCancelled_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var corporateAction = new CorporateAction(
            corporateActionId,
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            userId,
            DateTimeOffset.UtcNow);

        corporateAction.Cancel(
            DateTimeOffset.UtcNow,
            userId,
            "Corporate action cancelled.");

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var service = CreateService(
            corporateActionRepository,
            new FakeCorporateActionApplicationRepository(),
            new FakeTransactionRepository(),
            new FakePositionRepository(),
            new FakeUnitOfWork());

        var command = new ApplyCorporateActionCommand(
            corporateActionId,
            userId);

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<DomainException>();
    }

    private static ApplyCorporateActionService CreateService(
        FakeCorporateActionRepository corporateActionRepository,
        FakeCorporateActionApplicationRepository applicationRepository,
        FakeTransactionRepository transactionRepository,
        FakePositionRepository positionRepository,
        FakeUnitOfWork unitOfWork)
    {
        return new ApplyCorporateActionService(
            corporateActionRepository,
            applicationRepository,
            transactionRepository,
            positionRepository,
            unitOfWork,
            new CorporateActionPositionCalculator(
                new PositionCalculator(),
                new CorporateActionCalculator()),
            new CorporateActionCalculator(),
            new PositionCalculator(),
            NullLogger<ApplyCorporateActionService>.Instance);
    }
}
