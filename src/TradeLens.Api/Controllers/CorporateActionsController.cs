using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;
using TradeLens.Application.CorporateActions.Queries.GetCorporateAction;
using TradeLens.Application.Interfaces;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate-actions")]
public sealed class CorporateActionsController : ControllerBase
{
    private readonly CreateCorporateActionService _createCorporateActionService;
    private readonly GetCorporateActionService _getCorporateActionService;
    private readonly ApplyCorporateActionService _applyCorporateActionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateCorporateActionCommand> _validator;

    public CorporateActionsController(
        CreateCorporateActionService createCorporateActionService,
        GetCorporateActionService getCorporateActionService,
        ApplyCorporateActionService applyCorporateActionService,
        ICurrentUserService currentUserService,
        IValidator<CreateCorporateActionCommand> validator)
    {
        _createCorporateActionService = createCorporateActionService;
        _getCorporateActionService = getCorporateActionService;
        _applyCorporateActionService = applyCorporateActionService;
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
    public async Task<ActionResult<GetCorporateActionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await _getCorporateActionService.ExecuteAsync(
                id,
                cancellationToken);

        var response = new GetCorporateActionResponse(
            result.CorporateActionId,
            result.InstrumentId,
            result.Type,
            result.Numerator,
            result.Denominator,
            result.RecordDate,
            result.ExDate,
            result.EffectiveDate,
            result.Status,
            result.CreatedAt,
            result.CreatedBy,
            result.AppliedAt,
            result.AppliedBy,
            result.CancelledAt,
            result.CancelledBy,
            result.CancellationReason);

        return Ok(response);
    }

    [HttpPost("{id:guid}/apply")]
    public async Task<ActionResult<ApplyCorporateActionResponse>> Apply(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ApplyCorporateActionCommand(
            id,
            _currentUserService.UserId);

        var result =
            await _applyCorporateActionService.ExecuteAsync(
                command,
                cancellationToken);

        var response = new ApplyCorporateActionResponse(
            result.CorporateActionId,
            result.AppliedPortfolioCount);

        return Ok(response);
    }
}