using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for GlobalMission entity.
/// </summary>
public sealed class GlobalMissionConfiguration : IEntityTypeConfiguration<GlobalMission>
{
    public void Configure(EntityTypeBuilder<GlobalMission> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("GlobalMissions", "missions", t =>
        {
            t.HasCheckConstraint("CHK_GlobalMissions_TimeWindow", 
                "\"EndDate\" > \"StartDate\"");
            t.HasCheckConstraint("CHK_GlobalMissions_Status", 
                "\"Status\" IN ('DRAFT','ACTIVE','FINISHED','CANCELLED')");
            t.HasCheckConstraint("CHK_GlobalMissions_TargetValue", 
                "\"TargetValue\" > 0");
            t.HasCheckConstraint("CHK_GlobalMissions_CurrentValue", 
                "\"CurrentValue\" >= 0");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50);

        builder.Property(x => x.TargetMetric)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("STEPS");

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(x => x.HiddenRewardPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("ACTIVE");

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        // ===== Indexes =====
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_GlobalMissions_Status");

        builder.HasIndex(x => new { x.StartDate, x.EndDate })
            .HasDatabaseName("IX_GlobalMissions_TimeWindow");
    }
}
