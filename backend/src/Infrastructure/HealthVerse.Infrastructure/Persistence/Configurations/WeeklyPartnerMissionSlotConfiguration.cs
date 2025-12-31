using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for WeeklyPartnerMissionSlot entity.
/// Ensures one user can only participate in one partner mission per week.
/// </summary>
public sealed class WeeklyPartnerMissionSlotConfiguration : IEntityTypeConfiguration<WeeklyPartnerMissionSlot>
{
    public void Configure(EntityTypeBuilder<WeeklyPartnerMissionSlot> builder)
    {
        // Table mapping
        builder.ToTable("WeeklyPartnerMissionSlots", "missions");

        // Composite Primary Key equivalent via unique constraint
        // WeekId + UserId must be unique
        builder.HasKey(x => new { x.WeekId, x.UserId });

        // ===== Properties =====
        builder.Property(x => x.WeekId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MissionId)
            .IsRequired();

        // ===== Indexes =====
        builder.HasIndex(x => x.MissionId)
            .HasDatabaseName("IX_WPMSlots_Mission");
    }
}
