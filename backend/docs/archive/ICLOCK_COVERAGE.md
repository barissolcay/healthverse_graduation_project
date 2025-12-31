# IClock Coverage Map

Amaç: DateTimeOffset.UtcNow/DateTime.UtcNow kullanılan yerleri tespit etmek ve IClock’a taşınacak noktaları belirlemek.

## Özet
- Toplam 80+ kullanım bulundu.
- **Uygulama seviyesinde IClock**: API controller’ları ve Infrastructure jobs’ları zaten `IClock` kullanıyor.
- **Domain seviyesinde UtcNow**: Birçok entity factory/command `DateTimeOffset.UtcNow` kullanıyor. Hex hedefi için domain tarafında `IClock` yerine zamanın dışarıdan verilmesi tercih edilecek (factory parametreleri veya domain service).

## Detaylı Liste

### Shared Kernel
- AggregateRoot.CreatedAt/UpdatedAt, DomainEventBase.OccurredAt: default `UtcNow`. (Bırakılabilir veya `IClock` inject eden base factory ile kapatılabilir.)

### Competition Domain
- LeagueMember.Create → JoinedAt = UtcNow
- UserPointsHistory.CreateWeekly/CreateMonthly → CreatedAt = UtcNow

### Social Domain
- Friendship.Create → CreatedAt = UtcNow
- UserBlock.Create → CreatedAt = UtcNow
- Duel.Create → now = UtcNow (StartDate/EndDate set için kullanılır)

### Tasks Domain
- UserTask.Assign → now = UtcNow (deadline validation)
- UserTask.UpdateProgress → CompletedAt = UtcNow
- UserTask.ClaimReward → RewardClaimedAt = UtcNow
- UserTask.MarkAsFailed → FailedAt = UtcNow
- UserGoal.Create → now = UtcNow (deadline validation)
- UserGoal.UpdateProgress → CompletedAt = UtcNow
- UserInterest.Create → CreatedAt = UtcNow

### Missions Domain
- GlobalMissionParticipant.Create → JoinedAt = UtcNow
- GlobalMissionContribution.Create → CreatedAt = UtcNow
- WeeklyPartnerMission.Create → now = UtcNow

### Gamification Domain
- MilestoneReward.Create → CreatedAt = UtcNow; AchievedAt/ClaimedAt = UtcNow
- UserStreakFreezeLog.Create → CreatedAt = UtcNow
- UserDailyStats.Create → CreatedAt/UpdatedAt = UtcNow; Update methods set UpdatedAt = UtcNow
- PointTransaction.Create → CreatedAt = UtcNow

### Notifications Domain
- Notification.Create → CreatedAt = UtcNow; ReadAt = UtcNow
- NotificationDelivery.Create → CreatedAt/UpdatedAt = UtcNow; state transitions set UpdatedAt = UtcNow
- UserDevice.Create → now = UtcNow; Ping → LastActiveAt = UtcNow

### Identity Domain
- AuthIdentity.Create → now = UtcNow; UpdateLastLogin → LastLoginAt = UtcNow

### Infrastructure Jobs (already IClock injected)
- WeeklySummaryJob, StreakReminderJob, ReminderJob, WeeklyLeagueFinalizeJob (uses _clock.UtcNow and _clock.UtcNow.AddDays(-1)), PartnerMissionFinalizeJob, MilestoneCheckJob, GlobalMissionFinalizeJob, ExpireJob, DailyStreakJob (note: one usage of `DateTime.UtcNow` inside DateOnly.FromDateTime for 30-day window).

### API Controllers (already IClock injected)
- TasksController, GoalsController, PartnerMissionsController, GlobalMissionsController, DuelsController, LeagueController (HoursRemaining), StatusController (timestamp), plus others using `_clock.UtcNow`.

## Önerilen Geçiş
1) **Domain factory/command parametreleri**: Factory methodlara `DateTimeOffset now` parametresi ekleyip çağıran Application handler’dan `IClock.UtcNow` geçmek.
2) **UpdatedAt/CreatedAt defaultları**: Kısa vadede bırakılabilir; uzun vadede `IClock` veren bir `IDateTimeProvider` ile merkezi hale getirilebilir.
3) **DateTime.UtcNow tek kullanım**: DailyStreakJob → `DateTime.UtcNow` yerine `_clock.UtcNow` + `DateOnly.FromDateTime(_clock.UtcNow.AddDays(-30).DateTime)`.
4) **NotificationDelivery**: Migration yok; port + handler geldiğinde `IClock` üzerinden state change zamanları set edilebilir.

## Önceliklendirme
- Önce Application handler’ları `IClock` ile donatıp domain factory’lerine `now` geçir (Competition, Social, Tasks, Missions).
- Sonra jobs içindeki tek `DateTime.UtcNow` kullanımını düzelt.
- AggregateRoot/DomainEventBase defaultları şimdilik kalabilir; kapsamlı refaktörde ele alınır.
