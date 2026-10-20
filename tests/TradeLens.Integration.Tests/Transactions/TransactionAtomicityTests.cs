using Microsoft.EntityFrameworkCore;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Transactions;

public sealed class TransactionAtomicityTests
{
    [Fact]
    public async Task ExecuteInTransactionAsync_WhenOperationFailsAfterSaveChanges_ShouldRollbackChanges()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var instrumentId = Guid.NewGuid();
        var symbol = $"TST{Guid.NewGuid():N}"[..20];

        var instrument = new Instrument(
            instrumentId,
            symbol,
            "Atomicity Test Instrument",
            "IDR");

        await using var context =
            IntegrationTestDbContextFactory.Create();

        var unitOfWork = new TradeLensUnitOfWork(context);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.ExecuteInTransactionAsync<int>(
                async cancellationToken =>
                {
                    context.Instruments.Add(instrument);

                    await unitOfWork.SaveChangesAsync(
                        cancellationToken);

                    // Simulate a failure after changes have been flushed,
                    // but before the outer transaction is committed.
                    throw new InvalidOperationException(
                        "Simulated failure before commit.");
                }));

        // Assert: verify using a fresh DbContext.
        await using var verificationContext =
            IntegrationTestDbContextFactory.Create();

        var persistedInstrument = await verificationContext.Instruments
            .SingleOrDefaultAsync(x => x.Id == instrumentId);

        Assert.Null(persistedInstrument);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenFailureOccursAfterSavingTransactionPositionAndIdempotencyRecord_ShouldRollbackAllChanges()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        await factory.SeedAsync();

        var now = DateTimeOffset.UtcNow;
        var transactionId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var idempotencyRecordId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var brokerAccountId = Guid.NewGuid();
        var idempotencyKey = $"atomicity-{Guid.NewGuid():N}";

        var transaction = new Transaction(
            transactionId,
            CustomWebApplicationFactory.TestPortfolioId,
            brokerAccountId,
            instrumentId,
            TradeLens.Domain.Enums.TransactionType.Buy,
            100,
            10000m,
            100000m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            1,
            CustomWebApplicationFactory.TestUserId,
            now);

        var position = new Position(
            positionId,
            CustomWebApplicationFactory.TestPortfolioId,
            instrumentId,
            100,
            1100000m,
            11000m,
            1,
            now);

        var idempotencyRecord = new IdempotencyRecord(
            idempotencyRecordId,
            CustomWebApplicationFactory.TestUserId,
            idempotencyKey,
            new string('a', 64),
            transactionId,
            positionId,
            100,
            1100000m,
            11000m,
            now);

        await using var context =
            IntegrationTestDbContextFactory.Create();

        var unitOfWork = new TradeLensUnitOfWork(context);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.ExecuteInTransactionAsync<int>(
                async cancellationToken =>
                {
                    context.Transactions.Add(transaction);
                    context.Positions.Add(position);
                    context.IdempotencyRecords.Add(idempotencyRecord);

                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    throw new InvalidOperationException(
                        "Simulated failure before commit.");
                }));

        // Assert: query with a fresh DbContext.
        await using var verificationContext =
            IntegrationTestDbContextFactory.Create();

        Assert.False(
            await verificationContext.Transactions
                .AnyAsync(x => x.Id == transactionId));

        Assert.False(
            await verificationContext.Positions
                .AnyAsync(x => x.Id == positionId));

        Assert.False(
            await verificationContext.IdempotencyRecords
                .AnyAsync(x => x.Id == idempotencyRecordId));
    }
}
