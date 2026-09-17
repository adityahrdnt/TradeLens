using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Api.Errors;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public class TransactionsApiTests
{
    [Fact]
    public async Task PostTransaction_ShouldReturnCreated()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var request = new
        {
            portfolioId = CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId = Guid.NewGuid(),
            instrumentId = Guid.NewGuid(),
            type = "Buy",
            quantity = 100,
            price = 10000m,
            fee = 100000m,
            transactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            sequence = 1
        };

        // Act - POST
        var response = await client.PostAsJsonAsync(
            "/api/v1/transactions",
            request);

        var result = await response.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        // Act - GET
        var getResponse = await client.GetAsync(
            $"/api/v1/transactions/{result!.TransactionId}");

        var transaction =
            await getResponse.Content.ReadFromJsonAsync<GetTransactionResult>();

        // Act - Verify database
        Position? position;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            position = await dbContext.Positions
                .FirstOrDefaultAsync(x =>
                    x.Id == result.PositionId);
        }

        // Assert - POST response
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.NotEqual(
            Guid.Empty,
            result.TransactionId);

        Assert.NotEqual(
            Guid.Empty,
            result.PositionId);

        Assert.Equal(
            100,
            result.PositionQuantity);

        Assert.Equal(
            1_100_000m,
            result.PositionCostBasis);

        Assert.Equal(
            11_000m,
            result.PositionAveragePrice);

        // Assert - GET response
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        Assert.NotNull(transaction);

        Assert.Equal(
            result.TransactionId,
            transaction!.Id);

        Assert.Equal(
            request.portfolioId,
            transaction.PortfolioId);

        Assert.Equal(
            request.brokerAccountId,
            transaction.BrokerAccountId);

        Assert.Equal(
            request.instrumentId,
            transaction.InstrumentId);

        Assert.Equal(
            "Buy",
            transaction.Type);

        Assert.Equal(
            100,
            transaction.Quantity);

        Assert.Equal(
            10_000m,
            transaction.Price);

        Assert.Equal(
            100_000m,
            transaction.Fee);

        Assert.Equal(
            request.transactionDate,
            transaction.TransactionDate);

        Assert.Equal(
            1,
            transaction.Sequence);

        Assert.Equal(
            "Active",
            transaction.Status);

        // Assert - Database position
        Assert.NotNull(position);

        Assert.Equal(
            CustomWebApplicationFactory.TestPortfolioId,
            position!.PortfolioId);

        Assert.Equal(
            request.instrumentId,
            position.InstrumentId);

        Assert.Equal(
            100,
            position.Quantity);

        Assert.Equal(
            1_100_000m,
            position.CostBasis);

        Assert.Equal(
            11_000m,
            position.AveragePrice);
    }

    [Fact]
    public async Task PostTransaction_WithInvalidQuantity_ShouldReturnValidationError()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var request = new
        {
            portfolioId = CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId = Guid.NewGuid(),
            instrumentId = Guid.NewGuid(),
            type = "Buy",
            quantity = 0,
            price = 10000m,
            fee = 100000m,
            transactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            sequence = 1
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/transactions",
            request);

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "VALIDATION_ERROR",
            problem!.Code);

        Assert.NotNull(problem.Errors);

        Assert.True(
            problem.Errors!.ContainsKey("quantity"));
    }

    [Fact]
    public async Task GetTransaction_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var transactionId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/transactions/{transactionId}");

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "TRANSACTION_NOT_FOUND",
            problem!.Code);
    }

    [Fact]
    public async Task GetTransaction_WhenPortfolioDoesNotBelongToUser_ShouldReturnForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var otherUserId = Guid.Parse(
            "99999999-9999-9999-9999-999999999999");

        var otherPortfolioId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var portfolio = new Portfolio(
                otherPortfolioId,
                otherUserId,
                "Other User Portfolio");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            portfolioId = otherPortfolioId,
            brokerAccountId = Guid.NewGuid(),
            instrumentId = Guid.NewGuid(),
            type = "Buy",
            quantity = 100,
            price = 10000m,
            fee = 100000m,
            transactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            sequence = 1
        };

        var postResponse = await client.PostAsJsonAsync(
            "/api/v1/transactions",
            request);

        var result = await postResponse.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.NotNull(result);

        // Act
        var response = await client.GetAsync(
            $"/api/v1/transactions/{result!.TransactionId}");

        var problem = await response.Content
            .ReadFromJsonAsync<TradeLensProblemDetails>();

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        Assert.NotNull(problem);

        Assert.Equal(
            "PORTFOLIO_ACCESS_DENIED",
            problem!.Code);
    }

    [Fact]
    public async Task CorrectTransaction_ShouldReturnOkAndUpdatePosition()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var originalRequest = new
        {
            portfolioId = CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId,
            instrumentId,
            type = "Buy",
            quantity = 100,
            price = 10000m,
            fee = 100000m,
            transactionDate,
            sequence = 1
        };

        // Create original transaction
        var postResponse = await client.PostAsJsonAsync(
            "/api/v1/transactions",
            originalRequest);

        var originalResult = await postResponse.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(originalResult);

        // Act - Correction
        var correctionRequest = new
        {
            quantity = 120,
            price = 10000m,
            fee = 100000m,
            transactionDate,
            sequence = 2,
            reason = "Corrected quantity"
        };

        var correctionResponse = await client.PostAsJsonAsync(
            $"/api/v1/transactions/{originalResult!.TransactionId}/correction",
            correctionRequest);

        var correctionResult = await correctionResponse.Content
            .ReadFromJsonAsync<CorrectTransactionResponse>();

        // Act - GET original transaction
        var originalGetResponse = await client.GetAsync(
            $"/api/v1/transactions/{originalResult.TransactionId}");

        var originalTransaction = await originalGetResponse.Content
            .ReadFromJsonAsync<GetTransactionResult>();

        // Act - GET corrected transaction
        var correctedGetResponse = await client.GetAsync(
            $"/api/v1/transactions/{correctionResult!.CorrectedTransactionId}");

        var correctedTransaction = await correctedGetResponse.Content
            .ReadFromJsonAsync<GetTransactionResult>();

        // Act - Verify database position
        Position? position;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            position = await dbContext.Positions
                .FirstOrDefaultAsync(x =>
                    x.Id == correctionResult.PositionId);
        }

        // Assert - Correction response
        Assert.Equal(
            HttpStatusCode.OK,
            correctionResponse.StatusCode);

        Assert.NotNull(correctionResult);

        Assert.Equal(
            originalResult.TransactionId,
            correctionResult.OriginalTransactionId);

        Assert.NotEqual(
            correctionResult.OriginalTransactionId,
            correctionResult.CorrectedTransactionId);

        Assert.Equal(
            120,
            correctionResult.PositionQuantity);

        Assert.Equal(
            1_300_000m,
            correctionResult.PositionCostBasis);

        Assert.Equal(
            1_300_000m / 120,
            correctionResult.PositionAveragePrice);

        // Assert - Original transaction
        Assert.Equal(
            HttpStatusCode.OK,
            originalGetResponse.StatusCode);

        Assert.NotNull(originalTransaction);

        Assert.Equal(
            "Superseded",
            originalTransaction!.Status);

        // Assert - Corrected transaction
        Assert.Equal(
            HttpStatusCode.OK,
            correctedGetResponse.StatusCode);

        Assert.NotNull(correctedTransaction);

        Assert.Equal(
            correctionResult.CorrectedTransactionId,
            correctedTransaction!.Id);

        Assert.Equal(
            "Active",
            correctedTransaction.Status);

        Assert.Equal(
            120,
            correctedTransaction.Quantity);

        Assert.Equal(
            10_000m,
            correctedTransaction.Price);

        Assert.Equal(
            100_000m,
            correctedTransaction.Fee);

        Assert.Equal(
            "Corrected quantity",
            correctedTransaction.CorrectionReason);

        Assert.Equal(
            originalResult.TransactionId,
            correctedTransaction.SupersedesTransactionId);

        // Assert - Database position
        Assert.NotNull(position);

        Assert.Equal(
            CustomWebApplicationFactory.TestPortfolioId,
            position!.PortfolioId);

        Assert.Equal(
            instrumentId,
            position.InstrumentId);

        Assert.Equal(
            120,
            position.Quantity);

        Assert.Equal(
            1_300_000m,
            position.CostBasis);

        Assert.Equal(
            10833.33333333m,
            position.AveragePrice);
    }
}