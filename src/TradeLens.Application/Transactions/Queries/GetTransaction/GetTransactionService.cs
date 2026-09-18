using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionService(
        ITransactionRepository transactionRepository,
        IPortfolioAccessService portfolioAccessService,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _portfolioAccessService = portfolioAccessService;
        _currentUserService = currentUserService;
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

        await _portfolioAccessService.GetOwnedPortfolioAsync(
            transaction.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);

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
            transaction.CreatedAt,
            transaction.SupersedesTransactionId,
            transaction.CorrectionReason);
    }
}