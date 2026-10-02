using FluentAssertions;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;
using TradeLens.Application.Exceptions;
using TradeLens.Application.Tests.Fakes;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Enums;
using Xunit;

namespace TradeLens.Application.Tests.CorporateActions.Commands.CreateCorporateAction;

public sealed class CreateCorporateActionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateCorporateAction()
    {
        // Arrange
        var repository = new FakeCorporateActionRepository();
        var instrumentRepository = new FakeInstrumentRepository();
        var unitOfWork = new FakeUnitOfWork();

        var createdBy = Guid.NewGuid();
        var instrumentId = Guid.NewGuid();

        var instrument = new Instrument(
            instrumentId,
            "TEST",
            "Test Instrument",
            "IDR");

        instrumentRepository.Instruments.Add(instrument);

        var service = new CreateCorporateActionService(
            repository,
            instrumentRepository,
            unitOfWork);

        var command = new CreateCorporateActionCommand(
            instrumentId,
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            createdBy);

        // Act
        var result = await service.ExecuteAsync(command);

        // Assert
        result.CorporateActionId.Should().NotBeEmpty();

        repository.CorporateActions.Should().ContainSingle();

        var corporateAction = repository.CorporateActions.Single();

        corporateAction.Id.Should().Be(result.CorporateActionId);
        corporateAction.InstrumentId.Should().Be(instrumentId);
        corporateAction.Type.Should().Be(CorporateActionType.StockSplit);
        corporateAction.Numerator.Should().Be(2);
        corporateAction.Denominator.Should().Be(1);
        corporateAction.RecordDate.Should().Be(new DateOnly(2026, 9, 10));
        corporateAction.ExDate.Should().Be(new DateOnly(2026, 9, 11));
        corporateAction.EffectiveDate.Should().Be(new DateOnly(2026, 9, 12));
        corporateAction.Status.Should().Be(CorporateActionStatus.Scheduled);
        corporateAction.CreatedBy.Should().Be(createdBy);

        unitOfWork.TransactionCallCount.Should().Be(1);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInstrumentNotFound_WhenInstrumentDoesNotExist()
    {
        // Arrange
        var repository = new FakeCorporateActionRepository();
        var instrumentRepository = new FakeInstrumentRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCorporateActionService(
            repository,
            instrumentRepository,
            unitOfWork);

        var command = new CreateCorporateActionCommand(
            Guid.NewGuid(),
            CorporateActionType.StockSplit,
            2,
            1,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 11),
            new DateOnly(2026, 9, 12),
            Guid.NewGuid());

        // Act
        var act = () => service.ExecuteAsync(command);

        // Assert
        await act.Should()
            .ThrowAsync<InstrumentNotFoundException>();

        repository.CorporateActions.Should().BeEmpty();
        unitOfWork.TransactionCallCount.Should().Be(0);
    }
}