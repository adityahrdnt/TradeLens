using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Valuation.Queries.GetPositionValuation;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios/{portfolioId:guid}/positions")]
public sealed class PositionsController : ControllerBase
{
    private readonly GetPositionValuationService _getPositionValuationService;

    public PositionsController(
        GetPositionValuationService getPositionValuationService)
    {
        _getPositionValuationService = getPositionValuationService;
    }

    [HttpGet("{instrumentId:guid}/valuation")]
    public async Task<IActionResult> GetValuation(
        Guid portfolioId,
        Guid instrumentId,
        CancellationToken cancellationToken)
    {
        var query = new GetPositionValuationQuery(
            portfolioId,
            instrumentId);

        var result =
            await _getPositionValuationService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}