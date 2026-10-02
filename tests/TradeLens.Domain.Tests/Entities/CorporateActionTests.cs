using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public class CorporateActionTests
{
    private static readonly Guid Id =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly Guid InstrumentId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid UserId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static readonly DateTimeOffset CreatedAt =
        new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ShouldCreateScheduledCorporateAction()
    {
        var action = new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 13),
            new DateOnly(2026, 10, 15),
            UserId,
            CreatedAt);

        action.Id.Should().Be(Id);
        action.InstrumentId.Should().Be(InstrumentId);
        action.Type.Should().Be(CorporateActionType.StockSplit);
        action.Numerator.Should().Be(2);
        action.Denominator.Should().Be(1);
        action.RecordDate.Should().Be(new DateOnly(2026, 10, 10));
        action.ExDate.Should().Be(new DateOnly(2026, 10, 13));
        action.EffectiveDate.Should().Be(new DateOnly(2026, 10, 15));
        action.Status.Should().Be(CorporateActionStatus.Scheduled);
        action.CreatedBy.Should().Be(UserId);
        action.CreatedAt.Should().Be(CreatedAt);
        action.AppliedAt.Should().BeNull();
        action.AppliedBy.Should().BeNull();
        action.CancelledAt.Should().BeNull();
        action.CancelledBy.Should().BeNull();
        action.CancellationReason.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveNumerator()
    {
        var act = () => new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            0,
            1,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 13),
            new DateOnly(2026, 10, 15),
            UserId,
            CreatedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Corporate action numerator must be greater than zero.");
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveDenominator()
    {
        var act = () => new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            2,
            0,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 13),
            new DateOnly(2026, 10, 15),
            UserId,
            CreatedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Corporate action denominator must be greater than zero.");
    }

    [Fact]
    public void Constructor_ShouldRejectExDateBeforeRecordDate()
    {
        var act = () => new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 9),
            new DateOnly(2026, 10, 15),
            UserId,
            CreatedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Corporate action ex-date cannot be earlier than record date.");
    }

    [Fact]
    public void Constructor_ShouldRejectEffectiveDateBeforeExDate()
    {
        var act = () => new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 13),
            new DateOnly(2026, 10, 12),
            UserId,
            CreatedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Corporate action effective date cannot be earlier than ex-date.");
    }

    [Fact]
    public void Apply_ShouldChangeStatusToApplied()
    {
        var action = CreateAction();

        var appliedAt =
            new DateTimeOffset(2026, 10, 15, 9, 30, 0, TimeSpan.Zero);

        action.Apply(appliedAt, UserId);

        action.Status.Should().Be(CorporateActionStatus.Applied);
        action.AppliedAt.Should().Be(appliedAt);
        action.AppliedBy.Should().Be(UserId);
    }

    [Fact]
    public void Apply_ShouldRejectAlreadyAppliedCorporateAction()
    {
        var action = CreateAction();

        action.Apply(
            new DateTimeOffset(2026, 10, 15, 9, 30, 0, TimeSpan.Zero),
            UserId);

        var act = () => action.Apply(
            new DateTimeOffset(2026, 10, 15, 10, 0, 0, TimeSpan.Zero),
            UserId);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only a scheduled corporate action can be applied.");
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var action = CreateAction();

        var cancelledAt =
            new DateTimeOffset(2026, 10, 14, 8, 20, 0, TimeSpan.Zero);

        action.Cancel(
            cancelledAt,
            UserId,
            "Issuer cancelled corporate action.");

        action.Status.Should().Be(CorporateActionStatus.Cancelled);
        action.CancelledAt.Should().Be(cancelledAt);
        action.CancelledBy.Should().Be(UserId);
        action.CancellationReason.Should()
            .Be("Issuer cancelled corporate action.");
    }

    [Fact]
    public void Cancel_ShouldRejectAlreadyAppliedCorporateAction()
    {
        var action = CreateAction();

        action.Apply(
            new DateTimeOffset(2026, 10, 15, 9, 30, 0, TimeSpan.Zero),
            UserId);

        var act = () => action.Cancel(
            new DateTimeOffset(2026, 10, 15, 10, 0, 0, TimeSpan.Zero),
            UserId,
            "Cancellation.");

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only a scheduled corporate action can be cancelled.");
    }

    private static CorporateAction CreateAction()
    {
        return new CorporateAction(
            Id,
            InstrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 13),
            new DateOnly(2026, 10, 15),
            UserId,
            CreatedAt);
    }
}