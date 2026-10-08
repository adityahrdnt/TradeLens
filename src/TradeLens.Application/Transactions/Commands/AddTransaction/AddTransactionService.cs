using Microsoft.Extensions.Logging;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Transactions.Commands.AddTransaction;

public sealed class AddTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PositionCalculator _positionCalculator;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransactionRequestHasher _transactionRequestHasher;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ILogger<AddTransactionService> _logger;

    public AddTransactionService(
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IPortfolioAccessService portfolioAccessService,
        IIdempotencyRepository idempotencyRepository,
        IUnitOfWork unitOfWork,
        PositionCalculator positionCalculator,
        ITransactionRequestHasher transactionRequestHasher,
        ILogger<AddTransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _portfolioAccessService = portfolioAccessService;
        _idempotencyRepository = idempotencyRepository;
        _unitOfWork = unitOfWork;
        _positionCalculator = positionCalculator;
        _transactionRequestHasher = transactionRequestHasher;
        _logger = logger;
    }

    public async Task<AddTransactionResult> ExecuteAsync(
        AddTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            command.PortfolioId,
            command.CreatedBy,
            cancellationToken);

        var requestHash = _transactionRequestHasher.ComputeHash(
            command.PortfolioId,
            command.BrokerAccountId,
            command.InstrumentId,
            command.Type.ToString(),
            command.Quantity,
            command.Price,
            command.Fee,
            command.TransactionDate,
            command.Sequence);

        var existingRecord =
            await _idempotencyRepository.GetAsync(
                command.CreatedBy,
                command.IdempotencyKey,
                cancellationToken);

        if (existingRecord is not null)
        {
            if (existingRecord.RequestHash != requestHash)
            {
                throw new IdempotencyConflictException();
            }

            return CreateResult(existingRecord);
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    var now = DateTimeOffset.UtcNow;

                    var transaction = new Transaction(
                        Guid.NewGuid(),
                        command.PortfolioId,
                        command.BrokerAccountId,
                        command.InstrumentId,
                        command.Type,
                        command.Quantity,
                        command.Price,
                        command.Fee,
                        command.TransactionDate,
                        command.Sequence,
                        command.CreatedBy,
                        now);

                    var existingTransactions =
                        await _transactionRepository
                            .GetEffectiveTransactionsAsync(
                                command.PortfolioId,
                                command.InstrumentId,
                                transactionCancellationToken);

                    var transactions = existingTransactions
                        .Append(transaction)
                        .ToList();

                    var calculation =
                        _positionCalculator.Calculate(transactions);

                    await _transactionRepository.AddAsync(
                        transaction,
                        transactionCancellationToken);

                    var position =
                        await _positionRepository
                            .GetByPortfolioAndInstrumentAsync(
                                command.PortfolioId,
                                command.InstrumentId,
                                transactionCancellationToken);

                    if (position is null)
                    {
                        position = Position.Empty(
                            command.PortfolioId,
                            command.InstrumentId,
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

                    var idempotencyRecord = new IdempotencyRecord(
                        Guid.NewGuid(),
                        command.CreatedBy,
                        command.IdempotencyKey,
                        requestHash,
                        transaction.Id,
                        position.Id,
                        position.Quantity,
                        position.CostBasis,
                        position.AveragePrice,
                        now);

                    await _idempotencyRepository.AddAsync(
                        idempotencyRecord,
                        transactionCancellationToken);

                    await _unitOfWork.SaveChangesAsync(
                        transactionCancellationToken);

                    _logger.LogInformation(
                        "Transaction created. TransactionId: {TransactionId}, PortfolioId: {PortfolioId}, InstrumentId: {InstrumentId}",
                        transaction.Id,
                        transaction.PortfolioId,
                        transaction.InstrumentId);

                    return new AddTransactionResult(
                        transaction.Id,
                        position.Id,
                        position.Quantity,
                        position.CostBasis,
                        position.AveragePrice);
                },
                cancellationToken);
        }
        catch (IdempotencyConcurrencyException)
        {
            var concurrentRecord =
                await _idempotencyRepository.GetAsync(
                    command.CreatedBy,
                    command.IdempotencyKey,
                    cancellationToken);

            if (concurrentRecord is null)
            {
                throw;
            }

            if (concurrentRecord.RequestHash != requestHash)
            {
                throw new IdempotencyConflictException();
            }

            return CreateResult(concurrentRecord);
        }
    }

    private static AddTransactionResult CreateResult(
        IdempotencyRecord record)
    {
        return new AddTransactionResult(
            record.TransactionId,
            record.PositionId,
            record.PositionQuantity,
            record.PositionCostBasis,
            record.PositionAveragePrice);
    }
}
