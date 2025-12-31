using HealthVerse.Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// MilestoneReward EF Core configuration.
/// </summary>
public sealed class MilestoneRewardConfiguration : IEntityTypeConfiguration<MilestoneReward>
{
    public void Configure(EntityTypeBuilder<MilestoneReward> builder)
    {
        builder.ToTable("MilestoneRewards", "gamification");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RewardType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Metric)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.FreezeReward)
            .HasDefaultValue(0);

        builder.Property(x => x.PointsReward)
            .HasDefaultValue(0);

        builder.Property(x => x.IconName)
            .HasMaxLength(100);

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Unique constraint: Her milestone kodu benzersiz olmalı
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_MilestoneRewards_Code");

        // Aktif milestone'ları metriğe göre sorgulama
        builder.HasIndex(x => new { x.IsActive, x.Metric })
            .HasDatabaseName("IX_MilestoneRewards_Metric");
    }
}

/// <summary>
/// UserMilestone EF Core configuration.
/// </summary>
public sealed class UserMilestoneConfiguration : IEntityTypeConfiguration<UserMilestone>
{
    public void Configure(EntityTypeBuilder<UserMilestone> builder)
    {
        builder.ToTable("UserMilestones", "gamification");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MilestoneRewardId)
            .IsRequired();

        builder.Property(x => x.AchievedAt)
            .IsRequired();

        builder.Property(x => x.IsClaimed)
            .HasDefaultValue(false);

        builder.Property(x => x.ClaimedAt);

        // Unique constraint: Her kullanıcı her milestone'u bir kez kazanabilir
        builder.HasIndex(x => new { x.UserId, x.MilestoneRewardId })
            .IsUnique()
            .HasDatabaseName("IX_UserMilestones_UserMilestone");

        // Kullanıcının tüm milestone'larını bulmak için
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserMilestones_User");
    }
}
