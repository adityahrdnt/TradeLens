using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TradeLens.Integration.Tests.Infrastructure;

public sealed class CorrelationIdApiTests
{
    [Fact]
    public async Task Request_WithCorrelationId_ReturnsSameCorrelationId()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        const string correlationId =
            "integration-test-correlation-id";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/portfolios");

        request.Headers.Add(
            "X-Correlation-ID",
            correlationId);

        using var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.True(
            response.Headers.TryGetValues(
                "X-Correlation-ID",
                out var values));

        Assert.Equal(
            correlationId,
            values.Single());
    }

    [Fact]
    public async Task Request_WithoutCorrelationId_GeneratesAndReturnsCorrelationId()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        using var response =
            await client.GetAsync("/api/v1/portfolios");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.True(
            response.Headers.TryGetValues(
                "X-Correlation-ID",
                out var values));

        var correlationId = values.Single();

        Assert.False(
            string.IsNullOrWhiteSpace(correlationId));

        Assert.True(
            Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task BadRequest_WithCorrelationId_ReturnsCorrelationIdInProblemDetails()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        const string correlationId =
            "integration-test-error-correlation-id";

        var request = new
        {
            instrumentId = Guid.NewGuid(),
            type = "StockSplit",
            numerator = 0,
            denominator = 1,
            recordDate = new DateOnly(2026, 9, 10),
            exDate = new DateOnly(2026, 9, 11),
            effectiveDate = new DateOnly(2026, 9, 12)
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/corporate-actions");

        httpRequest.Headers.Add(
            "X-Correlation-ID",
            correlationId);

        httpRequest.Content = JsonContent.Create(request);

        using var response =
            await client.SendAsync(httpRequest);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.True(
            response.Headers.TryGetValues(
                "X-Correlation-ID",
                out var headerValues));

        Assert.Equal(
            correlationId,
            headerValues.Single());

        Assert.True(
            problemDetails.TryGetProperty(
                "correlationId",
                out var correlationIdProperty));

        Assert.Equal(
            correlationId,
            correlationIdProperty.GetString());

        Assert.True(
            problemDetails.TryGetProperty(
                "traceId",
                out var traceIdProperty));

        Assert.False(
            string.IsNullOrWhiteSpace(
                traceIdProperty.GetString()));
    }
}
