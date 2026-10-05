using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;

public sealed class CancelCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        IUnitOfWork unitOfWork)
    {
        _corporateActionRepository = corporateActionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CancelCorporateActionResult> ExecuteAsync(
        CancelCorporateActionCommand command,
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

                corporateAction.Cancel(
                    now,
                    command.CancelledBy,
                    command.Reason);

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                return new CancelCorporateActionResult(
                    corporateAction.Id);
            },
            cancellationToken);
    }
}
