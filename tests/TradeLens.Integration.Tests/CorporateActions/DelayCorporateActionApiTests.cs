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

public class DelayCorporateActionApiTests
{
    [Fact]
    public async Task PostDelayCorporateAction_ShouldDelayScheduledCorporateAction_AndCreateChangeHistory()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var originalEffectiveDate =
            new DateOnly(2026, 9, 12);

        var newEffectiveDate =
            new DateOnly(2026, 9, 20);

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
                originalEffectiveDate,
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        var request = new DelayCorporateActionRequest(
            newEffectiveDate,
            "Corporate action postponed by issuer.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/delay",
            request);

        var result = await response.Content
            .ReadFromJsonAsync<DelayCorporateActionResponse>();

        // Verify database
        CorporateAction? corporateActionResult;
        CorporateActionChange? changeResult;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            corporateActionResult =
                await dbContext.CorporateActions
                    .FirstOrDefaultAsync(
                        x => x.Id == corporateActionId);

            changeResult =
                await dbContext.CorporateActionChanges
                    .FirstOrDefaultAsync(
                        x => x.CorporateActionId == corporateActionId);
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
            CorporateActionStatus.Scheduled,
            corporateActionResult!.Status);

        Assert.Equal(
            originalEffectiveDate,
            corporateActionResult.OriginalEffectiveDate);

        Assert.Equal(
            newEffectiveDate,
            corporateActionResult.EffectiveDate);

        // Assert - Change History
        Assert.NotNull(changeResult);

        Assert.Equal(
            corporateActionId,
            changeResult!.CorporateActionId);

        Assert.Equal(
            CorporateActionChangeType.Delay,
            changeResult.ChangeType);

        Assert.Equal(
            originalEffectiveDate,
            changeResult.PreviousEffectiveDate);

        Assert.Equal(
            newEffectiveDate,
            changeResult.NewEffectiveDate);

        Assert.Equal(
            "Corporate action postponed by issuer.",
            changeResult.Reason);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            changeResult.ChangedBy);
    }

    [Fact]
    public async Task PostDelayCorporateAction_ShouldReturnBadRequest_WhenAlreadyApplied()
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

        var request = new DelayCorporateActionRequest(
            new DateOnly(2026, 9, 20),
            "Delay after application.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/delay",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostDelayCorporateAction_ShouldReturnNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();

        var request = new DelayCorporateActionRequest(
            new DateOnly(2026, 9, 20),
            "Corporate action does not exist.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/delay",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostDelayCorporateAction_ShouldReturnBadRequest_WhenAlreadyCancelled()
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

            corporateAction.Cancel(
                DateTimeOffset.UtcNow,
                CustomWebApplicationFactory.TestUserId,
                "Cancelled by issuer.");

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        var request = new DelayCorporateActionRequest(
            new DateOnly(2026, 9, 20),
            "Delay after cancellation.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/delay",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
