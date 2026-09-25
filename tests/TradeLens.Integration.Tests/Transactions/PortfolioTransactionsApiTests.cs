using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public sealed class PortfolioTransactionsApiTests
{
    [Fact]
    public async Task GetTransactions_WhenPortfolioBelongsToCurrentUser_ShouldReturnTransactions()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Transactions Integration Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var transaction1 = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            CustomWebApplicationFactory.TestUserId,
            DateTimeOffset.UtcNow.AddMinutes(-2));

        var transaction2 = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            50,
            11_000m,
            500m,
            new DateOnly(2026, 9, 16),
            1,
            CustomWebApplicationFactory.TestUserId,
            DateTimeOffset.UtcNow.AddMinutes(-1));

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Transactions.AddRange(
                transaction1,
                transaction2);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioTransactionsResponse>();

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
            transaction2.Id,
            result.Items[0].TransactionId);

        Assert.Equal(
            "Sell",
            result.Items[0].Type);

        Assert.Equal(
            50,
            result.Items[0].Quantity);

        Assert.Equal(
            11_000m,
            result.Items[0].Price);

        Assert.Equal(
            transaction1.Id,
            result.Items[1].TransactionId);

        Assert.Equal(
            "Buy",
            result.Items[1].Type);

        Assert.Equal(
            100,
            result.Items[1].Quantity);

        Assert.Equal(
            10_000m,
            result.Items[1].Price);
    }

    [Fact]
    public async Task GetTransactions_WhenRequestingSecondPage_ShouldReturnCorrectTransactions()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                CustomWebApplicationFactory.TestUserId,
                "Portfolio Transactions Pagination Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var transaction1 = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            new DateOnly(2026, 9, 17),
            1,
            CustomWebApplicationFactory.TestUserId,
            DateTimeOffset.UtcNow.AddMinutes(-3));

        var transaction2 = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            200,
            11_000m,
            0m,
            new DateOnly(2026, 9, 16),
            1,
            CustomWebApplicationFactory.TestUserId,
            DateTimeOffset.UtcNow.AddMinutes(-2));

        var transaction3 = new Transaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            300,
            12_000m,
            0m,
            new DateOnly(2026, 9, 15),
            1,
            CustomWebApplicationFactory.TestUserId,
            DateTimeOffset.UtcNow.AddMinutes(-1));

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            dbContext.Transactions.AddRange(
                transaction1,
                transaction2,
                transaction3);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions?page=2&pageSize=2");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioTransactionsResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            portfolioId,
            result!.PortfolioId);

        Assert.Equal(
            2,
            result.Page);

        Assert.Equal(
            2,
            result.PageSize);

        Assert.Equal(
            3,
            result.TotalCount);

        Assert.Single(result.Items);

        Assert.Equal(
            transaction3.Id,
            result.Items[0].TransactionId);

        Assert.Equal(
            300,
            result.Items[0].Quantity);

        Assert.Equal(
            12_000m,
            result.Items[0].Price);
    }

    [Fact]
    public async Task GetTransactions_WhenPortfolioDoesNotBelongToCurrentUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var portfolioId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                portfolioId,
                ownerUserId,
                "Unauthorized Portfolio Transactions Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            403,
            result!.Status);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            result.Code);

        Assert.Equal(
            "Portfolio Access Denied",
            result.Title);
    }

    [Fact]
    public async Task GetTransactions_WhenPageIsInvalid_ShouldReturnBadRequest()
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
                "Portfolio Transactions Validation Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions?page=0");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            400,
            result!.Status);
    }

    [Fact]
    public async Task GetTransactions_WhenPageSizeExceedsMaximum_ShouldReturnBadRequest()
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
                "Portfolio Transactions Page Size Validation Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions?pageSize=101");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<ProblemResponse>();

        Assert.NotNull(result);

        Assert.Equal(
            400,
            result!.Status);
    }

    [Fact]
    public async Task GetTransactions_WhenPortfolioHasNoTransactions_ShouldReturnEmptyResult()
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
                "Empty Portfolio Transactions Test");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/portfolios/{portfolioId}/transactions");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PortfolioTransactionsResponse>();

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
            0,
            result.TotalCount);

        Assert.Empty(result.Items);
    }

    private sealed record ProblemResponse(
        int Status,
        string Code,
        string Title);

    private sealed record PortfolioTransactionsResponse(
        Guid PortfolioId,
        IReadOnlyList<TransactionResponse> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record TransactionResponse(
        Guid TransactionId,
        Guid BrokerAccountId,
        Guid InstrumentId,
        string Type,
        long Quantity,
        decimal Price,
        decimal Fee,
        DateOnly TransactionDate,
        long Sequence,
        string Status,
        Guid CreatedBy,
        DateTimeOffset CreatedAt,
        Guid? SupersedesTransactionId,
        string? CorrectionReason);
}