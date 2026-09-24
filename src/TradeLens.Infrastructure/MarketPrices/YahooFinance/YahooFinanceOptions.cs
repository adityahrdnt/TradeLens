namespace TradeLens.Infrastructure.MarketPrices.YahooFinance;

public sealed class YahooFinanceOptions
{
    public const string SectionName = "YahooFinance";

    public string BaseUrl { get; init; } = string.Empty;
}