using FluentAssertions;
using TradeLens.Application.CorporateActions.Commands.CorrectCorporateAction;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.Tests.CorporateActions.Commands.CorrectCorporateAction;

public class CorrectCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCorrectCorporateAction_AndCreateChangeHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var originalRecordDate =
            new DateOnly(2026, 9, 10);

        var originalExDate =
            new DateOnly(2026, 9, 11);

        var originalEffectiveDate =
            new DateOnly(2026, 9, 12);

        var newRecordDate =
            new DateOnly(2026, 9, 14);

        var newExDate =
            new DateOnly(2026, 9, 15);

        var newEffectiveDate =
            new DateOnly(2026, 9, 16);

        var corporateAction = new CorporateAction(
            corporateActionId,
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            originalRecordDate,
            originalExDate,
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
            new CorrectCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new CorrectCorporateActionCommand(
                corporateActionId,
                3,
                2,
                newRecordDate,
                newExDate,
                newEffectiveDate,
                "Issuer corrected the corporate action ratio and schedule.",
                userId);

        // Act
        var result =
            await service.ExecuteAsync(command);

        // Assert
        result.CorporateActionId
            .Should()
            .Be(corporateActionId);

        corporateAction.Numerator
            .Should()
            .Be(3);

        corporateAction.Denominator
            .Should()
            .Be(2);

        corporateAction.RecordDate
            .Should()
            .Be(newRecordDate);

        corporateAction.ExDate
            .Should()
            .Be(newExDate);

        corporateAction.EffectiveDate
            .Should()
            .Be(newEffectiveDate);

        corporateAction.OriginalEffectiveDate
            .Should()
            .Be(originalEffectiveDate);

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
            .Be(CorporateActionChangeType.Correction);

        change.PreviousNumerator
            .Should()
            .Be(2);

        change.NewNumerator
            .Should()
            .Be(3);

        change.PreviousDenominator
            .Should()
            .Be(1);

        change.NewDenominator
            .Should()
            .Be(2);

        change.PreviousRecordDate
            .Should()
            .Be(originalRecordDate);

        change.NewRecordDate
            .Should()
            .Be(newRecordDate);

        change.PreviousExDate
            .Should()
            .Be(originalExDate);

        change.NewExDate
            .Should()
            .Be(newExDate);

        change.PreviousEffectiveDate
            .Should()
            .Be(originalEffectiveDate);

        change.NewEffectiveDate
            .Should()
            .Be(newEffectiveDate);

        change.Reason
            .Should()
            .Be("Issuer corrected the corporate action ratio and schedule.");

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
            new CorrectCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var corporateActionId = Guid.NewGuid();

        var command =
            new CorrectCorporateActionCommand(
                corporateActionId,
                3,
                2,
                new DateOnly(2026, 9, 14),
                new DateOnly(2026, 9, 15),
                new DateOnly(2026, 9, 16),
                "Corporate action correction.",
                Guid.NewGuid());

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Application.Exceptions.CorporateActionNotFoundException>();

        unitOfWork.TransactionCallCount
            .Should()
            .Be(0);

        changeRepository.Changes
            .Should()
            .BeEmpty();
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
            new CorrectCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new CorrectCorporateActionCommand(
                corporateActionId,
                3,
                2,
                new DateOnly(2026, 9, 14),
                new DateOnly(2026, 9, 15),
                new DateOnly(2026, 9, 16),
                "Correction after application.",
                userId);

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Domain.Exceptions.DomainException>()
            .WithMessage(
                "Only a scheduled corporate action can be corrected.");

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Applied);

        corporateAction.Numerator
            .Should()
            .Be(2);

        corporateAction.Denominator
            .Should()
            .Be(1);

        corporateAction.RecordDate
            .Should()
            .Be(new DateOnly(2026, 9, 10));

        corporateAction.ExDate
            .Should()
            .Be(new DateOnly(2026, 9, 11));

        corporateAction.EffectiveDate
            .Should()
            .Be(new DateOnly(2026, 9, 12));

        changeRepository.Changes
            .Should()
            .BeEmpty();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(0);
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
            new CorrectCorporateActionService(
                corporateActionRepository,
                changeRepository,
                unitOfWork);

        var command =
            new CorrectCorporateActionCommand(
                corporateActionId,
                3,
                2,
                new DateOnly(2026, 9, 14),
                new DateOnly(2026, 9, 15),
                new DateOnly(2026, 9, 16),
                "Correction after cancellation.",
                userId);

        // Act
        var act = () =>
            service.ExecuteAsync(command);

        // Assert
        await act
            .Should()
            .ThrowAsync<TradeLens.Domain.Exceptions.DomainException>()
            .WithMessage(
                "Only a scheduled corporate action can be corrected.");

        corporateAction.Status
            .Should()
            .Be(CorporateActionStatus.Cancelled);

        corporateAction.Numerator
            .Should()
            .Be(2);

        corporateAction.Denominator
            .Should()
            .Be(1);

        corporateAction.RecordDate
            .Should()
            .Be(new DateOnly(2026, 9, 10));

        corporateAction.ExDate
            .Should()
            .Be(new DateOnly(2026, 9, 11));

        corporateAction.EffectiveDate
            .Should()
            .Be(new DateOnly(2026, 9, 12));

        changeRepository.Changes
            .Should()
            .BeEmpty();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(0);
    }
}
