using FluentAssertions;
using TradeLens.Application.Portfolios.Queries.GetPortfolios;

namespace TradeLens.Application.Tests.Portfolios.Queries.GetPortfolios;

public sealed class GetPortfoliosQueryValidatorTests
{
    private readonly GetPortfoliosQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldPass()
    {
        var query = new GetPortfoliosQuery(
            1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPageIsZero_ShouldFail()
    {
        var query = new GetPortfoliosQuery(
            0,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageIsNegative_ShouldFail()
    {
        var query = new GetPortfoliosQuery(
            -1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIsZero_ShouldFail()
    {
        var query = new GetPortfoliosQuery(
            1,
            0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIsGreaterThan100_ShouldFail()
    {
        var query = new GetPortfoliosQuery(
            1,
            101);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIs100_ShouldPass()
    {
        var query = new GetPortfoliosQuery(
            1,
            100);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}