using FluentValidation;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;

namespace TradeLens.Application.Validators;

public sealed class CreateCorporateActionCommandValidator
    : AbstractValidator<CreateCorporateActionCommand>
{
    public CreateCorporateActionCommandValidator()
    {
        RuleFor(x => x.InstrumentId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Numerator)
            .GreaterThan(0);

        RuleFor(x => x.Denominator)
            .GreaterThan(0);

        RuleFor(x => x.RecordDate)
            .NotEmpty();

        RuleFor(x => x.ExDate)
            .NotEmpty();

        RuleFor(x => x.EffectiveDate)
            .NotEmpty();

        RuleFor(x => x.CreatedBy)
            .NotEmpty();
    }
}