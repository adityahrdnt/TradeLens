namespace TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;

public sealed record CancelCorporateActionCommand(
    Guid CorporateActionId,
    Guid CancelledBy,
    string Reason);
