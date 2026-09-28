using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Portfolios.Queries.GetPortfolios;
using TradeLens.Application.Positions.Queries.GetPortfolioPositions;
using TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;
using TradeLens.Application.Valuation.Queries.GetPortfolioValuation;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios")]
public sealed class PortfoliosController : ControllerBase
{
    private readonly GetPortfoliosService 
        _getPortfoliosService;

    private readonly GetPortfolioValuationService
        _getPortfolioValuationService;

    private readonly GetPortfolioTransactionsService
        _getPortfolioTransactionsService;

    private readonly GetPortfolioPositionsService
        _getPortfolioPositionsService;

    private readonly IValidator<GetPortfoliosQuery> 
        _getPortfoliosQueryValidator;

    private readonly IValidator<GetPortfolioTransactionsQuery>
        _getPortfolioTransactionsQueryValidator;

    private readonly IValidator<GetPortfolioPositionsQuery>
        _getPortfolioPositionsQueryValidator;

    public PortfoliosController(
        GetPortfoliosService getPortfoliosService,
        GetPortfolioValuationService getPortfolioValuationService,
        GetPortfolioTransactionsService getPortfolioTransactionsService,
        GetPortfolioPositionsService getPortfolioPositionsService,
        IValidator<GetPortfoliosQuery> 
            getPortfoliosQueryValidator,
        IValidator<GetPortfolioTransactionsQuery>
            getPortfolioTransactionsQueryValidator,
        IValidator<GetPortfolioPositionsQuery>
            getPortfolioPositionsQueryValidator)
    {
        _getPortfoliosService =
            getPortfoliosService;

        _getPortfolioValuationService =
            getPortfolioValuationService;

        _getPortfolioTransactionsService =
            getPortfolioTransactionsService;

        _getPortfolioPositionsService =
            getPortfolioPositionsService;

        _getPortfoliosQueryValidator =
            getPortfoliosQueryValidator;

        _getPortfolioTransactionsQueryValidator =
            getPortfolioTransactionsQueryValidator;

        _getPortfolioPositionsQueryValidator =
            getPortfolioPositionsQueryValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPortfolios(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetPortfoliosQuery(
                page,
                pageSize);

        var validationResult =
            await _getPortfoliosQueryValidator.ValidateAsync(
                query,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        var result =
            await _getPortfoliosService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
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

    [HttpGet("{portfolioId:guid}/positions")]
    public async Task<IActionResult> GetPositions(
        Guid portfolioId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetPortfolioPositionsQuery(
                portfolioId,
                page,
                pageSize);

        var validationResult =
            await _getPortfolioPositionsQueryValidator.ValidateAsync(
                query,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result =
            await _getPortfolioPositionsService.ExecuteAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}