using FluentValidation;

namespace TradeLens.Application.Portfolios.Queries.GetPortfolios;

public sealed class GetPortfoliosQueryValidator
    : AbstractValidator<GetPortfoliosQuery>
{
    private const int MaxPageSize = 100;

    public GetPortfoliosQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}