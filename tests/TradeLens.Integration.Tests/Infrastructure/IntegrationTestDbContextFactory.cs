using Microsoft.EntityFrameworkCore;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Integration.Tests.Infrastructure;

public static class IntegrationTestDbContextFactory
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("TRADELENS_TEST_CONNECTION_STRING")
        ?? throw new InvalidOperationException(
            "TRADELENS_TEST_CONNECTION_STRING environment variable is not configured.");

    public static TradeLensDbContext Create()
    {
        var options =
            new DbContextOptionsBuilder<TradeLensDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        return new TradeLensDbContext(options);
    }
}