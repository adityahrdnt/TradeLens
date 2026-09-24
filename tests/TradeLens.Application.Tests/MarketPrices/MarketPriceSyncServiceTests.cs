using FluentAssertions;
using TradeLens.Application.MarketPrices;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;

namespace TradeLens.Application.Tests.MarketPrices;

public sealed class MarketPriceSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_ShouldStoreMarketPriceFromProvider()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        var instrument =
            new Instrument(
                instrumentId,
                "BBCA",
                "Bank Central Asia",
                "IDR");

        var provider =
            new FakeMarketPriceProvider();

        provider.Quotes.Add(
            new MarketPriceQuote(
                "BBCA",
                8500m,
                new DateTimeOffset(
                    2026,
                    9,
                    23,
                    2,
                    0,
                    0,
                    TimeSpan.Zero),
                "FakeProvider"));

        var instrumentRepository =
            new FakeInstrumentRepository();

        instrumentRepository.Instruments.Add(instrument);

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            new[] { "BBCA" });

        // Assert
        marketPriceRepository.MarketPrices
            .Should()
            .ContainSingle();

        var marketPrice =
            marketPriceRepository.MarketPrices.Single();

        marketPrice.InstrumentId
            .Should()
            .Be(instrumentId);

        marketPrice.Price
            .Should()
            .Be(8500m);

        marketPrice.Source
            .Should()
            .Be("FakeProvider");

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task SyncAsync_ShouldSkipUnknownInstrument()
    {
        // Arrange
        var provider =
            new FakeMarketPriceProvider();

        provider.Quotes.Add(
            new MarketPriceQuote(
                "UNKNOWN",
                8500m,
                new DateTimeOffset(
                    2026,
                    9,
                    23,
                    2,
                    0,
                    0,
                    TimeSpan.Zero),
                "FakeProvider"));

        var instrumentRepository =
            new FakeInstrumentRepository();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            new[] { "UNKNOWN" });

        // Assert
        marketPriceRepository.MarketPrices
            .Should()
            .BeEmpty();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task SyncAsync_ShouldStoreMultipleMarketPrices()
    {
        // Arrange
        var bbcaId = Guid.NewGuid();
        var bbriId = Guid.NewGuid();

        var instrumentRepository =
            new FakeInstrumentRepository();

        instrumentRepository.Instruments.Add(
            new Instrument(
                bbcaId,
                "BBCA",
                "Bank Central Asia",
                "IDR"));

        instrumentRepository.Instruments.Add(
            new Instrument(
                bbriId,
                "BBRI",
                "Bank Rakyat Indonesia",
                "IDR"));

        var provider =
            new FakeMarketPriceProvider();

        provider.Quotes.Add(
            new MarketPriceQuote(
                "BBCA",
                8500m,
                DateTimeOffset.UtcNow,
                "FakeProvider"));

        provider.Quotes.Add(
            new MarketPriceQuote(
                "BBRI",
                4200m,
                DateTimeOffset.UtcNow,
                "FakeProvider"));

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            new[] { "BBCA", "BBRI" });

        // Assert
        marketPriceRepository.MarketPrices
            .Should()
            .HaveCount(2);

        marketPriceRepository.MarketPrices
            .Should()
            .ContainSingle(x =>
                x.InstrumentId == bbcaId &&
                x.Price == 8500m);

        marketPriceRepository.MarketPrices
            .Should()
            .ContainSingle(x =>
                x.InstrumentId == bbriId &&
                x.Price == 4200m);

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task SyncAsync_ShouldDoNothing_WhenSymbolsAreEmpty()
    {
        // Arrange
        var provider =
            new FakeMarketPriceProvider();

        var instrumentRepository =
            new FakeInstrumentRepository();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            Array.Empty<string>());

        // Assert
        marketPriceRepository.MarketPrices
            .Should()
            .BeEmpty();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task SyncAsync_ShouldPassCancellationTokenToProvider()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var provider =
            new FakeMarketPriceProvider();

        var instrumentRepository =
            new FakeInstrumentRepository();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            new[] { "BBCA" },
            cancellationToken);

        // Assert
        provider.LastCancellationToken
            .Should()
            .Be(cancellationToken);
    }

    [Fact]
    public async Task SyncAsync_ShouldPassCancellationTokenToMarketPriceRepository()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var instrumentId = Guid.NewGuid();

        var instrumentRepository =
            new FakeInstrumentRepository();

        instrumentRepository.Instruments.Add(
            new Instrument(
                instrumentId,
                "BBCA",
                "Bank Central Asia",
                "IDR"));

        var provider =
            new FakeMarketPriceProvider();

        provider.Quotes.Add(
            new MarketPriceQuote(
                "BBCA",
                8500m,
                DateTimeOffset.UtcNow,
                "FakeProvider"));

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        // Act
        await service.SyncAsync(
            new[] { "BBCA" },
            cancellationToken);

        // Assert
        marketPriceRepository.LastCancellationToken
            .Should()
            .Be(cancellationToken);
    }
}