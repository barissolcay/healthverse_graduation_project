using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserInterest entity.
/// Composite PK: (UserId, ActivityType)
/// </summary>
public sealed class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        // Table mapping
        builder.ToTable("UserInterests", "tasks");

        // Composite Primary Key
        builder.HasKey(x => new { x.UserId, x.ActivityType });

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Indexes =====
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserInterests_User");

        builder.HasIndex(x => x.ActivityType)
            .HasDatabaseName("IX_UserInterests_Activity");
    }
}
