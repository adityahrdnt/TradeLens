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
}
