using Microsoft.EntityFrameworkCore;
using TradeLens.Application.Exceptions;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Integration.Tests.Infrastructure;

namespace TradeLens.Integration.Tests.Concurrency;

public sealed class PositionConcurrencyTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PositionConcurrencyTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdatingSamePositionConcurrently_ShouldDetectConcurrencyConflict()
    {
        await _factory.SeedAsync();

        var instrumentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        await CreatePositionAsync(
            positionId,
            instrumentId);

        await using var contextA = IntegrationTestDbContextFactory.Create();
        await using var contextB = IntegrationTestDbContextFactory.Create();

        var positionA = await contextA.Positions
            .SingleAsync(x => x.Id == positionId);

        var positionB = await contextB.Positions
            .SingleAsync(x => x.Id == positionId);

        positionA.Apply(
            100,
            100000,
            1000,
            DateTimeOffset.UtcNow);

        positionB.Apply(
            200,
            200000,
            1000,
            DateTimeOffset.UtcNow);

        var unitOfWorkA = new TradeLensUnitOfWork(contextA);
        var unitOfWorkB = new TradeLensUnitOfWork(contextB);

        await unitOfWorkA.SaveChangesAsync();

        await Assert.ThrowsAsync<PositionConcurrencyException>(
            () => unitOfWorkB.SaveChangesAsync());
    }

    private async Task CreatePositionAsync(
        Guid positionId,
        Guid instrumentId)
    {
        await using var context = IntegrationTestDbContextFactory.Create();

        var position = new Position(
            positionId,
            CustomWebApplicationFactory.TestPortfolioId,
            instrumentId,
            0,
            0,
            0,
            0,
            DateTimeOffset.UtcNow);

        context.Positions.Add(position);

        await context.SaveChangesAsync();
    }
}