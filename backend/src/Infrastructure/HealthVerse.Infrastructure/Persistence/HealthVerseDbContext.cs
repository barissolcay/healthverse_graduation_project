using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Social.Domain.Entities;
using HealthVerse.Tasks.Domain.Entities;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HealthVerse.Infrastructure.Persistence;

public sealed class HealthVerseDbContext : DbContext
{
    public HealthVerseDbContext(DbContextOptions<HealthVerseDbContext> options)
        : base(options)
    {
    }

    // ===== Identity Module =====
    public DbSet<User> Users { get; set; }
    public DbSet<AuthIdentity> AuthIdentities { get; set; }

    // ===== Gamification Module =====
    public DbSet<PointTransaction> PointTransactions { get; set; }
    public DbSet<UserDailyStats> UserDailyStats { get; set; }
    public DbSet<UserStreakFreezeLog> UserStreakFreezeLogs { get; set; }
    public DbSet<MilestoneReward> MilestoneRewards { get; set; }
    public DbSet<UserMilestone> UserMilestones { get; set; }

    // ===== Competition Module =====
    public DbSet<LeagueConfig> LeagueConfigs { get; set; }
    public DbSet<LeagueRoom> LeagueRooms { get; set; }
    public DbSet<LeagueMember> LeagueMembers { get; set; }
    public DbSet<UserPointsHistory> UserPointsHistories { get; set; }

    // ===== Social Module =====
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<Duel> Duels { get; set; }

    // ===== Tasks Module =====
    public DbSet<TaskTemplate> TaskTemplates { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<UserInterest> UserInterests { get; set; }

    // ===== Missions Module =====
    public DbSet<GlobalMission> GlobalMissions { get; set; }
    public DbSet<GlobalMissionParticipant> GlobalMissionParticipants { get; set; }
    public DbSet<GlobalMissionContribution> GlobalMissionContributions { get; set; }
    public DbSet<WeeklyPartnerMission> WeeklyPartnerMissions { get; set; }
    public DbSet<WeeklyPartnerMissionSlot> WeeklyPartnerMissionSlots { get; set; }

    // ===== Notifications Module =====
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Infrastructure assembly'deki tüm IEntityTypeConfiguration'ları otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
