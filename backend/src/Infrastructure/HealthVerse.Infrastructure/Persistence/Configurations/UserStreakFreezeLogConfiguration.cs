using HealthVerse.Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserStreakFreezeLog entity.
/// Unique constraint: (UserId, UsedDate) - aynı gün iki kez freeze kullanılamaz.
/// </summary>
public sealed class UserStreakFreezeLogConfiguration : IEntityTypeConfiguration<UserStreakFreezeLog>
{
    public void Configure(EntityTypeBuilder<UserStreakFreezeLog> builder)
    {
        // Table mapping
        builder.ToTable("UserStreakFreezeLog", "gamification");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.UsedDate)
            .IsRequired();

        builder.Property(x => x.StreakCountAtTime)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Unique constraint: Aynı kullanıcı aynı gün birden fazla dondurma hakkı kullanamaz
        builder.HasIndex(x => new { x.UserId, x.UsedDate })
            .IsUnique()
            .HasDatabaseName("UX_UserStreakFreezeLog_UserDate");

        // Index for querying user's freeze history
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserStreakFreezeLog_User");
    }
}
