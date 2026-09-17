using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Api.Errors;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Application.Transactions.Commands.CorrectTransaction;
using TradeLens.Application.Transactions.Queries.GetTransaction;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly AddTransactionService _addTransactionService;
    private readonly CorrectTransactionService _correctTransactionService;
    private readonly GetTransactionService _getTransactionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AddTransactionCommand> _validator;

    public TransactionsController(
        AddTransactionService addTransactionService,
        CorrectTransactionService correctTransactionService,
        ICurrentUserService currentUserService,
        GetTransactionService getTransactionService,
        IValidator<AddTransactionCommand> validator)
    {
        _addTransactionService = addTransactionService;
        _correctTransactionService = correctTransactionService;
        _currentUserService = currentUserService;
        _getTransactionService = getTransactionService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] AddTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new IdempotencyKeyRequiredException();
        }

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
            _currentUserService.UserId,
            idempotencyKey ?? string.Empty);

        var validationResult =
            await _validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result = await _addTransactionService.ExecuteAsync(
            command,
            cancellationToken);
        
        var response = new AddTransactionResponse(
            result.TransactionId,
            result.PositionId,
            result.PositionQuantity,
            result.PositionCostBasis,
            result.PositionAveragePrice);

        return CreatedAtAction(
            nameof(GetTransaction),
            new { id = result.TransactionId },
            response);
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

    [HttpPost("{id:guid}/correction")]
    public async Task<ActionResult<CorrectTransactionResponse>> Correct(
        Guid id,
        [FromBody] CorrectTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CorrectTransactionCommand(
            id,
            request.Quantity,
            request.Price,
            request.Fee,
            request.TransactionDate,
            request.Sequence,
            _currentUserService.UserId,
            request.Reason);

        var result = await _correctTransactionService.ExecuteAsync(
            command,
            cancellationToken);

        var response = new CorrectTransactionResponse(
            result.OriginalTransactionId,
            result.CorrectedTransactionId,
            result.PositionId,
            result.PositionQuantity,
            result.PositionCostBasis,
            result.PositionAveragePrice);

        return Ok(response);
    }
}