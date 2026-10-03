using TradeLens.Domain.Enums;

namespace TradeLens.Application.CorporateActions.Queries.GetCorporateActionsByInstrument;

public sealed record GetCorporateActionsByInstrumentResult(
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