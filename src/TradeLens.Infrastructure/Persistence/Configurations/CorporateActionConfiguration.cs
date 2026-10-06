using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class CorporateActionConfiguration
    : IEntityTypeConfiguration<CorporateAction>
{
    public void Configure(
        EntityTypeBuilder<CorporateAction> builder)
    {
        builder.ToTable("corporate_actions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Numerator)
            .IsRequired();

        builder.Property(x => x.Denominator)
            .IsRequired();

        builder.Property(x => x.RecordDate)
            .IsRequired();

        builder.Property(x => x.ExDate)
            .IsRequired();

        builder.Property(x => x.OriginalEffectiveDate)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.AppliedBy);

        builder.Property(x => x.CancelledBy);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.InstrumentId,
            x.EffectiveDate,
            x.Status
        });
    }
}
