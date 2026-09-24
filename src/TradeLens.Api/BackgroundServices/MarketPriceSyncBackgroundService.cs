using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeLens.Application.Interfaces;

namespace TradeLens.Api.BackgroundServices;

public sealed class MarketPriceSyncBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MarketPriceSyncBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public MarketPriceSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MarketPriceSyncBackgroundService> logger,
        IOptions<MarketPriceOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await ExecuteSyncAsync(stoppingToken);

            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteSyncAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
    }

    private async Task ExecuteSyncAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var job =
                scope.ServiceProvider
                    .GetRequiredService<IMarketPriceSyncJob>();

            await job.ExecuteAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Market price synchronization failed.");
        }
    }
}