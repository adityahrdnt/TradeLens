using Microsoft.Extensions.Logging;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;

public sealed class DelayCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly ICorporateActionChangeRepository _changeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DelayCorporateActionService> _logger;

    public DelayCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        ICorporateActionChangeRepository changeRepository,
        IUnitOfWork unitOfWork,
        ILogger<DelayCorporateActionService> logger)
    {
        _corporateActionRepository = corporateActionRepository;
        _changeRepository = changeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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

                var change = CorporateActionChange.CreateDelay(
                    Guid.NewGuid(),
                    corporateAction.Id,
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

                _logger.LogInformation(
                    "Corporate action delayed. CorporateActionId: {CorporateActionId}, PreviousEffectiveDate: {PreviousEffectiveDate}, NewEffectiveDate: {NewEffectiveDate}",
                    corporateAction.Id,
                    previousEffectiveDate,
                    newEffectiveDate);

                return new DelayCorporateActionResult(
                    corporateAction.Id);
            },
            cancellationToken);
    }
}
