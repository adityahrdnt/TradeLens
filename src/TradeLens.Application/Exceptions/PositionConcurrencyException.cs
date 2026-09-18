namespace TradeLens.Application.Exceptions;

public sealed class PositionConcurrencyException : Exception
{
    public PositionConcurrencyException()
        : base("The position was modified by another request. Please retry the operation.")
    {
    }
}