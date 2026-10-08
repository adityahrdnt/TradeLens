using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Api.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly TradeLensDbContext _dbContext;

    public DatabaseHealthCheck(
        TradeLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect =
                await _dbContext.Database
                    .CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    "Database connection failed.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed.");
        }
    }
}
