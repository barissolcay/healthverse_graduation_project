using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries", "notification");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NotificationId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(DeliveryChannel.Push);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(DeliveryStatus.Pending);

        builder.Property(x => x.ScheduledAt)
            .IsRequired();

        builder.Property(x => x.SentAt);

        builder.Property(x => x.AttemptCount)
            .HasDefaultValue(0);

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes for efficient querying
        builder.HasIndex(x => new { x.Status, x.ScheduledAt })
            .HasDatabaseName("IX_NotificationDeliveries_Status_ScheduledAt");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_NotificationDeliveries_UserId");

        builder.HasIndex(x => x.NotificationId)
            .HasDatabaseName("IX_NotificationDeliveries_NotificationId");
    }
}
