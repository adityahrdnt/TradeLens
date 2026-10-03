using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.CorporateActions;

public class GetCorporateActionApiTests
{
    [Fact]
    public async Task GetCorporateAction_ShouldReturnOk()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(
            2026,
            9,
            10,
            8,
            30,
            0,
            TimeSpan.Zero);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var instrument = new Instrument(
                instrumentId,
                $"TEST{Guid.NewGuid():N}"[..10],
                "Integration Test Instrument",
                "IDR");

            var corporateAction = new CorporateAction(
                corporateActionId,
                instrumentId,
                TradeLens.Domain.Enums.CorporateActionType.StockSplit,
                2,
                1,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 12),
                CustomWebApplicationFactory.TestUserId,
                createdAt);

            dbContext.Instruments.Add(instrument);
            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync(
            $"/api/v1/corporate-actions/{corporateActionId}");

        var responseBody = await response.Content.ReadAsStringAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var result = JsonSerializer.Deserialize<GetCorporateActionResponse>(
            responseBody,
            jsonOptions);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.Equal(
            corporateActionId,
            result!.CorporateActionId);

        Assert.Equal(
            instrumentId,
            result.InstrumentId);

        Assert.Equal(
            "StockSplit",
            result.Type.ToString());

        Assert.Equal(
            2,
            result.Numerator);

        Assert.Equal(
            1,
            result.Denominator);

        Assert.Equal(
            new DateOnly(2026, 9, 10),
            result.RecordDate);

        Assert.Equal(
            new DateOnly(2026, 9, 11),
            result.ExDate);

        Assert.Equal(
            new DateOnly(2026, 9, 12),
            result.EffectiveDate);

        Assert.Equal(
            "Scheduled",
            result.Status.ToString());

        Assert.Equal(
            createdAt,
            result.CreatedAt);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            result.CreatedBy);

        Assert.Null(result.AppliedAt);
        Assert.Null(result.AppliedBy);
        Assert.Null(result.CancelledAt);
        Assert.Null(result.CancelledBy);
        Assert.Null(result.CancellationReason);
    }

    [Fact]
    public async Task GetCorporateAction_ShouldReturnNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var corporateActionId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(
            $"/api/v1/corporate-actions/{corporateActionId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}