using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Pnl.Queries.GetPortfolioPnl;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios")]
public sealed class PortfolioPnlController : ControllerBase
{
    private readonly GetPortfolioPnlService _getPortfolioPnlService;
    private readonly GetPortfolioPnlQueryValidator _validator;

    public PortfolioPnlController(
        GetPortfolioPnlService getPortfolioPnlService,
        GetPortfolioPnlQueryValidator validator)
    {
        _getPortfolioPnlService = getPortfolioPnlService;
        _validator = validator;
    }

    [HttpGet("{portfolioId:guid}/pnl")]
    public async Task<IActionResult> GetPortfolioPnl(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetPortfolioPnlQuery(portfolioId);

        var validationResult =
            await _validator.ValidateAsync(
                query,
                cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(
                validationResult.Errors);

        var result =
            await _getPortfolioPnlService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}