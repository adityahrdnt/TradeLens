using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;

public sealed class CreateCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        IInstrumentRepository instrumentRepository,
        IUnitOfWork unitOfWork)
    {
        _corporateActionRepository = corporateActionRepository;
        _instrumentRepository = instrumentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCorporateActionResult> ExecuteAsync(
        CreateCorporateActionCommand command,
        CancellationToken cancellationToken = default)
    {
        var instrument =
            await _instrumentRepository.GetByIdAsync(
                command.InstrumentId,
                cancellationToken);

        if (instrument is null)
        {
            throw new InstrumentNotFoundException(
                command.InstrumentId);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var now = DateTimeOffset.UtcNow;

                var corporateAction = new CorporateAction(
                    Guid.NewGuid(),
                    command.InstrumentId,
                    command.Type,
                    command.Numerator,
                    command.Denominator,
                    command.RecordDate,
                    command.ExDate,
                    command.EffectiveDate,
                    command.CreatedBy,
                    now);

                await _corporateActionRepository.AddAsync(
                    corporateAction,
                    transactionCancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                return new CreateCorporateActionResult(
                    corporateAction.Id);
            },
            cancellationToken);
    }
}