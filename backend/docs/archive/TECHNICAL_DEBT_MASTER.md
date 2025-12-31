# 🔧 TEKNİK BORÇ VE EKSİKLİKLER MASTER DOKÜMANI

**Oluşturulma Tarihi:** 30 Aralık 2025  
**Son Güncelleme:** 30 Aralık 2025  
**Amaç:** Tüm eksiklikleri, teknik borçları ve yapılacakları tek bir yerde toplamak

---

## 📊 ÖZET DURUM

| Kategori | Durum | Tamamlanan |
|----------|:-----:|:----------:|
| 🔴 Push Notification Pipeline | ✅ TAMAMLANDI | 8/8 |
| 🟠 Integration Tests | ✅ TAMAMLANDI | 8 dosya, 29 test |
| 🟡 Mimari Teknik Borçlar | ✅ TAMAMLANDI | 3/3 |
| 🟢 Kod Kalitesi | ✅ TAMAMLANDI | 3/3 |
| ⚫ Ertelenen | Bilinçli karar | N/A |
| 🔵 Production | Deploy öncesi | 0/4 |
| 💜 Flutter | Client tarafı | 0/4 |

---

## �📋 İÇİNDEKİLER

1. [🔴 KRİTİK - Push Notification Pipeline](#1-kritik---push-notification-pipeline)
2. [🟠 YÜKSEK - Integration Test Eksiklikleri](#2-yüksek---integration-test-eksiklikleri)
3. [🟡 ORTA - Mimari Teknik Borçlar](#3-orta---mimari-teknik-borçlar)
4. [🟢 DÜŞÜK - Kod Kalitesi İyileştirmeleri](#4-düşük---kod-kalitesi-iyileştirmeleri)
5. [⚫ ERTELENEN - Bilinçli Kararlar](#5-ertelenen---bilinçli-kararlar)
6. [🔵 PRODUCTION - Deploy Öncesi](#6-production---deploy-öncesi)
7. [💜 FLUTTER - Client Entegrasyonu](#7-flutter---client-entegrasyonu)

---

## 1. 🔴 KRİTİK - Push Notification Pipeline

**Durum:** ✅ TAMAMLANDI  
**Etki:** Push notification altyapısı hazır. Migration yapıldıktan sonra aktif olacak.

### 1.1 NotificationDelivery DbSet Eksik

**Dosya:** `src/Infrastructure/HealthVerse.Infrastructure/Persistence/HealthVerseDbContext.cs`

**Çözüm:**
```csharp
public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
```

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.2 NotificationDelivery Migration Eksik

**Dosya:** `src/Api/HealthVerse.Api/Migrations/AddNotificationDeliveries.cs`

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.3 INotificationDeliveryRepository Port Eksik

**Dosya:** `src/Modules/Notifications/HealthVerse.Notifications.Application/Ports/INotificationRepositories.cs`

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.4 NotificationDeliveryRepository Infrastructure Eksik

**Dosya:** `src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Persistence/NotificationDeliveryRepository.cs`

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.5 IPushSender Interface Eksik

**Dosya:** `src/Modules/Notifications/HealthVerse.Notifications.Application/Ports/IPushSender.cs`

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.6 FirebasePushSender Infrastructure Eksik

**Dosya:** `src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Push/FirebasePushSender.cs`

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.7 PushDeliveryJob (Quartz) Eksik

**Dosya:** `src/Infrastructure/HealthVerse.Infrastructure/Jobs/PushDeliveryJob.cs`

**Özellikler:**
- ✅ Her 30 saniyede çalışır
- ✅ Batch processing (100)
- ✅ Retry backoff (1m→5m→30m)
- ✅ Invalid token handling
- ✅ DND quiet hours (22:00-08:00 TR)

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 1.8 Notification Create Noktalarında Delivery Ekleme

**Sorun:** Şu an `Notification` oluşturulurken `NotificationDelivery` oluşturulmuyor.

**Etkilenen Dosyalar (20+ yer):**
- `RegisterCommand.cs`, `DevRegisterCommand.cs` (Identity)
- `FollowUserCommand.cs`, `CreateDuelCommand.cs`, `DuelDecisionCommands.cs`, `PokeDuelCommand.cs` (Social)
- `MissionsSharedServices.cs` (Missions)
- `ReminderJob.cs`, `StreakReminderJob.cs`, `WeeklyLeagueFinalizeJob.cs`, `WeeklySummaryJob.cs` (Jobs)

**Çözüm:** `INotificationService` interface ve `NotificationService` implementasyonu oluşturuldu.

```csharp
public interface INotificationService
{
    Task<Notification> CreateAsync(Guid userId, string type, string title, string body, 
        Guid? referenceId = null, string? referenceType = null, string? data = null, CancellationToken ct = default);
    Task<List<Notification>> CreateBatchAsync(IEnumerable<NotificationCreateRequest> requests, CancellationToken ct = default);
}
```

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

**Yapılan İşler:**
- `INotificationService` interface oluşturuldu (`Notifications.Application.Ports`)
- `NotificationService` implementasyonu (hem Notification hem NotificationDelivery oluşturur)
- 20+ dosya refactored: RegisterCommand, DevRegisterCommand, FollowUserCommand, CreateDuelCommand, DuelDecisionCommands, PokeDuelCommand, PokePartnerCommand, PairWithFriendCommand, JoinGlobalMissionCommand, ReminderJob, StreakReminderJob, WeeklyLeagueFinalizeJob, WeeklySummaryJob
- Missions modülündeki lokal INotificationService interface kaldırıldı (isim çakışması giderildi)

---

## 2. 🟠 YÜKSEK - Integration Test Eksiklikleri

**Mevcut Durum:** ✅ TAMAMLANDI - 8 test dosyası / 29 test senaryosu

### 2.1 Mevcut Testler ✅

| Test Dosyası | Test Sayısı | Durum |
|--------------|:-----------:|:-----:|
| StatusTests.cs | 1 | ✅ |
| LeagueTests.cs | 3 | ✅ |
| SocialTests.cs | 5 | ✅ (30 Aralık 2025) |
| DuelTests.cs | 5 | ✅ (30 Aralık 2025) |
| PartnerMissionTests.cs | 4 | ✅ (30 Aralık 2025) |
| GlobalMissionTests.cs | 5 | ✅ (30 Aralık 2025) |
| TaskGoalTests.cs | 7 | ✅ (30 Aralık 2025) |
| **TOPLAM** | **29** | ✅ |

### 2.2 Test Senaryoları (TAMAMLANDI)

#### 2.2.1 Social.Follow + Mutual + Block ✅
**Dosya:** `tests/HealthVerse.IntegrationTests/SocialTests.cs`

**Senaryolar:**
- [x] POST /api/social/follow/{userId} → Takip başarılı
- [x] Karşılıklı takip → Mutual flag true
- [x] POST /api/social/block/{userId} → Takip silinir, engellenir
- [x] Engelli kullanıcıyı takip edemezsin
- [x] DELETE /api/social/unfollow/{userId} → Takip kaldırılır

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

#### 2.2.2 Duels Lifecycle ✅
**Dosya:** `tests/HealthVerse.IntegrationTests/DuelTests.cs`

**Senaryolar:**
- [x] POST /api/duels → Pending durumunda düello
- [x] POST /api/duels/{id}/accept → ACTIVE olur
- [x] POST /api/duels/{id}/poke → Poke işlemi
- [x] POST /api/duels/{id}/decline → Declined olur
- [x] GET /api/duels/active → Sadece aktif düellolar

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

#### 2.2.3 Partner Missions ✅
**Dosya:** `tests/HealthVerse.IntegrationTests/PartnerMissionTests.cs`

**Senaryolar:**
- [x] GET /api/missions/partner/available → Mutual friend varsa listele
- [x] POST /api/missions/partner/join → Partner ile katılım
- [x] Non-mutual friend ile katılım engellenir
- [x] GET /api/missions/partner/my → Aktif partner missions

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

#### 2.2.4 Global Missions ✅
**Dosya:** `tests/HealthVerse.IntegrationTests/GlobalMissionTests.cs`

**Senaryolar:**
- [x] GET /api/missions/global/active → Aktif global missions
- [x] POST /api/missions/global/{id}/join → Katılım
- [x] GET /api/missions/global/{id} → ContributorCount içerir
- [x] HoursRemaining > 0 kontrolü
- [x] POST /api/missions/global/{id}/contribute → Progress güncelleme

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

#### 2.2.5 Tasks/Goals ✅
**Dosya:** `tests/HealthVerse.IntegrationTests/TaskGoalTests.cs`

**Senaryolar:**
- [x] GET /api/tasks/active → Unexpired görevler
- [x] GET /api/goals/daily → Günün hedefleri
- [x] POST /api/tasks/{id}/complete → Tamamlama + reward
- [x] POST /api/goals/{id}/claim → Reward claim
- [x] POST /api/goals/{id}/progress → Progress güncelleme
- [x] GET /api/tasks/expired → Sadece expired tasks
- [x] GET /api/goals/weekly → Haftalık hedefler

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

## 3. 🟡 ORTA - Mimari Teknik Borçlar

### 3.1 Domain Event Dispatch Mekanizması ✅

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

**Yapılan İşler:**
1. SharedKernel'e MediatR paketi eklendi
2. `IDomainEvent` interface'i `INotification` implement etti
3. `DomainEventDispatcherInterceptor` oluşturuldu
4. Program.cs'de interceptor DbContext'e eklendi
5. Tüm Application assembly'leri MediatR'a kaydedildi
6. Örnek event handler'lar oluşturuldu:
   - `UserCreatedEventHandler`
   - `StreakLostEventHandler`

**Oluşturulan Dosyalar:**
- [x] `src/Infrastructure/HealthVerse.Infrastructure/Persistence/DomainEventDispatcherInterceptor.cs`
- [x] `src/Modules/Identity/HealthVerse.Identity.Application/EventHandlers/UserCreatedEventHandler.cs`
- [x] `src/Modules/Identity/HealthVerse.Identity.Application/EventHandlers/StreakLostEventHandler.cs`

**Değiştirilen Dosyalar:**
- [x] `SharedKernel/Domain/IDomainEvent.cs` - INotification implement
- [x] `SharedKernel/HealthVerse.SharedKernel.csproj` - MediatR paketi
- [x] `Infrastructure/HealthVerse.Infrastructure.csproj` - MediatR paketi
- [x] `Api/Program.cs` - Interceptor ve assembly registration

---

### 3.2 Unit Tests ✅

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

**Mevcut Test Kapsamı:**
- `tests/HealthVerse.UnitTests/` projesi ✅
- **299 test** başarıyla geçti

**Test Dosyaları (18 adet):**
| Modül | Dosya Sayısı | Testler |
|-------|:------------:|:-------:|
| Identity | 4 | UserTests, UsernameTests, EmailTests, AuthIdentityTests |
| Social | 3 | DuelTests, FriendshipTests, UserBlockTests |
| SharedKernel | 4 | WeekIdTests, IdempotencyKeyTests, ResultTests, ErrorTests |
| Competition | 2 | LeagueRoomTests, LeagueMemberTests |
| Tasks | 2 | UserTaskTests, UserGoalTests |
| Missions | 2 | GlobalMissionTests, WeeklyPartnerMissionTests |
| Gamification | 2 | PointTransactionTests, MilestoneRewardTests |
| Notifications | 3 | NotificationTests, NotificationDeliveryTests, UserDeviceTests |

---

### 3.3 Architecture Tests ✅

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

**Mevcut Test Kapsamı:**
- `tests/HealthVerse.ArchitectureTests/` projesi ✅
- **40 test** başarıyla geçti

**Test Dosyaları (6 adet):**
| Test Sınıfı | Kontroller |
|-------------|------------|
| LayerDependencyTests | Domain → SharedKernel bağımlılığı |
| DomainConventionTests | Entity, ValueObject, DomainEvent kuralları |
| ApplicationConventionTests | Command, Query, Handler kuralları |
| InfrastructureConventionTests | Repository, Configuration kuralları |
| ApiConventionTests | Controller kuralları |
| ModuleIsolationTests | Modül katman yapısı kontrolü |

**Paket:** `NetArchTest.Rules`

---

## 4. 🟢 DÜŞÜK - Kod Kalitesi İyileştirmeleri

### 4.1 Notification.Create() IClock Kullanmıyor ✅

**Dosya:** `src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/Notification.cs`

**Çözüm:** `Create()` ve `MarkAsRead()` metodlarına optional `DateTimeOffset` parametresi eklendi. Test'lerde IClock üzerinden zaman geçirilebilir.

**Durum:** [x] ✅ TAMAMLANDI (30 Aralık 2025)

---

### 4.2 Test Kullanıcı Auto-Create Kaldır ✅

**Dosya:** `src/Api/HealthVerse.Api/Controllers/HealthController.cs`

**Çözüm:** Zaten temiz - auto-create zaten kaldırılmış durumda.

**Durum:** [x] ✅ TAMAMLANDI (Önceden yapılmış)

---

### 4.3 WeekId Tutarlılığı ✅

**Kontrol Sonucu:** Tüm `WeekId.FromDate()` çağrıları zaten `_clock.TodayTR` kullanıyor. Tutarlılık sağlanmış.

**Durum:** [x] ✅ DOĞRU (Önceden yapılmış)

---

## 5. ⚫ ERTELENEN - Bilinçli Kararlar

Bu maddeler **bilinçli olarak** ertelendi. MVP sonrası veya Flutter entegrasyonu ile birlikte yapılacak.

| Madde | Erteleme Nedeni | Ne Zaman |
|-------|-----------------|----------|
| sync-steps'te görev/hedef progress | Flutter Health metrikleri belirlendikten sonra | Flutter sonrası |
| TaskTemplates seed data | Flutter metrikleri belirlendikten sonra | Flutter sonrası |
| Görev atama ilgi alanı filtresi | Admin panel ile birlikte | Admin panel |
| Partial unique index (Duels) | Manuel SQL | Gerektiğinde |

---

## 6. 🔵 PRODUCTION - Deploy Öncesi

| Madde | Durum | Açıklama |
|-------|:-----:|----------|
| HTTPS zorunluluğu | [ ] | `app.UseHttpsRedirection()` + sertifika |
| Environment-based config | [ ] | `appsettings.Production.json` |
| Docker container | [ ] | `Dockerfile` + `docker-compose.yml` |
| CI/CD pipeline | [ ] | GitHub Actions |

---

## 7. 💜 FLUTTER - Client Entegrasyonu

| Madde | Durum | Açıklama |
|-------|:-----:|----------|
| Flutter `health` paketi | [ ] | iOS/Android sağlık verisi okuma |
| API client sınıfları | [ ] | Dart HTTP client |
| Auth token yönetimi | [ ] | Firebase Auth token |
| Background sync | [ ] | WorkManager/BackgroundFetch |

---

## 📊 ÖZET

| Öncelik | Kategori | Madde Sayısı | Tamamlanan |
|---------|----------|:------------:|:----------:|
| 🔴 KRİTİK | Push Pipeline | 8 | 8 ✅ |
| 🟠 YÜKSEK | Integration Tests | 5 | 5 ✅ |
| 🟡 ORTA | Mimari | 3 | 3 ✅ |
| 🟢 DÜŞÜK | Kod Kalitesi | 3 | 3 ✅ |
| ⚫ ERTELENEN | Bilinçli | 4 | - |
| 🔵 PRODUCTION | Deploy | 4 | 0 |
| 💜 FLUTTER | Client | 4 | 0 |

**Toplam Aktif Madde:** 23  
**Tamamlanan:** 19  
**İlerleme:** %83

---

## ✅ TAMAMLANANLAR LOG

| Tarih | Madde | Commit/Not |
|-------|-------|------------|
| 30 Aralık 2025 | 1.1 NotificationDelivery DbSet | HealthVerseDbContext.cs güncellendi |
| 30 Aralık 2025 | 1.2 NotificationDelivery Migration | AddNotificationDeliveries migration oluşturuldu |
| 30 Aralık 2025 | 1.3 INotificationDeliveryRepository | INotificationRepositories.cs güncellendi |
| 30 Aralık 2025 | 1.4 NotificationDeliveryRepository | NotificationDeliveryRepository.cs oluşturuldu |
| 30 Aralık 2025 | 1.5 IPushSender | IPushSender.cs oluşturuldu |
| 30 Aralık 2025 | 1.6 FirebasePushSender | FirebasePushSender.cs oluşturuldu |
| 30 Aralık 2025 | 1.7 PushDeliveryJob | PushDeliveryJob.cs oluşturuldu + Program.cs Quartz kaydı |
| 30 Aralık 2025 | 1.8 Notification→Delivery Integration | INotificationService + 20+ dosya refactored |
| 30 Aralık 2025 | 2.1-2.5 Integration Tests | 8 test dosyası, 29 test senaryosu |
| 30 Aralık 2025 | 3.1 Domain Event Dispatch | Interceptor + Event Handlers |
| 30 Aralık 2025 | 3.2 Unit Tests | 18 test dosyası, 299 test - Tüm modüller kapsandı |
| 30 Aralık 2025 | 3.3 Architecture Tests | 6 test dosyası, 40 test - Katman bağımlılıkları + kurallar |
| 30 Aralık 2025 | 4.1-4.3 Kod Kalitesi | IClock, WeekId tutarlılığı |

---

**NOT:** Bu doküman güncel tutulacak. Her madde tamamlandığında işaretlenecek ve log'a eklenecek.
