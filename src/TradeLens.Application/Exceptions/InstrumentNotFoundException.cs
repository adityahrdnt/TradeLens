namespace TradeLens.Application.Exceptions;

public sealed class InstrumentNotFoundException : Exception
{
    public InstrumentNotFoundException(Guid instrumentId)
        : base($"Instrument '{instrumentId}' was not found.")
    {
    }
}