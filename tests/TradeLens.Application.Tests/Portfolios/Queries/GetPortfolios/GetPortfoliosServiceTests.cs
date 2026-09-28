using FluentAssertions;
using TradeLens.Application.Portfolios.Queries.GetPortfolios;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Portfolios.Queries.GetPortfolios;

public sealed class GetPortfoliosServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCurrentUserHasPortfolios_ShouldReturnOwnedPortfolios()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var portfolio1 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Long Term");

        var portfolio2 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Trading");

        var otherUserPortfolio = new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Another User Portfolio");

        var repository =
            new FakePortfolioRepository();

        repository.Portfolios.Add(portfolio1);
        repository.Portfolios.Add(portfolio2);
        repository.Portfolios.Add(otherUserPortfolio);

        var service =
            new GetPortfoliosService(
                repository,
                new FakeCurrentUserService(userId));

        var query =
            new GetPortfoliosQuery(
                1,
                20);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        result.Items[0].PortfolioId
            .Should().Be(portfolio1.Id);

        result.Items[0].Name
            .Should().Be("Long Term");

        result.Items[1].PortfolioId
            .Should().Be(portfolio2.Id);

        result.Items[1].Name
            .Should().Be("Trading");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestingSecondPage_ShouldReturnCorrectPortfolios()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var portfolio1 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Alpha");

        var portfolio2 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Beta");

        var portfolio3 = new Portfolio(
            Guid.NewGuid(),
            userId,
            "Gamma");

        var repository =
            new FakePortfolioRepository();

        repository.Portfolios.Add(portfolio1);
        repository.Portfolios.Add(portfolio2);
        repository.Portfolios.Add(portfolio3);

        var service =
            new GetPortfoliosService(
                repository,
                new FakeCurrentUserService(userId));

        var query =
            new GetPortfoliosQuery(
                2,
                2);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);

        result.Items[0].PortfolioId
            .Should().Be(portfolio3.Id);

        result.Items[0].Name
            .Should().Be("Gamma");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentUserHasNoPortfolios_ShouldReturnEmptyResult()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var otherUserPortfolio = new Portfolio(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Another User Portfolio");

        var repository =
            new FakePortfolioRepository();

        repository.Portfolios.Add(otherUserPortfolio);

        var service =
            new GetPortfoliosService(
                repository,
                new FakeCurrentUserService(currentUserId));

        var query =
            new GetPortfoliosQuery(
                1,
                20);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }
}