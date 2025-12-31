using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for GlobalMissionParticipant entity.
/// Composite PK: (MissionId, UserId)
/// </summary>
public sealed class GlobalMissionParticipantConfiguration : IEntityTypeConfiguration<GlobalMissionParticipant>
{
    public void Configure(EntityTypeBuilder<GlobalMissionParticipant> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("GlobalMissionParticipants", "missions", t =>
        {
            t.HasCheckConstraint("CHK_GlobalMissionParticipants_ContributionValue", 
                "\"ContributionValue\" >= 0");
        });

        // Composite Primary Key
        builder.HasKey(x => new { x.MissionId, x.UserId });

        // ===== Properties =====
        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ContributionValue)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(x => x.IsRewardClaimed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        // ===== Indexes =====
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_GlobalMissionParticipants_ByUser");
    }
}
