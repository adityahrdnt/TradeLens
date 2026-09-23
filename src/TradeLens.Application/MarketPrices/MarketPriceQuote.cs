namespace TradeLens.Application.MarketPrices;

public sealed record MarketPriceQuote(
    string Symbol,
    decimal Price,
    DateTimeOffset PriceTimestamp,
    string Source);