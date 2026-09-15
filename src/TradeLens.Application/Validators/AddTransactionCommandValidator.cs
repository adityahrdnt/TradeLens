using FluentValidation;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.Validators;

public sealed class AddTransactionCommandValidator
    : AbstractValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty();

        RuleFor(x => x.BrokerAccountId)
            .NotEmpty();

        RuleFor(x => x.InstrumentId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TransactionDate)
            .NotEmpty();

        RuleFor(x => x.Sequence)
            .GreaterThan(0);

        RuleFor(x => x.CreatedBy)
            .NotEmpty();
            
    }
}