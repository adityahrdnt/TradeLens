using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionService(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<GetTransactionResult> ExecuteAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(
                transactionId,
                cancellationToken);

        if (transaction is null)
        {
            throw new TransactionNotFoundException(transactionId);
        }

        return new GetTransactionResult(
            transaction.Id,
            transaction.PortfolioId,
            transaction.BrokerAccountId,
            transaction.InstrumentId,
            transaction.Type.ToString(),
            transaction.Quantity,
            transaction.Price,
            transaction.Fee,
            transaction.TransactionDate,
            transaction.Sequence,
            transaction.Status.ToString(),
            transaction.CreatedBy,
            transaction.CreatedAt);
    }
}