using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;
using TradeLens.Application.Interfaces;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate-actions")]
public sealed class CorporateActionsController : ControllerBase
{
    private readonly CreateCorporateActionService _createCorporateActionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateCorporateActionCommand> _validator;

    public CorporateActionsController(
        CreateCorporateActionService createCorporateActionService,
        ICurrentUserService currentUserService,
        IValidator<CreateCorporateActionCommand> validator)
    {
        _createCorporateActionService = createCorporateActionService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCorporateActionResponse>> Create(
        [FromBody] CreateCorporateActionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCorporateActionCommand(
            request.InstrumentId,
            request.Type,
            request.Numerator,
            request.Denominator,
            request.RecordDate,
            request.ExDate,
            request.EffectiveDate,
            _currentUserService.UserId);

        var validationResult =
            await _validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result =
            await _createCorporateActionService.ExecuteAsync(
                command,
                cancellationToken);

        var response = new CreateCorporateActionResponse(
            result.CorporateActionId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CorporateActionId },
            response);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        throw new NotImplementedException();
    }
}