using FluentAssertions;
using TradeLens.Application.Transactions.Commands.CorrectTransaction;

namespace TradeLens.Application.Tests.Transactions.Commands.CorrectTransaction;

public sealed class CorrectTransactionCommandValidatorTests
{
    private readonly CorrectTransactionCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenCommandIsValid_ShouldPass()
    {
        var command = CreateValidCommand();

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenQuantityIsZero_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Quantity = 0
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPriceIsNegative_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Price = -1m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenFeeIsNegative_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Fee = -1m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenSequenceIsZero_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Sequence = 0
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenReasonIsEmpty_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Reason = string.Empty
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenReasonExceeds500Characters_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Reason = new string('x', 501)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenReasonHasExactly500Characters_ShouldPass()
    {
        var command = CreateValidCommand() with
        {
            Reason = new string('x', 500)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    private static CorrectTransactionCommand CreateValidCommand()
    {
        return new CorrectTransactionCommand(
            Guid.NewGuid(),
            100,
            10_000m,
            1_000m,
            new DateOnly(2026, 9, 16),
            1,
            Guid.NewGuid(),
            "Incorrect quantity");
    }
}