namespace TradeLens.Application.CorporateActions.Queries.GetCorporateAction;

public sealed record GetCorporateActionResult(
    Guid CorporateActionId,
    Guid InstrumentId,
    TradeLens.Domain.Enums.CorporateActionType Type,
    int Numerator,
    int Denominator,
    DateOnly RecordDate,
    DateOnly ExDate,
    DateOnly EffectiveDate,
    TradeLens.Domain.Enums.CorporateActionStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? AppliedAt,
    Guid? AppliedBy,
    DateTimeOffset? CancelledAt,
    Guid? CancelledBy,
    string? CancellationReason);