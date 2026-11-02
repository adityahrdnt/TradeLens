using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence.Configurations;

public sealed class RoleChangeAuditConfiguration
    : IEntityTypeConfiguration<RoleChangeAudit>
{
    public void Configure(EntityTypeBuilder<RoleChangeAudit> builder)
    {
        builder.ToTable("role_change_audits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousRole)
            .IsRequired();

        builder.Property(x => x.NewRole)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ActorUserId);

        builder.HasIndex(x => x.TargetUserId);

        builder.HasIndex(x => x.ChangedAtUtc);
    }
}
