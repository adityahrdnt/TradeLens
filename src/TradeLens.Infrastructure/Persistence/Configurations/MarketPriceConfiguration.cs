using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class MarketPriceConfiguration
    : IEntityTypeConfiguration<MarketPrice>
{
    public void Configure(
        EntityTypeBuilder<MarketPrice> builder)
    {
        builder.ToTable("market_prices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstrumentId)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.PriceTimestamp)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.InstrumentId,
            x.PriceTimestamp
        });

        builder.HasIndex(x => new
        {
            x.InstrumentId,
            x.Source,
            x.PriceTimestamp
        });
    }
}