using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeLens.Api.BackgroundServices;
using TradeLens.Application.Interfaces;

namespace TradeLens.Integration.Tests.MarketPrices;

public sealed class MarketPriceBackgroundServiceTests
{
    [Fact]
    public void HostedService_ShouldBeRegistered()
    {
        using var factory =
            new Infrastructure.CustomWebApplicationFactory();

        using var scope =
            factory.Services.CreateScope();

        var hostedServices =
            scope.ServiceProvider
                .GetServices<IHostedService>();

        hostedServices
            .Should()
            .ContainSingle(x =>
                x is MarketPriceSyncBackgroundService);
    }

    [Fact]
    public void MarketPriceSyncJob_ShouldBeRegistered()
    {
        using var factory =
            new Infrastructure.CustomWebApplicationFactory();

        using var scope =
            factory.Services.CreateScope();

        var job =
            scope.ServiceProvider
                .GetRequiredService<IMarketPriceSyncJob>();

        job.Should().NotBeNull();
    }
}