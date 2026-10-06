using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;

public sealed class DelayCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly ICorporateActionChangeRepository _changeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DelayCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        ICorporateActionChangeRepository changeRepository,
        IUnitOfWork unitOfWork)
    {
        _corporateActionRepository = corporateActionRepository;
        _changeRepository = changeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DelayCorporateActionResult> ExecuteAsync(
        DelayCorporateActionCommand command,
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

                var (
                    previousEffectiveDate,
                    newEffectiveDate) =
                    corporateAction.Delay(
                        command.NewEffectiveDate);

                var change = new CorporateActionChange(
                    Guid.NewGuid(),
                    corporateAction.Id,
                    CorporateActionChangeType.Delay,
                    previousEffectiveDate,
                    newEffectiveDate,
                    command.Reason,
                    now,
                    command.DelayedBy);

                await _changeRepository.AddAsync(
                    change,
                    transactionCancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                return new DelayCorporateActionResult(
                    corporateAction.Id);
            },
            cancellationToken);
    }
}
