namespace TradeLens.Application.Interfaces;

public interface IMarketPriceSyncJob
{
    Task ExecuteAsync(
        CancellationToken cancellationToken = default);
}