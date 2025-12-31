# Competition Port Plan (Hex Prep)

Bu dosya, Competition modülü için planlanan port (repository) imzalarını ve temel sözleşmeleri içerir. Amaç, Application katmanının Infrastructure’dan ayrılması ve controller → use-case → port akışının hazırlanmasıdır.

## Port İmzaları (Taslak)

```csharp
public interface ILeagueRoomRepository
{
    Task<List<LeagueRoom>> GetUnprocessedByWeekAsync(WeekId weekId, CancellationToken ct);
    Task<LeagueRoom?> GetByIdAsync(Guid roomId, CancellationToken ct);
    Task<LeagueRoom?> FindAvailableForTierAsync(WeekId weekId, string tier, int maxRoomSize, CancellationToken ct);
    Task AddAsync(LeagueRoom room, CancellationToken ct);
    Task MarkProcessedAsync(LeagueRoom room, DateTimeOffset processedAt, CancellationToken ct);
    Task IncrementUserCountAsync(Guid roomId, CancellationToken ct);
}

public interface ILeagueMemberRepository
{
    Task<LeagueMember?> GetMembershipByUserAndWeekAsync(Guid userId, WeekId weekId, CancellationToken ct);
    Task<List<LeagueMember>> GetMembersByRoomOrderedAsync(Guid roomId, CancellationToken ct);
    Task<int> CountByRoomAsync(Guid roomId, CancellationToken ct);
    Task<int> GetRankForUserAsync(Guid roomId, Guid userId, int userPoints, CancellationToken ct);
    Task AddAsync(LeagueMember member, CancellationToken ct);
}

public interface ILeagueConfigRepository
{
    Task<LeagueConfig?> GetByTierNameAsync(string tierName, CancellationToken ct);
    Task<List<LeagueConfig>> GetAllOrderedAsync(CancellationToken ct);
    Task<LeagueConfig?> GetNextByOrderAsync(int tierOrder, CancellationToken ct);
    Task<LeagueConfig?> GetPrevByOrderAsync(int tierOrder, CancellationToken ct);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
    Task<Dictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct);
    Task UpdateTierAsync(Guid userId, string newTier, CancellationToken ct);
}

public interface IUserPointsHistoryRepository
{
    Task AddWeeklyHistoryAsync(UserPointsHistory history, CancellationToken ct);
    Task<List<UserPointsHistory>> GetWeeklyHistoryAsync(Guid userId, int limit, CancellationToken ct);
}

public interface ICompetitionUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

## Use-Case / Handler Akışı (Taslak)

- **GetMyRoomQuery** (LeagueController.GetMyRoom):
    - Ports: `ILeagueMemberRepository.GetMembershipByUserAndWeekAsync`, `ILeagueRoomRepository.GetByIdAsync`, `ILeagueConfigRepository.GetByTierNameAsync`, `ILeagueMemberRepository.GetRankForUserAsync`, `ILeagueMemberRepository.CountByRoomAsync`
    - Amaç: Kullanıcının aktif oda snapshot’ını DTO’ya map etmek (HoursRemaining = EndsAt - now).

- **GetRoomLeaderboardQuery** (LeagueController.GetRoomLeaderboard):
    - Ports: `ILeagueRoomRepository.GetByIdAsync`, `ILeagueConfigRepository.GetByTierNameAsync`, `ILeagueMemberRepository.GetMembersByRoomOrderedAsync`, `IUserRepository.GetByIdsAsync`
    - Amaç: Oda sıralamasını rank/promotion-demotion kesme noktaları ile döndürmek.

- **GetTiersQuery** (LeagueController.GetTiers):
    - Ports: `ILeagueConfigRepository.GetAllOrderedAsync`, `IUserRepository.GetByIdAsync`
    - Amaç: Tier listesi + kullanıcının mevcut tier’ı.

- **GetHistoryQuery** (LeagueController.GetHistory):
    - Ports: `IUserPointsHistoryRepository.GetWeeklyHistoryAsync`
    - Amaç: Haftalık geçmiş listesini sıralı döndürmek.

- **JoinLeagueCommand** (LeagueController.JoinLeague):
    - Ports: `IUserRepository.GetByIdAsync`, `ILeagueConfigRepository.GetByTierNameAsync`, `ILeagueRoomRepository.FindAvailableForTierAsync`, `ILeagueRoomRepository.AddAsync` (yeni oda gerekiyorsa), `ILeagueMemberRepository.AddAsync`, `ILeagueRoomRepository.IncrementUserCountAsync`
    - Unit of Work: `ICompetitionUnitOfWork.SaveChangesAsync`
    - Amaç: Kapasite/katılım kurallarıyla odaya katılım; yeni oda yaratma + sayacı artırma + üyelik ekleme.

- **FinalizeWeekCommand** (LeagueFinalizeService / Quartz job):
    - Ports: `ILeagueRoomRepository.GetUnprocessedByWeekAsync`, `ILeagueConfigRepository.GetByTierNameAsync`, `ILeagueConfigRepository.GetNextByOrderAsync`, `ILeagueConfigRepository.GetPrevByOrderAsync`, `ILeagueMemberRepository.GetMembersByRoomOrderedAsync`, `IUserRepository.GetByIdAsync`, `IUserRepository.UpdateTierAsync`, `IUserPointsHistoryRepository.AddWeeklyHistoryAsync`, `ILeagueRoomRepository.MarkProcessedAsync`
    - Unit of Work: `ICompetitionUnitOfWork.SaveChangesAsync` (oda bazlı batch)
    - Amaç: Haftalık finalize; promote/demote/stay hesaplama, snapshot yazma, oda işaretleme.

### Handler/Yapı Notları
- Application katmanına MediatR handler’ları eklenecek (`record` command/query + `IRequestHandler`).
- DTO map’leri handler içinde kalabilir; domain entity → DTO dönüşü için küçük mapper sınıfları eklenebilir.
- `IClock` kullanımı `JoinLeague` ve `FinalizeWeek`’te kalacak (weekId, processedAt, HoursRemaining hesapları).

## LeagueFinalize + csproj Refaktör Planı

- **Repo ihtiyaçları (Finalize)**: Mevcut taslak portlar Finalize için yeterli; ek ihtiyaç yok.
- **Concurrency**: `MarkProcessedAsync` için row version veya processed flag concurrency check (repo implementasyonunda). `IncrementUserCountAsync` de aynı şekilde DB tarafında atomik yapılmalı.
- **Application csproj bağımlılığı**: `HealthVerse.Competition.Application` şu an `HealthVerse.Infrastructure`’a referanslı. Refaktör adımı:
    1) Application’dan Infrastructure referansını kaldır.
    2) Competition.Infrastructure’da EF repo implementasyonlarını ekle ve DI’da `IServiceCollection` extension ile register et.
    3) Api/Host tarafında Competition.Infrastructure DependencyInjection çağrısını ekle (modül bazlı register).
- **DbContext erişimi**: Repo implementasyonları `HealthVerseDbContext` veya Competition’a özel DbContext’e sarılacak; Application sadece port + `ICompetitionUnitOfWork` görecek.
- **Quartz job**: `LeagueFinalizeService` MediatR komutuna dönüştürülebilir; kısa vadede mevcut servis, portlar üzerinden çalışacak ve DI’dan çözülecek.

### Notlar
- `CancellationToken` tüm çağrılara ekleniyor.
- `FindAvailableForTierAsync` ve `IncrementUserCountAsync` join akışındaki kapasite yönetimini soyutlar.
- `MarkProcessedAsync` finalize job için; optimistic concurrency uygulanacak (repo tarafında RowVersion veya uygun EF pattern ile).
- `UpdateTierAsync` finalize sırasında kullanıcı tier güncellemelerini kapsar.
- `ICompetitionUnitOfWork`, EF `DbContext`’in yerini alacak minimal kontrat; transaction yönetimi burada yoğunlaşacak.
