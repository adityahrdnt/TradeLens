namespace TradeLens.Application.Exceptions;

public sealed class PositionNotFoundException : Exception
{
    public PositionNotFoundException()
        : base("Position was not found.")
    {
    }
}