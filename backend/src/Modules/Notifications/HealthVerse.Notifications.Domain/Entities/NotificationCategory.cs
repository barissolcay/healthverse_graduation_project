namespace HealthVerse.Notifications.Domain.Entities;

/// <summary>
/// Bildirim kategorileri.
/// Push policy ve kullanıcı tercihleri bu kategorilere göre yönetilir.
/// </summary>
public enum NotificationCategory
{
    /// <summary>
    /// Streak bildirimleri: STREAK_FROZEN, STREAK_LOST, STREAK_REMINDER
    /// Push default: ENABLED (kritik)
    /// </summary>
    Streak = 1,

    /// <summary>
    /// Duel bildirimleri: DUEL_REQUEST, DUEL_ACCEPTED, DUEL_REJECTED, vb.
    /// Push default: ENABLED
    /// </summary>
    Duel = 2,

    /// <summary>
    /// Task bildirimleri: TASK_ASSIGNED, TASK_COMPLETED, TASK_EXPIRING, vb.
    /// Push default: ENABLED
    /// </summary>
    Task = 3,

    /// <summary>
    /// Goal bildirimleri: GOAL_COMPLETED, GOAL_EXPIRING, GOAL_ARCHIVED
    /// Push default: DISABLED (düşük öncelik)
    /// </summary>
    Goal = 4,

    /// <summary>
    /// Partner mission bildirimleri: PARTNER_MATCHED, PARTNER_COMPLETED, vb.
    /// Push default: ENABLED
    /// </summary>
    PartnerMission = 5,

    /// <summary>
    /// Global mission bildirimleri: GLOBAL_MISSION_NEW, GLOBAL_MISSION_JOINED, vb.
    /// Push default: ENABLED
    /// </summary>
    GlobalMission = 6,

    /// <summary>
    /// League bildirimleri: LEAGUE_PROMOTED, LEAGUE_DEMOTED, LEAGUE_NEW_WEEK, vb.
    /// Push default: ENABLED
    /// </summary>
    League = 7,

    /// <summary>
    /// Social bildirimleri: NEW_FOLLOWER, MUTUAL_FRIEND
    /// Push default: ENABLED
    /// </summary>
    Social = 8,

    /// <summary>
    /// Milestone bildirimleri: MILESTONE_BADGE, MILESTONE_TITLE, vb.
    /// Push default: DISABLED (düşük öncelik)
    /// </summary>
    Milestone = 9,

    /// <summary>
    /// System bildirimleri: SYSTEM, WELCOME, DAILY_REMINDER, WEEKLY_SUMMARY
    /// Push default: ENABLED (kritik)
    /// </summary>
    System = 10
}
