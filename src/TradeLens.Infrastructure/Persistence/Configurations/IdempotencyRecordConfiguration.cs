using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(
        EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .IsRequired();

        builder.Property(x => x.PositionId)
            .IsRequired();

        builder.Property(x => x.PositionQuantity)
            .IsRequired();

        builder.Property(x => x.PositionCostBasis)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.PositionAveragePrice)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(
                x => new { x.UserId, x.IdempotencyKey })
            .IsUnique();
    }
}