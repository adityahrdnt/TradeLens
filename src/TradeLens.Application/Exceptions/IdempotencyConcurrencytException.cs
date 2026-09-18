namespace TradeLens.Application.Exceptions;

public sealed class IdempotencyConcurrencyException : Exception
{
    public IdempotencyConcurrencyException()
        : base("A concurrent request with the same idempotency key was detected.")
    {
    }
}