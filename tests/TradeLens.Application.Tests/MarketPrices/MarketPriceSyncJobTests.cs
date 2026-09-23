using FluentAssertions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using Xunit;

namespace TradeLens.Application.Tests.MarketPrices;

public sealed class MarketPriceSyncJobTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldSyncAllInstrumentSymbols()
    {
        var instrumentRepository =
            new FakeInstrumentRepository();

        instrumentRepository.Instruments.Add(
            new Instrument(
                Guid.NewGuid(),
                "BBCA",
                "Bank Central Asia",
                "IDR"));

        instrumentRepository.Instruments.Add(
            new Instrument(
                Guid.NewGuid(),
                "TLKM",
                "Telkom Indonesia",
                "IDR"));

        var provider =
            new FakeMarketPriceProvider();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var syncService =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        var job =
            new MarketPriceSyncJob(
                instrumentRepository,
                syncService);

        await job.ExecuteAsync();

        provider.LastCancellationToken
            .Should()
            .NotBe(default);

        marketPriceRepository.MarketPrices
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDoNothing_WhenNoInstrumentsExist()
    {
        var instrumentRepository =
            new FakeInstrumentRepository();

        var provider =
            new FakeMarketPriceProvider();

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var syncService =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        var job =
            new MarketPriceSyncJob(
                instrumentRepository,
                syncService);

        await job.ExecuteAsync();

        provider.LastCancellationToken
            .CanBeCanceled
            .Should()
            .BeFalse();

        unitOfWork.SaveChangesCallCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassCancellationToken()
    {
        var instrumentRepository =
            new FakeInstrumentRepository();

        instrumentRepository.Instruments.Add(
            new Instrument(
                Guid.NewGuid(),
                "BBCA",
                "Bank Central Asia",
                "IDR"));

        var provider =
            new FakeMarketPriceProvider();

        provider.Quotes.Add(
            new MarketPriceQuote(
                "BBCA",
                9000m,
                DateTimeOffset.UtcNow,
                "TEST"));

        var marketPriceRepository =
            new FakeMarketPriceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var syncService =
            new MarketPriceSyncService(
                provider,
                instrumentRepository,
                marketPriceRepository,
                unitOfWork);

        var job =
            new MarketPriceSyncJob(
                instrumentRepository,
                syncService);

        using var cts =
            new CancellationTokenSource();

        await job.ExecuteAsync(cts.Token);

        provider.LastCancellationToken
            .Should()
            .Be(cts.Token);

        marketPriceRepository.LastCancellationToken
            .Should()
            .Be(cts.Token);
    }
}