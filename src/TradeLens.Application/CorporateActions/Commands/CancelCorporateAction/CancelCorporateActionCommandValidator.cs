using FluentValidation;

namespace TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;

public sealed class CancelCorporateActionCommandValidator
    : AbstractValidator<CancelCorporateActionCommand>
{
    public CancelCorporateActionCommandValidator()
    {
        RuleFor(x => x.CorporateActionId)
            .NotEmpty();

        RuleFor(x => x.CancelledBy)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
