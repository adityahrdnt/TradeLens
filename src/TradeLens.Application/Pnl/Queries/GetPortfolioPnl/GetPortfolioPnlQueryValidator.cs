using FluentValidation;

namespace TradeLens.Application.Pnl.Queries.GetPortfolioPnl;

public sealed class GetPortfolioPnlQueryValidator
    : AbstractValidator<GetPortfolioPnlQuery>
{
    public GetPortfolioPnlQueryValidator()
    {
        RuleFor(x => x.PortfolioId)
            .NotEmpty()
            .WithMessage("Portfolio id is required.");
    }
}