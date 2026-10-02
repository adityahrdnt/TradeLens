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

        await using (var context = IntegrationTestDbContextFactory.Create())
        {
            context.Instruments.Add(
                new Instrument(
                    instrumentId,
                    symbol,
                    "Test Instrument",
                    "IDR"));

            await context.SaveChangesAsync();
        }

        await using var repositoryContext = IntegrationTestDbContextFactory.Create();

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

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllInstrumentsOrderedBySymbol()
    {
        // Arrange
        await _factory.SeedAsync();

        var firstInstrumentId = Guid.NewGuid();
        var secondInstrumentId = Guid.NewGuid();

        var firstSymbol =
            $"T{firstInstrumentId.ToString("N")[..10]}"
                .ToUpperInvariant();

        var secondSymbol =
            $"T{secondInstrumentId.ToString("N")[..10]}"
                .ToUpperInvariant();

        await using (var context = IntegrationTestDbContextFactory.Create())
        {
            context.Instruments.AddRange(
                new Instrument(
                    firstInstrumentId,
                    firstSymbol,
                    "First Test Instrument",
                    "IDR"),
                new Instrument(
                    secondInstrumentId,
                    secondSymbol,
                    "Second Test Instrument",
                    "IDR"));

            await context.SaveChangesAsync();
        }

        await using var repositoryContext = IntegrationTestDbContextFactory.Create();

        var repository =
            new InstrumentRepository(repositoryContext);

        // Act
        var result =
            await repository.GetAllAsync();

        // Assert
        result.Should().Contain(x =>
            x.Id == firstInstrumentId);

        result.Should().Contain(x =>
            x.Id == secondInstrumentId);

        result
            .Select(x => x.Symbol)
            .Should()
            .BeInAscendingOrder();
    }
}