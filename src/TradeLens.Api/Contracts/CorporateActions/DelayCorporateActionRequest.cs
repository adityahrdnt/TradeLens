namespace TradeLens.Api.Contracts.CorporateActions;

public sealed record DelayCorporateActionRequest(
    DateOnly NewEffectiveDate,
    string Reason);
