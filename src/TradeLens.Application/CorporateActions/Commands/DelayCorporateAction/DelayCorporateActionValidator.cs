using FluentValidation;

namespace TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;

public sealed class DelayCorporateActionValidator
    : AbstractValidator<DelayCorporateActionCommand>
{
    public DelayCorporateActionValidator()
    {
        RuleFor(x => x.CorporateActionId)
            .NotEmpty()
            .WithMessage("Corporate action id is required.");

        RuleFor(x => x.NewEffectiveDate)
            .NotEqual(default(DateOnly))
            .WithMessage("New effective date is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Delay reason is required.")
            .MaximumLength(500)
            .WithMessage("Delay reason must not exceed 500 characters.");

        RuleFor(x => x.DelayedBy)
            .NotEmpty()
            .WithMessage("Corporate action delayed by is required.");
    }
}
