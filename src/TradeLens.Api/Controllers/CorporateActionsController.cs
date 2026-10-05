using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;
using TradeLens.Application.CorporateActions.Queries.GetCorporateAction;
using TradeLens.Application.CorporateActions.Queries.GetCorporateActionsByInstrument;
using TradeLens.Application.Interfaces;

namespace TradeLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate-actions")]
public sealed class CorporateActionsController : ControllerBase
{
    private readonly CreateCorporateActionService _createCorporateActionService;
    private readonly GetCorporateActionService _getCorporateActionService;
    private readonly GetCorporateActionsByInstrumentService _getCorporateActionsByInstrumentService;
    private readonly ApplyCorporateActionService _applyCorporateActionService;
    private readonly CancelCorporateActionService _cancelCorporateActionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CancelCorporateActionCommand> _cancelValidator;
    private readonly IValidator<CreateCorporateActionCommand> _createValidator;

    public CorporateActionsController(
        CreateCorporateActionService createCorporateActionService,
        GetCorporateActionService getCorporateActionService,
        GetCorporateActionsByInstrumentService getCorporateActionsByInstrumentService,
        ApplyCorporateActionService applyCorporateActionService,
        CancelCorporateActionService cancelCorporateActionService,
        ICurrentUserService currentUserService,
        IValidator<CancelCorporateActionCommand> cancelValidator,
        IValidator<CreateCorporateActionCommand> createValidator)
    {
        _createCorporateActionService = createCorporateActionService;
        _getCorporateActionService = getCorporateActionService;
        _getCorporateActionsByInstrumentService = getCorporateActionsByInstrumentService;
        _applyCorporateActionService = applyCorporateActionService;
        _cancelCorporateActionService = cancelCorporateActionService;
        _currentUserService = currentUserService;
        _cancelValidator = cancelValidator;
        _createValidator = createValidator;
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
            await _createValidator.ValidateAsync(
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

    [HttpGet("instrument/{instrumentId:guid}")]
    public async Task<
        ActionResult<IReadOnlyList<GetCorporateActionsByInstrumentResponse>>>
        GetByInstrument(
            Guid instrumentId,
            CancellationToken cancellationToken)
    {
        var query =
            new GetCorporateActionsByInstrumentQuery(
                instrumentId);

        var results =
            await _getCorporateActionsByInstrumentService.ExecuteAsync(
                query,
                cancellationToken);

        var response =
            results
                .Select(x =>
                    new GetCorporateActionsByInstrumentResponse(
                        x.CorporateActionId,
                        x.InstrumentId,
                        x.Type,
                        x.Numerator,
                        x.Denominator,
                        x.RecordDate,
                        x.ExDate,
                        x.EffectiveDate,
                        x.Status,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.AppliedAt,
                        x.AppliedBy,
                        x.CancelledAt,
                        x.CancelledBy,
                        x.CancellationReason))
                .ToList();

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

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CancelCorporateActionResponse>> Cancel(
        Guid id,
        [FromBody] CancelCorporateActionRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new CancelCorporateActionCommand(
                id,
                _currentUserService.UserId,
                request.Reason);

        var validationResult =
            await _cancelValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result =
            await _cancelCorporateActionService.ExecuteAsync(
                command,
                cancellationToken);

        var response =
            new CancelCorporateActionResponse(
                result.CorporateActionId);

        return Ok(response);
    }
}
