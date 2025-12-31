using HealthVerse.Contracts.Notifications;
using HealthVerse.Notifications.Domain.Entities;

namespace HealthVerse.Notifications.Application.Services;

/// <summary>
/// NotificationType (string) → NotificationCategory mapping.
/// Kept in Application so Domain remains free of cross-module contracts.
/// </summary>
public static class NotificationTypeCategoryMapping
{
    private static readonly Dictionary<string, NotificationCategory> Mapping = new()
    {
        // Streak
        [NotificationType.STREAK_FROZEN] = NotificationCategory.Streak,
        [NotificationType.STREAK_LOST] = NotificationCategory.Streak,
        [NotificationType.STREAK_REMINDER] = NotificationCategory.Streak,

        // Duel
        [NotificationType.DUEL_REQUEST] = NotificationCategory.Duel,
        [NotificationType.DUEL_ACCEPTED] = NotificationCategory.Duel,
        [NotificationType.DUEL_REJECTED] = NotificationCategory.Duel,
        [NotificationType.DUEL_EXPIRED] = NotificationCategory.Duel,
        [NotificationType.DUEL_FINISHED] = NotificationCategory.Duel,
        [NotificationType.DUEL_POKE] = NotificationCategory.Duel,
        [NotificationType.DUEL_ENDING] = NotificationCategory.Duel,

        // Task
        [NotificationType.TASK_ASSIGNED] = NotificationCategory.Task,
        [NotificationType.TASK_COMPLETED] = NotificationCategory.Task,
        [NotificationType.TASK_EXPIRING] = NotificationCategory.Task,
        [NotificationType.TASK_CLAIM_PENDING] = NotificationCategory.Task,

        // Goal
        [NotificationType.GOAL_COMPLETED] = NotificationCategory.Goal,
        [NotificationType.GOAL_EXPIRING] = NotificationCategory.Goal,
        [NotificationType.GOAL_ARCHIVED] = NotificationCategory.Goal,

        // Partner Mission
        [NotificationType.PARTNER_MATCHED] = NotificationCategory.PartnerMission,
        [NotificationType.PARTNER_COMPLETED] = NotificationCategory.PartnerMission,
        [NotificationType.PARTNER_POKE] = NotificationCategory.PartnerMission,
        [NotificationType.PARTNER_PROGRESS] = NotificationCategory.PartnerMission,
        [NotificationType.PARTNER_ENDING] = NotificationCategory.PartnerMission,

        // Global Mission
        [NotificationType.GLOBAL_MISSION_NEW] = NotificationCategory.GlobalMission,
        [NotificationType.GLOBAL_MISSION_JOINED] = NotificationCategory.GlobalMission,
        [NotificationType.GLOBAL_MISSION_ENDING] = NotificationCategory.GlobalMission,
        [NotificationType.GLOBAL_MISSION_COMPLETED] = NotificationCategory.GlobalMission,
        [NotificationType.GLOBAL_MISSION_TOP3] = NotificationCategory.GlobalMission,

        // League
        [NotificationType.LEAGUE_PROMOTED] = NotificationCategory.League,
        [NotificationType.LEAGUE_DEMOTED] = NotificationCategory.League,
        [NotificationType.LEAGUE_STAYED] = NotificationCategory.League,
        [NotificationType.LEAGUE_NEW_WEEK] = NotificationCategory.League,
        [NotificationType.LEAGUE_PROMOTION_ZONE] = NotificationCategory.League,
        [NotificationType.LEAGUE_DEMOTION_ZONE] = NotificationCategory.League,
        [NotificationType.LEAGUE_WEEK_ENDING] = NotificationCategory.League,

        // Social
        [NotificationType.NEW_FOLLOWER] = NotificationCategory.Social,
        [NotificationType.MUTUAL_FRIEND] = NotificationCategory.Social,

        // Milestone
        [NotificationType.MILESTONE_BADGE] = NotificationCategory.Milestone,
        [NotificationType.MILESTONE_TITLE] = NotificationCategory.Milestone,
        [NotificationType.MILESTONE_FREEZE] = NotificationCategory.Milestone,
        [NotificationType.MILESTONE_APPROACHING] = NotificationCategory.Milestone,

        // System
        [NotificationType.SYSTEM] = NotificationCategory.System,
        [NotificationType.WELCOME] = NotificationCategory.System,
        [NotificationType.DAILY_REMINDER] = NotificationCategory.System,
        [NotificationType.WEEKLY_SUMMARY] = NotificationCategory.System
    };

    public static NotificationCategory GetCategory(string notificationType)
    {
        return Mapping.TryGetValue(notificationType, out var category)
            ? category
            : NotificationCategory.System;
    }

    public static bool GetDefaultPushEnabled(NotificationCategory category)
    {
        return category is not (NotificationCategory.Goal or NotificationCategory.Milestone);
    }
}

