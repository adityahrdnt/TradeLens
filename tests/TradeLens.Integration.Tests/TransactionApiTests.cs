using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.Transactions;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Integration.Tests;

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

        var transaction = await getResponse.Content
            .ReadFromJsonAsync<GetTransactionResult>();

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
}