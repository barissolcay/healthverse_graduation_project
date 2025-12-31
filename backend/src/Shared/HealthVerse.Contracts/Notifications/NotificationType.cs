namespace HealthVerse.Contracts.Notifications;

/// <summary>
/// Notification type constants for cross-module communication.
/// These constants define all supported notification types in the system.
/// </summary>
public static class NotificationType
{
    // ===== Streak =====
    public const string STREAK_FROZEN = "STREAK_FROZEN";
    public const string STREAK_LOST = "STREAK_LOST";
    public const string STREAK_REMINDER = "STREAK_REMINDER";
    
    // ===== Duel =====
    public const string DUEL_REQUEST = "DUEL_REQUEST";
    public const string DUEL_ACCEPTED = "DUEL_ACCEPTED";
    public const string DUEL_REJECTED = "DUEL_REJECTED";
    public const string DUEL_EXPIRED = "DUEL_EXPIRED";
    public const string DUEL_FINISHED = "DUEL_FINISHED";
    public const string DUEL_POKE = "DUEL_POKE";
    public const string DUEL_ENDING = "DUEL_ENDING";
    
    // ===== Task =====
    public const string TASK_ASSIGNED = "TASK_ASSIGNED";
    public const string TASK_COMPLETED = "TASK_COMPLETED";
    public const string TASK_EXPIRING = "TASK_EXPIRING";
    public const string TASK_CLAIM_PENDING = "TASK_CLAIM_PENDING";
    
    // ===== Goal =====
    public const string GOAL_COMPLETED = "GOAL_COMPLETED";
    public const string GOAL_EXPIRING = "GOAL_EXPIRING";
    public const string GOAL_ARCHIVED = "GOAL_ARCHIVED";
    
    // ===== Partner Mission =====
    public const string PARTNER_MATCHED = "PARTNER_MATCHED";
    public const string PARTNER_COMPLETED = "PARTNER_COMPLETED";
    public const string PARTNER_POKE = "PARTNER_POKE";
    public const string PARTNER_PROGRESS = "PARTNER_PROGRESS";
    public const string PARTNER_ENDING = "PARTNER_ENDING";
    
    // ===== Global Mission =====
    public const string GLOBAL_MISSION_NEW = "GLOBAL_MISSION_NEW";
    public const string GLOBAL_MISSION_JOINED = "GLOBAL_MISSION_JOINED";
    public const string GLOBAL_MISSION_ENDING = "GLOBAL_MISSION_ENDING";
    public const string GLOBAL_MISSION_COMPLETED = "GLOBAL_MISSION_COMPLETED";
    public const string GLOBAL_MISSION_TOP3 = "GLOBAL_MISSION_TOP3";
    
    // ===== League =====
    public const string LEAGUE_PROMOTED = "LEAGUE_PROMOTED";
    public const string LEAGUE_DEMOTED = "LEAGUE_DEMOTED";
    public const string LEAGUE_STAYED = "LEAGUE_STAYED";
    public const string LEAGUE_NEW_WEEK = "LEAGUE_NEW_WEEK";
    public const string LEAGUE_PROMOTION_ZONE = "LEAGUE_PROMOTION_ZONE";
    public const string LEAGUE_DEMOTION_ZONE = "LEAGUE_DEMOTION_ZONE";
    public const string LEAGUE_WEEK_ENDING = "LEAGUE_WEEK_ENDING";
    
    // ===== Social =====
    public const string NEW_FOLLOWER = "NEW_FOLLOWER";
    public const string MUTUAL_FRIEND = "MUTUAL_FRIEND";
    
    // ===== Milestone =====
    public const string MILESTONE_BADGE = "MILESTONE_BADGE";
    public const string MILESTONE_TITLE = "MILESTONE_TITLE";
    public const string MILESTONE_FREEZE = "MILESTONE_FREEZE";
    public const string MILESTONE_APPROACHING = "MILESTONE_APPROACHING";
    
    // ===== System =====
    public const string SYSTEM = "SYSTEM";
    public const string WELCOME = "WELCOME";
    public const string DAILY_REMINDER = "DAILY_REMINDER";
    public const string WEEKLY_SUMMARY = "WEEKLY_SUMMARY";
}
