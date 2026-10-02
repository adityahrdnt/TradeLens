namespace TradeLens.Application.Exceptions;

public sealed class CorporateActionNotFoundException : Exception
{
    public CorporateActionNotFoundException(Guid corporateActionId)
        : base($"Corporate action '{corporateActionId}' was not found.")
    {
    }
}