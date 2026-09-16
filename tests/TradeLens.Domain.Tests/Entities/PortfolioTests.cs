using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public class PortfolioTests
{
    [Fact]
    public void Should_Create_Portfolio()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var portfolio = new Portfolio(
            id,
            userId,
            "My Portfolio");

        portfolio.Id.Should().Be(id);
        portfolio.UserId.Should().Be(userId);
        portfolio.Name.Should().Be("My Portfolio");
    }

    [Fact]
    public void Should_Reject_Empty_UserId()
    {
        var action = () => new Portfolio(
            Guid.NewGuid(),
            Guid.Empty,
            "My Portfolio");

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Portfolio user id is required.");
    }

    [Fact]
    public void Should_Reject_Empty_Name()
    {
        var action = () => new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "");

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Portfolio name is required.");
    }

    [Fact]
    public void Should_Trim_Name()
    {
        var portfolio = new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  My Portfolio  ");

        portfolio.Name.Should().Be("My Portfolio");
    }
}