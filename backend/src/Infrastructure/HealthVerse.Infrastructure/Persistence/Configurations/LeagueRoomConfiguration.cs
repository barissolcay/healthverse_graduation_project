using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for LeagueRoom entity.
/// Configures weekly league rooms with tier reference and capacity tracking.
/// </summary>
public sealed class LeagueRoomConfiguration : IEntityTypeConfiguration<LeagueRoom>
{
    public void Configure(EntityTypeBuilder<LeagueRoom> builder)
    {
        // Table mapping
        builder.ToTable("LeagueRooms", "competition");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Value Object: WeekId - Use OwnsOne for proper mapping
        builder.OwnsOne(x => x.WeekId, weekId =>
        {
            weekId.Property(w => w.Value)
                .HasColumnName("WeekId")
                .HasMaxLength(20)
                .IsRequired();

            // Index defined inside OwnsOne
            weekId.HasIndex(w => w.Value)
                .HasDatabaseName("IX_LeagueRooms_WeekId");
        });

        // ===== Properties =====
        builder.Property(x => x.Tier)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.UserCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.StartsAt)
            .IsRequired();

        builder.Property(x => x.EndsAt)
            .IsRequired();

        builder.Property(x => x.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ProcessedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ===== Relationships =====
        // FK: Tier → LeagueConfigs.TierName
        builder.HasOne<LeagueConfig>()
            .WithMany()
            .HasForeignKey(x => x.Tier)
            .HasPrincipalKey(c => c.TierName)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to Members (one-to-many)
        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Indexes =====
        // For room allocation: find rooms by week, tier, and available capacity
        builder.HasIndex(x => new { x.Tier, x.UserCount })
            .HasDatabaseName("IX_LeagueRooms_Allocate");

        // For finding unprocessed rooms at week end
        builder.HasIndex(x => x.IsProcessed)
            .HasFilter("\"IsProcessed\" = false")
            .HasDatabaseName("IX_LeagueRooms_Unprocessed");

        // Redundant index removed. Id is already PK.
        // If composite FK (Id, WeekId) is needed in the future, add unique index on new { x.Id, x.WeekId }
    }
}
