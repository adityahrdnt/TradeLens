using FluentAssertions;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using DomainTransaction = TradeLens.Domain.Entities.Transaction;

namespace TradeLens.Application.Tests.Transactions.Queries.GetPortfolioTransactions;

public sealed class GetPortfolioTransactionsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToCurrentUser_ShouldReturnTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "My Portfolio");

        var transaction1 = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            userId,
            DateTimeOffset.UtcNow.AddMinutes(-2));

        var transaction2 = new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Sell,
            50,
            11_000m,
            500m,
            new DateOnly(2026, 9, 16),
            1,
            userId,
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var transactionRepository = new FakeTransactionRepository();
        var portfolioRepository = new FakePortfolioRepository();

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        portfolioRepository.Portfolios.Add(portfolio);

        transactionRepository.Transactions.Add(transaction1);
        transactionRepository.Transactions.Add(transaction2);

        var service = new GetPortfolioTransactionsService(
            transactionRepository,
            portfolioAccessService,
            currentUserService);

        var query = new GetPortfolioTransactionsQuery(
            portfolioId,
            1,
            20);

        // Act
        var result = await service.ExecuteAsync(query);

        // Assert
        result.PortfolioId.Should().Be(portfolioId);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        result.Items[0].TransactionId
            .Should().Be(transaction2.Id);

        result.Items[0].Type
            .Should().Be("Sell");

        result.Items[0].Quantity
            .Should().Be(50);

        result.Items[0].Price
            .Should().Be(11_000m);

        result.Items[1].TransactionId
            .Should().Be(transaction1.Id);

        result.Items[1].Type
            .Should().Be("Buy");

        result.Items[1].Quantity
            .Should().Be(100);

        result.Items[1].Price
            .Should().Be(10_000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestingSecondPage_ShouldReturnCorrectTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "My Portfolio");

        var transaction1 = CreateTransaction(
            portfolioId,
            brokerAccountId,
            instrumentId,
            userId,
            new DateOnly(2026, 9, 17),
            1);

        var transaction2 = CreateTransaction(
            portfolioId,
            brokerAccountId,
            instrumentId,
            userId,
            new DateOnly(2026, 9, 16),
            1);

        var transaction3 = CreateTransaction(
            portfolioId,
            brokerAccountId,
            instrumentId,
            userId,
            new DateOnly(2026, 9, 15),
            1);

        var transactionRepository = new FakeTransactionRepository();
        var portfolioRepository = new FakePortfolioRepository();

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var currentUserService =
            new FakeCurrentUserService(userId);

        portfolioRepository.Portfolios.Add(portfolio);

        transactionRepository.Transactions.Add(transaction1);
        transactionRepository.Transactions.Add(transaction2);
        transactionRepository.Transactions.Add(transaction3);

        var service = new GetPortfolioTransactionsService(
            transactionRepository,
            portfolioAccessService,
            currentUserService);

        var query = new GetPortfolioTransactionsQuery(
            portfolioId,
            2,
            2);

        // Act
        var result = await service.ExecuteAsync(query);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);

        result.Items.Should().HaveCount(1);

        result.Items[0].TransactionId
            .Should().Be(transaction3.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioBelongsToAnotherUser_ShouldThrowPortfolioAccessDeniedException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var portfolioOwnerId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            portfolioOwnerId,
            "Another User Portfolio");

        var transactionRepository =
            new FakeTransactionRepository();

        var portfolioRepository =
            new FakePortfolioRepository();

        portfolioRepository.Portfolios.Add(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var service =
            new GetPortfolioTransactionsService(
                transactionRepository,
                portfolioAccessService,
                new FakeCurrentUserService(currentUserId));

        var query = new GetPortfolioTransactionsQuery(
            portfolioId,
            1,
            20);

        // Act
        var act = async () =>
            await service.ExecuteAsync(query);

        // Assert
        await act.Should()
            .ThrowAsync<PortfolioAccessDeniedException>()
            .WithMessage("You do not have access to this portfolio.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionIsSuperseded_ShouldStillReturnIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var portfolio = new Portfolio(
            portfolioId,
            userId,
            "Test Portfolio");

        var transactionRepository =
            new FakeTransactionRepository();

        var originalTransaction =
            new DomainTransaction(
                Guid.NewGuid(),
                portfolioId,
                brokerAccountId,
                instrumentId,
                TransactionType.Buy,
                100,
                10_000m,
                0m,
                new DateOnly(2026, 9, 15),
                1,
                userId,
                DateTimeOffset.UtcNow);

        originalTransaction.Supersede(
            DateTimeOffset.UtcNow,
            userId,
            "Correction required.");

        transactionRepository.Transactions.Add(originalTransaction);

        var portfolioRepository =
            new FakePortfolioRepository();

        portfolioRepository.Portfolios.Add(portfolio);

        var portfolioAccessService =
            new FakePortfolioAccessService(portfolioRepository);

        var service =
            new GetPortfolioTransactionsService(
                transactionRepository,
                portfolioAccessService,
                new FakeCurrentUserService(userId));

        var query = new GetPortfolioTransactionsQuery(
            portfolioId,
            1,
            20);

        // Act
        var result =
            await service.ExecuteAsync(query);

        // Assert
        result.Items.Should().ContainSingle();

        result.Items[0].TransactionId
            .Should().Be(originalTransaction.Id);

        result.Items[0].Status
            .Should().Be(TransactionStatus.Superseded.ToString());

        result.Items[0].CorrectionReason
            .Should().Be("Correction required.");
    }

    private static DomainTransaction CreateTransaction(
        Guid portfolioId,
        Guid brokerAccountId,
        Guid instrumentId,
        Guid userId,
        DateOnly transactionDate,
        long sequence)
    {
        return new DomainTransaction(
            Guid.NewGuid(),
            portfolioId,
            brokerAccountId,
            instrumentId,
            TransactionType.Buy,
            100,
            10_000m,
            0m,
            transactionDate,
            sequence,
            userId,
            DateTimeOffset.UtcNow);
    }
}