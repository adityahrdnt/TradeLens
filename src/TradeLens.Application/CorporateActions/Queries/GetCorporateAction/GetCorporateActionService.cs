using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.CorporateActions.Queries.GetCorporateAction;

public sealed class GetCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;

    public GetCorporateActionService(
        ICorporateActionRepository corporateActionRepository)
    {
        _corporateActionRepository = corporateActionRepository;
    }

    public async Task<GetCorporateActionResult> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var corporateAction =
            await _corporateActionRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (corporateAction is null)
        {
            throw new CorporateActionNotFoundException(id);
        }

        return new GetCorporateActionResult(
            corporateAction.Id,
            corporateAction.InstrumentId,
            corporateAction.Type,
            corporateAction.Numerator,
            corporateAction.Denominator,
            corporateAction.RecordDate,
            corporateAction.ExDate,
            corporateAction.EffectiveDate,
            corporateAction.Status,
            corporateAction.CreatedAt,
            corporateAction.CreatedBy,
            corporateAction.AppliedAt,
            corporateAction.AppliedBy,
            corporateAction.CancelledAt,
            corporateAction.CancelledBy,
            corporateAction.CancellationReason);
    }
}