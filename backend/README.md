## HealthVerse

HealthVerse; kullanıcıların egzersiz verilerini oyunlaştırma, sosyal etkileşim ve rekabet mekanikleriyle birleştirerek sürdürülebilir motivasyon üretmeyi amaçlayan bir ürünün backend servisidir. Bu depo, HealthVerse backend tarafını (ASP.NET Core Web API + PostgreSQL) içerir.

Bu repo üzerinden yönetilen ana alanlar:
- **Identity**: Kimlik ve profil yönetimi
- **Gamification**: Puan/ledger, günlük istatistikler, streak/freeze ve milestone’lar
- **Competition**: Haftalık ligler ve liderlik tabloları
- **Social**: Sosyal takip/arkadaşlık ve düello sistemi
- **Tasks**: Görevler ve kişisel hedefler
- **Missions**: Global ve haftalık partner görevleri
- **Notifications**: Uygulama içi bildirimler ve push notification altyapısı

## Proje Durumu (31 Aralık 2025)

**Durum**: Backend MVP (v1.0)

### Gerçek metrikler (koddan doğrulandı)

| Metrik | Değer |
|--------|-------|
| **Modüller** | 7 modül (Identity, Gamification, Competition, Social, Tasks, Missions, Notifications) |
| **Controllers** | 14 controller (tümü MediatR kullanır) |
| **API Endpoints** | 62 endpoint (controller'larda `[Http*]` attribute sayımı) |
| **Quartz Jobs** | 10 arka plan işi (Quartz) |
| **Solution proje sayısı** | 28 proje (`src/HealthVerse.sln`) |
| **Runtime proje sayısı** | 24 proje (API + Infrastructure + SharedKernel + 7 modül x 3) |
| **Test projeleri** | 4 proje (Unit/Integration/Architecture + ChecklistRunner) |
| **Test sayısı** | 376+ test (Release'te tümü geçti) |
| **Build warnings** | 0 warning, 0 error |

### Test Suite (376+ test)

| Test Projesi | Test Dosyası | Test Sayısı |
|--------------|:------------:|:-----------:|
| **Integration Tests** | 10 | 29 |
| **Unit Tests** | 25 | 299 |
| **Architecture Tests** | 6 | 48 |
| **ChecklistRunner** | 1 | 13 (E2E) |
| **Toplam** | **42** | **389+** |

## Mimari Genel Bakış

Proje, modüler monolith yaklaşımıyla 7 iş modülüne ayrılmıştır. Her modül tipik olarak üç projeden oluşur:

- `*.Domain`: Entity/ValueObject/Domain kuralları
- `*.Application`: Use-case (Command/Query + Handler), Port arayüzleri
- `*.Infrastructure`: EF Core repository adapter’ları, DI kayıtları

Genel akış (tüm endpoint'ler için):
- **Controller** (`src/Api/HealthVerse.Api/Controllers`)  
  -> **MediatR Command/Query** (Module Application)  
  -> **Port (Repository/Service interface)** (Module Application/Ports)  
  -> **Adapter (EF Core)** (Module Infrastructure/Persistence)

Not:
- Tüm controller'lar MediatR kullanır (hexagonal architecture uyumlu).
- Quartz job'ları orchestrator-only prensibiyle çalışır (business logic Application katmanında).

## Teknoloji Yığını

| Kategori | Teknoloji | Versiyon / Not |
|----------|-----------|----------------|
| Runtime / Framework | .NET | 10 |
| Web | ASP.NET Core Web API | 10 |
| ORM | Entity Framework Core | 10.0.0 |
| Veritabanı | PostgreSQL | Npgsql provider |
| Swagger | Swashbuckle | 7.2.0 |
| CQRS / Messaging | MediatR | 12.4.1 |
| Scheduler | Quartz.NET | 3.13.1 |
| Auth | Firebase Admin SDK | 3.1.0 |
| Rate limiting | AspNetCoreRateLimit | 5.0.0 |

## Kimlik Doğrulama (Firebase) ve “Current User” Davranışı

### Firebase doğrulama

Korunan endpoint’lerde middleware şu mantıkla çalışır:
- `Authorization: Bearer <FIREBASE_ID_TOKEN>` varsa token doğrular ve claim set eder.
- Authorization yoksa Development/test senaryoları için `X-User-Id` header’ı varsa request’e izin verir.
- Bazı endpoint’ler “public” sayılır (swagger ve auth endpoint’leri gibi).

Kaynak: `src/Infrastructure/HealthVerse.Infrastructure/Auth/FirebaseAuthMiddleware.cs`

### Public endpoint'ler (middleware seviyesinde)

Middleware'in public kabul ettiği yollar:
- `/`
- `/swagger/**`
- `/status` (temel health check)
- `/status/live` (liveness probe)
- `/status/ready` (readiness probe)
- `/api/auth/register`
- `/api/auth/login`
- `/api/auth/dev-register` (controller içinde ayrıca "Development only" guard var)
- `/api/auth/dev-login` (controller içinde ayrıca "Development only" guard var)

Not: `/status/detailed` protected endpoint'tir (auth gerekli).

### "Current User" için mevcut gerçek davranış (önemli)

Tüm controller'lar `ICurrentUser` abstraction'ını kullanır:
- Arayüz: `src/Shared/HealthVerse.SharedKernel/Abstractions/ICurrentUser.cs`
- Implementasyon: `src/Infrastructure/HealthVerse.Infrastructure/Auth/CurrentUserAdapter.cs`
- Davranış:
  - Firebase token doğrulandıysa: `X-Firebase-Uid` header'ından Firebase UID okunur, `AuthIdentities` tablosundan canonical `Guid UserId`'ye mapping yapılır.
  - Development/test ortamında: `X-User-Id` header'ı varsa direkt kullanılır (bypass mekanizması).
  - Unauthorized/invalid durumda: `UnauthorizedException` fırlatılır (401 Unauthorized döner).

Kaynak: `src/Infrastructure/HealthVerse.Infrastructure/Auth/FirebaseAuthMiddleware.cs` (token doğrulama) + `CurrentUserAdapter.cs` (userId çözümleme)

### Auth endpoint’leri

- `POST /api/auth/register`: Body’de `IdToken` ile kayıt. Response: `UserId`.
- `POST /api/auth/login`: Body’de `IdToken` ile giriş. Response: `UserId` + `IsNewUser`.
- `GET /api/auth/me`: "mevcut kullanıcı" bilgisi (`ICurrentUser` üzerinden, Firebase token veya `X-User-Id` header gerekli).

Development-only:
- `POST /api/auth/dev-register`: Firebase bypass test user kayıt
- `POST /api/auth/dev-login`: Firebase bypass test login (response’taki `UserId` genelde `X-User-Id` header’ı için kullanılır)

## Zaman Standardı (TR) ve IClock

Sistem zaman standardı “Türkiye saat dilimi”ne göre yürütülür. `IClock` ile soyutlanır:
- Arayüz: `src/Shared/HealthVerse.SharedKernel/Abstractions/IClock.cs`
- Implementasyon: `src/Infrastructure/HealthVerse.Infrastructure/Clock/TurkeySystemClock.cs`
  - Linux/macOS: `Europe/Istanbul`
  - Windows: `Turkey Standard Time`

## Veritabanı

### DbContext

- `HealthVerseDbContext`: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/HealthVerseDbContext.cs`
- EF Core konfigurasyonları: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/Configurations/*.cs` (25 dosya)

### DbSet envanteri (yüksek seviye)

DbSet’lerin kaynağı: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/HealthVerseDbContext.cs`

- Identity: `Users`, `AuthIdentities`
- Gamification: `PointTransactions`, `UserDailyStats`, `UserStreakFreezeLogs`, `MilestoneRewards`, `UserMilestones`
- Competition: `LeagueConfigs`, `LeagueRooms`, `LeagueMembers`, `UserPointsHistories`
- Social: `Friendships`, `UserBlocks`, `Duels`
- Tasks: `TaskTemplates`, `UserTasks`, `UserGoals`, `UserInterests`
- Missions: `GlobalMissions`, `GlobalMissionParticipants`, `GlobalMissionContributions`, `WeeklyPartnerMissions`, `WeeklyPartnerMissionSlots`
- Notifications: `Notifications`, `UserDevices`, `NotificationDeliveries`

### Migrations

Migrations tek zincir halinde Infrastructure projesinde tutulur:
- Konum: `src/Infrastructure/HealthVerse.Infrastructure/Migrations/`
- Mevcut migration'lar: 2 migration
  - `20251229221633_AddNotificationDeliveries`
  - `20251230181523_AddUserNotificationPreferences`

Uygulama `Program.cs` içinde Npgsql konfigürasyonunda migrations assembly'i **`HealthVerse.Infrastructure`** olarak set ediyor:
- `src/Api/HealthVerse.Api/Program.cs` → `UseNpgsql(... b.MigrationsAssembly("HealthVerse.Infrastructure"))`

Bu nedenle migration komutları Infrastructure projesi üzerinden çalıştırılmalıdır (aşağıdaki komutlara bakın).

## Arka Plan İşleri (Quartz)

Quartz job’ları:
- Sınıflar: `src/Infrastructure/HealthVerse.Infrastructure/Jobs/*.cs`
- Kayıt/schedule: `src/Api/HealthVerse.Api/Program.cs` (job ve trigger kayıtları)

Job listesi (10):
- `ExpireJob`: saat başı
- `DailyStreakJob`: her gün 00:05 TR
- `WeeklyLeagueFinalizeJob`: her Pazartesi 00:05 TR
- `StreakReminderJob`: her gün 17:00 TR
- `ReminderJob`: her saat 30. dakika
- `GlobalMissionFinalizeJob`: her saat 45. dakika
- `PartnerMissionFinalizeJob`: her Pazar 23:55 TR
- `WeeklySummaryJob`: her Pazartesi 09:00 TR
- `MilestoneCheckJob`: her gün 02:00 TR
- `PushDeliveryJob`: her 30 saniye (Program.cs’te SimpleSchedule ile)

Not (lig finalize):
- `WeeklyLeagueFinalizeJob` her Pazartesi 00:05 TR'de çalışır ve `LeagueFinalizeService.FinalizeWeek()` metodunu çağırarak:
  1. Bir önceki haftanın tüm lig odalarını finalize eder (promote/demote)
  2. `UserPointsHistory` kayıtlarını oluşturur
  3. Terfi/tenzil bildirimlerini gönderir

## Bildirimler ve Push Notification Pipeline

### Entity’ler
- In-app notification: `src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/Notification.cs`
- Device token: `src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/UserDevice.cs` (dosya adı repo’da mevcut; DbSet + config var)
- Delivery/outbox: `src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/NotificationDelivery.cs`

### Servisler / portlar
- `INotificationService`: `src/Modules/Notifications/HealthVerse.Notifications.Application/Ports/INotificationService.cs`
- `NotificationService` implementasyonu: `src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Services/NotificationService.cs`
- `IPushSender` + FCM implementasyonu: `src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Push/FirebasePushSender.cs`

### PushDeliveryJob
- `src/Infrastructure/HealthVerse.Infrastructure/Jobs/PushDeliveryJob.cs`
- Özellikler:
  - Batch: 100
  - Retry backoff: 1m → 5m → 30m
  - Invalid token: device disable
  - DND: 22:00–08:00 TR

Önemli not:
- Kod tabanında bazı job’lar hâlâ doğrudan `Notification.Create(...)` ile `DbContext.Notifications.Add(...)` yapıyor. Bu durumda `NotificationDelivery` kaydı oluşmayabilir; dolayısıyla push-delivery mekanizması tüm notification kaynakları için “garantili” değildir.

## Domain Event Dispatch (MediatR)

- Domain event arayüzü: `src/Shared/HealthVerse.SharedKernel/Domain/IDomainEvent.cs` (MediatR `INotification` implement eder)
- Interceptor: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/DomainEventDispatcherInterceptor.cs`
- Örnek event’ler: `src/Modules/Identity/HealthVerse.Identity.Domain/Events/*`
- Örnek handler’lar: `src/Modules/Identity/HealthVerse.Identity.Application/EventHandlers/*`

## API Yüzeyi

**Base URL (launchSettings)**: `http://localhost:5000`  
**Swagger (Development)**: `http://localhost:5000/swagger`

Endpoint listesi (controller bazında) koddan doğrulanmıştır (61 endpoint):

### StatusController (`/status`)
- GET `/status`
- GET `/status/detailed`
- GET `/status/live`
- GET `/status/ready`

### AuthController (`/api/auth`)
- POST `/api/auth/register`
- POST `/api/auth/login`
- GET `/api/auth/me`
- POST `/api/auth/dev-register` (Development only)
- POST `/api/auth/dev-login` (Development only)

### HealthController (`/api/health`)
- POST `/api/health/sync` ⭐ **Yeni** - Flutter Health entegrasyonu
- POST `/api/health/sync-steps` _(Legacy - deprecated)_

Not: `/api/health/sync` endpoint'i Flutter Health paketinden gelen tüm sağlık verilerini tek seferde işler ve Goals, Tasks, Duels, Partner Missions, Global Missions modüllerini günceller. MANUAL ve UNKNOWN recording method'lar otomatik reddedilir.

### UsersController (`/api/users`)
- GET `/api/users/{id}/streak`
- GET `/api/users/{id}/stats`
- GET `/api/users/{id}/points-history`
- GET `/api/users/interests`
- POST `/api/users/interests`

### TasksController (`/api/tasks`)
- GET `/api/tasks/active`
- GET `/api/tasks/completed`
- POST `/api/tasks/{id}/claim`
- GET `/api/tasks/templates`

### GoalsController (`/api/goals`)
- POST `/api/goals`
- GET `/api/goals/active`
- GET `/api/goals/completed`
- DELETE `/api/goals/{id}`

### LeagueController (`/api/league`)
- GET `/api/league/my-room`
- GET `/api/league/room/{roomId}/leaderboard`
- GET `/api/league/tiers`
- GET `/api/league/history`
- POST `/api/league/join`

### LeaderboardController (`/api/leaderboard`)
- GET `/api/leaderboard/weekly`
- GET `/api/leaderboard/monthly`
- GET `/api/leaderboard/alltime`

### SocialController (`/api/social`)
- POST `/api/social/follow/{targetUserId}`
- DELETE `/api/social/unfollow/{targetUserId}`
- GET `/api/social/followers`
- GET `/api/social/following`
- GET `/api/social/friends`
- POST `/api/social/block/{targetUserId}`
- DELETE `/api/social/unblock/{targetUserId}`

### DuelsController (`/api/duels`)
- POST `/api/duels`
- GET `/api/duels/pending`
- POST `/api/duels/{id}/accept`
- POST `/api/duels/{id}/reject`
- GET `/api/duels/active`
- GET `/api/duels/{id}`
- POST `/api/duels/{id}/poke`
- GET `/api/duels/history`

### GlobalMissionsController (`/api/missions/global`)
- GET `/api/missions/global/active`
- POST `/api/missions/global/{id}/join`
- GET `/api/missions/global/{id}`
- GET `/api/missions/global/history`

### PartnerMissionsController (`/api/missions/partner`)
- GET `/api/missions/partner/available-friends`
- POST `/api/missions/partner/pair/{friendId}`
- GET `/api/missions/partner/active`
- POST `/api/missions/partner/{id}/poke`
- GET `/api/missions/partner/history`

### NotificationsController (`/api/notifications`)
- GET `/api/notifications`
- GET `/api/notifications/unread-count`
- POST `/api/notifications/mark-read`
- POST `/api/notifications/clear-all`

### DevicesController (`/api/devices`)
- POST `/api/devices/register`
- DELETE `/api/devices/{token}`

## Rate Limiting

- Middleware: `app.UseIpRateLimiting()` (`src/Api/HealthVerse.Api/Program.cs`)
- Config: `src/Api/HealthVerse.Api/appsettings.json` altında `IpRateLimiting`

Örnek kurallar:
- Genel: 1s/10 istek, 1m/100 istek
- Özel: register, login, sync-steps, duel poke, partner poke

## Kurulum ve Çalıştırma

### Gereksinimler
- .NET SDK 10
- PostgreSQL (geliştirme için)

### Build
```bash
cd healthverse_coding_project
dotnet build src/HealthVerse.sln
```

### Konfigürasyon
Zorunlu:
- `ConnectionStrings:DefaultConnection`

Opsiyonel (Firebase):
- `Firebase:CredentialsJson` (JSON içerik)
- `Firebase:CredentialPath` (dosya yolu)
- `FIREBASE_CREDENTIALS` (env var, JSON içerik)

User secrets örneği:
```bash
dotnet user-secrets --project src/Api/HealthVerse.Api set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=healthverse;Username=postgres;Password=postgres"
dotnet user-secrets --project src/Api/HealthVerse.Api set "Firebase:CredentialsJson" "<SERVICE_ACCOUNT_JSON>"
```

### Migrasyonları uygulama
Standart akış (migrations Infrastructure projesinde):
```bash
dotnet ef database update --project src/Infrastructure/HealthVerse.Infrastructure --startup-project src/Api/HealthVerse.Api --context HealthVerseDbContext
```

Migration listesi:
```bash
dotnet ef migrations list --project src/Infrastructure/HealthVerse.Infrastructure --startup-project src/Api/HealthVerse.Api --context HealthVerseDbContext
```

Yeni migration oluşturma:
```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure/HealthVerse.Infrastructure --startup-project src/Api/HealthVerse.Api --context HealthVerseDbContext
```

### API’yi çalıştırma
```bash
dotnet run --project src/Api/HealthVerse.Api
```

Swagger (Development): `http://localhost:5000/swagger`

### Testler
```bash
dotnet test src/HealthVerse.sln
```

### Development modunda hızlı istek
Development/test ortamında endpoint'ler `X-User-Id` header'ı ile çalışabilir (Firebase bypass):

```bash
curl -H "X-User-Id: 00000000-0000-0000-0000-000000000001" http://localhost:5000/api/users/interests
```

Not: Production'da Firebase token (`Authorization: Bearer <token>`) gerekli; `X-User-Id` bypass sadece Development/Test ortamlarında çalışır.

## Dokümanlar

- `TECHNICAL_DEBT_MASTER.md`: Teknik borç / eksikler
- `TEST_CHECKLIST.md`: Checklist runner ile yapılan otomatik test senaryoları
- `DEV_PROGRESS_AUDIT_REPORT.md`: Denetim raporu
- `docs/archive/`: Tamamlanan plan dokümanları (arşiv)

## Mimari Durum (31 Aralık 2025)

Proje **hexagonal architecture (ports & adapters)** prensiplerine uygun şekilde refactor edilmiştir:

- ✅ **Auth boundary**: `ICurrentUser` abstraction, Firebase UID → canonical `Guid UserId` mapping
- ✅ **API thinning**: Tüm controller'lar MediatR kullanır, DbContext doğrudan kullanılmaz
- ✅ **Migrations consolidation**: Tek zincir (Infrastructure projesinde)
- ✅ **Integration tests**: Testcontainers ile gerçek PostgreSQL
- ✅ **Module isolation**: Cross-module iletişim Contracts projesi üzerinden
- ✅ **Jobs orchestrator-only**: Business logic Application katmanında
- ✅ **CI/CD quality gates**: Architecture tests, integration tests, build warnings enforcement
- ✅ **Domain model**: Rich domain entities with private constructors + factory methods
- ✅ **Code standards**: EditorConfig enforced (file-scoped namespaces, async suffix)

Detaylı mimari dokümantasyon: `docs/architecture/HEXAGONAL_CONTRACT.md`, `docs/architecture/DEPENDENCY_MAP.md`, `docs/architecture/adr/`

