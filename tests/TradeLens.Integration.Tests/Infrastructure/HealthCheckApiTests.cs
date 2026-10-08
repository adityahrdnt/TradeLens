using System.Net;
using Xunit;

namespace TradeLens.Integration.Tests.Infrastructure;

public sealed class HealthCheckApiTests
{
    [Fact]
    public async Task Health_ReturnsOk_WhenDatabaseIsHealthy()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        using var response =
            await client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Healthy",
            content);
    }

    [Fact]
    public async Task Health_DoesNotExposeConnectionString()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        await factory.SeedAsync();

        using var client = factory.CreateClient();

        using var response =
            await client.GetAsync("/health");

        var content =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            "Host=",
            content,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Password=",
            content,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Username=",
            content,
            StringComparison.OrdinalIgnoreCase);
    }
}
