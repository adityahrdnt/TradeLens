using TradeLens.Application.Exceptions;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Services;

public sealed class PortfolioAccessService : IPortfolioAccessService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public PortfolioAccessService(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Portfolio> GetOwnedPortfolioAsync(
        Guid portfolioId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            portfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            throw new PortfolioNotFoundException(portfolioId);
        }

        if (portfolio.UserId != userId)
        {
            throw new PortfolioAccessDeniedException();
        }

        return portfolio;
    }
}