using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for GlobalMissionContribution entity.
/// Append-only ledger with idempotency key.
/// </summary>
public sealed class GlobalMissionContributionConfiguration : IEntityTypeConfiguration<GlobalMissionContribution>
{
    public void Configure(EntityTypeBuilder<GlobalMissionContribution> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("GlobalMissionContributions", "missions", t =>
        {
            t.HasCheckConstraint("CHK_GlobalMissionContributions_Amount", 
                "\"Amount\" > 0");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Unique Constraint =====
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_GlobalMissionContributions_Idempotency");

        // ===== Indexes =====
        builder.HasIndex(x => new { x.MissionId, x.CreatedAt })
            .HasDatabaseName("IX_GlobalMissionContrib_MissionTime");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("IX_GlobalMissionContrib_UserTime");
    }
}
