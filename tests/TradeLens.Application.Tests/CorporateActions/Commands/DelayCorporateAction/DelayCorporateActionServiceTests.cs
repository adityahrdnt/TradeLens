using FluentAssertions;
using TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.Tests.CorporateActions.Commands.DelayCorporateAction;

public class DelayCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldDelayCorporateAction_AndCreateChangeHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var originalEffectiveDate =
            new DateOnly(2026, 9, 12);

        var newEffectiveDate =
            new DateOnly(2026, 9, 20);

        var corporateAction = new CorporateAction(
            corporateActionId,
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            originalEffectiveDate,
            userId,
            DateTimeOffset.UtcNow);

        var corporateActionRepository =
            new FakeCorporateActionRepository();

        corporateActionRepository.CorporateActions.Add(
            corporateAction);

        var changeRepository =
            new FakeCorporateActionChangeRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DelayCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new DelayCorporateActionCommand(
                corporateActionId,
                newEffectiveDate,
                "Corporate action postponed by issuer.",
                userId);

        // Act
        var result =
            await service.ExecuteAsync(command);

        // Assert
        result.CorporateActionId
            .Should()
            .Be(corporateActionId);

        corporateAction.OriginalEffectiveDate
            .Should()
            .Be(originalEffectiveDate);

        corporateAction.EffectiveDate
            .Should()
            .Be(newEffectiveDate);

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Scheduled);

        changeRepository.Changes
            .Should()
            .ContainSingle();

        var change =
            changeRepository.Changes.Single();

        change.CorporateActionId
            .Should()
            .Be(corporateActionId);

        change.ChangeType
            .Should()
            .Be(CorporateActionChangeType.Delay);

        change.PreviousEffectiveDate
            .Should()
            .Be(originalEffectiveDate);

        change.NewEffectiveDate
            .Should()
            .Be(newEffectiveDate);

        change.Reason
            .Should()
            .Be("Corporate action postponed by issuer.");

        change.ChangedBy
            .Should()
            .Be(userId);

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

        var changeRepository =
            new FakeCorporateActionChangeRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DelayCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var corporateActionId = Guid.NewGuid();

        var command =
            new DelayCorporateActionCommand(
                corporateActionId,
                new DateOnly(2026, 9, 20),
                "Corporate action postponed.",
                Guid.NewGuid());

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

        var changeRepository =
            new FakeCorporateActionChangeRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DelayCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new DelayCorporateActionCommand(
                corporateActionId,
                new DateOnly(2026, 9, 20),
                "Delay after application.",
                userId);

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

        corporateAction.EffectiveDate
            .Should()
            .Be(new DateOnly(2026, 9, 12));

        changeRepository.Changes
            .Should()
            .BeEmpty();
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

        var changeRepository =
            new FakeCorporateActionChangeRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DelayCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new DelayCorporateActionCommand(
                corporateActionId,
                new DateOnly(2026, 9, 20),
                "Second delay attempt.",
                userId);

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

        corporateAction.EffectiveDate
            .Should()
            .Be(new DateOnly(2026, 9, 12));

        changeRepository.Changes
            .Should()
            .BeEmpty();
    }
}
