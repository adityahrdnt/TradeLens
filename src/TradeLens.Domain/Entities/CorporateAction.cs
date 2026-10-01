using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class CorporateAction
{
    public Guid Id { get; private set; }

    public Guid InstrumentId { get; private set; }

    public CorporateActionType Type { get; private set; }

    public int Numerator { get; private set; }

    public int Denominator { get; private set; }

    public DateOnly RecordDate { get; private set; }

    public DateOnly ExDate { get; private set; }

    public DateOnly EffectiveDate { get; private set; }

    public CorporateActionStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? AppliedAt { get; private set; }

    public Guid? AppliedBy { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public Guid? CancelledBy { get; private set; }

    public string? CancellationReason { get; private set; }

    private CorporateAction()
    {
    }

    public CorporateAction(
        Guid id,
        Guid instrumentId,
        CorporateActionType type,
        int numerator,
        int denominator,
        DateOnly recordDate,
        DateOnly exDate,
        DateOnly effectiveDate,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("Corporate action id is required.");

        if (instrumentId == Guid.Empty)
            throw new DomainException("Corporate action instrument id is required.");

        if (numerator <= 0)
            throw new DomainException(
                "Corporate action numerator must be greater than zero.");

        if (denominator <= 0)
            throw new DomainException(
                "Corporate action denominator must be greater than zero.");

        if (createdBy == Guid.Empty)
            throw new DomainException(
                "Corporate action created by is required.");

        if (exDate < recordDate)
            throw new DomainException(
                "Corporate action ex-date cannot be earlier than record date.");

        if (effectiveDate < exDate)
            throw new DomainException(
                "Corporate action effective date cannot be earlier than ex-date.");

        Id = id;
        InstrumentId = instrumentId;
        Type = type;
        Numerator = numerator;
        Denominator = denominator;
        RecordDate = recordDate;
        ExDate = exDate;
        EffectiveDate = effectiveDate;
        Status = CorporateActionStatus.Scheduled;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public void Apply(
        DateTimeOffset appliedAt,
        Guid appliedBy)
    {
        if (Status != CorporateActionStatus.Scheduled)
            throw new DomainException(
                "Only a scheduled corporate action can be applied.");

        if (appliedBy == Guid.Empty)
            throw new DomainException(
                "Corporate action applied by is required.");

        Status = CorporateActionStatus.Applied;
        AppliedAt = appliedAt;
        AppliedBy = appliedBy;
    }

    public void Cancel(
        DateTimeOffset cancelledAt,
        Guid cancelledBy,
        string reason)
    {
        if (Status != CorporateActionStatus.Scheduled)
            throw new DomainException(
                "Only a scheduled corporate action can be cancelled.");

        if (cancelledBy == Guid.Empty)
            throw new DomainException(
                "Corporate action cancelled by is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(
                "Corporate action cancellation reason is required.");

        Status = CorporateActionStatus.Cancelled;
        CancelledAt = cancelledAt;
        CancelledBy = cancelledBy;
        CancellationReason = reason.Trim();
    }
}