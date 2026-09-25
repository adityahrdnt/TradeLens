using FluentValidation;

namespace TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

public sealed class GetPortfolioTransactionsQueryValidator
    : AbstractValidator<GetPortfolioTransactionsQuery>
{
    private const int MaxPageSize = 100;

    public GetPortfolioTransactionsQueryValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}