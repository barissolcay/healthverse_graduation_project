using HealthVerse.Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserDailyStats entity.
/// Composite Primary Key: (UserId, LogDate)
/// </summary>
public sealed class UserDailyStatsConfiguration : IEntityTypeConfiguration<UserDailyStats>
{
    public void Configure(EntityTypeBuilder<UserDailyStats> builder)
    {
        // Table mapping
        builder.ToTable("UserDailyStats", "gamification");

        // Composite Primary Key
        builder.HasKey(x => new { x.UserId, x.LogDate });

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.LogDate)
            .IsRequired();

        builder.Property(x => x.DailySteps)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.DailyPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Primary key already provides index on (UserId, LogDate)
    }
}
