using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "social");

            migrationBuilder.EnsureSchema(
                name: "missions");

            migrationBuilder.EnsureSchema(
                name: "competition");

            migrationBuilder.EnsureSchema(
                name: "gamification");

            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.EnsureSchema(
                name: "tasks");

            migrationBuilder.CreateTable(
                name: "AuthIdentities",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirebaseUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duels",
                schema: "social",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "WAITING"),
                    ChallengerScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OpponentScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChallengerLastPokeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OpponentLastPokeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duels", x => x.Id);
                    table.CheckConstraint("CHK_Duels_ActiveHasEnd", "\"Status\" <> 'ACTIVE' OR \"EndDate\" IS NOT NULL");
                    table.CheckConstraint("CHK_Duels_ActiveHasStart", "\"Status\" <> 'ACTIVE' OR \"StartDate\" IS NOT NULL");
                    table.CheckConstraint("CHK_Duels_DurationDays", "\"DurationDays\" BETWEEN 1 AND 7");
                    table.CheckConstraint("CHK_Duels_ExpiredHasNoDates", "\"Status\" <> 'EXPIRED' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");
                    table.CheckConstraint("CHK_Duels_FinishedHasEndAndResult", "\"Status\" <> 'FINISHED' OR (\"EndDate\" IS NOT NULL AND \"Result\" IS NOT NULL)");
                    table.CheckConstraint("CHK_Duels_FinishedHasStart", "\"Status\" <> 'FINISHED' OR \"StartDate\" IS NOT NULL");
                    table.CheckConstraint("CHK_Duels_NoSelf", "\"ChallengerId\" <> \"OpponentId\"");
                    table.CheckConstraint("CHK_Duels_RejectedHasNoDates", "\"Status\" <> 'REJECTED' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");
                    table.CheckConstraint("CHK_Duels_Result", "\"Result\" IS NULL OR \"Result\" IN ('CHALLENGER_WIN','OPPONENT_WIN','BOTH_WIN','BOTH_LOSE')");
                    table.CheckConstraint("CHK_Duels_ResultOnlyWhenFinished", "\"Result\" IS NULL OR \"Status\" = 'FINISHED'");
                    table.CheckConstraint("CHK_Duels_Scores", "\"ChallengerScore\" >= 0 AND \"OpponentScore\" >= 0");
                    table.CheckConstraint("CHK_Duels_Status", "\"Status\" IN ('WAITING','ACTIVE','FINISHED','REJECTED','EXPIRED')");
                    table.CheckConstraint("CHK_Duels_TargetValue", "\"TargetValue\" > 0");
                    table.CheckConstraint("CHK_Duels_TimeOrder", "\"StartDate\" IS NULL OR \"EndDate\" IS NULL OR \"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("CHK_Duels_WaitingHasNoDates", "\"Status\" <> 'WAITING' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                schema: "social",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => new { x.FollowerId, x.FollowingId });
                    table.CheckConstraint("CHK_Friendships_NoSelf", "\"FollowerId\" <> \"FollowingId\"");
                });

            migrationBuilder.CreateTable(
                name: "GlobalMissionContributions",
                schema: "missions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalMissionContributions", x => x.Id);
                    table.CheckConstraint("CHK_GlobalMissionContributions_Amount", "\"Amount\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "GlobalMissionParticipants",
                schema: "missions",
                columns: table => new
                {
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionValue = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    IsRewardClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalMissionParticipants", x => new { x.MissionId, x.UserId });
                    table.CheckConstraint("CHK_GlobalMissionParticipants_ContributionValue", "\"ContributionValue\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "GlobalMissions",
                schema: "missions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "STEPS"),
                    TargetValue = table.Column<long>(type: "bigint", nullable: false),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    HiddenRewardPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalMissions", x => x.Id);
                    table.CheckConstraint("CHK_GlobalMissions_CurrentValue", "\"CurrentValue\" >= 0");
                    table.CheckConstraint("CHK_GlobalMissions_Status", "\"Status\" IN ('DRAFT','ACTIVE','FINISHED','CANCELLED')");
                    table.CheckConstraint("CHK_GlobalMissions_TargetValue", "\"TargetValue\" > 0");
                    table.CheckConstraint("CHK_GlobalMissions_TimeWindow", "\"EndDate\" > \"StartDate\"");
                });

            migrationBuilder.CreateTable(
                name: "LeagueConfigs",
                schema: "competition",
                columns: table => new
                {
                    TierName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TierOrder = table.Column<int>(type: "integer", nullable: false),
                    PromotePercentage = table.Column<int>(type: "integer", nullable: false),
                    DemotePercentage = table.Column<int>(type: "integer", nullable: false),
                    MinRoomSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    MaxRoomSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 20)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueConfigs", x => x.TierName);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneRewards",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RewardType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Metric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    FreezeReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PointsReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IconName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneRewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDeliveries",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Push"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceIdText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    RewardPoints = table.Column<int>(type: "integer", nullable: false),
                    BadgeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TitleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                schema: "social",
                columns: table => new
                {
                    BlockerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => new { x.BlockerId, x.BlockedId });
                    table.CheckConstraint("CHK_UserBlocks_NoSelf", "\"BlockerId\" <> \"BlockedId\"");
                });

            migrationBuilder.CreateTable(
                name: "UserDailyStats",
                schema: "gamification",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DailySteps = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DailyPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyStats", x => new { x.UserId, x.LogDate });
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PushToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeviceModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AppVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGoals",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoals", x => x.Id);
                    table.CheckConstraint("CHK_UserGoals_Completed", "\"CompletedAt\" IS NULL OR \"CurrentValue\" >= \"TargetValue\"");
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                schema: "tasks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => new { x.UserId, x.ActivityType });
                });

            migrationBuilder.CreateTable(
                name: "UserMilestones",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneRewardId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMilestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPointsHistory",
                schema: "competition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PeriodId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    LeagueRank = table.Column<int>(type: "integer", nullable: true),
                    TierAtTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPointsHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    AvatarId = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TotalPoints = table.Column<long>(type: "bigint", nullable: false),
                    FreezeInventory = table.Column<int>(type: "integer", nullable: false),
                    StreakCount = table.Column<int>(type: "integer", nullable: false),
                    LastStreakDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LongestStreakCount = table.Column<int>(type: "integer", nullable: false),
                    TotalTasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalDuelsWon = table.Column<int>(type: "integer", nullable: false),
                    TotalGlobalMissions = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SelectedTitleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HealthPermissionGranted = table.Column<bool>(type: "boolean", nullable: false),
                    HealthPermissionGrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStreakFreezeLog",
                schema: "gamification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StreakCountAtTime = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStreakFreezeLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTasks",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RewardClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTasks", x => x.Id);
                    table.CheckConstraint("CHK_UserTasks_CompletedAt", "\"Status\" <> 'COMPLETED' OR \"CompletedAt\" IS NOT NULL");
                    table.CheckConstraint("CHK_UserTasks_FailedAt", "\"Status\" <> 'FAILED' OR \"FailedAt\" IS NOT NULL");
                    table.CheckConstraint("CHK_UserTasks_RewardClaimedAt", "\"Status\" <> 'REWARD_CLAIMED' OR \"RewardClaimedAt\" IS NOT NULL");
                    table.CheckConstraint("CHK_UserTasks_RewardImpliesCompleted", "\"Status\" <> 'REWARD_CLAIMED' OR \"CompletedAt\" IS NOT NULL");
                    table.CheckConstraint("CHK_UserTasks_TimeWindow", "\"ValidUntil\" > \"AssignedAt\"");
                });

            migrationBuilder.CreateTable(
                name: "WeeklyPartnerMissions",
                schema: "missions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetMetric = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "STEPS"),
                    TargetValue = table.Column<int>(type: "integer", nullable: false, defaultValue: 100000),
                    InitiatorProgress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PartnerProgress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    InitiatorLastPokeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PartnerLastPokeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyPartnerMissions", x => x.Id);
                    table.CheckConstraint("CHK_WPM_NoSelf", "\"InitiatorId\" <> \"PartnerId\"");
                    table.CheckConstraint("CHK_WPM_Progress", "\"InitiatorProgress\" >= 0 AND \"PartnerProgress\" >= 0");
                    table.CheckConstraint("CHK_WPM_Status", "\"Status\" IN ('ACTIVE','FINISHED','CANCELLED','EXPIRED')");
                    table.CheckConstraint("CHK_WPM_TargetValue", "\"TargetValue\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "WeeklyPartnerMissionSlots",
                schema: "missions",
                columns: table => new
                {
                    WeekId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyPartnerMissionSlots", x => new { x.WeekId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "LeagueRooms",
                schema: "competition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Tier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueRooms_LeagueConfigs_Tier",
                        column: x => x.Tier,
                        principalSchema: "competition",
                        principalTable: "LeagueConfigs",
                        principalColumn: "TierName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeagueMembers",
                schema: "competition",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PointsInRoom = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RankSnapshot = table.Column<int>(type: "integer", nullable: true),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMembers", x => new { x.RoomId, x.UserId });
                    table.ForeignKey(
                        name: "FK_LeagueMembers_LeagueRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "competition",
                        principalTable: "LeagueRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "competition",
                table: "LeagueConfigs",
                columns: new[] { "TierName", "DemotePercentage", "MaxRoomSize", "MinRoomSize", "PromotePercentage", "TierOrder" },
                values: new object[,]
                {
                    { "ANTRENMAN", 10, 20, 10, 20, 2 },
                    { "DAYANIKLILIK", 10, 20, 10, 20, 6 },
                    { "FORM", 10, 20, 10, 20, 4 },
                    { "ISINMA", 0, 20, 10, 30, 1 },
                    { "KONDISYON", 10, 20, 10, 20, 5 },
                    { "SAMPIYON", 20, 20, 10, 0, 7 },
                    { "TEMPO", 10, 20, 10, 20, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthIdentities_ProviderEmail",
                schema: "identity",
                table: "AuthIdentities",
                columns: new[] { "Provider", "ProviderEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthIdentities_User",
                schema: "identity",
                table: "AuthIdentities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_AuthIdentities_FirebaseUid",
                schema: "identity",
                table: "AuthIdentities",
                column: "FirebaseUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Duels_ActivityType",
                schema: "social",
                table: "Duels",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_Duels_Challenger",
                schema: "social",
                table: "Duels",
                column: "ChallengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Duels_Expire_WaitingCreatedAt",
                schema: "social",
                table: "Duels",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Duels_Opponent",
                schema: "social",
                table: "Duels",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Duels_Pair",
                schema: "social",
                table: "Duels",
                columns: new[] { "ChallengerId", "OpponentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_ByFollowing",
                schema: "social",
                table: "Friendships",
                columns: new[] { "FollowingId", "FollowerId" });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalMissionContrib_MissionTime",
                schema: "missions",
                table: "GlobalMissionContributions",
                columns: new[] { "MissionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalMissionContrib_UserTime",
                schema: "missions",
                table: "GlobalMissionContributions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_GlobalMissionContributions_Idempotency",
                schema: "missions",
                table: "GlobalMissionContributions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalMissionParticipants_ByUser",
                schema: "missions",
                table: "GlobalMissionParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalMissions_Status",
                schema: "missions",
                table: "GlobalMissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalMissions_TimeWindow",
                schema: "missions",
                table: "GlobalMissions",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueConfigs_TierOrder",
                schema: "competition",
                table: "LeagueConfigs",
                column: "TierOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_UserId",
                schema: "competition",
                table: "LeagueMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_WeekId",
                schema: "competition",
                table: "LeagueMembers",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueRooms_Allocate",
                schema: "competition",
                table: "LeagueRooms",
                columns: new[] { "Tier", "UserCount" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueRooms_Unprocessed",
                schema: "competition",
                table: "LeagueRooms",
                column: "IsProcessed",
                filter: "\"IsProcessed\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueRooms_WeekId",
                schema: "competition",
                table: "LeagueRooms",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneRewards_Code",
                schema: "gamification",
                table: "MilestoneRewards",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneRewards_Metric",
                schema: "gamification",
                table: "MilestoneRewards",
                columns: new[] { "IsActive", "Metric" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NotificationId",
                schema: "notification",
                table: "NotificationDeliveries",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_Status_ScheduledAt",
                schema: "notification",
                table: "NotificationDeliveries",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_UserId",
                schema: "notification",
                table: "NotificationDeliveries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                schema: "notifications",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserTime",
                schema: "notifications",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserUnread",
                schema: "notifications",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CreatedAt",
                schema: "gamification",
                table: "PointTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_IdempotencyKey",
                schema: "gamification",
                table: "PointTransactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId",
                schema: "gamification",
                table: "PointTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId_CreatedAt",
                schema: "gamification",
                table: "PointTransactions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_Active",
                schema: "tasks",
                table: "TaskTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_Activity",
                schema: "tasks",
                table: "TaskTemplates",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_Blocked",
                schema: "social",
                table: "UserBlocks",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_Blocker",
                schema: "social",
                table: "UserBlocks",
                column: "BlockerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_User",
                schema: "notifications",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_UserDevices_Token",
                schema: "notifications",
                table: "UserDevices",
                column: "PushToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_User_Active",
                schema: "tasks",
                table: "UserGoals",
                columns: new[] { "UserId", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_User_Completed",
                schema: "tasks",
                table: "UserGoals",
                columns: new[] { "UserId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_Activity",
                schema: "tasks",
                table: "UserInterests",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_User",
                schema: "tasks",
                table: "UserInterests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMilestones_User",
                schema: "gamification",
                table: "UserMilestones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMilestones_UserMilestone",
                schema: "gamification",
                table: "UserMilestones",
                columns: new[] { "UserId", "MilestoneRewardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPointsHistory_Period",
                schema: "competition",
                table: "UserPointsHistory",
                columns: new[] { "PeriodType", "PeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPointsHistory_User",
                schema: "competition",
                table: "UserPointsHistory",
                columns: new[] { "UserId", "PeriodType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_UserPointsHistory_UserPeriod",
                schema: "competition",
                table: "UserPointsHistory",
                columns: new[] { "UserId", "PeriodType", "PeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStreakFreezeLog_User",
                schema: "gamification",
                table: "UserStreakFreezeLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_UserStreakFreezeLog_UserDate",
                schema: "gamification",
                table: "UserStreakFreezeLog",
                columns: new[] { "UserId", "UsedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_Active_ValidUntil",
                schema: "tasks",
                table: "UserTasks",
                columns: new[] { "UserId", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_Template",
                schema: "tasks",
                table: "UserTasks",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_User_Status",
                schema: "tasks",
                table: "UserTasks",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WPM_ByWeek",
                schema: "missions",
                table: "WeeklyPartnerMissions",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_WPM_Initiator",
                schema: "missions",
                table: "WeeklyPartnerMissions",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WPM_Partner",
                schema: "missions",
                table: "WeeklyPartnerMissions",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "UX_WPM_IdWeek",
                schema: "missions",
                table: "WeeklyPartnerMissions",
                columns: new[] { "Id", "WeekId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WPMSlots_Mission",
                schema: "missions",
                table: "WeeklyPartnerMissionSlots",
                column: "MissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthIdentities",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Duels",
                schema: "social");

            migrationBuilder.DropTable(
                name: "Friendships",
                schema: "social");

            migrationBuilder.DropTable(
                name: "GlobalMissionContributions",
                schema: "missions");

            migrationBuilder.DropTable(
                name: "GlobalMissionParticipants",
                schema: "missions");

            migrationBuilder.DropTable(
                name: "GlobalMissions",
                schema: "missions");

            migrationBuilder.DropTable(
                name: "LeagueMembers",
                schema: "competition");

            migrationBuilder.DropTable(
                name: "MilestoneRewards",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "NotificationDeliveries",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "PointTransactions",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "TaskTemplates",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "UserBlocks",
                schema: "social");

            migrationBuilder.DropTable(
                name: "UserDailyStats",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "UserDevices",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "UserGoals",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "UserInterests",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "UserMilestones",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "UserPointsHistory",
                schema: "competition");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserStreakFreezeLog",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "UserTasks",
                schema: "tasks");

            migrationBuilder.DropTable(
                name: "WeeklyPartnerMissions",
                schema: "missions");

            migrationBuilder.DropTable(
                name: "WeeklyPartnerMissionSlots",
                schema: "missions");

            migrationBuilder.DropTable(
                name: "LeagueRooms",
                schema: "competition");

            migrationBuilder.DropTable(
                name: "LeagueConfigs",
                schema: "competition");
        }
    }
}
