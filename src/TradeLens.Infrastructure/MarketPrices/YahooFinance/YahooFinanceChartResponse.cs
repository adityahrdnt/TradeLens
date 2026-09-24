using System.Text.Json.Serialization;

namespace TradeLens.Infrastructure.MarketPrices.YahooFinance;

public sealed class YahooFinanceChartResponse
{
    [JsonPropertyName("chart")]
    public YahooFinanceChart Chart { get; init; } = new();
}

public sealed class YahooFinanceChart
{
    [JsonPropertyName("result")]
    public List<YahooFinanceChartResult>? Result { get; init; }

    [JsonPropertyName("error")]
    public YahooFinanceError? Error { get; init; }
}

public sealed class YahooFinanceChartResult
{
    [JsonPropertyName("timestamp")]
    public List<long>? Timestamp { get; init; }

    [JsonPropertyName("indicators")]
    public YahooFinanceIndicators? Indicators { get; init; }
}

public sealed class YahooFinanceIndicators
{
    [JsonPropertyName("quote")]
    public List<YahooFinanceQuote>? Quote { get; init; }
}

public sealed class YahooFinanceQuote
{
    [JsonPropertyName("close")]
    public List<decimal?>? Close { get; init; }
}

public sealed class YahooFinanceError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}