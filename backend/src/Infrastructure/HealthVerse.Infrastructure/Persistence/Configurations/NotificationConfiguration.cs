using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for Notification entity.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Table mapping
        builder.ToTable("Notifications", "notifications");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasMaxLength(1000);

        builder.Property(x => x.ReferenceId);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50);

        builder.Property(x => x.Data);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ReadAt);

        // ===== Indexes =====
        // Kullanıcının bildirimlerini çekmek için
        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Notifications_UserTime");

        // Okunmamış bildirimleri hızlı çekmek için
        builder.HasIndex(x => new { x.UserId, x.IsRead })
            .HasDatabaseName("IX_Notifications_UserUnread");

        // Tip bazlı sorgular
        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_Notifications_Type");
    }
}
