using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserBlock entity.
/// Composite Primary Key: (BlockerId, BlockedId)
/// Self-block prevention via check constraint.
/// </summary>
public sealed class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        // Table mapping with check constraint
        builder.ToTable("UserBlocks", "social", t =>
        {
            // Kendini engelleme yasağı
            t.HasCheckConstraint("CHK_UserBlocks_NoSelf", "\"BlockerId\" <> \"BlockedId\"");
        });

        // Composite Primary Key
        builder.HasKey(x => new { x.BlockerId, x.BlockedId });

        // ===== Properties =====
        builder.Property(x => x.BlockerId)
            .IsRequired();

        builder.Property(x => x.BlockedId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Blocker bazlı sorgu için
        builder.HasIndex(x => x.BlockerId)
            .HasDatabaseName("IX_UserBlocks_Blocker");

        // Blocked bazlı sorgu için (kim tarafından engellendim?)
        builder.HasIndex(x => x.BlockedId)
            .HasDatabaseName("IX_UserBlocks_Blocked");

        // ===== Foreign Keys =====
        // NOT: Navigation property olmadan FK tanımı.
        // Users tablosuyla ilişki migration'da manuel eklenecek (ON DELETE CASCADE).
    }
}
