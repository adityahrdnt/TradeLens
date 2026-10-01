using FluentAssertions;
using TradeLens.Domain.Entities;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Tests.Entities;

public class CorporateActionApplicationTests
{
    private static readonly Guid Id =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly Guid CorporateActionId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid PortfolioId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static readonly Guid InstrumentId =
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private static readonly DateTimeOffset AppliedAt =
        new(2026, 10, 15, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ShouldCreateApplication()
    {
        var application = new CorporateActionApplication(
            Id,
            CorporateActionId,
            PortfolioId,
            InstrumentId,
            1_000,
            2_000,
            AppliedAt);

        application.Id.Should().Be(Id);
        application.CorporateActionId.Should().Be(CorporateActionId);
        application.PortfolioId.Should().Be(PortfolioId);
        application.InstrumentId.Should().Be(InstrumentId);
        application.EligibleQuantity.Should().Be(1_000);
        application.ResultingQuantity.Should().Be(2_000);
        application.AppliedAt.Should().Be(AppliedAt);
    }

    [Fact]
    public void Constructor_ShouldAllowZeroEligibleQuantity()
    {
        var application = new CorporateActionApplication(
            Id,
            CorporateActionId,
            PortfolioId,
            InstrumentId,
            0,
            0,
            AppliedAt);

        application.EligibleQuantity.Should().Be(0);
        application.ResultingQuantity.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeEligibleQuantity()
    {
        var act = () => new CorporateActionApplication(
            Id,
            CorporateActionId,
            PortfolioId,
            InstrumentId,
            -1,
            0,
            AppliedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action eligible quantity cannot be negative.");
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeResultingQuantity()
    {
        var act = () => new CorporateActionApplication(
            Id,
            CorporateActionId,
            PortfolioId,
            InstrumentId,
            1_000,
            -1,
            AppliedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action resulting quantity cannot be negative.");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyCorporateActionId()
    {
        var act = () => new CorporateActionApplication(
            Id,
            Guid.Empty,
            PortfolioId,
            InstrumentId,
            1_000,
            2_000,
            AppliedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action id is required.");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyPortfolioId()
    {
        var act = () => new CorporateActionApplication(
            Id,
            CorporateActionId,
            Guid.Empty,
            InstrumentId,
            1_000,
            2_000,
            AppliedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action application portfolio id is required.");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyInstrumentId()
    {
        var act = () => new CorporateActionApplication(
            Id,
            CorporateActionId,
            PortfolioId,
            Guid.Empty,
            1_000,
            2_000,
            AppliedAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Corporate action application instrument id is required.");
    }
}