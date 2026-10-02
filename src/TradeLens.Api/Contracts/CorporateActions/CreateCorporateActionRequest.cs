using TradeLens.Domain.Enums;

namespace TradeLens.Api.Contracts.CorporateActions;

public sealed record CreateCorporateActionRequest(
    Guid InstrumentId,
    CorporateActionType Type,
    int Numerator,
    int Denominator,
    DateOnly RecordDate,
    DateOnly ExDate,
    DateOnly EffectiveDate);