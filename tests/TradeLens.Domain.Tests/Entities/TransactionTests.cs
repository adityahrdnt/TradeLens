using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public class TransactionTests
{
    private static Transaction CreateTransaction()
    {
        return new Transaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            100,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            1,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Supersede_WhenTransactionIsActive_ShouldMarkAsSuperseded()
    {
        // Arrange
        var transaction = CreateTransaction();
        var supersededAt = DateTimeOffset.UtcNow;
        var supersededBy = Guid.NewGuid();

        // Act
        transaction.Supersede(
            supersededAt,
            supersededBy,
            "Incorrect quantity");

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Superseded);
        transaction.SupersededAt.Should().Be(supersededAt);
        transaction.SupersededBy.Should().Be(supersededBy);
        transaction.CorrectionReason.Should().Be("Incorrect quantity");
    }

    [Fact]
    public void Supersede_WhenTransactionIsAlreadySuperseded_ShouldThrow()
    {
        // Arrange
        var transaction = CreateTransaction();

        transaction.Supersede(
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            "First correction");

        // Act
        var act = () => transaction.Supersede(
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            "Second correction");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Supersede_WhenReasonIsEmpty_ShouldThrow()
    {
        // Arrange
        var transaction = CreateTransaction();

        // Act
        var act = () => transaction.Supersede(
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            "");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Supersede_WhenSupersededByIsEmpty_ShouldThrow()
    {
        // Arrange
        var transaction = CreateTransaction();

        // Act
        var act = () => transaction.Supersede(
            DateTimeOffset.UtcNow,
            Guid.Empty,
            "Incorrect quantity");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateCorrection_ShouldCreateNewActiveTransaction()
    {
        // Arrange
        var original = CreateTransaction();
        var correctedId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var corrected = Transaction.CreateCorrection(
            correctedId,
            original,
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            createdBy,
            createdAt,
            "Incorrect quantity");

        // Assert
        corrected.Id.Should().Be(correctedId);
        corrected.Status.Should().Be(TransactionStatus.Active);

        corrected.PortfolioId.Should().Be(original.PortfolioId);
        corrected.BrokerAccountId.Should().Be(original.BrokerAccountId);
        corrected.InstrumentId.Should().Be(original.InstrumentId);
        corrected.Type.Should().Be(original.Type);

        corrected.Quantity.Should().Be(120);
        corrected.Price.Should().Be(10_000m);
        corrected.Fee.Should().Be(100_000m);

        corrected.SupersedesTransactionId.Should().Be(original.Id);
        corrected.CorrectionReason.Should().Be("Incorrect quantity");
    }

    [Fact]
    public void CreateCorrection_WhenOriginalIsNotActive_ShouldThrow()
    {
        // Arrange
        var original = CreateTransaction();

        original.Supersede(
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            "First correction");

        // Act
        var act = () => Transaction.CreateCorrection(
            Guid.NewGuid(),
            original,
            120,
            10_000m,
            100_000m,
            new DateOnly(2026, 9, 16),
            2,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "Second correction");

        // Assert
        act.Should().Throw<DomainException>();
    }
}