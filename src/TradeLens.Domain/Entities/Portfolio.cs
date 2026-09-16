using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    private Portfolio()
    {
    }

    public Portfolio(
        Guid id,
        Guid userId,
        string name)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(
                "Portfolio id is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "Portfolio user id is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "Portfolio name is required.");
        }

        Id = id;
        UserId = userId;
        Name = name.Trim();
    }
}