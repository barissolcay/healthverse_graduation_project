using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for Friendship entity.
/// Composite Primary Key: (FollowerId, FollowingId)
/// Self-follow prevention via check constraint.
/// </summary>
public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("Friendships", "social", t =>
        {
            // Kendini takip etme yasağı
            t.HasCheckConstraint("CHK_Friendships_NoSelf", "\"FollowerId\" <> \"FollowingId\"");
        });

        // Composite Primary Key
        builder.HasKey(x => new { x.FollowerId, x.FollowingId });

        // ===== Properties =====
        builder.Property(x => x.FollowerId)
            .IsRequired();

        builder.Property(x => x.FollowingId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Reverse lookup için index (FollowingId, FollowerId)
        builder.HasIndex(x => new { x.FollowingId, x.FollowerId })
            .HasDatabaseName("IX_Friendships_ByFollowing");

        // ===== Foreign Keys =====
        // NOT: Navigation property olmadan FK tanımı.
        // EF Core 7+ ile shadow FK kullanılabilir ama burada explicit tanım tercih edildi.
        // Users tablosuyla ilişki migration'da manuel eklenecek (ON DELETE CASCADE).
    }
}
