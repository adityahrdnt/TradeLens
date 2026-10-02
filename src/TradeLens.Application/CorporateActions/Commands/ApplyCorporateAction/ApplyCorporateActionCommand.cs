namespace TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;

public sealed record ApplyCorporateActionCommand(
    Guid CorporateActionId,
    Guid AppliedBy);