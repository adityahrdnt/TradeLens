using FluentValidation;

namespace TradeLens.Application.CorporateActions.Commands.CorrectCorporateAction;

public sealed class CorrectCorporateActionValidator
    : AbstractValidator<CorrectCorporateActionCommand>
{
    public CorrectCorporateActionValidator()
    {
        RuleFor(x => x.CorporateActionId)
            .NotEmpty()
            .WithMessage("Corporate action id is required.");

        RuleFor(x => x.NewNumerator)
            .GreaterThan(0)
            .WithMessage("Corporate action numerator must be greater than zero.");

        RuleFor(x => x.NewDenominator)
            .GreaterThan(0)
            .WithMessage("Corporate action denominator must be greater than zero.");

        RuleFor(x => x.NewRecordDate)
            .NotEqual(default(DateOnly))
            .WithMessage("New record date is required.");

        RuleFor(x => x.NewExDate)
            .NotEqual(default(DateOnly))
            .WithMessage("New ex-date is required.");

        RuleFor(x => x.NewEffectiveDate)
            .NotEqual(default(DateOnly))
            .WithMessage("New effective date is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Correction reason is required.")
            .MaximumLength(500)
            .WithMessage("Correction reason must not exceed 500 characters.");

        RuleFor(x => x.CorrectedBy)
            .NotEmpty()
            .WithMessage("Corporate action corrected by is required.");
    }
}
