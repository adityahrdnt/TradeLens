public sealed class IdempotencyKeyRequiredException : Exception
{
    public IdempotencyKeyRequiredException()
        : base("The Idempotency-Key header is required.")
    {
    }
}