using FluentAssertions;
using TradeLens.Application.CorporateActions.Queries.GetCorporateAction;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using Xunit;

namespace TradeLens.Application.Tests.CorporateActions.Queries.GetCorporateAction;

public sealed class GetCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnCorporateActionDetail()
    {
        // Arrange
        var repository = new FakeCorporateActionRepository();

        var corporateActionId = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var createdAt = new DateTimeOffset(
            2026,
            9,
            10,
            8,
            30,
            0,
            TimeSpan.Zero);

        var corporateAction = new CorporateAction(
            corporateActionId,
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            createdBy,
            createdAt);

        repository.CorporateActions.Add(corporateAction);

        var service = new GetCorporateActionService(repository);

        // Act
        var result = await service.ExecuteAsync(corporateActionId);

        // Assert
        result.CorporateActionId.Should().Be(corporateActionId);
        result.InstrumentId.Should().Be(instrumentId);
        result.Type.Should().Be(CorporateActionType.StockSplit);
        result.Numerator.Should().Be(2);
        result.Denominator.Should().Be(1);
        result.RecordDate.Should().Be(new DateOnly(2026, 9, 10));
        result.ExDate.Should().Be(new DateOnly(2026, 9, 11));
        result.EffectiveDate.Should().Be(new DateOnly(2026, 9, 12));
        result.Status.Should().Be(CorporateActionStatus.Scheduled);
        result.CreatedAt.Should().Be(createdAt);
        result.CreatedBy.Should().Be(createdBy);
        result.AppliedAt.Should().BeNull();
        result.AppliedBy.Should().BeNull();
        result.CancelledAt.Should().BeNull();
        result.CancelledBy.Should().BeNull();
        result.CancellationReason.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowCorporateActionNotFound_WhenCorporateActionDoesNotExist()
    {
        // Arrange
        var repository = new FakeCorporateActionRepository();
        var service = new GetCorporateActionService(repository);

        var corporateActionId = Guid.NewGuid();

        // Act
        var act = () => service.ExecuteAsync(corporateActionId);

        // Assert
        await act.Should()
            .ThrowAsync<CorporateActionNotFoundException>();
    }
}