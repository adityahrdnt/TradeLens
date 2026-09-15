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

    public AddTransactionService(
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IUnitOfWork unitOfWork,
        PositionCalculator positionCalculator)
    {
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
        _positionCalculator = positionCalculator;
    }

    public async Task<AddTransactionResult> ExecuteAsync(
        AddTransactionCommand command,
        CancellationToken cancellationToken = default)
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
            await _transactionRepository.GetEffectiveTransactionsAsync(
                command.PortfolioId,
                command.InstrumentId,
                cancellationToken);

        var transactions = existingTransactions
            .Append(transaction)
            .ToList();

        var calculation = _positionCalculator.Calculate(transactions);

        await _transactionRepository.AddAsync(
            transaction,
            cancellationToken);

        var position =
            await _positionRepository.GetByPortfolioAndInstrumentAsync(
                command.PortfolioId,
                command.InstrumentId,
                cancellationToken);

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
                cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddTransactionResult(
            transaction.Id,
            position.Id,
            position.Quantity,
            position.CostBasis,
            position.AveragePrice);
    }
}