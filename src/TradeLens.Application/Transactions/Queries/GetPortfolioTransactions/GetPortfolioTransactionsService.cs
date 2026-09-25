using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

public sealed class GetPortfolioTransactionsService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioTransactionsService(
        ITransactionRepository transactionRepository,
        IPortfolioAccessService portfolioAccessService,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _portfolioAccessService = portfolioAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<GetPortfolioTransactionsResult> ExecuteAsync(
        GetPortfolioTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            query.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);

        var skip =
            (query.Page - 1) * query.PageSize;

        var transactions =
            await _transactionRepository.GetByPortfolioAsync(
                query.PortfolioId,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _transactionRepository.CountByPortfolioAsync(
                query.PortfolioId,
                cancellationToken);

        var items =
            transactions
                .Select(transaction =>
                    new GetPortfolioTransactionsItem(
                        transaction.Id,
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
                        transaction.CorrectionReason))
                .ToList();

        return new GetPortfolioTransactionsResult(
            query.PortfolioId,
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }
}