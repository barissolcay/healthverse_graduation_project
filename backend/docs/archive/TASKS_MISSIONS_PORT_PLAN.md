# Tasks & Missions Port Plan (Hex Prep)

Amaç: Tasks, Goals, Partner Missions ve Global Missions akışlarını Application katmanına taşıyıp portlar üzerinden soyutlamak; Social/Notification bağımlılıklarını portlarla çözmek.

## Port İmzaları (Taslak)

```csharp
public interface IUserTaskRepository
{
    Task<List<UserTask>> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<List<UserTask>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct);
    Task<UserTask?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken ct);
    Task<List<UserTask>> GetExpiredAsync(Guid userId, DateTimeOffset now, CancellationToken ct);
    Task AddAsync(UserTask task, CancellationToken ct);
}

public interface ITaskTemplateRepository
{
    Task<Dictionary<Guid, TaskTemplate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<List<TaskTemplate>> GetAllAsync(bool activeOnly, CancellationToken ct);
}

public interface IUserGoalRepository
{
    Task AddAsync(UserGoal goal, CancellationToken ct);
    Task<List<UserGoal>> GetActiveByUserAsync(Guid userId, DateTimeOffset now, CancellationToken ct);
    Task<List<UserGoal>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct);
    Task<UserGoal?> GetByIdAsync(Guid goalId, Guid userId, CancellationToken ct);
    Task RemoveAsync(UserGoal goal, CancellationToken ct);
}

public interface IUserInterestRepository
{
    Task<List<string>> GetActivityTypesAsync(Guid userId, CancellationToken ct);
    Task ReplaceAsync(Guid userId, IEnumerable<string> activityTypes, CancellationToken ct);
}

public interface IPartnerMissionRepository
{
    Task<WeeklyPartnerMission?> GetByIdAsync(Guid missionId, CancellationToken ct);
    Task<WeeklyPartnerMission?> GetActiveByUserAsync(string weekId, Guid userId, CancellationToken ct);
    Task<List<WeeklyPartnerMission>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct);
    Task AddAsync(WeeklyPartnerMission mission, CancellationToken ct);
}

public interface IPartnerMissionSlotRepository
{
    Task<bool> IsUserBusyAsync(string weekId, Guid userId, CancellationToken ct);
    Task AddAsync(WeeklyPartnerMissionSlot slot, CancellationToken ct);
}

public interface IGlobalMissionRepository
{
    Task<List<GlobalMission>> GetActiveAsync(DateTimeOffset now, CancellationToken ct);
    Task<List<GlobalMission>> GetHistoryAsync(DateTimeOffset now, int limit, CancellationToken ct);
    Task<GlobalMission?> GetByIdAsync(Guid missionId, CancellationToken ct);
    Task UpdateCacheAsync(GlobalMission mission, CancellationToken ct); // CurrentValue/Status değişimleri için
}

public interface IGlobalMissionParticipantRepository
{
    Task<GlobalMissionParticipant?> GetAsync(Guid missionId, Guid userId, CancellationToken ct);
    Task AddAsync(GlobalMissionParticipant participant, CancellationToken ct);
    Task<Dictionary<Guid, GlobalMissionParticipant>> GetByUserAsync(Guid userId, IEnumerable<Guid> missionIds, CancellationToken ct);
    Task<List<GlobalMissionParticipant>> GetTopContributorsAsync(Guid missionId, int take, CancellationToken ct);
    Task<int> CountAsync(Guid missionId, CancellationToken ct);
}

public interface IGlobalMissionContributionRepository
{
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct);
    Task AddAsync(GlobalMissionContribution contribution, CancellationToken ct);
}

// Paylaşılan portlar (diğer dokümanlarla ortak)
public interface IFriendshipRepository { /* mutual check + lists (bkz: SOCIAL_DUELS_PORT_PLAN) */ }
public interface IUserDirectory { /* username/avatar sözlüğü */ }
public interface INotificationPort { /* bildirim ekle */ }

public interface ITasksUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public interface IMissionsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

_Notlar_: Task/Goal ilerleme update akışları (sync jobs) burada yok; gerektiğinde `UpdateProgressAsync` benzeri metotlar eklenebilir. `UpdateCacheAsync` global mission katkı işleyicilerinde CurrentValue güncellemesi için kullanılır.

## Use-Case / Handler Akışı (Taslak)

### Tasks
- **GetActiveTasksQuery** (TasksController.GetActiveTasks):
  - Ports: `IUserTaskRepository.GetExpiredAsync` (fail işareti), `IUserTaskRepository.GetActiveByUserAsync`, `ITaskTemplateRepository.GetByIdsAsync`
  - UoW: `ITasksUnitOfWork.SaveChangesAsync` (expire sonrası)
- **GetCompletedTasksQuery**: `IUserTaskRepository.GetCompletedByUserAsync`, `ITaskTemplateRepository.GetByIdsAsync`
- **ClaimRewardCommand**: `IUserTaskRepository.GetByIdAsync`, `ITaskTemplateRepository.GetByIdsAsync` (tekli), domain `ClaimReward`, `ITasksUnitOfWork.SaveChangesAsync`
- **GetTaskTemplatesQuery**: `ITaskTemplateRepository.GetAllAsync`

### Goals
- **CreateGoalCommand**: `IUserGoalRepository.AddAsync`, `ITasksUnitOfWork.SaveChangesAsync`
- **GetActiveGoalsQuery**: `IUserGoalRepository.GetActiveByUserAsync`
- **GetCompletedGoalsQuery**: `IUserGoalRepository.GetCompletedByUserAsync`
- **DeleteGoalCommand**: `IUserGoalRepository.GetByIdAsync`, `RemoveAsync`, `ITasksUnitOfWork.SaveChangesAsync`

### Partner Missions
- **GetAvailableFriendsQuery**: `IFriendshipRepository.GetMutualFriendsAsync` (new method), `IPartnerMissionSlotRepository.IsUserBusyAsync`, `IUserDirectory.GetProfilesAsync`
- **PairWithFriendCommand**: `IFriendshipRepository.IsMutualAsync`, `IPartnerMissionSlotRepository.IsUserBusyAsync`, `IPartnerMissionRepository.AddAsync`, `IPartnerMissionSlotRepository.AddAsync` (2 slot), `INotificationPort.AddAsync`; UoW: `IMissionsUnitOfWork.SaveChangesAsync`
- **GetActivePartnerMissionQuery**: `IPartnerMissionRepository.GetActiveByUserAsync`, `IUserDirectory.GetProfilesAsync`
- **PokePartnerCommand**: `IPartnerMissionRepository.GetByIdAsync`, domain `Poke`, `INotificationPort.AddAsync`; UoW: `IMissionsUnitOfWork.SaveChangesAsync`
- **GetPartnerMissionHistoryQuery**: `IPartnerMissionRepository.GetHistoryByUserAsync`, `IUserDirectory.GetProfilesAsync`

### Global Missions
- **GetActiveGlobalMissionsQuery**: `IGlobalMissionRepository.GetActiveAsync`, `IGlobalMissionParticipantRepository.GetByUserAsync`, `IGlobalMissionParticipantRepository.GetTopContributorsAsync`, `IGlobalMissionParticipantRepository.CountAsync`, `IUserDirectory.GetProfilesAsync`
- **JoinGlobalMissionCommand**: `IGlobalMissionRepository.GetByIdAsync`, `IGlobalMissionParticipantRepository.GetAsync/AddAsync`, `INotificationPort.AddAsync`; UoW: `IMissionsUnitOfWork.SaveChangesAsync`
- **GetGlobalMissionDetailQuery**: `IGlobalMissionRepository.GetByIdAsync`, `IGlobalMissionParticipantRepository.GetAsync`, `GetTopContributorsAsync`, `CountAsync`, `IUserDirectory.GetProfilesAsync`
- **GetGlobalMissionHistoryQuery**: `IGlobalMissionRepository.GetHistoryAsync`, `IGlobalMissionParticipantRepository.GetByUserAsync`
- **(Dış akış) AddContributionCommand** (şu an controller yok, sync job): `IGlobalMissionContributionRepository.ExistsByIdempotencyKeyAsync/AddAsync`, `IGlobalMissionParticipantRepository.GetAsync/AddAsync`, `IGlobalMissionRepository.UpdateCacheAsync`; UoW: `IMissionsUnitOfWork.SaveChangesAsync`

### Yapı Notları
- `IClock` tüm handler’lara enjekte edilecek (deadline, expire, poke limitleri için).
- Partner/Global Mission bildirimleri `INotificationPort` ile soyutlanacak.
- Friendship kontrolleri Social portunu tekrar kullanır; ortak arayüz paylaşılarak kod tekrarı önlenir.
- Paginated sonuçlar partner/global akışları için gerekirse repo düzeyinde eklenebilir.
