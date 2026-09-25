using FluentAssertions;
using TradeLens.Application.Transactions.Commands.VoidTransaction;

namespace TradeLens.Application.Tests.Transactions.Commands.VoidTransaction;

public class VoidTransactionCommandValidatorTests
{
    private readonly VoidTransactionCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldPass()
    {
        // Arrange
        var command = new VoidTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Transaction entered by mistake");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenTransactionIdIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new VoidTransactionCommand(
            Guid.Empty,
            Guid.NewGuid(),
            "Transaction entered by mistake");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenVoidedByIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new VoidTransactionCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "Transaction entered by mistake");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenReasonIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new VoidTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenReasonExceeds500Characters_ShouldFail()
    {
        // Arrange
        var command = new VoidTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('x', 501));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}