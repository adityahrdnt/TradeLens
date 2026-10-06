namespace TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;

public sealed record DelayCorporateActionCommand(
    Guid CorporateActionId,
    DateOnly NewEffectiveDate,
    string Reason,
    Guid DelayedBy);
