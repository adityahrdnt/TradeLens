namespace TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;

public sealed record ApplyCorporateActionResult(
    Guid CorporateActionId,
    int AppliedPortfolioCount);