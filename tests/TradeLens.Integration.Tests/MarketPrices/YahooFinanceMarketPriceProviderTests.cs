using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using FluentAssertions;
using TradeLens.Infrastructure.MarketPrices.YahooFinance;
using TradeLens.Integration.Tests.Fakes;

namespace TradeLens.Integration.Tests.MarketPrices;

public class YahooFinanceMarketPriceProviderTests
{
    [Fact]
    public async Task GetLatestPricesAsync_ShouldMapYahooFinanceResponse()
    {
        const string json = """
        {
          "chart": {
            "result": [
              {
                "timestamp": [
                  1758603600
                ],
                "indicators": {
                  "quote": [
                    {
                      "close": [
                        9125.0
                      ]
                    }
                  ]
                }
              }
            ],
            "error": null
          }
        }
        """;

        var handler =
            new FakeHttpMessageHandler(json);

        using var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri(
                        "https://query1.finance.yahoo.com/")
            };

        var httpClientFactory =
            new FakeHttpClientFactory(httpClient);

        var provider =
            new YahooFinanceMarketPriceProvider(
                httpClientFactory,
                NullLogger<YahooFinanceMarketPriceProvider>.Instance);

        var result =
            await provider.GetLatestPricesAsync(
                new[] { "BBCA" });

        result.Should().ContainSingle();

        var quote = result.Single();

        quote.Symbol.Should().Be("BBCA");
        quote.Price.Should().Be(9125.0m);
        quote.Source.Should().Be("YahooFinance");

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString()
            .Should()
            .Be(
                "https://query1.finance.yahoo.com/v8/finance/chart/BBCA.JK");
    }

    [Fact]
    public async Task GetLatestPricesAsync_ShouldUseLatestAvailableClose()
    {
        const string json = """
        {
        "chart": {
            "result": [
            {
                "timestamp": [
                1758603600,
                1758690000,
                1758776400
                ],
                "indicators": {
                "quote": [
                    {
                    "close": [
                        9000.0,
                        9125.0,
                        null
                    ]
                    }
                ]
                }
            }
            ],
            "error": null
        }
        }
        """;

        var handler =
            new FakeHttpMessageHandler(json);

        using var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri(
                        "https://query1.finance.yahoo.com/")
            };

        var httpClientFactory =
            new FakeHttpClientFactory(httpClient);

        var provider =
            new YahooFinanceMarketPriceProvider(
                httpClientFactory,
                NullLogger<YahooFinanceMarketPriceProvider>.Instance);

        var result =
            await provider.GetLatestPricesAsync(
                new[] { "BBCA" });

        result.Should().ContainSingle();

        var quote = result.Single();

        quote.Symbol.Should().Be("BBCA");
        quote.Price.Should().Be(9125.0m);
        quote.Source.Should().Be("YahooFinance");
    }

    [Fact]
    public async Task GetLatestPricesAsync_ShouldSkip_WhenYahooFinanceReturnsHttpError()
    {
        var handler =
            new FakeHttpMessageHandler(
                """
                {
                "error": "service unavailable"
                }
                """,
                System.Net.HttpStatusCode.ServiceUnavailable);

        using var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri(
                        "https://query1.finance.yahoo.com/")
            };

        var httpClientFactory =
            new FakeHttpClientFactory(httpClient);

        var provider =
            new YahooFinanceMarketPriceProvider(
                httpClientFactory,
                NullLogger<YahooFinanceMarketPriceProvider>.Instance);

        var result =
            await provider.GetLatestPricesAsync(
                new[] { "BBCA" });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestPricesAsync_ShouldContinue_WhenOneSymbolFails()
    {
        const string bbcaJson = """
        {
        "chart": {
            "result": [
            {
                "timestamp": [
                1758603600
                ],
                "indicators": {
                "quote": [
                    {
                    "close": [
                        9125.0
                    ]
                    }
                ]
                }
            }
            ],
            "error": null
        }
        }
        """;

        const string tlkmJson = """
        {
        "chart": {
            "result": [
            {
                "timestamp": [
                1758603600
                ],
                "indicators": {
                "quote": [
                    {
                    "close": [
                        2850.0
                    ]
                    }
                ]
                }
            }
            ],
            "error": null
        }
        }
        """;

        var handler =
            new FakeHttpMessageHandler(request =>
            {
                var url =
                    request.RequestUri!.ToString();

                if (url.EndsWith("/BBRI.JK"))
                {
                    return (
                        System.Net.HttpStatusCode.ServiceUnavailable,
                        """
                        {
                        "error": "service unavailable"
                        }
                        """);
                }

                if (url.EndsWith("/BBCA.JK"))
                {
                    return (
                        System.Net.HttpStatusCode.OK,
                        bbcaJson);
                }

                if (url.EndsWith("/TLKM.JK"))
                {
                    return (
                        System.Net.HttpStatusCode.OK,
                        tlkmJson);
                }

                return (
                    System.Net.HttpStatusCode.NotFound,
                    """
                    {
                    "error": "not found"
                    }
                    """);
            });

        using var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri(
                        "https://query1.finance.yahoo.com/")
            };

        var httpClientFactory =
            new FakeHttpClientFactory(httpClient);

        var provider =
            new YahooFinanceMarketPriceProvider(
                httpClientFactory,
                NullLogger<YahooFinanceMarketPriceProvider>.Instance);

        var result =
            await provider.GetLatestPricesAsync(
                new[] { "BBCA", "BBRI", "TLKM" });

        result.Should().HaveCount(2);

        result.Should().ContainSingle(
            x => x.Symbol == "BBCA" &&
                x.Price == 9125.0m);

        result.Should().ContainSingle(
            x => x.Symbol == "TLKM" &&
                x.Price == 2850.0m);

        result.Select(x => x.Symbol)
            .Should()
            .NotContain("BBRI");

        handler.RequestedUrls.Should().HaveCount(3);
        handler.RequestedUrls.Should().Contain(
            x => x.EndsWith("/BBCA.JK"));
        handler.RequestedUrls.Should().Contain(
            x => x.EndsWith("/BBRI.JK"));
        handler.RequestedUrls.Should().Contain(
            x => x.EndsWith("/TLKM.JK"));
    }

    [Fact]
    public async Task GetLatestPricesAsync_ShouldSkip_WhenResponseHasNoPrice()
    {
        const string json = """
        {
        "chart": {
            "result": [
            {
                "timestamp": [
                1758603600
                ],
                "indicators": {
                "quote": [
                    {
                    "close": [
                        null
                    ]
                    }
                ]
                }
            }
            ],
            "error": null
        }
        }
        """;

        var handler =
            new FakeHttpMessageHandler(json);

        using var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri(
                        "https://query1.finance.yahoo.com/")
            };

        var httpClientFactory =
            new FakeHttpClientFactory(httpClient);

        var provider =
            new YahooFinanceMarketPriceProvider(
                httpClientFactory,
                NullLogger<YahooFinanceMarketPriceProvider>.Instance);

        var result =
            await provider.GetLatestPricesAsync(
                new[] { "BBCA" });

        result.Should().BeEmpty();
    }
}