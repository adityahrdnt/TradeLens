using FluentAssertions;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Application.Validators;
using TradeLens.Domain.Enums;

namespace TradeLens.Application.Tests.Validators;

public class AddTransactionCommandValidatorTests
{
    private readonly AddTransactionCommandValidator _validator = new();

    [Fact]
    public void ShouldBeValid_WhenCommandIsValid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenPortfolioIdIsEmpty()
    {
        var command = CreateValidCommand() with
        {
            PortfolioId = Guid.Empty
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenBrokerAccountIdIsEmpty()
    {
        var command = CreateValidCommand() with
        {
            BrokerAccountId = Guid.Empty
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenInstrumentIdIsEmpty()
    {
        var command = CreateValidCommand() with
        {
            InstrumentId = Guid.Empty
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenQuantityIsZero()
    {
        var command = CreateValidCommand() with
        {
            Quantity = 0
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenQuantityIsNegative()
    {
        var command = CreateValidCommand() with
        {
            Quantity = -1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenPriceIsZero()
    {
        var command = CreateValidCommand() with
        {
            Price = 0m
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenFeeIsNegative()
    {
        var command = CreateValidCommand() with
        {
            Fee = -1m
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenSequenceIsZero()
    {
        var command = CreateValidCommand() with
        {
            Sequence = 0
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenCreatedByIsEmpty()
    {
        var command = CreateValidCommand() with
        {
            CreatedBy = Guid.Empty
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
    private static AddTransactionCommand CreateValidCommand()
    {
        return new AddTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Buy,
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 15),
            1,
            Guid.NewGuid(),
            "test-idempotency-key-001");
    }
}