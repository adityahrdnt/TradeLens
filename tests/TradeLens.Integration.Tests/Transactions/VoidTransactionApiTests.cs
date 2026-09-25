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

public class VoidTransactionApiTests
{
    [Fact]
    public async Task VoidTransaction_ShouldReturnOkAndUpdatePosition()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var originalRequest = new
        {
            portfolioId =
                CustomWebApplicationFactory.TestPortfolioId,
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

        var postResponse =
            await client.SendAsync(httpRequest);

        var originalResult =
            await postResponse.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(originalResult);

        // Act - Void
        var voidRequest = new
        {
            reason = "Transaction entered by mistake"
        };

        var voidResponse = await client.PostAsJsonAsync(
            $"/api/v1/transactions/{originalResult!.TransactionId}/void",
            voidRequest);

        var voidResult =
            await voidResponse.Content
                .ReadFromJsonAsync<VoidTransactionResponse>();

        // Act - GET transaction
        var getResponse = await client.GetAsync(
            $"/api/v1/transactions/{originalResult.TransactionId}");

        var transaction =
            await getResponse.Content
                .ReadFromJsonAsync<GetTransactionResult>();

        // Act - Verify database position
        Position? position;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            position = await dbContext.Positions
                .FirstOrDefaultAsync(x =>
                    x.Id == voidResult!.PositionId);
        }

        // Assert - Void response
        Assert.Equal(
            HttpStatusCode.OK,
            voidResponse.StatusCode);

        Assert.NotNull(voidResult);

        Assert.Equal(
            originalResult.TransactionId,
            voidResult.TransactionId);

        Assert.Equal(
            0,
            voidResult.PositionQuantity);

        Assert.Equal(
            0m,
            voidResult.PositionCostBasis);

        Assert.Equal(
            0m,
            voidResult.PositionAveragePrice);

        // Assert - Transaction
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        Assert.NotNull(transaction);

        Assert.Equal(
            originalResult.TransactionId,
            transaction!.Id);

        Assert.Equal(
            "Voided",
            transaction.Status);

        Assert.Equal(
            "Transaction entered by mistake",
            transaction.VoidReason);

        // Assert - Database position
        Assert.NotNull(position);

        Assert.Equal(
            CustomWebApplicationFactory.TestPortfolioId,
            position!.PortfolioId);

        Assert.Equal(
            instrumentId,
            position.InstrumentId);

        Assert.Equal(
            0,
            position.Quantity);

        Assert.Equal(
            0m,
            position.CostBasis);

        Assert.Equal(
            0m,
            position.AveragePrice);
    }

    [Fact]
    public async Task VoidTransaction_WhenReasonIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var originalRequest = new
        {
            portfolioId =
                CustomWebApplicationFactory.TestPortfolioId,
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

        var postResponse =
            await client.SendAsync(httpRequest);

        var originalResult =
            await postResponse.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(originalResult);

        var voidRequest = new
        {
            reason = ""
        };

        // Act
        var voidResponse = await client.PostAsJsonAsync(
            $"/api/v1/transactions/{originalResult!.TransactionId}/void",
            voidRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            voidResponse.StatusCode);
    }

    [Fact]
    public async Task VoidTransaction_WhenVoidingSell_ShouldRestorePosition()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        // Create BUY
        var buyRequest = new
        {
            portfolioId =
                CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId,
            instrumentId,
            type = "Buy",
            quantity = 100,
            price = 10_000m,
            fee = 100_000m,
            transactionDate,
            sequence = 1
        };

        using var buyHttpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(buyRequest)
        };

        buyHttpRequest.Headers.Add(
            "Idempotency-Key",
            Guid.NewGuid().ToString());

        var buyResponse =
            await client.SendAsync(buyHttpRequest);

        var buyResult =
            await buyResponse.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            buyResponse.StatusCode);

        Assert.NotNull(buyResult);

        // Create SELL
        var sellRequest = new
        {
            portfolioId =
                CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId,
            instrumentId,
            type = "Sell",
            quantity = 40,
            price = 12_000m,
            fee = 40_000m,
            transactionDate = transactionDate.AddDays(1),
            sequence = 2
        };

        using var sellHttpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(sellRequest)
        };

        sellHttpRequest.Headers.Add(
            "Idempotency-Key",
            Guid.NewGuid().ToString());

        var sellResponse =
            await client.SendAsync(sellHttpRequest);

        var sellResult =
            await sellResponse.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.Equal(
            HttpStatusCode.Created,
            sellResponse.StatusCode);

        Assert.NotNull(sellResult);

        // Act - Void SELL
        var voidRequest = new
        {
            reason = "Sell entered by mistake"
        };

        var voidResponse = await client.PostAsJsonAsync(
            $"/api/v1/transactions/{sellResult!.TransactionId}/void",
            voidRequest);

        var voidResult =
            await voidResponse.Content
                .ReadFromJsonAsync<VoidTransactionResponse>();

        // GET voided SELL
        var getResponse = await client.GetAsync(
            $"/api/v1/transactions/{sellResult.TransactionId}");

        var transaction =
            await getResponse.Content
                .ReadFromJsonAsync<GetTransactionResult>();

        // Verify database position
        Position? position;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            position = await dbContext.Positions
                .FirstOrDefaultAsync(x =>
                    x.Id == voidResult!.PositionId);
        }

        // Assert - Void response
        Assert.Equal(
            HttpStatusCode.OK,
            voidResponse.StatusCode);

        Assert.NotNull(voidResult);

        Assert.Equal(
            sellResult.TransactionId,
            voidResult.TransactionId);

        Assert.Equal(
            100,
            voidResult.PositionQuantity);

        Assert.Equal(
            1_100_000m,
            voidResult.PositionCostBasis);

        Assert.Equal(
            11_000m,
            voidResult.PositionAveragePrice);

        // Assert - Transaction
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        Assert.NotNull(transaction);

        Assert.Equal(
            "Voided",
            transaction!.Status);

        Assert.Equal(
            "Sell entered by mistake",
            transaction.VoidReason);

        // Assert - Database position
        Assert.NotNull(position);

        Assert.Equal(
            100,
            position!.Quantity);

        Assert.Equal(
            1_100_000m,
            position.CostBasis);

        Assert.Equal(
            11_000m,
            position.AveragePrice);
    }
}