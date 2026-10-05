using FluentAssertions;
using TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.Tests.CorporateActions.Commands.CancelCorporateAction;

public class CancelCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCancelCorporateAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

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

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CancelCorporateActionService(
                corporateActionRepository,
                unitOfWork);

        var command =
            new CancelCorporateActionCommand(
                corporateActionId,
                userId,
                "Corporate action cancelled by issuer.");

        // Act
        var result =
            await service.ExecuteAsync(command);

        // Assert
        result.CorporateActionId
            .Should()
            .Be(corporateActionId);

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Cancelled);

        corporateAction.CancelledBy
            .Should()
            .Be(userId);

        corporateAction.CancelledAt
            .Should()
            .NotBeNull();

        corporateAction.CancellationReason
            .Should()
            .Be("Corporate action cancelled by issuer.");

        unitOfWork.TransactionCallCount
            .Should()
            .Be(1);

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowCorporateActionNotFoundException_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        var corporateActionRepository =
            new FakeCorporateActionRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CancelCorporateActionService(
                corporateActionRepository,
                unitOfWork);

        var corporateActionId = Guid.NewGuid();

        var command =
            new CancelCorporateActionCommand(
                corporateActionId,
                Guid.NewGuid(),
                "Corporate action cancelled.");

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Application.Exceptions.CorporateActionNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowDomainException_WhenCorporateActionIsAlreadyApplied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var corporateAction =
            new CorporateAction(
                corporateActionId,
                Guid.NewGuid(),
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

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CancelCorporateActionService(
                corporateActionRepository,
                unitOfWork);

        var command =
            new CancelCorporateActionCommand(
                corporateActionId,
                userId,
                "Cancel after application.");

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Domain.Exceptions.DomainException>();

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Applied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowDomainException_WhenCorporateActionIsAlreadyCancelled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var corporateAction =
            new CorporateAction(
                corporateActionId,
                Guid.NewGuid(),
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
            "Original cancellation.");

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CancelCorporateActionService(
                corporateActionRepository,
                unitOfWork);

        var command =
            new CancelCorporateActionCommand(
                corporateActionId,
                userId,
                "Second cancellation.");

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Domain.Exceptions.DomainException>();

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Cancelled);

        corporateAction.CancellationReason
            .Should()
            .Be("Original cancellation.");
    }
}
