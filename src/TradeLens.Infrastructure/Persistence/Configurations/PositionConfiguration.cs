using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public class PositionConfiguration
    : IEntityTypeConfiguration<Position>
{
    public void Configure(
        EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CostBasis)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.AveragePrice)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsConcurrencyToken();

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.InstrumentId
        })
        .IsUnique();
    }
}