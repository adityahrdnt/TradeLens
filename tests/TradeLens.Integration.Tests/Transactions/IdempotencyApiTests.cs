using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Api.Errors;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public class IdempotencyApiTests
{
    [Fact]
    public async Task PostTransaction_WithSameIdempotencyKeyConcurrently_ShouldCreateOnlyOneTransaction()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        var client1 = factory.CreateClient();
        var client2 = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var idempotencyKey = Guid.NewGuid().ToString();

        var request = new
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

        async Task<HttpResponseMessage> SendRequestAsync(
            HttpClient client)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/v1/transactions")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Add(
                "Idempotency-Key",
                idempotencyKey);

            return await client.SendAsync(httpRequest);
        }

        // Act
        var responses = await Task.WhenAll(
            SendRequestAsync(client1),
            SendRequestAsync(client2));

        // Assert
        Assert.All(
            responses,
            response =>
                Assert.Equal(
                    HttpStatusCode.Created,
                    response.StatusCode));

        var results = new List<AddTransactionResponse>();

        foreach (var response in responses)
        {
            var result = await response.Content
                .ReadFromJsonAsync<AddTransactionResponse>();

            Assert.NotNull(result);
            results.Add(result!);
        }

        Assert.Equal(
            results[0].TransactionId,
            results[1].TransactionId);

        // Verify database
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<TradeLensDbContext>();

        var transactions = await dbContext.Transactions
            .Where(x =>
                x.PortfolioId ==
                CustomWebApplicationFactory.TestPortfolioId &&
                x.InstrumentId == instrumentId)
            .ToListAsync();

        var idempotencyRecords = await dbContext.IdempotencyRecords
            .Where(x =>
                x.UserId ==
                CustomWebApplicationFactory.TestUserId &&
                x.IdempotencyKey == idempotencyKey)
            .ToListAsync();

        Assert.Single(transactions);
        Assert.Single(idempotencyRecords);
    }

    [Fact]
    public async Task PostTransaction_WithSameIdempotencyKeyButDifferentPayloadConcurrently_ShouldReturnConflict()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var client1 = factory.CreateClient();
        var client2 = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var idempotencyKey = Guid.NewGuid().ToString();

        var request1 = new
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

        var request2 = new
        {
            portfolioId = CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId,
            instrumentId,
            type = "Buy",
            quantity = 200,
            price = 10000m,
            fee = 200000m,
            transactionDate,
            sequence = 1
        };

        async Task<HttpResponseMessage> SendRequestAsync(
            HttpClient client,
            object request)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/v1/transactions")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Add(
                "Idempotency-Key",
                idempotencyKey);

            return await client.SendAsync(httpRequest);
        }

        var responses = await Task.WhenAll(
            SendRequestAsync(client1, request1),
            SendRequestAsync(client2, request2));

        Assert.Contains(
            responses,
            response => response.StatusCode == HttpStatusCode.Created);

        Assert.Contains(
            responses,
            response => response.StatusCode == HttpStatusCode.Conflict);

        var conflictResponse = responses.Single(
            response => response.StatusCode == HttpStatusCode.Conflict);

        var problem =
            await conflictResponse.Content
                .ReadFromJsonAsync<TradeLensProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(
            "IDEMPOTENCY_CONFLICT",
            problem!.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

        var transactions = await dbContext.Transactions
            .Where(x =>
                x.PortfolioId ==
                    CustomWebApplicationFactory.TestPortfolioId &&
                x.InstrumentId == instrumentId)
            .ToListAsync();

        var idempotencyRecords =
            await dbContext.IdempotencyRecords
                .Where(x =>
                    x.UserId ==
                        CustomWebApplicationFactory.TestUserId &&
                    x.IdempotencyKey == idempotencyKey)
                .ToListAsync();

        Assert.Single(transactions);
        Assert.Single(idempotencyRecords);
    }

    [Fact]
    public async Task PostTransaction_WithSameIdempotencyKeySequentially_ShouldReturnSameResult()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        var client = factory.CreateClient();

        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var idempotencyKey = Guid.NewGuid().ToString();

        var request = new
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

        using var httpRequest1 = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest1.Headers.Add(
            "Idempotency-Key",
            idempotencyKey);

        // Act - first request
        var response1 = await client.SendAsync(httpRequest1);

        using var httpRequest2 = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/transactions")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest2.Headers.Add(
            "Idempotency-Key",
            idempotencyKey);

        // Act - replay
        var response2 = await client.SendAsync(httpRequest2);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response1.StatusCode);

        Assert.Equal(
            HttpStatusCode.Created,
            response2.StatusCode);

        var result1 = await response1.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        var result2 = await response2.Content
            .ReadFromJsonAsync<AddTransactionResponse>();

        Assert.NotNull(result1);
        Assert.NotNull(result2);

        Assert.Equal(
            result1!.TransactionId,
            result2!.TransactionId);

        Assert.Equal(
            result1.PositionId,
            result2.PositionId);

        Assert.Equal(
            result1.PositionQuantity,
            result2.PositionQuantity);

        Assert.Equal(
            result1.PositionCostBasis,
            result2.PositionCostBasis);

        Assert.Equal(
            result1.PositionAveragePrice,
            result2.PositionAveragePrice);

        // Verify database
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<TradeLensDbContext>();

        var transactions = await dbContext.Transactions
            .Where(x =>
                x.PortfolioId ==
                    CustomWebApplicationFactory.TestPortfolioId &&
                x.InstrumentId == instrumentId)
            .ToListAsync();

        var idempotencyRecords = await dbContext.IdempotencyRecords
            .Where(x =>
                x.UserId ==
                    CustomWebApplicationFactory.TestUserId &&
                x.IdempotencyKey == idempotencyKey)
            .ToListAsync();

        Assert.Single(transactions);
        Assert.Single(idempotencyRecords);
    }
}