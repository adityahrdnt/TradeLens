using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Integration.Tests.Infrastructure;

public sealed class JwtWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TradeLens",
                ["Jwt:Audience"] = "TradeLens.Api",
                ["Jwt:SecretKey"] =
                    "TradeLens-Integration-Test-Secret-Key-At-Least-32",

                ["ConnectionStrings:TradeLens"] =
                    Environment.GetEnvironmentVariable(
                        "TRADELENS_TEST_CONNECTION_STRING")
            };

            config.AddInMemoryCollection(settings);
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<TradeLensDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
