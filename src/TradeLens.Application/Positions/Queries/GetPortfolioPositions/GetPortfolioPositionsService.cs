using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Positions.Queries.GetPortfolioPositions;

public sealed class GetPortfolioPositionsService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IPortfolioAccessService _portfolioAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetPortfolioPositionsService(
        IPositionRepository positionRepository,
        IPortfolioAccessService portfolioAccessService,
        ICurrentUserService currentUserService)
    {
        _positionRepository = positionRepository;
        _portfolioAccessService = portfolioAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<GetPortfolioPositionsResult> ExecuteAsync(
        GetPortfolioPositionsQuery query,
        CancellationToken cancellationToken = default)
    {
        await _portfolioAccessService.GetOwnedPortfolioAsync(
            query.PortfolioId,
            _currentUserService.UserId,
            cancellationToken);

        var skip =
            (query.Page - 1) * query.PageSize;

        var positions =
            await _positionRepository.GetPagedByPortfolioAsync(
                query.PortfolioId,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _positionRepository.CountByPortfolioAsync(
                query.PortfolioId,
                cancellationToken);

        var items =
            positions
                .Select(position =>
                    new GetPortfolioPositionsItem(
                        position.Id,
                        position.InstrumentId,
                        position.Quantity,
                        position.CostBasis,
                        position.AveragePrice))
                .ToList();

        return new GetPortfolioPositionsResult(
            query.PortfolioId,
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }
}