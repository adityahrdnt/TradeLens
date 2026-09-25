using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;
using TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios")]
public sealed class PortfoliosController : ControllerBase
{
    private readonly GetPortfolioValuationService
        _getPortfolioValuationService;

    private readonly GetPortfolioTransactionsService
        _getPortfolioTransactionsService;

    private readonly IValidator<GetPortfolioTransactionsQuery>
        _getPortfolioTransactionsQueryValidator;

    public PortfoliosController(
        GetPortfolioValuationService getPortfolioValuationService,
        GetPortfolioTransactionsService getPortfolioTransactionsService,
        IValidator<GetPortfolioTransactionsQuery>
            getPortfolioTransactionsQueryValidator)
    {
        _getPortfolioValuationService =
            getPortfolioValuationService;

        _getPortfolioTransactionsService =
            getPortfolioTransactionsService;

        _getPortfolioTransactionsQueryValidator =
            getPortfolioTransactionsQueryValidator;
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

    [HttpGet("{portfolioId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid portfolioId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetPortfolioTransactionsQuery(
                portfolioId,
                page,
                pageSize);

        var validationResult =
            await _getPortfolioTransactionsQueryValidator.ValidateAsync(
                query,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result =
            await _getPortfolioTransactionsService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}