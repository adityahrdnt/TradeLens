using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Api.Errors;
using TradeLens.Application.Transactions.Queries.GetTransaction;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly AddTransactionService _addTransactionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AddTransactionCommand> _validator;
    private readonly GetTransactionService _getTransactionService;

    public TransactionsController(
        AddTransactionService addTransactionService,
        ICurrentUserService currentUserService,
        IValidator<AddTransactionCommand> validator,
        GetTransactionService getTransactionService)
    {
        _addTransactionService = addTransactionService;
        _currentUserService = currentUserService;
        _validator = validator;
        _getTransactionService = getTransactionService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction(
        [FromBody] AddTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTransactionCommand(
            request.PortfolioId,
            request.BrokerAccountId,
            request.InstrumentId,
            request.Type,
            request.Quantity,
            request.Price,
            request.Fee,
            request.TransactionDate,
            request.Sequence,
            _currentUserService.UserId);

        var validationResult =
            await _validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => char.ToLowerInvariant(group.Key[0]) + group.Key[1..],
                    group => group
                        .Select(x => x.ErrorMessage)
                        .ToArray());

            var problemDetails = new TradeLensProblemDetails
            {
                Type = "https://api.tradelens.com/problems/validation",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Code = TradeLensErrorCode.ValidationError,
                Detail = "One or more validation errors occurred.",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            };

            return BadRequest(problemDetails);
        }

        return Ok(validationResult);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransaction(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getTransactionService.ExecuteAsync(
            id,
            cancellationToken);

        return Ok(result);
    }
}