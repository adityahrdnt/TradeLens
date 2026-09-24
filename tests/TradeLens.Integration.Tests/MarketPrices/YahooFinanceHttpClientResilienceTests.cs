using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using TradeLens.Integration.Tests.Fakes;

namespace TradeLens.Integration.Tests.MarketPrices;

public class YahooFinanceHttpClientResilienceTests
{
    [Fact]
    public async Task YahooFinanceHttpClient_ShouldRetryTransientHttpErrors()
    {
        var attempt = 0;

        var handler =
            new FakeHttpMessageHandler(_ =>
            {
                attempt++;

                if (attempt < 3)
                {
                    return (
                        HttpStatusCode.ServiceUnavailable,
                        """
                        {
                          "error": "service unavailable"
                        }
                        """);
                }

                return (
                    HttpStatusCode.OK,
                    """
                    {
                      "success": true
                    }
                    """);
            });

        var services =
            new ServiceCollection();

        services
            .AddHttpClient(
                "YahooFinance",
                client =>
                {
                    client.BaseAddress =
                        new Uri(
                            "https://query1.finance.yahoo.com/");
                })
            .ConfigurePrimaryHttpMessageHandler(
                () => handler)
            .AddStandardResilienceHandler(
                options =>
                {
                    options.Retry.MaxRetryAttempts = 2;
                    options.Retry.Delay =
                        TimeSpan.FromMilliseconds(10);
                    options.Retry.UseJitter = false;
                });

        await using var serviceProvider =
            services.BuildServiceProvider();

        var factory =
            serviceProvider
                .GetRequiredService<IHttpClientFactory>();

        var client =
            factory.CreateClient("YahooFinance");

        var response =
            await client.GetAsync(
                "v8/finance/chart/BBCA.JK");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        attempt.Should().Be(3);

        handler.RequestedUrls
            .Should()
            .HaveCount(3);
    }
}