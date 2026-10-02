using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Services;

public class CorporateActionCalculator
{
    public long CalculateResultingQuantity(
        CorporateAction corporateAction,
        long eligibleQuantity)
    {
        if (corporateAction.Status != CorporateActionStatus.Applied)
            throw new DomainException(
                "Only an applied corporate action can be calculated.");

        if (eligibleQuantity < 0)
            throw new DomainException(
                "Eligible quantity cannot be negative.");

        var resultingQuantity =
            (decimal)eligibleQuantity
            * corporateAction.Numerator
            / corporateAction.Denominator;

        return (long)Math.Floor(resultingQuantity);
    }
}