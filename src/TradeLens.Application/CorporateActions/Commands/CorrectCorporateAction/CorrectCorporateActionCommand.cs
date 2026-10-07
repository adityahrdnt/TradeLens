using TradeLens.Domain.Enums;

namespace TradeLens.Application.CorporateActions.Commands.CorrectCorporateAction;

public sealed record CorrectCorporateActionCommand(
    Guid CorporateActionId,
    int NewNumerator,
    int NewDenominator,
    DateOnly NewRecordDate,
    DateOnly NewExDate,
    DateOnly NewEffectiveDate,
    string Reason,
    Guid CorrectedBy);
