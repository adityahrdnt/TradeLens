using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration
    : IEntityTypeConfiguration<Transaction>
{
    public void Configure(
        EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Fee)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.CorrectionReason)
            .HasMaxLength(500);

        builder.Property(x => x.VoidReason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.InstrumentId,
            x.TransactionDate,
            x.Sequence
        });

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.InstrumentId,
            x.Status
        });
    }
}