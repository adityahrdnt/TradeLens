using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class CorporateActionChange
{
    public Guid Id { get; private set; }

    public Guid CorporateActionId { get; private set; }

    public CorporateActionChangeType ChangeType { get; private set; }

    public int? PreviousNumerator { get; private set; }

    public int? NewNumerator { get; private set; }

    public int? PreviousDenominator { get; private set; }

    public int? NewDenominator { get; private set; }

    public DateOnly? PreviousRecordDate { get; private set; }

    public DateOnly? NewRecordDate { get; private set; }

    public DateOnly? PreviousExDate { get; private set; }

    public DateOnly? NewExDate { get; private set; }

    public DateOnly? PreviousEffectiveDate { get; private set; }

    public DateOnly? NewEffectiveDate { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public Guid ChangedBy { get; private set; }

    private CorporateActionChange()
    {
        Reason = string.Empty;
    }

    private CorporateActionChange(
        Guid id,
        Guid corporateActionId,
        CorporateActionChangeType changeType,
        int? previousNumerator,
        int? newNumerator,
        int? previousDenominator,
        int? newDenominator,
        DateOnly? previousRecordDate,
        DateOnly? newRecordDate,
        DateOnly? previousExDate,
        DateOnly? newExDate,
        DateOnly? previousEffectiveDate,
        DateOnly? newEffectiveDate,
        string reason,
        DateTimeOffset changedAt,
        Guid changedBy)
    {
        ValidateCommon(
            id,
            corporateActionId,
            reason,
            changedBy);

        ValidateChange(
            changeType,
            previousNumerator,
            newNumerator,
            previousDenominator,
            newDenominator,
            previousRecordDate,
            newRecordDate,
            previousExDate,
            newExDate,
            previousEffectiveDate,
            newEffectiveDate);

        Id = id;
        CorporateActionId = corporateActionId;
        ChangeType = changeType;

        PreviousNumerator = previousNumerator;
        NewNumerator = newNumerator;

        PreviousDenominator = previousDenominator;
        NewDenominator = newDenominator;

        PreviousRecordDate = previousRecordDate;
        NewRecordDate = newRecordDate;

        PreviousExDate = previousExDate;
        NewExDate = newExDate;

        PreviousEffectiveDate = previousEffectiveDate;
        NewEffectiveDate = newEffectiveDate;

        Reason = reason.Trim();
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }

    public static CorporateActionChange CreateDelay(
        Guid id,
        Guid corporateActionId,
        DateOnly previousEffectiveDate,
        DateOnly newEffectiveDate,
        string reason,
        DateTimeOffset changedAt,
        Guid changedBy)
    {
        return new CorporateActionChange(
            id,
            corporateActionId,
            CorporateActionChangeType.Delay,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            previousEffectiveDate,
            newEffectiveDate,
            reason,
            changedAt,
            changedBy);
    }

    public static CorporateActionChange CreateCorrection(
        Guid id,
        Guid corporateActionId,
        int previousNumerator,
        int newNumerator,
        int previousDenominator,
        int newDenominator,
        DateOnly previousRecordDate,
        DateOnly newRecordDate,
        DateOnly previousExDate,
        DateOnly newExDate,
        DateOnly previousEffectiveDate,
        DateOnly newEffectiveDate,
        string reason,
        DateTimeOffset changedAt,
        Guid changedBy)
    {
        return new CorporateActionChange(
            id,
            corporateActionId,
            CorporateActionChangeType.Correction,
            previousNumerator,
            newNumerator,
            previousDenominator,
            newDenominator,
            previousRecordDate,
            newRecordDate,
            previousExDate,
            newExDate,
            previousEffectiveDate,
            newEffectiveDate,
            reason,
            changedAt,
            changedBy);
    }

    private static void ValidateCommon(
        Guid id,
        Guid corporateActionId,
        string reason,
        Guid changedBy)
    {
        if (id == Guid.Empty)
            throw new DomainException(
                "Corporate action change id is required.");

        if (corporateActionId == Guid.Empty)
            throw new DomainException(
                "Corporate action change corporate action id is required.");

        if (changedBy == Guid.Empty)
            throw new DomainException(
                "Corporate action change changed by is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(
                "Corporate action change reason is required.");
    }

    private static void ValidateChange(
        CorporateActionChangeType changeType,
        int? previousNumerator,
        int? newNumerator,
        int? previousDenominator,
        int? newDenominator,
        DateOnly? previousRecordDate,
        DateOnly? newRecordDate,
        DateOnly? previousExDate,
        DateOnly? newExDate,
        DateOnly? previousEffectiveDate,
        DateOnly? newEffectiveDate)
    {
        switch (changeType)
        {
            case CorporateActionChangeType.Delay:
                ValidateDelay(
                    previousEffectiveDate,
                    newEffectiveDate);
                break;

            case CorporateActionChangeType.Correction:
                ValidateCorrection(
                    previousNumerator,
                    newNumerator,
                    previousDenominator,
                    newDenominator,
                    previousRecordDate,
                    newRecordDate,
                    previousExDate,
                    newExDate,
                    previousEffectiveDate,
                    newEffectiveDate);
                break;

            default:
                throw new DomainException(
                    "Unsupported corporate action change type.");
        }
    }

    private static void ValidateDelay(
        DateOnly? previousEffectiveDate,
        DateOnly? newEffectiveDate)
    {
        if (!previousEffectiveDate.HasValue ||
            !newEffectiveDate.HasValue)
        {
            throw new DomainException(
                "Delay change must contain previous and new effective dates.");
        }

        if (newEffectiveDate.Value <= previousEffectiveDate.Value)
        {
            throw new DomainException(
                "New effective date must be later than previous effective date.");
        }
    }

    private static void ValidateCorrection(
        int? previousNumerator,
        int? newNumerator,
        int? previousDenominator,
        int? newDenominator,
        DateOnly? previousRecordDate,
        DateOnly? newRecordDate,
        DateOnly? previousExDate,
        DateOnly? newExDate,
        DateOnly? previousEffectiveDate,
        DateOnly? newEffectiveDate)
    {
        if (!previousNumerator.HasValue ||
            !newNumerator.HasValue ||
            !previousDenominator.HasValue ||
            !newDenominator.HasValue ||
            !previousRecordDate.HasValue ||
            !newRecordDate.HasValue ||
            !previousExDate.HasValue ||
            !newExDate.HasValue ||
            !previousEffectiveDate.HasValue ||
            !newEffectiveDate.HasValue)
        {
            throw new DomainException(
                "Correction change must contain complete previous and new corporate action state.");
        }

        ValidateCorporateActionState(
            previousNumerator.Value,
            previousDenominator.Value,
            previousRecordDate.Value,
            previousExDate.Value,
            previousEffectiveDate.Value);

        ValidateCorporateActionState(
            newNumerator.Value,
            newDenominator.Value,
            newRecordDate.Value,
            newExDate.Value,
            newEffectiveDate.Value);
    }

    private static void ValidateCorporateActionState(
        int numerator,
        int denominator,
        DateOnly recordDate,
        DateOnly exDate,
        DateOnly effectiveDate)
    {
        if (numerator <= 0)
        {
            throw new DomainException(
                "Corporate action numerator must be greater than zero.");
        }

        if (denominator <= 0)
        {
            throw new DomainException(
                "Corporate action denominator must be greater than zero.");
        }

        if (exDate < recordDate)
        {
            throw new DomainException(
                "Corporate action ex-date cannot be earlier than record date.");
        }

        if (effectiveDate < exDate)
        {
            throw new DomainException(
                "Corporate action effective date cannot be earlier than ex-date.");
        }
    }
}
