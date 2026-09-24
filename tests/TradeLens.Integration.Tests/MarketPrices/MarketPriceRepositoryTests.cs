using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.MarketPrices;

public sealed class MarketPriceRepositoryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MarketPriceRepositoryTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnLatestPrice()
    {
        await _factory.SeedAsync();

        var instrumentId = Guid.NewGuid();

        await CreateInstrumentAsync(instrumentId);

        await CreateMarketPriceAsync(
            instrumentId,
            1000m,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            "TEST");

        await CreateMarketPriceAsync(
            instrumentId,
            1100m,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            "TEST");

        await CreateMarketPriceAsync(
            instrumentId,
            1200m,
            DateTimeOffset.UtcNow,
            "TEST");

        await using var context = CreateDbContext();

        var repository = new MarketPriceRepository(context);

        var result = await repository.GetLatestAsync(instrumentId);

        result.Should().NotBeNull();
        result!.Price.Should().Be(1200m);
    }

    private TradeLensDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TradeLensDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5433;Database=tradelens_test;Username=tradelens;Password=tradelens")
            .Options;

        return new TradeLensDbContext(options);
    }

    private async Task CreateInstrumentAsync(Guid instrumentId)
    {
        await using var context = CreateDbContext();

        context.Instruments.Add(
            new Instrument(
                instrumentId,
                $"T{instrumentId.ToString("N")[..10]}",
                "Test Instrument",
                "IDR"));

        await context.SaveChangesAsync();
    }

    private async Task CreateMarketPriceAsync(
        Guid instrumentId,
        decimal price,
        DateTimeOffset timestamp,
        string source)
    {
        await using var context = CreateDbContext();

        context.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                price,
                timestamp,
                source));

        await context.SaveChangesAsync();
    }
}