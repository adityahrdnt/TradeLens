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

public class CorrectCorporateActionApiTests
{
    [Fact]
    public async Task PostCorrectCorporateAction_ShouldCorrectScheduledCorporateAction_AndCreateChangeHistory()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var originalRecordDate =
            new DateOnly(2026, 9, 10);

        var originalExDate =
            new DateOnly(2026, 9, 11);

        var originalEffectiveDate =
            new DateOnly(2026, 9, 12);

        var newRecordDate =
            new DateOnly(2026, 10, 11);

        var newExDate =
            new DateOnly(2026, 10, 14);

        var newEffectiveDate =
            new DateOnly(2026, 10, 16);

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
                originalRecordDate,
                originalExDate,
                originalEffectiveDate,
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        var request = new CorrectCorporateActionRequest(
            3,
            2,
            newRecordDate,
            newExDate,
            newEffectiveDate,
            "Issuer corrected the corporate action ratio and schedule.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/correct",
            request);

        var result = await response.Content
            .ReadFromJsonAsync<CorrectCorporateActionResponse>();

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
            CorporateActionType.StockSplit,
            corporateActionResult!.Type);

        Assert.Equal(
            instrumentId,
            corporateActionResult.InstrumentId);

        Assert.Equal(
            CorporateActionStatus.Scheduled,
            corporateActionResult.Status);

        Assert.Equal(
            3,
            corporateActionResult.Numerator);

        Assert.Equal(
            2,
            corporateActionResult.Denominator);

        Assert.Equal(
            newRecordDate,
            corporateActionResult.RecordDate);

        Assert.Equal(
            newExDate,
            corporateActionResult.ExDate);

        Assert.Equal(
            newEffectiveDate,
            corporateActionResult.EffectiveDate);

        // Original effective date must remain unchanged.
        Assert.Equal(
            originalEffectiveDate,
            corporateActionResult.OriginalEffectiveDate);

        // Assert - Change History
        Assert.NotNull(changeResult);

        Assert.Equal(
            corporateActionId,
            changeResult!.CorporateActionId);

        Assert.Equal(
            CorporateActionChangeType.Correction,
            changeResult.ChangeType);

        Assert.Equal(
            2,
            changeResult.PreviousNumerator);

        Assert.Equal(
            3,
            changeResult.NewNumerator);

        Assert.Equal(
            1,
            changeResult.PreviousDenominator);

        Assert.Equal(
            2,
            changeResult.NewDenominator);

        Assert.Equal(
            originalRecordDate,
            changeResult.PreviousRecordDate);

        Assert.Equal(
            newRecordDate,
            changeResult.NewRecordDate);

        Assert.Equal(
            originalExDate,
            changeResult.PreviousExDate);

        Assert.Equal(
            newExDate,
            changeResult.NewExDate);

        Assert.Equal(
            originalEffectiveDate,
            changeResult.PreviousEffectiveDate);

        Assert.Equal(
            newEffectiveDate,
            changeResult.NewEffectiveDate);

        Assert.Equal(
            "Issuer corrected the corporate action ratio and schedule.",
            changeResult.Reason);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            changeResult.ChangedBy);
    }

    [Fact]
    public async Task PostCorrectCorporateAction_ShouldReturnBadRequest_WhenAlreadyApplied()
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

        var request = new CorrectCorporateActionRequest(
            3,
            2,
            new DateOnly(2026, 10, 11),
            new DateOnly(2026, 10, 14),
            new DateOnly(2026, 10, 16),
            "Correction after application.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/correct",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCorrectCorporateAction_ShouldReturnNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();

        var request = new CorrectCorporateActionRequest(
            3,
            2,
            new DateOnly(2026, 10, 11),
            new DateOnly(2026, 10, 14),
            new DateOnly(2026, 10, 16),
            "Corporate action does not exist.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/correct",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCorrectCorporateAction_ShouldReturnBadRequest_WhenAlreadyCancelled()
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

        var request = new CorrectCorporateActionRequest(
            3,
            2,
            new DateOnly(2026, 10, 11),
            new DateOnly(2026, 10, 14),
            new DateOnly(2026, 10, 16),
            "Correction after cancellation.");

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/v1/corporate-actions/{corporateActionId}/correct",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
