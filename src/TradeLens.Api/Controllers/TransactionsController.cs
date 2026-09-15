using Microsoft.AspNetCore.Mvc;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Commands.AddTransaction;

namespace TradeLens.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly AddTransactionService _addTransactionService;
    private readonly ICurrentUserService _currentUserService;

    public TransactionsController(
        AddTransactionService addTransactionService,
        ICurrentUserService currentUserService)
    {
        _addTransactionService = addTransactionService;
        _currentUserService = currentUserService;
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

        var result = await _addTransactionService.ExecuteAsync(
            command,
            cancellationToken);

        return Ok(result);
    }
}