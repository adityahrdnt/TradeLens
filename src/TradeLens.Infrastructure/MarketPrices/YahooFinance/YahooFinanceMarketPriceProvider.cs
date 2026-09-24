using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;

namespace TradeLens.Infrastructure.MarketPrices.YahooFinance;

public sealed class YahooFinanceMarketPriceProvider
    : IMarketPriceProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<YahooFinanceMarketPriceProvider> _logger;

    public YahooFinanceMarketPriceProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<YahooFinanceMarketPriceProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<MarketPriceQuote>>
        GetLatestPricesAsync(
            IReadOnlyCollection<string> symbols,
            CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("YahooFinance");

        var quotes = new List<MarketPriceQuote>();

        foreach (var symbol in symbols)
        {
            var normalizedSymbol =
                symbol.Trim().ToUpperInvariant();

            var yahooSymbol =
                $"{normalizedSymbol}.JK";

            try
            {
                var response =
                    await client.GetAsync(
                        $"v8/finance/chart/{yahooSymbol}",
                        cancellationToken);

                response.EnsureSuccessStatusCode();

                await using var stream =
                    await response.Content.ReadAsStreamAsync(
                        cancellationToken);

                var data =
                    await JsonSerializer.DeserializeAsync<
                        YahooFinanceChartResponse>(
                        stream,
                        cancellationToken: cancellationToken);

                var result =
                    data?.Chart.Result?.FirstOrDefault();

                if (result?.Timestamp is null ||
                    result.Indicators?.Quote is null ||
                    result.Indicators.Quote.Count == 0)
                {
                    continue;
                }

                var closes =
                    result.Indicators.Quote[0].Close;

                if (closes is null)
                    continue;

                for (var i = closes.Count - 1; i >= 0; i--)
                {
                    var close = closes[i];

                    if (close is null)
                        continue;

                    if (i >= result.Timestamp.Count)
                        continue;

                    var timestamp =
                        DateTimeOffset.FromUnixTimeSeconds(
                            result.Timestamp[i]);

                    quotes.Add(
                        new MarketPriceQuote(
                            normalizedSymbol,
                            close.Value,
                            timestamp,
                            "YahooFinance"));

                    break;
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to retrieve market price for symbol {Symbol}.",
                    normalizedSymbol);
            }
        }

        return quotes;
    }
}