using FluentAssertions;
using TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;

namespace TradeLens.Application.Tests.Transactions.Queries.GetPortfolioTransactions;

public sealed class GetPortfolioTransactionsQueryValidatorTests
{
    private readonly GetPortfolioTransactionsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldPass()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPageIsZero_ShouldFail()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            0,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageIsNegative_ShouldFail()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            -1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIsZero_ShouldFail()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            1,
            0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIsGreaterThan100_ShouldFail()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            1,
            101);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenPageSizeIs100_ShouldPass()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.NewGuid(),
            1,
            100);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPortfolioIdIsEmpty_ShouldFail()
    {
        var query = new GetPortfolioTransactionsQuery(
            Guid.Empty,
            1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}