using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class CorporateActionChange
{
    public Guid Id { get; private set; }

    public Guid CorporateActionId { get; private set; }

    public CorporateActionChangeType ChangeType { get; private set; }

    public DateOnly? PreviousEffectiveDate { get; private set; }

    public DateOnly? NewEffectiveDate { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public Guid ChangedBy { get; private set; }

    private CorporateActionChange()
    {
        Reason = string.Empty;
    }

    public CorporateActionChange(
        Guid id,
        Guid corporateActionId,
        CorporateActionChangeType changeType,
        DateOnly? previousEffectiveDate,
        DateOnly? newEffectiveDate,
        string reason,
        DateTimeOffset changedAt,
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

        if (changeType == CorporateActionChangeType.Delay)
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

        Id = id;
        CorporateActionId = corporateActionId;
        ChangeType = changeType;
        PreviousEffectiveDate = previousEffectiveDate;
        NewEffectiveDate = newEffectiveDate;
        Reason = reason.Trim();
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }
}
