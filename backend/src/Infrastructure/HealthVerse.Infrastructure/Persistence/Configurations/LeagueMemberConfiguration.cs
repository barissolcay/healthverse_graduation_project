using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for LeagueMember entity.
/// Configures room membership with composite primary key and weekly uniqueness.
/// </summary>
public sealed class LeagueMemberConfiguration : IEntityTypeConfiguration<LeagueMember>
{
    public void Configure(EntityTypeBuilder<LeagueMember> builder)
    {
        // Table mapping
        builder.ToTable("LeagueMembers", "competition");

        // Composite Primary Key: (RoomId, UserId)
        builder.HasKey(x => new { x.RoomId, x.UserId });

        // Ignore the Id property from Entity base class (we use composite PK)
        builder.Ignore(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.RoomId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        // Value Object: WeekId - Use OwnsOne for proper mapping
        builder.OwnsOne(x => x.WeekId, weekId =>
        {
            weekId.Property(w => w.Value)
                .HasColumnName("WeekId")
                .HasMaxLength(20)
                .IsRequired();

            // Index MUST be defined inside OwnsOne for owned types
            weekId.HasIndex(w => w.Value)
                .HasDatabaseName("IX_LeagueMembers_WeekId");
        });

        builder.Property(x => x.PointsInRoom)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.RankSnapshot);

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        // ===== Indexes =====
        // Note: For the unique constraint (UserId + WeekId), we need to use raw SQL
        // because EF Core doesn't support mixing owned property in composite indexes easily.
        // We'll add this via a separate migration with raw SQL, or use a different approach.
        
        // Index for UserId (for queries filtering by user)
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_LeagueMembers_UserId");
    }
}
