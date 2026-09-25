using TradeLens.Application.Interfaces;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

public sealed class GetPortfolioValuationService
{
    private const string CompleteStatus = "COMPLETE";
    private const string PartialStatus = "PARTIAL";

    private const string FreshPriceStatus = "FRESH";
    private const string StalePriceStatus = "STALE";
    private const string PriceNotAvailableStatus = "NOT_AVAILABLE";

    private readonly IPositionRepository _positionRepository;
    private readonly IMarketPriceRepository _marketPriceRepository;
    private readonly IMarketPriceFreshnessPolicy
        _marketPriceFreshnessPolicy;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ValuationCalculator _valuationCalculator;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioValuationService(
        IPositionRepository positionRepository,
        IMarketPriceRepository marketPriceRepository,
        IMarketPriceFreshnessPolicy marketPriceFreshnessPolicy,
        IPortfolioAccessService portfolioAccessService,
        ValuationCalculator valuationCalculator,
        ICurrentUserService currentUserService)
    {
        _positionRepository = positionRepository;
        _marketPriceRepository = marketPriceRepository;
        _marketPriceFreshnessPolicy = marketPriceFreshnessPolicy;
        _portfolioAccessService = portfolioAccessService;
        _valuationCalculator = valuationCalculator;
        _currentUserService = currentUserService;
    }

    public async Task<PortfolioValuationResult> ExecuteAsync(
        GetPortfolioValuationQuery query,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            query.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);

        var positions =
            await _positionRepository.GetByPortfolioAsync(
                query.PortfolioId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var positionResults =
            new List<PortfolioPositionValuationResult>();

        var totalCostBasis = 0m;
        var totalMarketValue = 0m;
        var totalUnrealizedPnl = 0m;

        var allPricesAreFresh = true;

        foreach (var position in positions)
        {
            totalCostBasis += position.CostBasis;

            var marketPrice =
                await _marketPriceRepository.GetLatestAsync(
                    position.InstrumentId,
                    cancellationToken);

            if (marketPrice is null)
            {
                allPricesAreFresh = false;

                positionResults.Add(
                    new PortfolioPositionValuationResult(
                        position.InstrumentId,
                        position.Quantity,
                        position.CostBasis,
                        position.AveragePrice,
                        null,
                        null,
                        null,
                        null,
                        PriceNotAvailableStatus));

                continue;
            }

            if (!_marketPriceFreshnessPolicy.IsFresh(
                    marketPrice,
                    now))
            {
                allPricesAreFresh = false;

                positionResults.Add(
                    new PortfolioPositionValuationResult(
                        position.InstrumentId,
                        position.Quantity,
                        position.CostBasis,
                        position.AveragePrice,
                        null,
                        null,
                        null,
                        null,
                        StalePriceStatus));

                continue;
            }

            var valuation =
                _valuationCalculator.Calculate(
                    position.Quantity,
                    position.CostBasis,
                    marketPrice.Price);

            totalMarketValue += valuation.MarketValue;
            totalUnrealizedPnl += valuation.UnrealizedPnl;

            positionResults.Add(
                new PortfolioPositionValuationResult(
                    position.InstrumentId,
                    position.Quantity,
                    position.CostBasis,
                    position.AveragePrice,
                    marketPrice.Price,
                    valuation.MarketValue,
                    valuation.UnrealizedPnl,
                    valuation.UnrealizedPnlPercentage,
                    FreshPriceStatus));
        }

        if (!allPricesAreFresh)
        {
            return new PortfolioValuationResult(
                query.PortfolioId,
                PartialStatus,
                totalCostBasis,
                null,
                null,
                null,
                positionResults);
        }

        var totalUnrealizedPnlPercentage =
            totalCostBasis == 0
                ? 0
                : totalUnrealizedPnl / totalCostBasis * 100;

        return new PortfolioValuationResult(
            query.PortfolioId,
            CompleteStatus,
            totalCostBasis,
            totalMarketValue,
            totalUnrealizedPnl,
            totalUnrealizedPnlPercentage,
            positionResults);
    }
}