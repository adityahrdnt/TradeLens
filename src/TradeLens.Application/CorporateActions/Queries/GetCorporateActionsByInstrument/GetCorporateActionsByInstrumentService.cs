using TradeLens.Application.Interfaces;

namespace TradeLens.Application.CorporateActions.Queries.GetCorporateActionsByInstrument;

public sealed class GetCorporateActionsByInstrumentService
{
    private readonly ICorporateActionRepository _corporateActionRepository;

    public GetCorporateActionsByInstrumentService(
        ICorporateActionRepository corporateActionRepository)
    {
        _corporateActionRepository = corporateActionRepository;
    }

    public async Task<IReadOnlyList<GetCorporateActionsByInstrumentResult>> ExecuteAsync(
        GetCorporateActionsByInstrumentQuery query,
        CancellationToken cancellationToken = default)
    {
        var corporateActions =
            await _corporateActionRepository.GetByInstrumentAsync(
                query.InstrumentId,
                cancellationToken);

        return corporateActions
            .Select(x =>
                new GetCorporateActionsByInstrumentResult(
                    x.Id,
                    x.InstrumentId,
                    x.Type,
                    x.Numerator,
                    x.Denominator,
                    x.RecordDate,
                    x.ExDate,
                    x.EffectiveDate,
                    x.Status,
                    x.CreatedAt,
                    x.CreatedBy,
                    x.AppliedAt,
                    x.AppliedBy,
                    x.CancelledAt,
                    x.CancelledBy,
                    x.CancellationReason))
            .ToList();
    }
}