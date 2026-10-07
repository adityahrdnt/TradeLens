using Microsoft.Extensions.Logging;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;

namespace TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;

public sealed class ApplyCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly ICorporateActionApplicationRepository _applicationRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CorporateActionPositionCalculator _corporateActionPositionCalculator;
    private readonly CorporateActionCalculator _corporateActionCalculator;
    private readonly PositionCalculator _positionCalculator;
    private readonly ILogger<ApplyCorporateActionService> _logger;

    public ApplyCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        ICorporateActionApplicationRepository applicationRepository,
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IUnitOfWork unitOfWork,
        CorporateActionPositionCalculator corporateActionPositionCalculator,
        CorporateActionCalculator corporateActionCalculator,
        PositionCalculator positionCalculator,
        ILogger<ApplyCorporateActionService> logger)
    {
        _corporateActionRepository = corporateActionRepository;
        _applicationRepository = applicationRepository;
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
        _corporateActionPositionCalculator = corporateActionPositionCalculator;
        _corporateActionCalculator = corporateActionCalculator;
        _positionCalculator = positionCalculator;
        _logger = logger;
    }

    public async Task<ApplyCorporateActionResult> ExecuteAsync(
        ApplyCorporateActionCommand command,
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

                corporateAction.Apply(
                    now,
                    command.AppliedBy);

                var portfolioIds =
                    await _transactionRepository
                        .GetPortfolioIdsByInstrumentAsOfDateAsync(
                            corporateAction.InstrumentId,
                            corporateAction.RecordDate,
                            transactionCancellationToken);

                var appliedPortfolioCount = 0;

                foreach (var portfolioId in portfolioIds)
                {
                    var transactions =
                        await _transactionRepository
                            .GetEffectiveTransactionsAsync(
                                portfolioId,
                                corporateAction.InstrumentId,
                                transactionCancellationToken);

                    var recordDateTransactions =
                        transactions
                            .Where(x =>
                                x.TransactionDate <=
                                corporateAction.RecordDate)
                            .ToList();

                    var recordDatePosition =
                        _positionCalculator.Calculate(
                            recordDateTransactions);

                    if (recordDatePosition.Quantity <= 0)
                    {
                        continue;
                    }

                    var existingApplication =
                        await _applicationRepository
                            .GetByCorporateActionAndPortfolioAsync(
                                corporateAction.Id,
                                portfolioId,
                                transactionCancellationToken);

                    if (existingApplication is not null)
                    {
                        continue;
                    }

                    var calculation =
                        _corporateActionPositionCalculator.Calculate(
                            corporateAction,
                            transactions);

                    var resultingQuantity =
                        _corporateActionCalculator.CalculateResultingQuantity(
                            corporateAction,
                            recordDatePosition.Quantity);

                    var application =
                        new CorporateActionApplication(
                            Guid.NewGuid(),
                            corporateAction.Id,
                            portfolioId,
                            corporateAction.InstrumentId,
                            recordDatePosition.Quantity,
                            resultingQuantity,
                            now);

                    await _applicationRepository.AddAsync(
                        application,
                        transactionCancellationToken);

                    var position =
                        await _positionRepository
                            .GetByPortfolioAndInstrumentAsync(
                                portfolioId,
                                corporateAction.InstrumentId,
                                transactionCancellationToken);

                    if (position is null)
                    {
                        position = Position.Empty(
                            portfolioId,
                            corporateAction.InstrumentId,
                            now);

                        position.Apply(
                            calculation.Quantity,
                            calculation.CostBasis,
                            calculation.AveragePrice,
                            now);

                        await _positionRepository.AddAsync(
                            position,
                            transactionCancellationToken);
                    }
                    else
                    {
                        position.Apply(
                            calculation.Quantity,
                            calculation.CostBasis,
                            calculation.AveragePrice,
                            now);

                        _positionRepository.Update(position);
                    }

                    appliedPortfolioCount++;
                }

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                _logger.LogInformation(
                    "Corporate action applied. CorporateActionId: {CorporateActionId}, InstrumentId: {InstrumentId}, AppliedPortfolioCount: {AppliedPortfolioCount}",
                    corporateAction.Id,
                    corporateAction.InstrumentId,
                    appliedPortfolioCount);

                return new ApplyCorporateActionResult(
                    corporateAction.Id,
                    appliedPortfolioCount);
            },
            cancellationToken);
    }
}
