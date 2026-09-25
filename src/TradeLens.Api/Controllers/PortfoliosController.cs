using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios")]
public sealed class PortfoliosController : ControllerBase
{
    private readonly GetPortfolioValuationService
        _getPortfolioValuationService;

    public PortfoliosController(
        GetPortfolioValuationService getPortfolioValuationService)
    {
        _getPortfolioValuationService =
            getPortfolioValuationService;
    }

    [HttpGet("{portfolioId:guid}/valuation")]
    public async Task<IActionResult> GetValuation(
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var query =
            new GetPortfolioValuationQuery(
                portfolioId);

        var result =
            await _getPortfolioValuationService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}