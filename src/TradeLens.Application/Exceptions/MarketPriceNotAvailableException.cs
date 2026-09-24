namespace TradeLens.Application.Exceptions;

public sealed class MarketPriceNotAvailableException
    : Exception
{
    public MarketPriceNotAvailableException()
        : base("Market price is not available.")
    {
    }
}