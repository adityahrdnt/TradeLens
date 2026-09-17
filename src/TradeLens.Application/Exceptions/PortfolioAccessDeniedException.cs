namespace TradeLens.Application.Exceptions;

public sealed class PortfolioAccessDeniedException
    : Exception
{
    public PortfolioAccessDeniedException()
        : base("You do not have access to this portfolio.")
    {
    }
}