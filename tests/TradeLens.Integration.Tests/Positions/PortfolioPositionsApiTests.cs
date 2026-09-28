using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Positions;

public sealed class PortfolioPositionsApiTests
{
    [Fact]
    public async Task GetPositions_WhenPortfolioBelongsToCurrentUser_ShouldReturnPositions()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        var instrumentId1 =
            Guid.Parse("00000000-0000-0000-0000-000000000001");

        var instrumentId2 =
            Guid.Parse("00000000-0000-0000-0000-000000000002");

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Positions Integration Test");

            dbContext.Portfolios.Add(portfolio);

            var position1 = new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId1,
                100,
                1_000_000m,
                10_000m,
                1,
                DateTimeOffset.UtcNow);

            var position2 = new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId2,
                50,
                600_000m,
                12_000m,
                1,
                DateTimeOffset.UtcNow);

            dbContext.Positions.AddRange(
                position1,
                position2);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioPositionsResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            portfolioId,
            result!.PortfolioId);

        Assert.Equal(
            1,
            result.Page);

        Assert.Equal(
            20,
            result.PageSize);

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            2,
            result.Items.Count);

        Assert.Equal(
            instrumentId1,
            result.Items[0].InstrumentId);

        Assert.Equal(
            100,
            result.Items[0].Quantity);

        Assert.Equal(
            1_000_000m,
            result.Items[0].CostBasis);

        Assert.Equal(
            10_000m,
            result.Items[0].AveragePrice);

        Assert.Equal(
            instrumentId2,
            result.Items[1].InstrumentId);

        Assert.Equal(
            50,
            result.Items[1].Quantity);

        Assert.Equal(
            600_000m,
            result.Items[1].CostBasis);

        Assert.Equal(
            12_000m,
            result.Items[1].AveragePrice);
    }

    [Fact]
    public async Task GetPositions_WhenRequestingSecondPage_ShouldReturnSecondPage()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        var instrumentId1 =
            Guid.Parse("00000000-0000-0000-0000-000000000001");

        var instrumentId2 =
            Guid.Parse("00000000-0000-0000-0000-000000000002");

        var instrumentId3 =
            Guid.Parse("00000000-0000-0000-0000-000000000003");

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Positions Pagination Test");

            dbContext.Portfolios.Add(portfolio);

            var position1 = new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId1,
                100,
                1_000_000m,
                10_000m,
                1,
                DateTimeOffset.UtcNow);

            var position2 = new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId2,
                50,
                600_000m,
                12_000m,
                1,
                DateTimeOffset.UtcNow);

            var position3 = new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId3,
                75,
                900_000m,
                12_000m,
                1,
                DateTimeOffset.UtcNow);

            dbContext.Positions.AddRange(
                position1,
                position2,
                position3);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions?page=2&pageSize=2");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioPositionsResponse>();

        Assert.NotNull(result);

        Assert.Equal(portfolioId, result!.PortfolioId);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);

        Assert.Single(result.Items);

        Assert.Equal(
            instrumentId3,
            result.Items[0].InstrumentId);

        Assert.Equal(
            75,
            result.Items[0].Quantity);

        Assert.Equal(
            900_000m,
            result.Items[0].CostBasis);

        Assert.Equal(
            12_000m,
            result.Items[0].AveragePrice);
    }

    [Fact]
    public async Task GetPositions_WhenPortfolioBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                otherUserId,
                "Another User Portfolio");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(problem);

        Assert.Equal(
            (int)HttpStatusCode.Forbidden,
            problem!.Status);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            problem.Code);
    }

    [Fact]
    public async Task GetPositions_WhenPageIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Invalid Page Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions?page=0");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(problem);

        Assert.Equal(
            (int)HttpStatusCode.BadRequest,
            problem!.Status);
    }

    [Fact]
    public async Task GetPositions_WhenPageSizeExceedsMaximum_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Invalid Page Size Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions?pageSize=101");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(problem);

        Assert.Equal(
            (int)HttpStatusCode.BadRequest,
            problem!.Status);
    }

    [Fact]
    public async Task GetPositions_WhenPortfolioHasNoPositions_ShouldReturnEmptyResult()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Empty Positions Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/positions");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioPositionsResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            portfolioId,
            result!.PortfolioId);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);

        Assert.Empty(result.Items);
    }

    private sealed record PortfolioPositionsResponse(
        Guid PortfolioId,
        IReadOnlyList<PositionResponse> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record PositionResponse(
        Guid PositionId,
        Guid InstrumentId,
        long Quantity,
        decimal CostBasis,
        decimal AveragePrice);

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);
}