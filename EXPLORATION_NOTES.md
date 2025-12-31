# HealthVerse - Proje Keşif Notları

> **Son Güncelleme:** 2025-12-31 15:43  
> **Amaç:** Bu doküman projeyi anlamak, çalıştırmak ve geliştirmek için gereken TÜM bilgileri içerir.

---

# 🚀 HIZLI BAŞLANGIÇ (AI için)

## Projeyi Çalıştırma

### 1. Backend Başlat (Terminal 1)
```powershell
# ÖNCE Veritabanını Güncelle (İlk kurulum veya değişiklik sonrası)
cd c:\Users\Baris\Desktop\healthverse\backend
dotnet ef database update --project src/Infrastructure/HealthVerse.Infrastructure --startup-project src/Api/HealthVerse.Api

# Sonra API'yi Başlat
cd src/Api/HealthVerse.Api
dotnet run
# http://localhost:5000 adresinde başlar
```

### 2. Android Emulator + Flutter (Terminal 2)
```powershell
cd c:\Users\Baris\Desktop\healthverse\mobile
flutter emulators --launch Medium_Phone_API_36.1
# Emulator açılana kadar bekle (1-3 dk)
flutter run
```

### 3. Test Komutları
```powershell
# Backend testleri
cd c:\Users\Baris\Desktop\healthverse\backend
dotnet test tests/HealthVerse.UnitTests
dotnet test tests/HealthVerse.ArchitectureTests

# Flutter analiz
cd c:\Users\Baris\Desktop\healthverse\mobile
flutter analyze
```

---

# 📋 31 ARALIK 2025 OTURUMU - YAPILAN DEĞİŞİKLİKLER

## Backend Düzeltmeleri (3 adet)

| # | Dosya | Değişiklik | Neden |
|---|-------|------------|-------|
| 1 | `src/Api/Infrastructure/GlobalExceptionHandler.cs` | DomainException/ArgumentException → 400 mapping | Önceden tüm hatalar 500 dönüyordu |
| 2 | `src/Shared/Contracts/Notifications/DevicePlatform.cs` | **YENİ DOSYA** - Sabitler Contracts'a taşındı | Hexagonal architecture kuralı |
| 3 | `src/Api/Controllers/DevicesController.cs` | Import değişikliği: Domain → Contracts | Hexagonal compliance |
| 4 | `src/Modules/Identity/Domain/Entities/User.cs` | `TotalDuels` property + `IncrementTotalDuels()` eklendi | Milestone tracking için |
| 5 | `src/Infrastructure/Jobs/MilestoneCheckJob.cs` | DUEL_TOTAL → TotalDuels kullanıyor, TODO kaldırıldı | Fix #4'ün tamamlayıcısı |
| 6 | `Identity.Infrastructure/Persistence/UserRepository.cs` | Hem `Application` hem `Domain` IUserRepository implement ediyor | **CRITICAL FIX:** DI hatası çözüldü |
| 7 | `Api/Program.cs` | `AddHttpContextAccessor()` eklendi | CurrentUserAdapter için gerekli |

## Mobile Düzeltmeleri (3 adet)

| # | Dosya | Değişiklik | Neden |
|---|-------|------------|-------|
| 1 | `ios/Runner/Info.plist` | HealthKit permission keys eklendi | iOS'ta Health verisi için zorunlu |
| 2 | `ios/Runner/Runner.entitlements` | **YENİ DOSYA** - HealthKit capability | iOS entitlement |
| 3 | `android/app/build.gradle.kts` | `minSdk = 26` (önceden flutter default) | health package gereksinimu |
| 4 | `android/app/src/main/AndroidManifest.xml` | Yanlış yerleştirilmiş intent-filter kaldırıldı | Build hatası düzeltmesi |

---

# ⚙️ TEKNİK DETAYLAR

## Teknoloji Stack

| Bileşen | Teknoloji | Versiyon |
|---------|-----------|----------|
| Backend | .NET | 10 |
| ORM | Entity Framework Core | 10.0.0 |
| Database | PostgreSQL | - |
| CQRS | MediatR | 12.4.1 |
| Scheduler | Quartz.NET | 3.13.1 |
| Auth | Firebase Admin SDK | 3.1.0 |
| Mobile | Flutter | 3.35+ |
| Health | health package | 13.2.1 |
| HTTP | Dio | 5.9.0 |

## Kritik Kurallar

1. **Timezone:** Tüm sistemde `Europe/Istanbul` (TR) kullanılıyor
2. **Auth (Development):** `X-User-Id` header ile Firebase bypass
3. **minSdk:** Android için 26 (Health Connect gereksinimu)
4. **API Base URL (Emulator):** `http://10.0.2.2:5000`

## Proje Yapısı

```
healthverse/
├── backend/                    # .NET 10 Monolith API
│   ├── src/
│   │   ├── Api/               # Controllers, Program.cs
│   │   ├── Infrastructure/    # DbContext, Jobs, Auth
│   │   ├── Modules/           # 7 modül (Identity, Gamification, ...)
│   │   └── Shared/            # SharedKernel, Contracts
│   └── tests/                 # Unit, Integration, Architecture
│
└── mobile/                    # Flutter App
    ├── lib/
    │   ├── main.dart          # Entry point + UI
    │   └── core/              # API client, Health service
    ├── android/               # Android native
    └── ios/                   # iOS native
```

## Bekleyen Sorunlar (MVP için yeterli)

| # | Proje | Sorun | Öncelik |
|---|-------|-------|---------|
| 1 | Mobile | setState → Riverpod | Orta |
| 2 | Mobile | Flutter testleri yok | Düşük |
| 3 | Mobile | Release signing | Düşük |
| 4 | Backend | Test coverage artırılabilir | Düşük |

---

# 📊 PROJE METRİKLERİ

