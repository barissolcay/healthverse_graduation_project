using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserGoal entity.
/// </summary>
public sealed class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("UserGoals", "tasks", t =>
        {
            t.HasCheckConstraint("CHK_UserGoals_Completed", 
                "\"CompletedAt\" IS NULL OR \"CurrentValue\" >= \"TargetValue\"");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50);

        builder.Property(x => x.TargetMetric)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ValidUntil)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt);

        // ===== Indexes =====
        // Aktif hedefler için
        builder.HasIndex(x => new { x.UserId, x.ValidUntil })
            .HasDatabaseName("IX_UserGoals_User_Active");

        // Tamamlanan hedefler için
        builder.HasIndex(x => new { x.UserId, x.CompletedAt })
            .HasDatabaseName("IX_UserGoals_User_Completed");
    }
}
