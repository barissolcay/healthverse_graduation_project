using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for WeeklyPartnerMission entity.
/// </summary>
public sealed class WeeklyPartnerMissionConfiguration : IEntityTypeConfiguration<WeeklyPartnerMission>
{
    public void Configure(EntityTypeBuilder<WeeklyPartnerMission> builder)
    {
        // Table mapping with check constraints
        builder.ToTable("WeeklyPartnerMissions", "missions", t =>
        {
            t.HasCheckConstraint("CHK_WPM_NoSelf", 
                "\"InitiatorId\" <> \"PartnerId\"");
            t.HasCheckConstraint("CHK_WPM_Status", 
                "\"Status\" IN ('ACTIVE','FINISHED','CANCELLED','EXPIRED')");
            t.HasCheckConstraint("CHK_WPM_TargetValue", 
                "\"TargetValue\" > 0");
            t.HasCheckConstraint("CHK_WPM_Progress", 
                "\"InitiatorProgress\" >= 0 AND \"PartnerProgress\" >= 0");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.WeekId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.InitiatorId)
            .IsRequired();

        builder.Property(x => x.PartnerId)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50);

        builder.Property(x => x.TargetMetric)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("STEPS");

        builder.Property(x => x.TargetValue)
            .IsRequired()
            .HasDefaultValue(100000);

        builder.Property(x => x.InitiatorProgress)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.PartnerProgress)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("ACTIVE");

        builder.Property(x => x.InitiatorLastPokeAt);
        builder.Property(x => x.PartnerLastPokeAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ===== Indexes =====
        builder.HasIndex(x => x.WeekId)
            .HasDatabaseName("IX_WPM_ByWeek");

        // Composite index for finding by Id+WeekId (for slot FK)
        builder.HasIndex(x => new { x.Id, x.WeekId })
            .IsUnique()
            .HasDatabaseName("UX_WPM_IdWeek");

        // For finding user's missions
        builder.HasIndex(x => x.InitiatorId)
            .HasDatabaseName("IX_WPM_Initiator");

        builder.HasIndex(x => x.PartnerId)
            .HasDatabaseName("IX_WPM_Partner");
    }
}
