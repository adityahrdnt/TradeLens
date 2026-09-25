using FluentValidation;

namespace TradeLens.Application.Transactions.Commands.CorrectTransaction;

public sealed class CorrectTransactionCommandValidator
    : AbstractValidator<CorrectTransactionCommand>
{
    private const int MaxReasonLength = 500;

    public CorrectTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Sequence)
            .GreaterThan(0);

        RuleFor(x => x.CorrectedBy)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(MaxReasonLength);
    }
}