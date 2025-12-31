# Social & Duels Port Plan (Hex Prep)

Amaç: Social + Duels controller’larını Application katmanına taşıyıp portlar üzerinden çalıştırmak; Notification ve User bağımlılıklarını soyutlamak.

## Port İmzaları (Taslak)

```csharp
public interface IFriendshipRepository
{
    Task<Friendship?> GetAsync(Guid followerId, Guid followingId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken ct);
    Task<bool> IsMutualAsync(Guid userId1, Guid userId2, CancellationToken ct);
    Task AddAsync(Friendship friendship, CancellationToken ct);
    Task RemoveAsync(Friendship friendship, CancellationToken ct);
    Task<int> CountFollowingAsync(Guid userId, CancellationToken ct);
    Task<Paginated<UserSummary>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task<Paginated<UserSummary>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task<Paginated<UserSummary>> GetMutualFriendsAsync(Guid userId, int page, int pageSize, CancellationToken ct);
}

public interface IUserBlockRepository
{
    Task<bool> IsBlockedEitherWayAsync(Guid userId, Guid targetUserId, CancellationToken ct);
    Task<UserBlock?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken ct);
    Task AddAsync(UserBlock block, CancellationToken ct);
    Task RemoveAsync(UserBlock block, CancellationToken ct);
}

public interface IUserSocialRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
    Task IncrementFollowingAsync(Guid userId, CancellationToken ct);
    Task DecrementFollowingAsync(Guid userId, CancellationToken ct);
    Task IncrementFollowersAsync(Guid userId, CancellationToken ct);
    Task DecrementFollowersAsync(Guid userId, CancellationToken ct);
}

public interface IDuelRepository
{
    Task<bool> HasActiveOrPendingBetweenAsync(Guid userA, Guid userB, CancellationToken ct);
    Task AddAsync(Duel duel, CancellationToken ct);
    Task<Duel?> GetByIdAsync(Guid duelId, CancellationToken ct);
    Task<List<Duel>> GetPendingIncomingAsync(Guid userId, CancellationToken ct);
    Task<List<Duel>> GetPendingOutgoingAsync(Guid userId, CancellationToken ct);
    Task<List<Duel>> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<List<Duel>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct);
    Task<int> ExpireOldDuelsAsync(DateTimeOffset now, CancellationToken ct);
    Task<int> FinishExpiredDuelsAsync(DateTimeOffset now, CancellationToken ct);
}

public interface IUserDirectory
{
    Task<Dictionary<Guid, UserProfileSnapshot>> GetProfilesAsync(IEnumerable<Guid> userIds, CancellationToken ct);
}

public interface INotificationPort
{
    Task AddAsync(Notification notification, CancellationToken ct);
}

public interface ISocialUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed record UserProfileSnapshot(Guid UserId, string Username, int AvatarId);
public sealed record Paginated<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
```

_Notlar_: `UserSummary` domain model yerine query-only projection kullanabilir; burada Paginated wrapper ile döndürülür. `UserDirectory` sadece isim/avatar sözlüğü sağlar (Duel DTO’larında kullanılıyor).

## Use-Case / Handler Akışı (Taslak)

### Social (Follow/Block)
- **FollowCommand** (SocialController.Follow):
  - Ports: `IUserSocialRepository.GetByIdAsync`, `IUserBlockRepository.IsBlockedEitherWayAsync`, `IFriendshipRepository.GetAsync/ExistsAsync`, `IFriendshipRepository.AddAsync`, `IUserSocialRepository.IncrementFollowingAsync`, `IUserSocialRepository.IncrementFollowersAsync`, `IFriendshipRepository.IsMutualAsync`, `INotificationPort.AddAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`
  - İş: Takip ekle, counter update, mutual ise çift bildirim, değilse follower bildirimi.

- **UnfollowCommand** (SocialController.Unfollow):
  - Ports: `IFriendshipRepository.GetAsync`, `IFriendshipRepository.RemoveAsync`, `IUserSocialRepository.DecrementFollowingAsync`, `IUserSocialRepository.DecrementFollowersAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

- **GetFollowersQuery / GetFollowingQuery / GetFriendsQuery**:
  - Ports: `IFriendshipRepository.GetFollowersAsync`, `GetFollowingAsync`, `GetMutualFriendsAsync`
  - Amaç: Sayfalı liste DTO’ya map.

- **BlockCommand** (SocialController.Block):
  - Ports: `IUserBlockRepository.GetAsync`, `IUserBlockRepository.AddAsync`, `IFriendshipRepository.GetAsync` (varsa follow’ları silmek için), `IFriendshipRepository.RemoveAsync`, `IUserSocialRepository.DecrementFollowingAsync`, `IUserSocialRepository.DecrementFollowersAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

- **UnblockCommand** (SocialController.Unblock):
  - Ports: `IUserBlockRepository.GetAsync`, `IUserBlockRepository.RemoveAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

### Duels
- **CreateDuelCommand** (DuelsController.CreateDuel):
  - Ports: `IUserDirectory.GetProfilesAsync` (rakip var mı diye User repo da olabilir), `IFriendshipRepository.IsMutualAsync`, `IDuelRepository.HasActiveOrPendingBetweenAsync`, `IDuelRepository.AddAsync`, `INotificationPort.AddAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

- **GetPendingDuelsQuery**:
  - Ports: `IDuelRepository.ExpireOldDuelsAsync`, `IDuelRepository.GetPendingIncomingAsync`, `GetPendingOutgoingAsync`, `IUserDirectory.GetProfilesAsync`

- **AcceptDuelCommand / RejectDuelCommand**:
  - Ports: `IDuelRepository.GetByIdAsync`, domain method `Accept/Reject`, `INotificationPort.AddAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

- **GetActiveDuelsQuery**:
  - Ports: `IDuelRepository.FinishExpiredDuelsAsync`, `IDuelRepository.GetActiveByUserAsync`, `IUserDirectory.GetProfilesAsync`

- **GetDuelDetailQuery**:
  - Ports: `IDuelRepository.GetByIdAsync`, `IUserDirectory.GetProfilesAsync`

- **PokeDuelCommand**:
  - Ports: `IDuelRepository.GetByIdAsync`, domain `Poke`, `INotificationPort.AddAsync`
  - UoW: `ISocialUnitOfWork.SaveChangesAsync`

- **GetDuelHistoryQuery**:
  - Ports: `IDuelRepository.GetHistoryByUserAsync`, `IUserDirectory.GetProfilesAsync`

### Yapı Notları
- Duels domain zaten durum geçişlerini içeriyor; handler tarafında sadece port çağrısı + domain metodu + SaveChanges yapılacak.
- `INotificationPort` mevcut Notification entity ile uyumlu bir ekleme kapısı; ileride message bus’a dönebilir.
- `IClock` Social/Duels handler’larına enjekte edilecek (self-duel, expiry, poke limitleri için `UtcNow`).
- `Paginated` çıktılar EF LINQ yerine repo tarafında projeksiyonla sağlanabilir.
