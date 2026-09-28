using FluentValidation;

namespace TradeLens.Application.Positions.Queries.GetPortfolioPositions;

public sealed class GetPortfolioPositionsQueryValidator
    : AbstractValidator<GetPortfolioPositionsQuery>
{
    private const int MaxPageSize = 100;

    public GetPortfolioPositionsQueryValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}