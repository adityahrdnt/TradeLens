namespace TradeLens.Application.Exceptions;

public sealed class MarketPriceStaleException
    : Exception
{
    public MarketPriceStaleException()
        : base("Market price is stale.")
    {
    }
}