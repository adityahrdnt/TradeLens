using TradeLens.Domain.Enums;

namespace TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;

public sealed record CreateCorporateActionCommand(
    Guid InstrumentId,
    CorporateActionType Type,
    int Numerator,
    int Denominator,
    DateOnly RecordDate,
    DateOnly ExDate,
    DateOnly EffectiveDate,
    Guid CreatedBy);