using Microsoft.Extensions.Logging;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Transactions.Commands.CorrectTransaction;

public sealed class CorrectTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PositionCalculator _positionCalculator;
    private readonly ILogger<CorrectTransactionService> _logger;

    public CorrectTransactionService(
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IPortfolioAccessService portfolioAccessService,
        IUnitOfWork unitOfWork,
        PositionCalculator positionCalculator,
        ILogger<CorrectTransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _portfolioAccessService = portfolioAccessService;
        _unitOfWork = unitOfWork;
        _positionCalculator = positionCalculator;
        _logger = logger;
    }

    public async Task<CorrectTransactionResult> ExecuteAsync(
        CorrectTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var original = await _transactionRepository.GetByIdAsync(
            command.TransactionId,
            cancellationToken);

        if (original is null)
            throw new TransactionNotFoundException(command.TransactionId);

        await _portfolioAccessService.GetOwnedPortfolioAsync(
            original.PortfolioId,
            command.CorrectedBy,
            cancellationToken);

        return await _unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var now = DateTimeOffset.UtcNow;

                var corrected = Transaction.CreateCorrection(
                    Guid.NewGuid(),
                    original,
                    command.Quantity,
                    command.Price,
                    command.Fee,
                    command.TransactionDate,
                    command.Sequence,
                    command.CorrectedBy,
                    now,
                    command.Reason);

                original.Supersede(
                    now,
                    command.CorrectedBy,
                    command.Reason);

                var effectiveTransactions =
                    await _transactionRepository
                        .GetEffectiveTransactionsAsync(
                            original.PortfolioId,
                            original.InstrumentId,
                            transactionCancellationToken);

                var transactions = effectiveTransactions
                    .Append(corrected)
                    .ToList();

                var calculation =
                    _positionCalculator.Calculate(transactions);

                await _transactionRepository.AddAsync(
                    corrected,
                    transactionCancellationToken);

                var position =
                    await _positionRepository
                        .GetByPortfolioAndInstrumentAsync(
                            original.PortfolioId,
                            original.InstrumentId,
                            transactionCancellationToken);

                if (position is null)
                {
                    position = Position.Empty(
                        original.PortfolioId,
                        original.InstrumentId,
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

                await _unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                _logger.LogInformation(
                    "Transaction corrected. OriginalTransactionId: {OriginalTransactionId}, CorrectedTransactionId: {CorrectedTransactionId}, PortfolioId: {PortfolioId}, InstrumentId: {InstrumentId}",
                    original.Id,
                    corrected.Id,
                    original.PortfolioId,
                    original.InstrumentId);

                return new CorrectTransactionResult(
                    original.Id,
                    corrected.Id,
                    position.Id,
                    position.Quantity,
                    position.CostBasis,
                    position.AveragePrice);
            },
            cancellationToken);
    }
}
