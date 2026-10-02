using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class CorporateActionApplicationConfiguration
    : IEntityTypeConfiguration<CorporateActionApplication>
{
    public void Configure(
        EntityTypeBuilder<CorporateActionApplication> builder)
    {
        builder.ToTable("corporate_action_applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EligibleQuantity)
            .IsRequired();

        builder.Property(x => x.ResultingQuantity)
            .IsRequired();

        builder.Property(x => x.AppliedAt)
            .IsRequired();

        builder.HasOne<CorporateAction>()
            .WithMany()
            .HasForeignKey(x => x.CorporateActionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.CorporateActionId,
            x.PortfolioId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.InstrumentId,
            x.AppliedAt
        });
    }
}