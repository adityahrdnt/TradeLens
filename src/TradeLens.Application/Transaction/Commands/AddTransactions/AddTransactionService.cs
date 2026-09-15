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

    //TODO : public async Task<AddTransactionResult> ExecuteAsync(
    public Task<AddTransactionResult> ExecuteAsync(
        AddTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}