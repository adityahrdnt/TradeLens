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

public class CancelCorporateActionApiTests
{
    [Fact]
    public async Task PostCancelCorporateAction_ShouldCancelScheduledCorporateAction()
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

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        var request = new CancelCorporateActionRequest(
            "Corporate action cancelled by issuer.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/cancel",
            request);

        var result = await response.Content
            .ReadFromJsonAsync<CancelCorporateActionResponse>();

        // Verify database
        CorporateAction? corporateActionResult;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            corporateActionResult =
                await dbContext.CorporateActions
                    .FirstOrDefaultAsync(
                        x => x.Id == corporateActionId);
        }

        // Assert - HTTP response
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.Equal(
            corporateActionId,
            result!.CorporateActionId);

        // Assert - Corporate Action
        Assert.NotNull(corporateActionResult);

        Assert.Equal(
            CorporateActionStatus.Cancelled,
            corporateActionResult!.Status);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            corporateActionResult.CancelledBy);

        Assert.NotNull(
            corporateActionResult.CancelledAt);

        Assert.Equal(
            "Corporate action cancelled by issuer.",
            corporateActionResult.CancellationReason);
    }

    [Fact]
    public async Task PostCancelCorporateAction_ShouldReturnBadRequest_WhenAlreadyApplied()
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

        var request = new CancelCorporateActionRequest(
            "Cancel already applied corporate action.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/cancel",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCancelCorporateAction_ShouldReturnNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();

        var request = new CancelCorporateActionRequest(
            "Corporate action does not exist.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/cancel",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCancelCorporateAction_ShouldReturnBadRequest_WhenReasonIsEmpty()
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

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        var request = new CancelCorporateActionRequest(
            "");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/cancel",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
