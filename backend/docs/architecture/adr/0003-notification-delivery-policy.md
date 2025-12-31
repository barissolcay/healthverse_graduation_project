# ADR 0003: Notification Delivery Policy

## Status
Accepted (2024-12-30)

## Context
Currently, the application generates notifications (In-App) in various places (Services, Jobs). 
Push notifications are handled separately and inconsistently, often tightly coupled with specific job logic or missing entirely.
We need a standardized, centralized way to decide **"Who gets a notification, through which channel, and when?"**.
This is critical for user experience (avoiding spam) and architectural cleaness (decoupling Logic from Delivery).

## Decision
We will implement a **"Single Entry Point"** policy with a **"Channel Decision Engine"**.

### 1. Single Entry Point
All business logic (Services, Jobs, Commands) MUST use `INotificationService` (or a higher-level Domain Event like `NotificationCreated`) to report an event. 
Direct usage of `Notification.Create()` entity method is restricted to the Service layer's internal logic.
Direct usage of `FirebaseMessaging` in Business Logic is **FORBIDDEN**.

### 2. Channel Decision Engine (Policy)
When `INotificationService.SendAsync(...)` is called, the system will evaluate delivery channels based on:
- **In-App**: Enabled by default for ALL categories (User History).
- **Push**: Enabled selectively based on:
    1.  **Category Policy**: Is this category "Push-worthy"? (e.g. `STREAK_WARNING` = Yes, `XP_GAIN` = Maybe No)
    2.  **User Preference**: Did the user explicitly disable this category?
    3.  **Device Availability**: Does the user have a registered FCM token?
    4.  **Quiet Hours**: Is it currently within user's quiet hours? (If yes, queue for later or drop, depending on urgency). *Decision: For MVP, drop if non-critical, or send silently.* -> **Revised**: MVP: Send anyway (let OS handle DND) OR simple check "if (urgent) send else skip".

### 3. Category Definition
We map technical `NotificationType` to user-facing `NotificationCategory`.
*   **STREAK**: Daily streak warnings, freeze alerts using `DailyStreakJob`.
*   **SOCIAL**: Friend requests, duel invites, league updates.
*   **MISSION**: Partner mission poke, mission completion.
*   **ACHIEVEMENT**: Level up, badge earned (mostly In-App only).
*   **SYSTEM**: Maintenance, announcements (Always Push).

### 4. User Preferences
A new table `UserNotificationPreferences` will store overrides.
Default: **ALL PUSH ENABLED** (Opt-out model).

## Consequences
### Positive
- **Consistency**: Centralized logic prevents "forgotten push" bugs.
- **User Control**: Users can opt-out of specific noise without turning off all app notifications.
- **Maintainability**: Changing a notification from "In-App only" to "Push" is a configuration/policy change, not a code rewrite in every job.

### Negative
- **Complexity**: Adds a specific "Policy" layer/class (`INotificationPushPolicy`) and Preference entities.
- **Database Load**: Checking preferences for every bulk notification (e.g. League end) requires efficient caching or batch querying.

## Tech Stack Changes
- **Domain**: `NotificationCategory` enum.
- **Infrastructure**: `FirebaseService` refactored to be a "dumb" sender.
- **Application**: `NotificationService` implements the orchestration logic (Create Entity -> Check Policy -> Call Infra).
