namespace TradeLens.Api.BackgroundServices;

public sealed class MarketPriceOptions
{
    public const string SectionName = "MarketPrice";

    public int SyncIntervalMinutes { get; init; } = 5;
}