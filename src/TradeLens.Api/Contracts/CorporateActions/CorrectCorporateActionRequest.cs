namespace TradeLens.Api.Contracts.CorporateActions;

public sealed record CorrectCorporateActionRequest(
    int NewNumerator,
    int NewDenominator,
    DateOnly NewEligibilityDate,
    DateOnly NewRecordDate,
    DateOnly NewExDate,
    DateOnly NewEffectiveDate,
    string Reason);
