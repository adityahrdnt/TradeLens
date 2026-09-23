using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Instruments;

public sealed class InstrumentRepositoryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public InstrumentRepositoryTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBySymbolAsync_ShouldReturnInstrument()
    {
        await _factory.SeedAsync();

        var instrumentId = Guid.NewGuid();
        var symbol =
            $"T{instrumentId.ToString("N")[..10]}"
                .ToUpperInvariant();

        await using (var context = CreateDbContext())
        {
            context.Instruments.Add(
                new Instrument(
                    instrumentId,
                    symbol,
                    "Test Instrument",
                    "IDR"));

            await context.SaveChangesAsync();
        }

        await using var repositoryContext = CreateDbContext();

        var repository =
            new InstrumentRepository(repositoryContext);

        var result =
            await repository.GetBySymbolAsync(symbol);

        result.Should().NotBeNull();
        result!.Id.Should().Be(instrumentId);
        result.Symbol.Should().Be(symbol);
        result.Name.Should().Be("Test Instrument");
        result.Currency.Should().Be("IDR");
    }

    private TradeLensDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<TradeLensDbContext>()
                .UseNpgsql(
                    "Host=localhost;Port=5433;Database=tradelens_test;Username=tradelens;Password=tradelens")
                .Options;

        return new TradeLensDbContext(options);
    }
}