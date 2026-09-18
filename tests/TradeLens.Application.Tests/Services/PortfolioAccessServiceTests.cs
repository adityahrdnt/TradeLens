using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Services;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.Services;

public sealed class PortfolioAccessServiceTests
{
    [Fact]
    public async Task GetOwnedPortfolioAsync_WhenPortfolioBelongsToUser_ShouldReturnPortfolio()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var repository = new FakePortfolioRepository(portfolio);

        var service = new PortfolioAccessService(repository);

        // Act
        var result = await service.GetOwnedPortfolioAsync(
            portfolioId,
            userId);

        // Assert
        result.Should().BeSameAs(portfolio);
    }

    [Fact]
    public async Task GetOwnedPortfolioAsync_WhenPortfolioDoesNotExist_ShouldThrowPortfolioNotFoundException()
    {
        // Arrange
        var repository = new FakePortfolioRepository();

        var service = new PortfolioAccessService(repository);

        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var act = () => service.GetOwnedPortfolioAsync(
            portfolioId,
            userId);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioNotFoundException>()
            .WithMessage($"Portfolio '{portfolioId}' was not found.");
    }

    [Fact]
    public async Task GetOwnedPortfolioAsync_WhenPortfolioBelongsToAnotherUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            ownerId,
            "Owner Portfolio");

        var repository = new FakePortfolioRepository(portfolio);

        var service = new PortfolioAccessService(repository);

        // Act
        var act = () => service.GetOwnedPortfolioAsync(
            portfolioId,
            anotherUserId);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>()
            .WithMessage("You do not have access to this portfolio.");
    }
}