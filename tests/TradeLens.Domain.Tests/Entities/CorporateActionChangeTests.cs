using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public class CorporateActionChangeTests
{
    private static readonly Guid CorporateActionId =
        Guid.NewGuid();

    private static readonly Guid ChangedBy =
        Guid.NewGuid();

    private static readonly DateTimeOffset ChangedAt =
        new(2026, 10, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateDelay_WithValidData_ShouldCreateChange()
    {
        var previousEffectiveDate =
            new DateOnly(2026, 10, 5);

        var newEffectiveDate =
            new DateOnly(2026, 10, 10);

        var change =
            CorporateActionChange.CreateDelay(
                Guid.NewGuid(),
                CorporateActionId,
                previousEffectiveDate,
                newEffectiveDate,
                "Issuer postponed the effective date.",
                ChangedAt,
                ChangedBy);

        change.Id.Should().NotBe(Guid.Empty);
        change.CorporateActionId.Should().Be(CorporateActionId);
        change.ChangeType.Should().Be(
            CorporateActionChangeType.Delay);

        change.PreviousEffectiveDate.Should().Be(
            previousEffectiveDate);

        change.NewEffectiveDate.Should().Be(
            newEffectiveDate);

        change.PreviousNumerator.Should().BeNull();
        change.NewNumerator.Should().BeNull();
        change.PreviousDenominator.Should().BeNull();
        change.NewDenominator.Should().BeNull();

        change.PreviousRecordDate.Should().BeNull();
        change.NewRecordDate.Should().BeNull();
        change.PreviousExDate.Should().BeNull();
        change.NewExDate.Should().BeNull();

        change.Reason.Should().Be(
            "Issuer postponed the effective date.");

        change.ChangedAt.Should().Be(ChangedAt);
        change.ChangedBy.Should().Be(ChangedBy);
    }

    [Fact]
    public void CreateDelay_WhenNewDateIsNotLater_ShouldThrowDomainException()
    {
        var previousEffectiveDate =
            new DateOnly(2026, 10, 10);

        var newEffectiveDate =
            new DateOnly(2026, 10, 5);

        var action = () =>
            CorporateActionChange.CreateDelay(
                Guid.NewGuid(),
                CorporateActionId,
                previousEffectiveDate,
                newEffectiveDate,
                "Invalid delay.",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "New effective date must be later than previous effective date.");
    }

    [Fact]
    public void CreateDelay_WhenReasonIsEmpty_ShouldThrowDomainException()
    {
        var action = () =>
            CorporateActionChange.CreateDelay(
                Guid.NewGuid(),
                CorporateActionId,
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 10),
                " ",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action change reason is required.");
    }

    [Fact]
    public void CreateCorrection_WithValidData_ShouldCreateFullSnapshot()
    {
        var previousRecordDate =
            new DateOnly(2026, 10, 1);

        var newRecordDate =
            new DateOnly(2026, 10, 2);

        var previousExDate =
            new DateOnly(2026, 10, 3);

        var newExDate =
            new DateOnly(2026, 10, 4);

        var previousEffectiveDate =
            new DateOnly(2026, 10, 5);

        var newEffectiveDate =
            new DateOnly(2026, 10, 6);

        var change =
            CorporateActionChange.CreateCorrection(
                Guid.NewGuid(),
                CorporateActionId,
                1,
                2,
                10,
                10,
                previousRecordDate,
                newRecordDate,
                previousExDate,
                newExDate,
                previousEffectiveDate,
                newEffectiveDate,
                "Issuer corrected the corporate action terms.",
                ChangedAt,
                ChangedBy);

        change.CorporateActionId.Should().Be(
            CorporateActionId);

        change.ChangeType.Should().Be(
            CorporateActionChangeType.Correction);

        change.PreviousNumerator.Should().Be(1);
        change.NewNumerator.Should().Be(2);

        change.PreviousDenominator.Should().Be(10);
        change.NewDenominator.Should().Be(10);

        change.PreviousRecordDate.Should().Be(
            previousRecordDate);

        change.NewRecordDate.Should().Be(
            newRecordDate);

        change.PreviousExDate.Should().Be(
            previousExDate);

        change.NewExDate.Should().Be(
            newExDate);

        change.PreviousEffectiveDate.Should().Be(
            previousEffectiveDate);

        change.NewEffectiveDate.Should().Be(
            newEffectiveDate);

        change.Reason.Should().Be(
            "Issuer corrected the corporate action terms.");

        change.ChangedAt.Should().Be(ChangedAt);
        change.ChangedBy.Should().Be(ChangedBy);
    }

    [Fact]
    public void CreateCorrection_WhenNumeratorIsZero_ShouldThrowDomainException()
    {
        var action = () =>
            CorporateActionChange.CreateCorrection(
                Guid.NewGuid(),
                CorporateActionId,
                0,
                2,
                10,
                10,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 2),
                new DateOnly(2026, 10, 3),
                new DateOnly(2026, 10, 4),
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 6),
                "Correction.",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action numerator must be greater than zero.");
    }

    [Fact]
    public void CreateCorrection_WhenDenominatorIsZero_ShouldThrowDomainException()
    {
        var action = () =>
            CorporateActionChange.CreateCorrection(
                Guid.NewGuid(),
                CorporateActionId,
                1,
                2,
                0,
                10,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 2),
                new DateOnly(2026, 10, 3),
                new DateOnly(2026, 10, 4),
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 6),
                "Correction.",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action denominator must be greater than zero.");
    }

    [Fact]
    public void CreateCorrection_WhenExDateIsBeforeRecordDate_ShouldThrowDomainException()
    {
        var action = () =>
            CorporateActionChange.CreateCorrection(
                Guid.NewGuid(),
                CorporateActionId,
                1,
                2,
                10,
                10,
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 4),
                new DateOnly(2026, 10, 6),
                new DateOnly(2026, 10, 7),
                new DateOnly(2026, 10, 8),
                "Correction.",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action ex-date cannot be earlier than record date.");
    }

    [Fact]
    public void CreateCorrection_WhenEffectiveDateIsBeforeExDate_ShouldThrowDomainException()
    {
        var action = () =>
            CorporateActionChange.CreateCorrection(
                Guid.NewGuid(),
                CorporateActionId,
                1,
                2,
                10,
                10,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 2),
                new DateOnly(2026, 10, 3),
                new DateOnly(2026, 10, 4),
                new DateOnly(2026, 10, 2),
                new DateOnly(2026, 10, 3),
                "Correction.",
                ChangedAt,
                ChangedBy);

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action effective date cannot be earlier than ex-date.");
    }
}
