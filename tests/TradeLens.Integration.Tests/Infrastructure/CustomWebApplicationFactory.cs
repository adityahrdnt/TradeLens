using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Application.Interfaces;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Fakes;

namespace TradeLens.Integration.Tests.Infrastructure;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public static readonly Guid TestUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid TestPortfolioId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:TradeLens"] =
                    Environment.GetEnvironmentVariable(
                        "TRADELENS_TEST_CONNECTION_STRING")
                    ?? throw new InvalidOperationException(
                        "TRADELENS_TEST_CONNECTION_STRING environment variable is not configured.")
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            var fakeMarketPriceProvider =
                new FakeMarketPriceProvider();

            services.AddSingleton<IMarketPriceProvider>(
                fakeMarketPriceProvider);

            services.AddSingleton(
                fakeMarketPriceProvider);
        });
    }

    public async Task SeedAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<TradeLensDbContext>();

        await dbContext.Database.MigrateAsync();

        var portfolio = await dbContext.Portfolios
            .FirstOrDefaultAsync(x => x.Id == TestPortfolioId);

        if (portfolio is null)
        {
            portfolio = new Portfolio(
                TestPortfolioId,
                TestUserId,
                "Integration Test Portfolio");

            dbContext.Portfolios.Add(portfolio);

            await dbContext.SaveChangesAsync();
        }
    }
}