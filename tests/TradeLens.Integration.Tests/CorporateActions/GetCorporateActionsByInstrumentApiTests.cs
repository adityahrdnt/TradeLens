using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Api.Contracts.CorporateActions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.CorporateActions;

public class GetCorporateActionsByInstrumentApiTests
{
    [Fact]
    public async Task GetCorporateActionsByInstrument_ShouldReturnOkAndOrderedList()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var otherInstrumentId = Guid.NewGuid();

        var firstCorporateActionId = Guid.NewGuid();
        var secondCorporateActionId = Guid.NewGuid();
        var otherCorporateActionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            var instrument = new Instrument(
                instrumentId,
                $"TEST{Guid.NewGuid():N}"[..10],
                "Integration Test Instrument",
                "IDR");

            var otherInstrument = new Instrument(
                otherInstrumentId,
                $"TEST{Guid.NewGuid():N}"[..10],
                "Other Integration Test Instrument",
                "IDR");

            var firstCorporateAction = new CorporateAction(
                firstCorporateActionId,
                instrumentId,
                CorporateActionType.StockSplit,
                2,
                1,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 20),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            var secondCorporateAction = new CorporateAction(
                secondCorporateActionId,
                instrumentId,
                CorporateActionType.ReverseSplit,
                1,
                2,
                new DateOnly(2026, 9, 12),
                new DateOnly(2026, 9, 13),
                new DateOnly(2026, 9, 15),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            var otherCorporateAction = new CorporateAction(
                otherCorporateActionId,
                otherInstrumentId,
                CorporateActionType.BonusShares,
                1,
                10,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 12),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            dbContext.Instruments.AddRange(
                instrument,
                otherInstrument);

            dbContext.CorporateActions.AddRange(
                firstCorporateAction,
                secondCorporateAction,
                otherCorporateAction);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync(
            $"/api/v1/corporate-actions/instrument/{instrumentId}");

        var responseBody = await response.Content.ReadAsStringAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var result =
            JsonSerializer.Deserialize<
                List<GetCorporateActionsByInstrumentResponse>>(
                responseBody,
                jsonOptions);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);

        Assert.Equal(
            2,
            result!.Count);

        Assert.Equal(
            secondCorporateActionId,
            result[0].CorporateActionId);

        Assert.Equal(
            firstCorporateActionId,
            result[1].CorporateActionId);

        Assert.All(
            result,
            item => Assert.Equal(
                instrumentId,
                item.InstrumentId));

        Assert.Equal(
            "ReverseSplit",
            result[0].Type.ToString());

        Assert.Equal(
            1,
            result[0].Numerator);

        Assert.Equal(
            2,
            result[0].Denominator);

        Assert.Equal(
            new DateOnly(2026, 9, 15),
            result[0].EffectiveDate);

        Assert.Equal(
            "StockSplit",
            result[1].Type.ToString());

        Assert.Equal(
            2,
            result[1].Numerator);

        Assert.Equal(
            1,
            result[1].Denominator);

        Assert.Equal(
            new DateOnly(2026, 9, 20),
            result[1].EffectiveDate);
    }

    [Fact]
    public async Task GetCorporateActionsByInstrument_ShouldReturnEmptyList_WhenInstrumentHasNoCorporateActions()
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

        // Act
        var response = await client.GetAsync(
            $"/api/v1/corporate-actions/instrument/{instrumentId}");

        var responseBody = await response.Content.ReadAsStringAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result =
            JsonSerializer.Deserialize<
                List<GetCorporateActionsByInstrumentResponse>>(
                responseBody,
                jsonOptions);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetCorporateActionsByInstrument_ShouldReturnLifecycleFields()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        var instrumentId = Guid.NewGuid();
        var corporateActionId = Guid.NewGuid();

        var appliedAt = new DateTimeOffset(
            2026,
            9,
            15,
            10,
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
                CorporateActionType.StockSplit,
                2,
                1,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 11),
                new DateOnly(2026, 9, 12),
                CustomWebApplicationFactory.TestUserId,
                DateTimeOffset.UtcNow);

            corporateAction.Apply(
                appliedAt,
                CustomWebApplicationFactory.TestUserId);

            dbContext.Instruments.Add(instrument);
            dbContext.CorporateActions.Add(corporateAction);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync(
            $"/api/v1/corporate-actions/instrument/{instrumentId}");

        var responseBody = await response.Content.ReadAsStringAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var result =
            JsonSerializer.Deserialize<
                List<GetCorporateActionsByInstrumentResponse>>(
                responseBody,
                jsonOptions);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);
        Assert.Single(result);

        var resultCorporateAction  = result![0];

        Assert.Equal(
            corporateActionId,
            resultCorporateAction .CorporateActionId);

        Assert.Equal(
            "Applied",
            resultCorporateAction .Status.ToString());

        Assert.Equal(
            appliedAt,
            resultCorporateAction .AppliedAt);

        Assert.Equal(
            CustomWebApplicationFactory.TestUserId,
            resultCorporateAction .AppliedBy);

        Assert.Null(resultCorporateAction .CancelledAt);
        Assert.Null(resultCorporateAction .CancelledBy);
        Assert.Null(resultCorporateAction .CancellationReason);
    }
}