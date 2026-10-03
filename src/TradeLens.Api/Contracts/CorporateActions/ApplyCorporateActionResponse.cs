namespace TradeLens.Api.Contracts.CorporateActions;

public sealed record ApplyCorporateActionResponse(
    Guid CorporateActionId,
    int AppliedPortfolioCount);