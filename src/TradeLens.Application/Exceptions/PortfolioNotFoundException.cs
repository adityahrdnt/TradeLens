namespace TradeLens.Application.Exceptions;

public sealed class PortfolioNotFoundException
    : Exception
{
    public PortfolioNotFoundException(Guid portfolioId)
        : base($"Portfolio '{portfolioId}' was not found.")
    {
    }
}