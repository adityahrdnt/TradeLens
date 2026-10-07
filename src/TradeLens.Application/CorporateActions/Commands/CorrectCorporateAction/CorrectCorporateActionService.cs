using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.CorporateActions.Commands.CorrectCorporateAction;

public sealed class CorrectCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly ICorporateActionChangeRepository _changeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CorrectCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        ICorporateActionChangeRepository changeRepository,
        IUnitOfWork unitOfWork)
    {
        _corporateActionRepository = corporateActionRepository;
        _changeRepository = changeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorrectCorporateActionResult> ExecuteAsync(
        CorrectCorporateActionCommand command,
        CancellationToken cancellationToken = default)
    {
        var corporateAction =
            await _corporateActionRepository.GetByIdAsync(
                command.CorporateActionId,
                cancellationToken);

        if (corporateAction is null)
        {
            throw new CorporateActionNotFoundException(
                command.CorporateActionId);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var now = DateTimeOffset.UtcNow;

                var correction =
                    corporateAction.Correct(
                        command.NewNumerator,
                        command.NewDenominator,
                        command.NewRecordDate,
                        command.NewExDate,
                        command.NewEffectiveDate);

                var change =
                    CorporateActionChange.CreateCorrection(
                        Guid.NewGuid(),
                        corporateAction.Id,
                        correction.PreviousNumerator,
                        correction.NewNumerator,
                        correction.PreviousDenominator,
                        correction.NewDenominator,
                        correction.PreviousRecordDate,
                        correction.NewRecordDate,
                        correction.PreviousExDate,
                        correction.NewExDate,
                        correction.PreviousEffectiveDate,
                        correction.NewEffectiveDate,
                        command.Reason,
                        now,
                        command.CorrectedBy);

                await _changeRepository.AddAsync(
                    change,
                    transactionCancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                return new CorrectCorporateActionResult(
                    corporateAction.Id);
            },
            cancellationToken);
    }
}
