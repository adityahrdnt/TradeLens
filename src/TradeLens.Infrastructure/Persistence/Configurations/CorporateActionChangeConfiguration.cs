using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class CorporateActionChangeConfiguration
    : IEntityTypeConfiguration<CorporateActionChange>
{
    public void Configure(
        EntityTypeBuilder<CorporateActionChange> builder)
    {
        builder.ToTable("corporate_action_changes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChangeType)
            .IsRequired();

        builder.Property(x => x.PreviousNumerator);

        builder.Property(x => x.NewNumerator);

        builder.Property(x => x.PreviousDenominator);

        builder.Property(x => x.NewDenominator);

        builder.Property(x => x.PreviousRecordDate);

        builder.Property(x => x.NewRecordDate);

        builder.Property(x => x.PreviousExDate);

        builder.Property(x => x.NewExDate);

        builder.Property(x => x.PreviousEffectiveDate);

        builder.Property(x => x.NewEffectiveDate);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.ChangedBy)
            .IsRequired();

        builder.HasOne<CorporateAction>()
            .WithMany()
            .HasForeignKey(x => x.CorporateActionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.CorporateActionId,
            x.ChangedAt
        });
    }
}
