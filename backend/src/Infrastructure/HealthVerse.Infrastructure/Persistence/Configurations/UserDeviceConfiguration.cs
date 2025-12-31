using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserDevice entity.
/// </summary>
public sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        // Table mapping
        builder.ToTable("UserDevices", "notifications");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PushToken)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DeviceModel)
            .HasMaxLength(100);

        builder.Property(x => x.AppVersion)
            .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastActiveAt)
            .IsRequired();

        // ===== Indexes =====
        // Token unique olmalı
        builder.HasIndex(x => x.PushToken)
            .IsUnique()
            .HasDatabaseName("UX_UserDevices_Token");

        // Kullanıcının cihazlarını bulmak için
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserDevices_User");
    }
}
