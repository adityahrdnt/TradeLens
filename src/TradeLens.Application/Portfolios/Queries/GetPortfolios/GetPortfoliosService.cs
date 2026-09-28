using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Portfolios.Queries.GetPortfolios;

public sealed class GetPortfoliosService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfoliosService(
        IPortfolioRepository portfolioRepository,
        ICurrentUserService currentUserService)
    {
        _portfolioRepository = portfolioRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetPortfoliosResult> ExecuteAsync(
        GetPortfoliosQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var skip =
            (query.Page - 1) * query.PageSize;

        var portfolios =
            await _portfolioRepository.GetPagedByUserAsync(
                userId,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _portfolioRepository.CountByUserAsync(
                userId,
                cancellationToken);

        var items =
            portfolios
                .Select(portfolio =>
                    new GetPortfoliosItem(
                        portfolio.Id,
                        portfolio.Name))
                .ToList();

        return new GetPortfoliosResult(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }
}