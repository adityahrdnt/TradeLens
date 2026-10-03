using TradeLens.Domain.Enums;

namespace TradeLens.Api.Contracts.CorporateActions;

public sealed record GetCorporateActionResponse(
    Guid CorporateActionId,
    Guid InstrumentId,
    CorporateActionType Type,
    int Numerator,
    int Denominator,
    DateOnly RecordDate,
    DateOnly ExDate,
    DateOnly EffectiveDate,
    CorporateActionStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? AppliedAt,
    Guid? AppliedBy,
    DateTimeOffset? CancelledAt,
    Guid? CancelledBy,
    string? CancellationReason);