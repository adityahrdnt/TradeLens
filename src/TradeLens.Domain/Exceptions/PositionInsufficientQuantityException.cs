namespace TradeLens.Domain.Exceptions;

public sealed class PositionInsufficientQuantityException
    : DomainException
{
    public PositionInsufficientQuantityException()
        : base("Sell quantity cannot exceed current position.")
    {
    }
}