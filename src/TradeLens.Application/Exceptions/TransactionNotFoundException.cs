namespace TradeLens.Application.Exceptions;

public sealed class TransactionNotFoundException
    : Exception
{
    public TransactionNotFoundException(Guid transactionId)
        : base($"Transaction '{transactionId}' was not found.")
    {
    }
}