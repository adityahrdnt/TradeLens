using FluentAssertions;
using TradeLens.Application.Pnl.Queries.GetPortfolioPnl;

namespace TradeLens.Application.Tests.Pnl.Queries.GetPortfolioPnl;

public sealed class GetPortfolioPnlQueryValidatorTests
{
    private readonly GetPortfolioPnlQueryValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenPortfolioIdIsValid_ReturnsValid()
    {
        var query = new GetPortfolioPnlQuery(
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPortfolioIdIsEmpty_ReturnsInvalid()
    {
        var query = new GetPortfolioPnlQuery(
            Guid.Empty);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle(
            x => x.PropertyName == nameof(query.PortfolioId)
                 && x.ErrorMessage == "Portfolio id is required.");
    }
}