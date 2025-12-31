using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for TaskTemplate entity.
/// </summary>
public sealed class TaskTemplateConfiguration : IEntityTypeConfiguration<TaskTemplate>
{
    public void Configure(EntityTypeBuilder<TaskTemplate> builder)
    {
        // Table mapping
        builder.ToTable("TaskTemplates", "tasks");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.Category)
            .HasMaxLength(50);

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50);

        builder.Property(x => x.TargetMetric)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.RewardPoints)
            .IsRequired();

        builder.Property(x => x.BadgeId)
            .HasMaxLength(50);

        builder.Property(x => x.TitleId)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ===== Indexes =====
        // Aktif şablonlar için
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_TaskTemplates_Active");

        // Aktivite tipine göre filtreleme
        builder.HasIndex(x => x.ActivityType)
            .HasDatabaseName("IX_TaskTemplates_Activity");
    }
}
