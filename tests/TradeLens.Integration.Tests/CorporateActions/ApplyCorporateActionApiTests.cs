using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.CorporateActions;

public class ApplyCorporateActionApiTests
{
    [Fact]
    public async Task PostApplyCorporateAction_ShouldApplySplitAndUpdatePosition()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var instrument = new Instrument(
                instrumentId,
                $"TEST{Guid.NewGuid():N}"[..10],
                "Integration Test Instrument",
                "IDR");

            dbContext.Instruments.Add(instrument);

            var transaction = new Transaction(
                transactionId,
                CustomWebApplicationFactory.TestPortfolioId,
                brokerAccountId,
                instrumentId,
                TransactionType.Buy,
                1000,
                100m,
                0m,
                new DateOnly(2026, 9, 1),
                1,
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            dbContext.Transactions.Add(transaction);

            var corporateAction = new CorporateAction(
                corporateActionId,
                instrumentId,
                CorporateActionType.StockSplit,
                2,
                1,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 12),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.PostAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/apply",
            null);

        var result = await response.Content
            .ReadFromJsonAsync<ApplyCorporateActionResponse>();

        // Verify database
        CorporateAction? corporateActionResult;
        CorporateActionApplication? application;
        Position? position;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            corporateActionResult =
                await dbContext.CorporateActions
                    .FirstOrDefaultAsync(
                        x => x.Id == corporateActionId);

            application =
                await dbContext.CorporateActionApplications
                    .FirstOrDefaultAsync(
                        x =>
                            x.CorporateActionId ==
                            corporateActionId &&
                            x.PortfolioId ==
                            CustomWebApplicationFactory.TestPortfolioId);

            position =
                await dbContext.Positions
                    .FirstOrDefaultAsync(
                        x =>
                            x.PortfolioId ==
                            CustomWebApplicationFactory.TestPortfolioId &&
                            x.InstrumentId == instrumentId);
        }

        // Assert - HTTP response
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.Equal(
            corporateActionId,
            result!.CorporateActionId);

        Assert.Equal(
            1,
            result.AppliedPortfolioCount);
            
    // Assert - Corporate Action
        Assert.NotNull(corporateActionResult);

        Assert.Equal(
            CorporateActionStatus.Applied,
            corporateActionResult!.Status);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            corporateActionResult.AppliedBy);

        Assert.NotNull(
            corporateActionResult.AppliedAt);

        // Assert - Application
        Assert.NotNull(application);

        Assert.Equal(
            1000,
            application!.EligibleQuantity);

        Assert.Equal(
            2000,
            application.ResultingQuantity);

        Assert.Equal(
            CustomWebApplicationFactory.TestPortfolioId,
            application.PortfolioId);

        Assert.Equal(
            instrumentId,
            application.InstrumentId);

        // Assert - Position
        Assert.NotNull(position);

        Assert.Equal(
            2000,
            position!.Quantity);
    }

    [Fact]
    public async Task PostApplyCorporateAction_ShouldReturnBadRequest_WhenAlreadyApplied()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var instrument = new Instrument(
                instrumentId,
                $"TEST{Guid.NewGuid():N}"[..10],
                "Integration Test Instrument",
                "IDR");

            dbContext.Instruments.Add(instrument);

            var corporateAction = new CorporateAction(
                corporateActionId,
                instrumentId,
                CorporateActionType.StockSplit,
                2,
                1,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 12),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            corporateAction.Apply(
                DateTimeOffset.UtcNow,
                CustomWebApplicationFactory.TestUserId);

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.PostAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/apply",
            null);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostApplyCorporateAction_ShouldReturnNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/apply",
            null);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}