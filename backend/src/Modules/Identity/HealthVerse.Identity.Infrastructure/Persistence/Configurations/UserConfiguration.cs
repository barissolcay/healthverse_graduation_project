using HealthVerse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for User entity.
/// This keeps the domain entity clean (POCO) and moves all persistence concerns to Infrastructure.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table mapping
        builder.ToTable("Users", "identity");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Value Objects =====
        builder.OwnsOne(x => x.Username, username =>
        {
            username.Property(u => u.Value)
                .HasColumnName("Username")
                .HasMaxLength(30)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();
        });

        // ===== Core Properties =====
        builder.Property(x => x.AvatarId)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.Bio)
            .HasMaxLength(150);

        // ===== Gamification Counters =====
        builder.Property(x => x.TotalPoints)
            .IsRequired();

        builder.Property(x => x.FreezeInventory)
            .IsRequired();

        builder.Property(x => x.StreakCount)
            .IsRequired();

        builder.Property(x => x.LongestStreakCount)
            .IsRequired();

        // ===== Statistics Counters =====
        builder.Property(x => x.TotalTasksCompleted)
            .IsRequired();

        builder.Property(x => x.TotalDuelsWon)
            .IsRequired();

        builder.Property(x => x.TotalGlobalMissions)
            .IsRequired();

        // ===== Social Counters =====
        builder.Property(x => x.FollowingCount)
            .IsRequired();

        builder.Property(x => x.FollowersCount)
            .IsRequired();

        // ===== League =====
        builder.Property(x => x.CurrentTier)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SelectedTitleId)
            .HasMaxLength(50);

        // ===== Health Permission =====
        builder.Property(x => x.HealthPermissionGranted)
            .IsRequired();

        // ===== Metadata =====
        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");
    }
}
