using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserNotificationPreference entity.
/// </summary>
public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        // Table mapping
        builder.ToTable("UserNotificationPreferences", "notifications");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.PushEnabled)
            .IsRequired();

        builder.Property(x => x.QuietHoursStart);

        builder.Property(x => x.QuietHoursEnd);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Her kullanıcı her kategori için tek bir tercih kaydı olabilir
        builder.HasIndex(x => new { x.UserId, x.Category })
            .IsUnique()
            .HasDatabaseName("UX_UserNotificationPreferences_User_Category");

        // Kullanıcının tüm tercihlerini bulmak için
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserNotificationPreferences_User");
    }
}
