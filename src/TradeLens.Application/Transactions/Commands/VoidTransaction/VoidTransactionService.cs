using Microsoft.Extensions.Logging;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Transactions.Commands.VoidTransaction;

public sealed class VoidTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PositionCalculator _positionCalculator;
    private readonly ILogger<VoidTransactionService> _logger;

    public VoidTransactionService(
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IPortfolioAccessService portfolioAccessService,
        IUnitOfWork unitOfWork,
        PositionCalculator positionCalculator,
        ILogger<VoidTransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _portfolioAccessService = portfolioAccessService;
        _unitOfWork = unitOfWork;
        _positionCalculator = positionCalculator;
        _logger = logger;
    }

    public async Task<VoidTransactionResult> ExecuteAsync(
        VoidTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(
                command.TransactionId,
                cancellationToken);

        if (transaction is null)
        {
            throw new TransactionNotFoundException(
                command.TransactionId);
        }

        await _portfolioAccessService.GetOwnedPortfolioAsync(
            transaction.PortfolioId,
            command.VoidedBy,
            cancellationToken);

        return await _unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var now = DateTimeOffset.UtcNow;

                transaction.Void(command.Reason);

                var effectiveTransactions =
                    await _transactionRepository
                        .GetEffectiveTransactionsAsync(
                            transaction.PortfolioId,
                            transaction.InstrumentId,
                            transactionCancellationToken);

                var calculation =
                    _positionCalculator.Calculate(
                        effectiveTransactions);

                var position =
                    await _positionRepository
                        .GetByPortfolioAndInstrumentAsync(
                            transaction.PortfolioId,
                            transaction.InstrumentId,
                            transactionCancellationToken);

                if (position is null)
                {
                    position = Position.Empty(
                        transaction.PortfolioId,
                        transaction.InstrumentId,
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
                    "Transaction voided. TransactionId: {TransactionId}, PortfolioId: {PortfolioId}, InstrumentId: {InstrumentId}",
                    transaction.Id,
                    transaction.PortfolioId,
                    transaction.InstrumentId);

                return new VoidTransactionResult(
                    transaction.Id,
                    position.Id,
                    position.Quantity,
                    position.CostBasis,
                    position.AveragePrice);
            },
            cancellationToken);
    }
}
