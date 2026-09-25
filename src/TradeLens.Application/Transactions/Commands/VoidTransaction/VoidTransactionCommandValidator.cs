using FluentValidation;

namespace TradeLens.Application.Transactions.Commands.VoidTransaction;

public sealed class VoidTransactionCommandValidator
    : AbstractValidator<VoidTransactionCommand>
{
    private const int MaxReasonLength = 500;

    public VoidTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();

        RuleFor(x => x.VoidedBy)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(MaxReasonLength);
    }
}