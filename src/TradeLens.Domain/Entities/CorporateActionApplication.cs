namespace TradeLens.Domain.Entities;

using TradeLens.Domain.Exceptions;

public class CorporateActionApplication
{
    public Guid Id { get; private set; }

    public Guid CorporateActionId { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid InstrumentId { get; private set; }

    public long EligibleQuantity { get; private set; }

    public long ResultingQuantity { get; private set; }

    public DateTimeOffset AppliedAt { get; private set; }

    private CorporateActionApplication()
    {
    }

    public CorporateActionApplication(
        Guid id,
        Guid corporateActionId,
        Guid portfolioId,
        Guid instrumentId,
        long eligibleQuantity,
        long resultingQuantity,
        DateTimeOffset appliedAt)
    {
        if (id == Guid.Empty)
            throw new DomainException(
                "Corporate action application id is required.");

        if (corporateActionId == Guid.Empty)
            throw new DomainException(
                "Corporate action id is required.");

        if (portfolioId == Guid.Empty)
            throw new DomainException(
                "Corporate action application portfolio id is required.");

        if (instrumentId == Guid.Empty)
            throw new DomainException(
                "Corporate action application instrument id is required.");

        if (eligibleQuantity < 0)
            throw new DomainException(
                "Corporate action eligible quantity cannot be negative.");

        if (resultingQuantity < 0)
            throw new DomainException(
                "Corporate action resulting quantity cannot be negative.");

        Id = id;
        CorporateActionId = corporateActionId;
        PortfolioId = portfolioId;
        InstrumentId = instrumentId;
        EligibleQuantity = eligibleQuantity;
        ResultingQuantity = resultingQuantity;
        AppliedAt = appliedAt;
    }
}