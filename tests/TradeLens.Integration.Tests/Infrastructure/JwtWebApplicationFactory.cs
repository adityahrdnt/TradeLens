using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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
                    "TradeLens-Integration-Test-Secret-Key-At-Least-32"
            };

            config.AddInMemoryCollection(settings);
        });
    }
}