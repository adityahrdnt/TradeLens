using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Services;

namespace TradeLens.Application.Valuation.Queries.GetPositionValuation;

public sealed class GetPositionValuationService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IMarketPriceRepository _marketPriceRepository;
    private readonly IMarketPriceFreshnessPolicy _marketPriceFreshnessPolicy;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ValuationCalculator _valuationCalculator;
    private readonly ICurrentUserService _currentUserService;

    public GetPositionValuationService(
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

    public async Task<GetPositionValuationResult> ExecuteAsync(
        GetPositionValuationQuery query,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            query.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);
            
        var position =
            await _positionRepository
                .GetByPortfolioAndInstrumentAsync(
                    query.PortfolioId,
                    query.InstrumentId,
                    cancellationToken);

        if (position is null)
        {
            throw new PositionNotFoundException();
        }

        var marketPrice =
            await _marketPriceRepository.GetLatestAsync(
                query.InstrumentId,
                cancellationToken);

        if (marketPrice is null)
        {
            throw new MarketPriceNotAvailableException();
        }

        if (!_marketPriceFreshnessPolicy.IsFresh(
                marketPrice,
                DateTimeOffset.UtcNow))
        {
            throw new MarketPriceStaleException();
        }

        var valuation =
            _valuationCalculator.Calculate(
                position.Quantity,
                position.CostBasis,
                marketPrice.Price);

        return new GetPositionValuationResult(
            position.PortfolioId,
            position.InstrumentId,
            position.Quantity,
            position.CostBasis,
            position.AveragePrice,
            marketPrice.Price,
            valuation.MarketValue,
            valuation.UnrealizedPnl,
            valuation.UnrealizedPnlPercentage);
    }
}