| Metrik | Değer |
|--------|-------|
| Backend Controllers | 14 |
| API Endpoints | 62 |
| Quartz Jobs | 10 |
| Unit Tests | ~299 |
| Integration Tests | ~29 |
| Architecture Tests | ~48 |
| **Toplam Test** | **~376+** |

---

## 📂 0. Monorepo Root Dosyaları

| Dosya | İçerik |
|-------|--------|
| `README.md` | Proje yapısı, hızlı başlangıç |
| `QUICKSTART.md` | Developer onboarding |
| `EXPLORATION_NOTES.md` | Bu doküman |

---

## 📂 1. `.github/` Klasörü

### 1.1 CODEOWNERS
- **Durum:** ✅ Başarılı
- Tüm kritik klasörler için `@barissolcay` code owner olarak atanmış.
- Architecture, Migrations, Contracts, Controllers gibi hassas alanlar korunuyor.

### 1.2 PULL_REQUEST_TEMPLATE.md
- **Durum:** ✅ Mükemmel
- Hexagonal Architecture Checklist içeriyor (Domain'de framework bağımlılığı yok mu? Controller thin mi?).
- Test, ADR ve Migration etki soruları var.
- PR kalitesini artıracak profesyonel bir template.

### 1.3 workflows/ci.yml
- **Durum:** ✅ Başarılı
- **Fast Gate:** Build + Unit Tests + Architecture Tests
- **Heavy Gate:** Integration Tests (Testcontainers + Postgres)
- **Code Quality:** Format check (warning only)
- `.NET 10.0.x` kullanılıyor (stabil sürüm).

---

## 📂 2. `docs/` Klasörü

### 2.1 `docs/architecture/`
| Dosya | İçerik |
|-------|--------|
| `HEXAGONAL_CONTRACT.md` | Mimari kurallar, katman bağımlılıkları, modül izolasyonu |
| `DEPENDENCY_MAP.md` | Proje referans haritası |
| `EF_COMMANDS.md` | Migration komutları quick reference |
| `BASELINE_20251230.md` | Test sayıları snapshot (299 Unit, 29 Integration, 48 Arch) |
| `adr/` | Architecture Decision Records (4 adet) |
| `phase-reports/` | Geliştirme fazları raporları |

**Önemli Mimari Kurallar (HEXAGONAL_CONTRACT.md):**
```
Domain → Hiçbir şeye bağımlı değil (Saf C#)
Application → Domain'e bağımlı, Infrastructure'a ASLA
Infrastructure → Application + Domain'e bağımlı
Api → Application + Infrastructure (DI wiring için)
```

### 2.2 `docs/archive/`
| Dosya | Önem | İçerik |
|-------|------|--------|
| `23_Güncel_Proje_Anlatımı.txt` | ⭐⭐⭐ | Projenin tüm iş kuralları, UI akışları, mekanikler |
| `20_database_şeması.txt` | ⭐⭐⭐ | Tüm DB şeması + Trigger'lar + Constraint'ler |
| `HEXAGONAL_ROADMAP.md` | ⭐⭐ | Mimari dönüşüm yol haritası |
| `DEV_PROGRESS.md` | ⭐ | Geliştirme ilerleme takibi |

**Kritik İş Kuralları (PRD'den):**
- **Streak:** Günlük 3000 adım serini korur. Yoksa ve Freeze varsa otomatik kullanılır. Yoksa seri sıfırlanır.
- **Puan:** 3000 adımdan sonra her 1000 adım = 1 puan.
- **Lig:** Haftalık, Pazartesi 00:00 TR başlar. Promote/Demote yüzdelik.
- **Düello:** Puan vermez! Sadece milestone/rozet kazandırır (Win-trading koruması).
- **Partner Görevi:** Haftada tek eşleşme. Slot sistemiyle DB seviyesinde korunuyor.
- **Timezone:** Her şey `Europe/Istanbul` bazlı.

**DB Tasarım Güçlü Noktaları:**
- `PointTransactions` Ledger yapısı (append-only, idempotent)
- Partial unique index'ler (aynı ikili arası tek aktif düello)
- Trigger'lar ile otomatik cache güncelleme (FollowersCount, TotalPoints)
- WeekId format validation (regex constraint)

---

## 📂 3. `src/Api/HealthVerse.Api/`

### 3.1 Dosya Yapısı
```
HealthVerse.Api/
├── Program.cs                    # Composition Root
├── HealthVerse.Api.csproj        # 7 modül referansı, .NET 10
├── appsettings.json              # Rate limit kuralları
├── firebase-credentials.json     # ✅ .gitignore'da (güvenli)
├── Controllers/                  # 14 Controller
├── Application/Queries/          # 1 API-specific Query
└── Infrastructure/               # GlobalExceptionHandler
```

### 3.2 Program.cs (Composition Root)
**✅ Başarılı Noktalar:**
- Tüm modül Infrastructure'ları kayıtlı (`AddCompetitionInfrastructure()` vb.)
- MediatR tüm Application assembly'lerinden handler'ları buluyor.
- `TurkeySystemClock` singleton olarak kayıtlı (IClock -> TR saati).
- Environment-aware yapılandırma (Test/Integration vs Production).

**Quartz Job Zamanlamaları (TR -> UTC):**
| Job | Amaç | TR Saati | UTC CRON |
|-----|------|----------|----------|
| DailyStreakJob | Seri kontrolü | 00:05 | `0 5 21 * * ?` |
| WeeklyLeagueFinalizeJob | Lig kapanışı | Pzt 00:05 | `0 5 21 ? * SUN` |
| PartnerMissionFinalizeJob | Partner kapanışı | Paz 23:55 | `0 55 20 ? * SUN` |
| PushDeliveryJob | Push gönderimi | Her 30 sn | Simple schedule |
| MilestoneCheckJob | Başarı kontrolü | 02:00 | `0 0 23 * * ?` |

### 3.3 Controllers (14 adet)
**✅ Tümü Thin Controller Prensibi Uyguluyor:**
```csharp
var response = await _mediator.Send(new SomeCommand(...));
if (!response.Success) return BadRequest(response);
return Ok(response);
```
Hiçbir controller'da iş mantığı yok. MediatR'a delege ediliyor.

| Controller | Endpoints | Notlar |
|------------|-----------|--------|
| AuthController | 5 | Firebase + Dev bypass |
| HealthController | 2 | sync (yeni), sync-steps (legacy) |
| LeagueController | 4 | join, my-room, tiers, history |
| DuelsController | 7 | CRUD + poke + history |
| SocialController | 6 | Follow/Block + listeler |
| TasksController | 4 | Templates endpoint `[AllowAnonymous]` |
| GoalsController | 4 | Create, Delete, Active, Completed |
| Missions | 4+4 | Global + Partner ayrı controller |
| NotificationsController | 5 | Preferences endpoint'i var |
| LeaderboardController | 3 | `[AllowAnonymous]` - Bilinçli karar |
| DevicesController | 2 | Push token yönetimi |
| StatusController | 4 | K8s health probes |

### 3.4 appsettings.json (Rate Limiting)
```json
"GeneralRules": [
  { "Endpoint": "*", "Period": "1s", "Limit": 10 },
  { "Endpoint": "*", "Period": "1m", "Limit": 100 },
  { "Endpoint": "post:/api/auth/register", "Period": "1h", "Limit": 5 },
  { "Endpoint": "post:/api/duels/*/poke", "Period": "1h", "Limit": 10 }
]
```
**✅ Akıllıca tasarlanmış.** Spam ve abuse prevention düşünülmüş.

### 3.5 GlobalExceptionHandler.cs
**⚠️ İyileştirme Gerekiyor:**
```csharp
// TODO: Handle specific Domain Exceptions here to map to 400 Bad Request
```
Şu an tüm hatalar 500 dönüyor. Domain exception'lar 400'e map'lenmeli.

---

## � 4. `src/Infrastructure/HealthVerse.Infrastructure/`

### 4.1 Dosya Yapısı
```
HealthVerse.Infrastructure/
├── HealthVerse.Infrastructure.csproj   # .NET 10, 11 modül referansı
├── Auth/                               # Firebase + CurrentUser
├── Clock/                              # TurkeySystemClock
├── Jobs/                               # 10 Quartz background job
├── Migrations/                         # EF Core migrations
├── Persistence/                        # DbContext + 26 Configuration
└── Services/                           # SystemCheckService
```

### 4.2 Auth Klasörü
| Dosya | Açıklama |
|-------|----------|
| `FirebaseAuthMiddleware.cs` | Firebase token doğrulama + DB lookup (FirebaseUid → UserId) |
| `CurrentUserAdapter.cs` | `ICurrentUser` implementasyonu (HttpContext'ten user_id claim okur) |

**Önemli Akış:**
```
Bearer Token → Firebase Verify → AuthIdentities Lookup → UserId Claim
```
- Development bypass: `X-User-Id` header ile auth atlanabilir
- Public endpoints: `/swagger`, `/status`, `/api/auth/register|login`

### 4.3 Clock Klasörü
| Dosya | Açıklama |
|-------|----------|
| `TurkeySystemClock.cs` | `IClock` implementasyonu |

**Özellikleri:**
- Cross-platform: `Europe/Istanbul` (Linux) / `Turkey Standard Time` (Windows)
- `TodayTR`, `NowTR`, `CurrentWeekId` (ISO format: `2024-W52`)
- `CurrentWeekStart`, `CurrentWeekEnd` (Pazartesi-Pazar)
- `IsWithinQuietHours(start, end)` - DND desteği (gece yarısı geçişi destekler)

### 4.4 Jobs Klasörü (10 Quartz Job)
**Tümü `[DisallowConcurrentExecution]` ile korunuyor.**

| Job | Zamanlama | Amaç |
|-----|-----------|------|
| `DailyStreakJob` | 00:05 TR | Dünkü adımları kontrol et, streak güncelle/freeze kullan/sıfırla |
| `WeeklyLeagueFinalizeJob` | Pzt 00:05 TR | Lig kapanışı, promote/demote, UserPointsHistory oluştur |
| `PartnerMissionFinalizeJob` | Paz 23:55 TR | Partner görevlerini COMPLETED/EXPIRED yap |
| `GlobalMissionFinalizeJob` | Her saat | Süresi dolan global görevleri finalize et |
| `ExpireJob` | Her saat | Task FAILED, Duel EXPIRED/FINISHED |
| `ReminderJob` | Her saat | Deadline hatırlatmaları (24h/6h kala) |
| `StreakReminderJob` | 17:00 TR | 3000 adıma ulaşmamış kullanıcılara uyarı |
| `MilestoneCheckJob` | 02:00 TR | Milestone kontrol, ödül dağıtımı |
| `WeeklySummaryJob` | Pzt 09:00 TR | Haftalık özet bildirimi |
| `PushDeliveryJob` | Her 30 sn | FCM push gönderimi (DND, retry, invalid token handling) |

**PushDeliveryJob Özellikleri:**
- Batch: 100 bildirim/çevirim
- Retry backoff: 1m → 5m → 30m
- DND: 22:00-08:00 TR → Sabah 08:00'e ertelenir
- Invalid token: Cihaz devre dışı bırakılır

### 4.5 Persistence Klasörü
| Dosya | Açıklama |
|-------|----------|
| `HealthVerseDbContext.cs` | 23 DbSet, 7 modülün entity'leri |
| `DomainEventDispatcherInterceptor.cs` | SaveChanges sonrası domain event dispatch |
| `DesignTimeDbContextFactory.cs` | Migration CLI için factory |
| `Configurations/` | 26 adet IEntityTypeConfiguration |

**DuelConfiguration.cs (Örnek - 14 Check Constraint):**
- `CHK_Duels_NoSelf` - Kendine düello açılamaz
- `CHK_Duels_Status` - Geçerli status değerleri
- `CHK_Duels_TimeOrder` - EndDate > StartDate
- `CHK_Duels_WaitingHasNoDates` - WAITING'de tarih olmaz
- `CHK_Duels_ResultOnlyWhenFinished` - Result sadece FINISHED'da
- ... ve 9 constraint daha

**UserConfiguration.cs:**
- Value Objects: `Username`, `Email` owned entity olarak
- JSONB: `Metadata` kolonu
- Tüm sayaçlar: `TotalPoints`, `StreakCount`, `FollowersCount` vb.

### 4.6 Services Klasörü
| Dosya | Açıklama |
|-------|----------|
| `SystemCheckService.cs` | DB bağlantı kontrolü (`ISystemCheckService`) |

---

## 📂 5. `src/Modules/` (7 Modül)

Her modül Hexagonal Architecture ile 3 katmandan oluşuyor: **Domain → Application → Infrastructure**

### 5.1 Competition Modülü (Lig Sistemi)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `LeagueConfig` | Tier kuralları (ISINMA→ŞAMPİYON), promote/demote yüzdeleri |
| `LeagueRoom` | Haftalık oda (AggregateRoot), kapasite kontrolü |
| `LeagueMember` | Oda üyeliği, PointsInRoom takibi |
| `UserPointsHistory` | Haftalık/aylık puan geçmişi, PROMOTED/DEMOTED/STAYED |

**Application Services:**
- `JoinLeagueCommand` - Lige katılma (oda yoksa otomatik oluştur)
- `LeagueFinalizeService` - Hafta sonu promote/demote işlemi

### 5.2 Identity Modülü (Kullanıcı Yönetimi)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `User` | AggregateRoot, 249 satır zengin domain model |
| `AuthIdentity` | Firebase→UserId mapping, multi-provider desteği |

**Value Objects:**
- `Username` (3-50 karakter, case-insensitive)
- `Email` (regex validation, lowercase)

**Domain Events:**
- `UserCreatedEvent`, `HealthPermissionGrantedEvent`, `StreakLostEvent`

**User Entity Özellikleri:**
- Streak yönetimi: `UpdateStreak()`, `ResetStreak()`, `UseFreeze()`
- Puan: `AddPoints()`, `TotalPoints` (long)
- Sosyal: `FollowingCount`, `FollowersCount` increment/decrement
- Health: `GrantHealthPermission()`, `RevokeHealthPermission()`

### 5.3 Gamification Modülü (Puanlama Sistemi)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `PointTransaction` | Ledger (append-only), IdempotencyKey ile duplicate koruması |
| `UserDailyStats` | Günlük adım/kalori/mesafe cache'i |
| `MilestoneReward` | Başarı tanımları (badge, title, freeze ödülü) |
| `UserStreakFreezeLog` | Freeze kullanım geçmişi |

**Domain Services:**
- `StreakService` - 3000 adım kuralı, Freeze mantığı
- `PointCalculationService` - Adımdan puana hesaplama

**PointTransaction Factory Methods:**
```csharp
FromDailySteps(userId, points, logDate, steps)  // IdempotencyKey: userId+logDate
FromTaskCompletion(userId, points, taskId, title)
FromCorrection(userId, amount, originalId, reason)
```

### 5.4 Social Modülü (Düello & Takip)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `Duel` | 296 satır, tam state machine |
| `Friendship` | Takip ilişkisi (follower→following) |
| `UserBlock` | Engelleme |

**Duel State Machine:**
```
WAITING ─┬─► ACTIVE ─► FINISHED (EndDate veya hedef)
         ├─► REJECTED
         └─► EXPIRED (24 saat yanıt yok)
```

**Duel Özellikleri:**
- `Poke()` - Günde 1 kez rakibi dürt
- `CalculateResult()` - CHALLENGER_WIN/OPPONENT_WIN/BOTH_WIN/BOTH_LOSE
- Score güncelleme: TargetValue'yu aşamaz

### 5.5 Tasks Modülü (Görevler)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `TaskTemplate` | Görev şablonları (admin tarafından tanımlanır) |
| `UserTask` | Kullanıcıya atanmış görev |
| `UserGoal` | Kullanıcının kendi hedefleri |
| `UserInterest` | Kullanıcı aktivite tercihleri |

**UserTask State Machine:**
```
ACTIVE ─┬─► COMPLETED ─► REWARD_CLAIMED
        └─► FAILED (süre doldu)
```

**Domain Kuralları:**
- ValidUntil max 7 gün
- CurrentValue ≤ TargetValue

### 5.6 Missions Modülü (Görevler)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `GlobalMission` | Topluluk hedefi (admin oluşturur) |
| `GlobalMissionParticipant` | Katılımcı listesi |
| `GlobalMissionContribution` | Katkı kayıtları |
| `WeeklyPartnerMission` | 2 kişilik haftalık hedef |
| `WeeklyPartnerMissionSlot` | Haftalık slot koruması |

**GlobalMission States:** `DRAFT → ACTIVE → FINISHED/CANCELLED`

**WeeklyPartnerMission Özellikleri:**
- `Poke()` - Günde 1 kez partner'ı dürt
- `ProgressPercent` - Toplam ilerleme %
- `TotalProgress` = InitiatorProgress + PartnerProgress

### 5.7 Notifications Modülü (Bildirimler)

**Domain Entities:**
| Entity | Açıklama |
|--------|----------|
| `Notification` | In-app bildirim (Title, Body, ReferenceId) |
| `NotificationDelivery` | Outbox pattern, push teslimat durumu |
| `UserDevice` | FCM/APNS token yönetimi |
| `UserNotificationPreference` | Bildirim tercihleri |
| `NotificationCategory` | Bildirim kategorileri |

**NotificationDelivery Özellikleri:**
- `MaxRetries = 3`
- `DeliveryStatus`: Pending → Sent/Failed/Cancelled
- `Reschedule()` - DND için erteleme
- `RecordFailedAttempt()` - Retry backoff

### 5.8 Modül Mimarisi Özeti

| Modül | Domain Entities | Value Objects | Services |
|-------|-----------------|---------------|----------|
| Competition | 4 | - | 1 (LeagueFinalizeService) |
| Identity | 2 | 2 | - |
| Gamification | 4 | - | 2 |
| Social | 3 | - | - |
| Tasks | 4 | - | - |
| Missions | 5 | - | - |
| Notifications | 5 | - | - |
| **Toplam** | **27** | **2** | **3** |
---

## 📂 6. `src/Shared/` (2 Proje)

### 6.1 HealthVerse.SharedKernel

Tüm modüller tarafından kullanılan temel yapı taşları.

#### Abstractions (5 Interface)
| Interface | Amacı |
|-----------|-------|
| `IClock` | TR timezone (Europe/Istanbul) saat işlemleri |
| `ICurrentUser` | Mevcut kullanıcı kimliği (HttpContext'ten izole) |
| `IUnitOfWork` | Transaction yönetimi |
| `IRepository<T>` | Generic repository pattern |
| `ISystemCheckService` | Sistem sağlık kontrolü |

**IClock Özellikleri:**
- `UtcNow`, `TodayTR`, `NowTR`
- `CurrentWeekId` (ISO format: 2025-W03)
- `CurrentWeekStart`, `CurrentWeekEnd` (Pazartesi-Pazar)
- `IsWithinQuietHours()` - DND kontrolü

#### Domain (6 Base Class/Interface)
| Sınıf | Amacı |
|-------|-------|
| `Entity` | Base class, Guid Id, DomainEvents collection |
| `AggregateRoot` | Entity + CreatedAt, UpdatedAt |
| `ValueObject` | Immutable, equality by components |
| `IDomainEvent` | Domain event marker interface |
| `DomainEventBase` | Base domain event |
| `DomainException` | Domain rule violation (Code + Message) |

#### Results (2 Class)
| Sınıf | Amacı |
|-------|-------|
| `Result` | Success/Failure pattern (exception-free) |
| `Error` | Error code + message |

#### ValueObjects (2 Class)
| Value Object | Amacı |
|--------------|-------|
| `IdempotencyKey` | Ledger duplicate prevention (7 factory methods) |
| `WeekId` | ISO hafta ID (YYYY-Www, regex validated) |

**IdempotencyKey Factory Methods:**
```csharp
ForDailySteps(userId, logDate)     // STEPS_DAILY:userId:date
ForTaskReward(userTaskId)          // TASK_REWARD:taskId
ForWeeklyPartnerReward(weekId, userId)
ForGlobalMissionReward(missionId, userId)
ForLeagueReward(weekId, userId)
ForMilestoneReward(milestoneId, userId)
ForCorrection(originalTransactionId)
```

### 6.2 HealthVerse.Contracts

Modüller arası iletişim sözleşmeleri (API değil, in-process).

#### Notifications (2 File)
| Dosya | İçerik |
|-------|--------|
| `INotificationService` | `CreateAsync()`, `CreateBatchAsync()` |
| `NotificationType` | 40+ sabit (STREAK_LOST, DUEL_REQUEST, vb.) |

**NotificationType Kategorileri:**
- Streak: STREAK_FROZEN, STREAK_LOST, STREAK_REMINDER
- Duel: DUEL_REQUEST, DUEL_ACCEPTED, DUEL_FINISHED, DUEL_POKE
- Task: TASK_COMPLETED, TASK_EXPIRING
- League: LEAGUE_PROMOTED, LEAGUE_DEMOTED, LEAGUE_NEW_WEEK
- Partner: PARTNER_MATCHED, PARTNER_COMPLETED, PARTNER_POKE
- Global: GLOBAL_MISSION_NEW, GLOBAL_MISSION_TOP3
- Milestone: MILESTONE_BADGE, MILESTONE_TITLE, MILESTONE_FREEZE

#### Health (5 File)
| Dosya | İçerik |
|-------|--------|
| `HealthActivityData` | Flutter'dan gelen sağlık verisi DTO |
| `HealthConstants` | Aktivite/metrik sabitleri |
| `IHealthProgressUpdater` | Modül progress güncelleme interface |
| `HealthProgressResult` | Güncelleme sonuç DTO |
| `HealthDataSyncedEvent` | Domain event |

**IHealthProgressUpdater Orchectration:**
```
Order: Steps(10) → Goals(20) → Tasks(30) → Duels(40) → Missions(50)
```

#### Gamification (1 File)
| Dosya | İçerik |
|-------|--------|
| `UserPointsEarnedEvent` | Puan kazanımı domain event |

---

## ✅ Çözülen Backend Sorunları (31 Aralık 2025)

| # | Konum | Sorun | Çözüm | Durum |
|---|-------|-------|-------|-------|
| 1 | `GlobalExceptionHandler` | Domain exceptions 500 dönüyor | DomainException → 400 mapping eklendi | ✅ |
| 2 | `DevicesController` | Domain import var | DevicePlatform → Contracts'a taşındı | ✅ |
| 3 | `MilestoneCheckJob` | TODO: total duels için ayrı sayaç | User.TotalDuels + IncrementTotalDuels() | ✅ |

**Değişen Dosyalar:**
- `src/Api/HealthVerse.Api/Infrastructure/GlobalExceptionHandler.cs`
- `src/Shared/HealthVerse.Contracts/Notifications/DevicePlatform.cs` (NEW)
- `src/Api/HealthVerse.Api/Controllers/DevicesController.cs`
- `src/Modules/Identity/HealthVerse.Identity.Domain/Entities/User.cs`
- `src/Infrastructure/HealthVerse.Infrastructure/Jobs/MilestoneCheckJob.cs`

---

## 🟢 Başarılı Tasarım Kararları

1. **Ledger-based Puan Sistemi:** Append-only, idempotent, audit trail.
2. **Thin Controllers:** MediatR pattern tam uygulanmış.
3. **TR Timezone Tutarlılığı:** `IClock`, Job'lar, DB hepsi uyumlu.
4. **Rate Limiting:** Endpoint-specific kurallar.
5. **DB Constraint'ler:** Farm önleme DB seviyesinde (14+ check constraint/tablo).
6. **Test Stratejisi:** Fast Gate / Heavy Gate ayrımı.
7. **Güvenlik:** `firebase-credentials.json` `.gitignore`'da korunuyor.
8. **Concurrent Job Protection:** Tüm job'lar `[DisallowConcurrentExecution]`.
9. **Push Retry & DND:** Exponential backoff + Quiet hours desteği.
10. **Domain Events:** EF Interceptor ile SaveChanges sonrası MediatR dispatch.

---

## 📊 Veri Akışı (Özet)

```
Flutter App
    │
    ▼
[HealthController.sync()]  ──► SyncHealthDataCommand
    │                              │
    │                              ▼
    │                    [Gamification.Application]
    │                              │
    ├─► Steps → UserDailyStats + PointTransactions (Ledger)
    ├─► Goals → UserGoals.CurrentValue güncelleme
    ├─► Tasks → UserTasks.CurrentValue güncelleme
    ├─► Duels → Duels.ChallengerScore/OpponentScore
    └─► Missions → GlobalMissionContributions / WeeklyPartnerMissions
```

---

## 📂 8. Kök Dosyalar (Root Files)

### 8.1 `.editorconfig` (159 satır)

Kapsamlı C# kod stili kuralları:

| Kategori | Kural |
|----------|-------|
| **Indent** | 4 space (C#), 2 space (JSON/YAML) |
| **Namespace** | `file_scoped` (suggestion) |
| **var** | Built-in types: false, Type apparent: true |
| **Braces** | `csharp_prefer_braces = true` |
| **using** | `outside_namespace:warning` |
| **Private fields** | `_camelCase` prefix |
| **Async methods** | `*Async` suffix |
| **Expression-bodied** | `when_on_single_line` |

### 8.2 `.gitignore` (37 satır)

| Kategori | Korunan Dosyalar |
|----------|------------------|
| OS | `.DS_Store`, `Thumbs.db` |
| Editor | `.vscode/`, `.idea/` |
| Dependencies | `node_modules/`, `venv/` |
| Build | `dist/`, `out/`, `*.apk` |
| **Secrets** | `.env`, `*.key`, `firebase-credentials.json`, `google-services.json` |

**Güvenlik Notu:** Firebase credentials ve tüm hassas dosyalar düzgün korunuyor.

### 8.3 `README.md` (408 satır)

Kapsamlı proje dokümantasyonu:

| Bölüm | İçerik |
|-------|--------|
| Proje Durumu | MVP v1.0, 376 test, 0 warning |
| Mimari | Modüler monolith, Hexagonal Architecture |
| Teknoloji | .NET 10, EF Core 10, PostgreSQL, MediatR, Quartz |
| Auth | Firebase + X-User-Id bypass (dev) |
| IClock | TR timezone (Europe/Istanbul) |
| DbContext | 23 DbSet, 25 configuration |
| Quartz Jobs | 10 job with TR timezone CRON |
| API Surface | 62 endpoint, 14 controller |
| Kurulum | dotnet build/run/test komutları |

---

## 📂 7. `tests/` (4 Proje)

### 7.1 HealthVerse.ArchitectureTests

**Araç:** NetArchTest.Rules + FluentAssertions

| Test Sınıfı | Test Sayısı | Kontrol Alanı |
|-------------|-------------|---------------|
| `LayerDependencyTests` | 5 | Katman bağımlılık kuralları |
| `ModuleIsolationTests` | 4 | Modüller arası izolasyon |
| `DomainConventionTests` | 5 | DDD pattern uyumu |
| `ApplicationConventionTests` | - | Handler/Query naming |
| `InfrastructureConventionTests` | - | Repository pattern |
| `ApiConventionTests` | - | Controller thin check |

**Örnek Kurallar:**
```csharp
// Domain Infrastructure'a bağımlı olamaz
ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")

// Entity'ler private constructor + factory method olmalı
hasPrivateConstructor && hasFactoryMethod(Create/Assign)

// Modüller arası doğrudan bağımlılık yasak
Application_ShouldNotDependOnOtherModuleApplicationLayers
```

### 7.2 HealthVerse.IntegrationTests

**Araçlar:** Testcontainers (PostgreSQL 15-alpine), Respawner, WebApplicationFactory

| Fixture | İşlev |
|---------|-------|
| `PostgresContainerFixture` | Docker PostgreSQL container |
| `CustomWebApplicationFactory` | Test sunucu yapılandırması |
| `IntegrationTestBase` | DB reset, HttpClient, X-User-Id header |
| `TestAuthHandler` | Firebase bypass authentication |

**Test Sınıfları:**
| Sınıf | Test Sayısı | Kapsam |
|-------|-------------|--------|
| `DuelTests` | 5 | Create, Accept, Reject, Poke, GetActive |
| `PartnerMissionTests` | 4 | Pair, Progress, Complete |
| `TaskGoalTests` | 5 | CRUD, Complete, Claim |
| `LeagueTests` | 3 | Join, MyRoom, Leaderboard |
| `GlobalMissionTests` | 4 | Join, Contribute, Progress |
| `SocialTests` | 4 | Follow/Unfollow, Block |
| `StatusTests` | 1 | Health check |

**Test Pattern:**
```csharp
public class DuelTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    // Arrange: Create users + mutual friendship
    // Act: POST /api/duels, POST /api/duels/{id}/accept
    // Assert: Status == ACTIVE
}
```

### 7.3 HealthVerse.UnitTests

**Organizasyon:** Modül başına klasör

| Modül | Test Dosyası Sayısı | Örnek Testler |
|-------|---------------------|---------------|
| Identity | 4 | UserTests (25), UsernameTests, EmailTests, AuthIdentityTests |
| Gamification | 4 | PointTransactionTests, MilestoneRewardTests, SyncHandlerTests |
| Social | 3 | DuelTests (15+), FriendshipTests, UserBlockTests |
| Tasks | 2 | UserTaskTests, UserGoalTests |
| Missions | 2 | GlobalMissionTests, PartnerMissionTests |
| Notifications | 3 | NotificationTests, DeliveryTests, DeviceTests |
| SharedKernel | 4 | ValueObjectTests, IdempotencyKeyTests |
| Competition | 3 | LeagueRoomTests, LeagueMemberTests |

**UserTests Kapsam (25 test):**
- `Create_WithValidData_ShouldCreateUser`
- `Create_ShouldRaiseDomainEvent`
- `GrantHealthPermission_ShouldSetPermissionAndRaiseEvent`
- `AddPoints_ShouldAccumulateCorrectly`
- `UseFreeze_WhenHasFreezes_ShouldDecrementAndReturnTrue`
- `ResetStreak_WhenHasStreak_ShouldRaiseEventAndReset`
- `ChangeTier_WithEmptyTier_ShouldThrow`

### 7.4 HealthVerse.ChecklistRunner

**Tür:** Console application (end-to-end API testing)

**13 Test Senaryosu:**
```
KISIM 1: Auth
 [1] POST /api/auth/dev-register
 [2] POST /api/auth/dev-login

KISIM 2: Health & Gamification  
 [3] POST /api/health/sync-steps (+ idempotency check)
 [4] GET /api/leaderboard/weekly

KISIM 3: Liga
 [5] POST /api/league/join
 [6] GET /api/league/my-room

KISIM 4: Düello (2 kullanıcı)
 [7] POST /api/duels
 [8] POST /api/duels/{id}/accept

KISIM 5: Partner Mission
 [9] POST /api/missions/partner/pair/{friendId}

KISIM 6: Notifications
[10] GET /api/notifications
[11] POST /api/notifications/mark-read

KISIM 7: Tasks
[12] GET /api/tasks/active
[13] POST /api/tasks/{id}/claim
```

**Özellikler:**
- 2 kullanıcı oluşturma (multi-user)
- Karşılıklı takip kurulumu
- X-User-Id header auth
- Idempotency testi
- Renkli konsol output

### 7.5 Test Sayıları (Baseline)

| Kategori | Sayı | Araç |
|----------|------|------|
| Unit Tests | ~299 | xUnit |
| Integration Tests | ~29 | Testcontainers |
| Architecture Tests | ~48 | NetArchTest |
| **Toplam** | **~376** | |

---

---

# 📱 MOBILE (Flutter) - KEŞİF NOTLARI

---

## � 1. Proje Genel Yapısı

```
mobile/
├── lib/
│   ├── main.dart                    # Entry point + UI (237 satır)
│   └── core/
│       ├── constants/api_constants.dart  # Endpoint sabitleri
│       ├── network/api_client.dart       # Dio HTTP client
│       └── services/health_sync_service.dart  # Health entegrasyonu
├── android/                         # Android native
├── ios/                             # iOS native
├── pubspec.yaml                     # Dependencies
└── test/                            # Test klasörü (boş)
```

## 📂 2. pubspec.yaml

| Özellik | Değer |
|---------|-------|
| **SDK** | ^3.9.2 (Flutter 3.35+) |
| **Version** | 1.0.0+1 |
| **publish_to** | none (private) |

**Dependencies:**
| Paket | Versiyon | Amaç |
|-------|----------|------|
| `health` | ^13.2.1 | iOS HealthKit / Android Health Connect |
| `dio` | ^5.9.0 | HTTP client |
| `flutter_secure_storage` | ^10.0.0 | Secure token storage |
| `cupertino_icons` | ^1.0.8 | iOS icons |

## 📂 3. lib/core/ Yapısı

### 3.1 api_constants.dart
```dart
static const String baseUrl = 'http://10.0.2.2:5000'; // Android Emulator
static const String healthSync = '/api/health/sync';
static const String devLogin = '/api/auth/dev-login';
```

### 3.2 api_client.dart (95 satır)
- **HTTP Client:** Dio
- **Auth Modes:**
  - Dev: `X-User-Id` header (FlutterSecureStorage'dan)
  - Prod: `Authorization: Bearer <token>`
- **Storage Keys:** `user_id`, `firebase_token`
- **Timeout:** 30 saniye (connect + receive)

### 3.3 health_sync_service.dart (244 satır)

**Anahtar Özellikler:**

| Özellik | Değer |
|---------|-------|
| **Data Types** | STEPS, DISTANCE, ACTIVE_ENERGY_BURNED, WORKOUT |
| **Permission** | READ only |
| **Sync Period** | Bugünün verileri (midnight → now) |

**Mapping Tabloları:**
```dart
// HealthDataType → Backend Metric
STEPS → "STEPS"
DISTANCE_WALKING_RUNNING → "DISTANCE"
ACTIVE_ENERGY_BURNED → "CALORIES"
WORKOUT → "DURATION"

// WorkoutActivityType → Backend Activity
RUNNING → "RUNNING"
BIKING → "CYCLING"
SWIMMING → "SWIMMING"
Default → "WALKING"

// RecordingMethod → Backend
automatic → "AUTOMATIC"
active → "ACTIVE"
manual → "MANUAL" (backend rejects!)
```

**HealthSyncResult DTO:**
- `success`, `message`
- `totalSteps`, `stepPointsEarned`, `taskPointsEarned`
- `goalsCompleted`, `tasksCompleted`, `duelsUpdated`

## 📂 4. main.dart (237 satır)

**UI Akışı:**
1. **Dev Login** → `/api/auth/dev-login` (random email/username)
2. **İzin İste** → Health Connect/HealthKit permission
3. **Sync** → `/api/health/sync` POST
4. **Sonuç Kartı** → Steps, Points, Goals, Tasks summary

**State Management:** StatefulWidget (setState)

**UI Bileşenleri:**
- AppBar + Logout button
- Status Card (loading/success icon)
- Action Buttons (Login/Permissions/Sync)
- Result Card (sync sonuçları)

## 📂 5. README.md (198 satır)

Kapsamlı dokümantasyon:
- Kurulum adımları
- API bağlantı konfigürasyonu (Emulator/Simulator/Device)
- Health izin listesi (Android/iOS)
- Recording Method kuralları
- Yapılacaklar listesi

## � 6. Android Native (`android/`)

### 6.1 build.gradle.kts (Root)
- Kotlin DSL format
- Google + MavenCentral repositories
- Flutter Gradle plugin entegrasyonu

### 6.2 app/build.gradle.kts
| Özellik | Değer |
|---------|-------|
| **App ID** | `com.healthverse.healthverse_app` |
| **Namespace** | `com.healthverse.healthverse_app` |
| **Java Version** | 11 |
| **minSdk/targetSdk** | Flutter default |
| **Signing** | Debug keys (TODO: release signing) |

### 6.3 AndroidManifest.xml ✅ **İYİ YAPILANDIRILMIŞ**

**Health Connect Permissions (8 adet):**
```xml
<uses-permission android:name="android.permission.health.READ_STEPS"/>
<uses-permission android:name="android.permission.health.READ_DISTANCE"/>
<uses-permission android:name="android.permission.health.READ_TOTAL_CALORIES_BURNED"/>
<uses-permission android:name="android.permission.health.READ_ACTIVE_CALORIES_BURNED"/>
<uses-permission android:name="android.permission.health.READ_HEART_RATE"/>
<uses-permission android:name="android.permission.health.READ_SLEEP"/>
<uses-permission android:name="android.permission.health.READ_EXERCISE"/>
<uses-permission android:name="android.permission.ACTIVITY_RECOGNITION"/>
```

**Ek Konfigürasyonlar:**
- `ACTION_SHOW_PERMISSIONS_RATIONALE` intent filter
- Health Connect package query (`com.google.android.apps.healthdata`)

---

## 📂 7. iOS Native (`ios/`)

### 7.1 Info.plist ✅ **TAMAMLANDI (31 Aralık 2025)**

Eklenen HealthKit izinleri:
```xml
<key>NSHealthShareUsageDescription</key>
<string>Sağlık verilerinizi senkronize etmek için izin gerekli</string>
<key>NSHealthUpdateUsageDescription</key>
<string>Sağlık verilerinizi güncellemek için izin gerekli</string>
```

### 7.2 Runner.entitlements ✅ **YENİ DOSYA**

HealthKit capability:
```xml
<key>com.apple.developer.healthkit</key>
<true/>
<key>com.apple.developer.healthkit.background-delivery</key>
<true/>
```

### 7.3 AppDelegate.swift
Standart Flutter template (14 satır).

### 7.4 Diğer iOS Dosyaları
- `Runner.xcodeproj/` - Xcode project
- `Runner.xcworkspace/` - CocoaPods workspace
- `Assets.xcassets/` - App icons
- `Base.lproj/` - Main storyboard, Launch screen

---

## ✅ Çözülen Mobile Sorunları (31 Aralık 2025)

| # | Konum | Sorun | Çözüm | Durum |
|---|-------|-------|-------|-------|
| 1 | `ios/Runner/Info.plist` | HealthKit izinleri EKSİK | NSHealthShareUsageDescription + NSHealthUpdateUsageDescription eklendi | ✅ |

**Değişen Dosyalar:**
- `ios/Runner/Info.plist` - HealthKit permission keys
- `ios/Runner/Runner.entitlements` (NEW) - HealthKit capability

## 🟡 Bekleyen Mobile Sorunları (MVP için yeterli)

| # | Konum | Sorun | Önem | Durum |
|---|-------|-------|------|-------|
| 2 | `main.dart` | setState state management | Orta | ⏳ |
| 3 | `test/` | Boş, test yok | Düşük | ⏳ |
| 4 | `app/build.gradle.kts` | Release signing TODO | Düşük | ⏳ |

---

## 🟢 Başarılı Tasarım Kararları (Mobile)

1. **Android Health Connect:** Tüm READ izinleri doğru tanımlanmış
2. **Permission Rationale:** Intent filter ile kullanıcıya açıklama gösterilebilir
3. **health paketi:** iOS/Android tek abstraction
4. **FlutterSecureStorage:** Token güvenliği
5. **Dio interceptors:** Auth header otomatik ekleme
6. **RecordingMethod mapping:** Backend ile uyumlu
7. **Activity aggregation:** Aynı tip verileri birleştirme

---

## 📝 Mobile İnceleme Durumu

- [x] `pubspec.yaml` ✅
- [x] `lib/main.dart` ✅
- [x] `lib/core/` (3 dosya) ✅
- [x] `README.md` ✅
- [x] `android/` (AndroidManifest, build.gradle) ✅
- [x] `ios/` (Info.plist, AppDelegate) ✅
- [x] `test/` (boş) ✅

---

*Bu doküman proje keşfi sırasında güncellenmektedir.*
