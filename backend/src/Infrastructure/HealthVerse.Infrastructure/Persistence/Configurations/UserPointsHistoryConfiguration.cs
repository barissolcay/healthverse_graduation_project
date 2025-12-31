using HealthVerse.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserPointsHistory entity.
/// </summary>
public sealed class UserPointsHistoryConfiguration : IEntityTypeConfiguration<UserPointsHistory>
{
    public void Configure(EntityTypeBuilder<UserPointsHistory> builder)
    {
        // Table mapping
        builder.ToTable("UserPointsHistory", "competition");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PeriodType)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.PeriodId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.LeagueRank);

        builder.Property(x => x.TierAtTime)
            .HasMaxLength(20);

        builder.Property(x => x.Result)
            .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Unique constraint: Aynı kullanıcı için aynı dönem bir kez kaydedilir
        builder.HasIndex(x => new { x.UserId, x.PeriodType, x.PeriodId })
            .IsUnique()
            .HasDatabaseName("UX_UserPointsHistory_UserPeriod");

        // User history lookup
        builder.HasIndex(x => new { x.UserId, x.PeriodType, x.CreatedAt })
            .HasDatabaseName("IX_UserPointsHistory_User");

        // Period lookup
        builder.HasIndex(x => new { x.PeriodType, x.PeriodId })
            .HasDatabaseName("IX_UserPointsHistory_Period");
    }
}
