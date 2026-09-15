namespace TradeLens.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}