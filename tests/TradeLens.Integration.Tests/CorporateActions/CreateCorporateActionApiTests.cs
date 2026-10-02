using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.CorporateActions;

public class CreateCorporateActionApiTests
{
    [Fact]
    public async Task PostCorporateAction_ShouldReturnCreated()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();

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

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            instrumentId,
            type = "StockSplit",
            numerator = 2,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 10),
            exDate = new DateOnly(2026, 9, 11),
            effectiveDate = new DateOnly(2026, 9, 12)
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/corporate-actions",
            request);

        var result = await response.Content
            .ReadFromJsonAsync<CreateCorporateActionResponse>();

        // Verify database
        CorporateAction? corporateAction;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            corporateAction = await dbContext.CorporateActions
                .FirstOrDefaultAsync(x =>
                    x.Id == result!.CorporateActionId);
        }

        // Assert - POST response
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.NotEqual(
            Guid.Empty,
            result!.CorporateActionId);

        // Assert - Database
        Assert.NotNull(corporateAction);

        Assert.Equal(
            instrumentId,
            corporateAction!.InstrumentId);

        Assert.Equal(
            "StockSplit",
            corporateAction.Type.ToString());

        Assert.Equal(
            2,
            corporateAction.Numerator);

        Assert.Equal(
            1,
            corporateAction.Denominator);

        Assert.Equal(
            new DateOnly(2026, 9, 10),
            corporateAction.RecordDate);

        Assert.Equal(
            new DateOnly(2026, 9, 11),
            corporateAction.ExDate);

        Assert.Equal(
            new DateOnly(2026, 9, 12),
            corporateAction.EffectiveDate);

        Assert.Equal(
            "Scheduled",
            corporateAction.Status.ToString());

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            corporateAction.CreatedBy);

        Assert.NotEqual(
            default,
            corporateAction.CreatedAt);
    }

    [Fact]
    public async Task PostCorporateAction_ShouldReturnBadRequest_WhenNumeratorIsZero()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();

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

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            instrumentId,
            type = "StockSplit",
            numerator = 0,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 10),
            exDate = new DateOnly(2026, 9, 11),
            effectiveDate = new DateOnly(2026, 9, 12)
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/corporate-actions",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCorporateAction_ShouldReturnBadRequest_WhenExDateIsBeforeRecordDate()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();

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

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            instrumentId,
            type = "StockSplit",
            numerator = 2,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 12),
            exDate = new DateOnly(2026, 9, 11),
            effectiveDate = new DateOnly(2026, 9, 13)
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/corporate-actions",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCorporateAction_ShouldReturnBadRequest_WhenEffectiveDateIsBeforeExDate()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();

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

            await dbContext.SaveChangesAsync();
        }

        var request = new
        {
            instrumentId,
            type = "StockSplit",
            numerator = 2,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 10),
            exDate = new DateOnly(2026, 9, 12),
            effectiveDate = new DateOnly(2026, 9, 11)
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/corporate-actions",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task PostCorporateAction_ShouldReturnNotFound_WhenInstrumentDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var request = new
        {
            instrumentId = Guid.NewGuid(),
            type = "StockSplit",
            numerator = 2,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 10),
            exDate = new DateOnly(2026, 9, 11),
            effectiveDate = new DateOnly(2026, 9, 12)
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/corporate-actions",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}