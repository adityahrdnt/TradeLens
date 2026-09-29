using TradeLens.Application.Interfaces;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Pnl.Queries.GetPortfolioPnl;

public sealed class GetPortfolioPnlService
{
    private const string CompleteStatus = "COMPLETE";
    private const string PartialStatus = "PARTIAL";

    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IMarketPriceRepository _marketPriceRepository;
    private readonly IMarketPriceFreshnessPolicy _marketPriceFreshnessPolicy;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly PositionCalculator _positionCalculator;
    private readonly ValuationCalculator _valuationCalculator;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioPnlService(
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IMarketPriceRepository marketPriceRepository,
        IMarketPriceFreshnessPolicy marketPriceFreshnessPolicy,
        IPortfolioAccessService portfolioAccessService,
        PositionCalculator positionCalculator,
        ValuationCalculator valuationCalculator,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _marketPriceRepository = marketPriceRepository;
        _marketPriceFreshnessPolicy = marketPriceFreshnessPolicy;
        _portfolioAccessService = portfolioAccessService;
        _positionCalculator = positionCalculator;
        _valuationCalculator = valuationCalculator;
        _currentUserService = currentUserService;
    }

    public async Task<GetPortfolioPnlResult> ExecuteAsync(
        GetPortfolioPnlQuery query,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            query.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);

        var asOf = DateTimeOffset.UtcNow;

        var transactions =
            await _transactionRepository.GetEffectiveByPortfolioAsync(
                query.PortfolioId,
                cancellationToken);

        var realizedPnl = transactions
            .GroupBy(x => x.InstrumentId)
            .Sum(group =>
                _positionCalculator
                    .Calculate(group)
                    .RealizedPnl);

        var positions =
            await _positionRepository.GetByPortfolioAsync(
                query.PortfolioId,
                cancellationToken);

        var unrealizedPnl = 0m;
        var allPricesAreFresh = true;

        foreach (var position in positions)
        {
            var marketPrice =
                await _marketPriceRepository.GetLatestAsync(
                    position.InstrumentId,
                    cancellationToken);

            if (marketPrice is null ||
                !_marketPriceFreshnessPolicy.IsFresh(
                    marketPrice,
                    asOf))
            {
                allPricesAreFresh = false;
                continue;
            }

            var valuation =
                _valuationCalculator.Calculate(
                    position.Quantity,
                    position.CostBasis,
                    marketPrice.Price);

            unrealizedPnl += valuation.UnrealizedPnl;
        }

        if (!allPricesAreFresh)
        {
            return new GetPortfolioPnlResult(
                query.PortfolioId,
                asOf,
                PartialStatus,
                realizedPnl,
                null,
                null);
        }

        return new GetPortfolioPnlResult(
            query.PortfolioId,
            asOf,
            CompleteStatus,
            realizedPnl,
            unrealizedPnl,
            realizedPnl + unrealizedPnl);
    }
}