using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public class CorrectTransactionApiTests
{
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
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(originalRequest)
        };

        httpRequest.Headers.Add(
            "Idempotency-Key",
            Guid.NewGuid().ToString());

        var postResponse = await client.SendAsync(httpRequest);

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

    [Fact]
    public async Task CorrectTransaction_WhenQuantityIsZero_ShouldReturnBadRequest()
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

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(originalRequest)
        };

        httpRequest.Headers.Add(
            "Idempotency-Key",
            Guid.NewGuid().ToString());

        var postResponse = await client.SendAsync(httpRequest);

        var originalResult =
            await postResponse.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(originalResult);

        var correctionRequest = new
        {
            quantity = 0,
            price = 10000m,
            fee = 100000m,
            transactionDate,
            sequence = 2,
            reason = "Invalid quantity"
        };

        // Act
        var correctionResponse = await client.PostAsJsonAsync(
            $"/api/v1/transactions/{originalResult!.TransactionId}/correction",
            correctionRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            correctionResponse.StatusCode);
    }
}