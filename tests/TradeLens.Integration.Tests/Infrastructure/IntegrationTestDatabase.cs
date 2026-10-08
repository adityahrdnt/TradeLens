using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Integration.Tests.Infrastructure;

public static class IntegrationTestDatabase
{
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);

    public static async Task InitializeAsync(
        IServiceProvider services)
    {
        await ExecuteAsync(async () =>
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<TradeLensDbContext>();

            await dbContext.Database.MigrateAsync();
        });
    }

    public static async Task ExecuteAsync(
        Func<Task> action)
    {
        await InitializationLock.WaitAsync();

        try
        {
            await action();
        }
        finally
        {
            InitializationLock.Release();
        }
    }
}
