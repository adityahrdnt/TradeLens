using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionService(
        ITransactionRepository transactionRepository,
        IPortfolioRepository portfolioRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _portfolioRepository = portfolioRepository;
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

        var portfolio =
            await _portfolioRepository.GetByIdAsync(
                transaction.PortfolioId,
                cancellationToken);

        if (portfolio is null)
        {
            throw new PortfolioNotFoundException(
                transaction.PortfolioId);
        }

        if (portfolio.UserId != _currentUserService.UserId)
        {
            throw new PortfolioAccessDeniedException();
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