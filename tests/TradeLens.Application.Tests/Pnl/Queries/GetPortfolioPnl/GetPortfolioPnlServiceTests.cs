using FluentAssertions;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Pnl.Queries.GetPortfolioPnl;
using TradeLens.Application.Services;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Services;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Pnl.Queries.GetPortfolioPnl;

public sealed class GetPortfolioPnlServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioHasNoTransactionsAndNoPositions_ReturnsZeroCompletePnl()
    {
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            out _,
            out _,
            out _);

        var result = await service.ExecuteAsync(
            new GetPortfolioPnlQuery(portfolioId));

        result.PortfolioId.Should().Be(portfolioId);
        result.Status.Should().Be("COMPLETE");
        result.RealizedPnl.Should().Be(0m);
        result.UnrealizedPnl.Should().Be(0m);
        result.TotalPnl.Should().Be(0m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionsContainBuyAndSell_CalculatesRealizedPnl()
    {
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            out var transactionRepository,
            out _,
            out _);

        var buy = CreateTransaction(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            100m,
            10m,
            new DateOnly(2026, 1, 1),
            1,
            userId);

        var sell = CreateTransaction(
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            40,
            120m,
            5m,
            new DateOnly(2026, 1, 2),
            1,
            userId);

        transactionRepository.Transactions.Add(buy);
        transactionRepository.Transactions.Add(sell);

        var result = await service.ExecuteAsync(
            new GetPortfolioPnlQuery(portfolioId));

        // BUY cost = 100 * 100 + 10 = 10,010
        // Average = 10,010 / 100 = 100.10
        // Cost sold = 40 * 100.10 = 4,004
        // Net proceeds = 40 * 120 - 5 = 4,795
        // Realized P&L = 4,795 - 4,004 = 791
        result.Status.Should().Be("COMPLETE");
        result.RealizedPnl.Should().Be(791m);
        result.UnrealizedPnl.Should().Be(0m);
        result.TotalPnl.Should().Be(791m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMarketPriceIsFresh_ReturnsUnrealizedAndTotalPnl()
    {
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            out _,
            out var positionRepository,
            out var marketPriceRepository);

        var now = DateTimeOffset.UtcNow;

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId,
                100,
                10000m,
                100m,
                1,
                now));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                120m,
                now,
                "TEST"));

        var result = await service.ExecuteAsync(
            new GetPortfolioPnlQuery(portfolioId));

        // Market value = 100 * 120 = 12,000
        // Unrealized P&L = 12,000 - 10,000 = 2,000
        result.Status.Should().Be("COMPLETE");
        result.RealizedPnl.Should().Be(0m);
        result.UnrealizedPnl.Should().Be(2000m);
        result.TotalPnl.Should().Be(2000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMarketPriceIsStale_ReturnsRealizedPnlButNullUnrealizedAndTotalPnl()
    {
        var portfolioId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var service = CreateService(
            portfolioId,
            userId,
            out _,
            out var positionRepository,
            out var marketPriceRepository);

        var now = DateTimeOffset.UtcNow;

        positionRepository.Positions.Add(
            new Position(
                Guid.NewGuid(),
                portfolioId,
                instrumentId,
                100,
                10000m,
                100m,
                1,
                now));

        marketPriceRepository.MarketPrices.Add(
            new MarketPrice(
                Guid.NewGuid(),
                instrumentId,
                120m,
                now.AddHours(-2),
                "TEST"));

        var result = await service.ExecuteAsync(
            new GetPortfolioPnlQuery(portfolioId));

        result.Status.Should().Be("PARTIAL");
        result.RealizedPnl.Should().Be(0m);
        result.UnrealizedPnl.Should().BeNull();
        result.TotalPnl.Should().BeNull();
    }

    private static GetPortfolioPnlService CreateService(
        Guid portfolioId,
        Guid userId,
        out FakeTransactionRepository transactionRepository,
        out FakePositionRepository positionRepository,
        out FakeMarketPriceRepository marketPriceRepository)
    {
        transactionRepository = new FakeTransactionRepository();
        positionRepository = new FakePositionRepository();
        marketPriceRepository = new FakeMarketPriceRepository();

        var portfolioRepository =
            new FakePortfolioRepository(
                new Portfolio(
                    portfolioId,
                    userId,
                    "Test Portfolio"));

        var portfolioAccessService =
            new PortfolioAccessService(
                portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        var freshnessPolicy =
            new TestMarketPriceFreshnessPolicy();

        return new GetPortfolioPnlService(
            transactionRepository,
            positionRepository,
            marketPriceRepository,
            freshnessPolicy,
            portfolioAccessService,
            new PositionCalculator(),
            new ValuationCalculator(),
            currentUserService);
    }

    private static DomainTransaction CreateTransaction(
        Guid portfolioId,
        Guid brokerAccountId,
        Guid instrumentId,
        TransactionType type,
        long quantity,
        decimal price,
        decimal fee,
        DateOnly date,
        long sequence,
        Guid createdBy)
    {
        return new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            type,
            quantity,
            price,
            fee,
            date,
            sequence,
            createdBy,
            DateTimeOffset.UtcNow);
    }

    private sealed class TestMarketPriceFreshnessPolicy
        : IMarketPriceFreshnessPolicy
    {
        public bool IsFresh(
            MarketPrice marketPrice,
            DateTimeOffset now)
        {
            return marketPrice.PriceTimestamp >=
                   now.AddHours(-1);
        }
    }
}