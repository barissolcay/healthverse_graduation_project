using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for UserTask entity.
/// </summary>
public sealed class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
{
    public void Configure(EntityTypeBuilder<UserTask> builder)
    {
        // Table mapping with check constraints
        builder.ToTable("UserTasks", "tasks", t =>
        {
            // Status constraints
            t.HasCheckConstraint("CHK_UserTasks_CompletedAt", 
                "\"Status\" <> 'COMPLETED' OR \"CompletedAt\" IS NOT NULL");
            t.HasCheckConstraint("CHK_UserTasks_RewardClaimedAt", 
                "\"Status\" <> 'REWARD_CLAIMED' OR \"RewardClaimedAt\" IS NOT NULL");
            t.HasCheckConstraint("CHK_UserTasks_FailedAt", 
                "\"Status\" <> 'FAILED' OR \"FailedAt\" IS NOT NULL");
            t.HasCheckConstraint("CHK_UserTasks_RewardImpliesCompleted", 
                "\"Status\" <> 'REWARD_CLAIMED' OR \"CompletedAt\" IS NOT NULL");
            // Time window constraints
            t.HasCheckConstraint("CHK_UserTasks_TimeWindow", 
                "\"ValidUntil\" > \"AssignedAt\"");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TemplateId)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("ACTIVE");

        builder.Property(x => x.ValidUntil)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt);
        builder.Property(x => x.RewardClaimedAt);
        builder.Property(x => x.FailedAt);

        // ===== Indexes =====
        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_UserTasks_User_Status");

        builder.HasIndex(x => new { x.UserId, x.ValidUntil })
            .HasDatabaseName("IX_UserTasks_Active_ValidUntil");

        builder.HasIndex(x => x.TemplateId)
            .HasDatabaseName("IX_UserTasks_Template");
    }
}
