# Phase 4 Report: Notification Policy + UserNotificationPreference

**Tarih**: 2024-12-30  
**Durum**: ✅ Tamamlandı

---

## Özet

Phase 4 kapsamında tam bir Notification Push Policy sistemi oluşturuldu:
- `UserNotificationPreference` entity ve tablosu
- `NotificationCategory` enum ve type→category mapping
- `INotificationPushPolicy` port ve implementasyonu
- API endpoint'leri (GET/PUT preferences)
- `NotificationService` policy entegrasyonu

---

## Yapılan Değişiklikler

### 1. Domain Katmanı (Notifications.Domain)

#### NotificationCategory Enum
`src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/NotificationCategory.cs`

```
10 kategori tanımlandı:
- Streak (push: default ON - kritik)
- Duel (push: default ON)
- Task (push: default ON)
- Goal (push: default OFF - düşük öncelik)
- PartnerMission (push: default ON)
- GlobalMission (push: default ON)
- League (push: default ON)
- Social (push: default ON)
- Milestone (push: default OFF - düşük öncelik)
- System (push: default ON - kritik)
```

#### TypeCategoryMapping
NotificationType string → NotificationCategory enum mapping:
- Tüm mevcut NotificationType'lar kategorilere eşlendi
- `GetCategory(string)`: Type'dan kategori döner
- `GetDefaultPushEnabled(NotificationCategory)`: Kategori için default push durumu

#### UserNotificationPreference Entity
`src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/UserNotificationPreference.cs`

| Alan | Tip | Açıklama |
|------|-----|----------|
| UserId | Guid | Kullanıcı ID |
| Category | NotificationCategory | Kategori enum |
| PushEnabled | bool | Push açık/kapalı |
| QuietHoursStart | TimeOnly? | Sessiz saat başlangıcı (UTC) |
| QuietHoursEnd | TimeOnly? | Sessiz saat bitişi (UTC) |
| CreatedAt | DateTimeOffset | Oluşturma zamanı |
| UpdatedAt | DateTimeOffset | Güncelleme zamanı |

### 2. Application Katmanı (Notifications.Application)

#### Ports
- `IUserNotificationPreferenceRepository`: Preference CRUD operasyonları
- `INotificationPushPolicy`: Push karar policy'si
  - `ShouldSendPushAsync`: Tek kullanıcı için karar
  - `ShouldSendPushBatchAsync`: Toplu karar (performans optimizasyonu)

#### PushDecision
- `ShouldSend`: Push gönderilmeli mi?
- `Reason`: Karar sebebi (Allowed, CategoryDisabledByDefault, DisabledByUser, QuietHours)
- `ScheduledAt`: Sessiz saatler için zamanlanmış gönderim

#### Query/Commands
- `GetNotificationPreferencesQuery`: Tüm tercihleri getir
- `UpdateNotificationPreferencesCommand`: Tercihleri güncelle

#### DTOs
- `NotificationPreferenceDto`: Tek kategori tercihi
- `NotificationPreferencesResponse`: Tüm tercihler
- `UpdateNotificationPreferencesRequest`: Güncelleme request
- `UpdatePreferencesResponse`: Güncelleme response

### 3. Infrastructure Katmanı

#### NotificationPushPolicy
`src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Services/NotificationPushPolicy.cs`

Push karar mantığı:
1. Category default policy kontrol
2. User preference kontrol (varsa)
3. Quiet hours kontrol (varsa)
4. Karar dön (Send / Block / Delay)

#### NotificationService Refactor
`src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/Services/NotificationService.cs`

- Artık `INotificationPushPolicy` kullanıyor
- In-app notification her zaman oluşturuluyor
- Push delivery sadece policy izin verirse oluşturuluyor
- Quiet hours durumunda zamanlanmış delivery oluşturuluyor

### 4. API Endpoint'leri

`src/Api/HealthVerse.Api/Controllers/NotificationsController.cs`

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/notifications/preferences` | Tüm kategoriler için tercihleri getir |
| PUT | `/api/notifications/preferences` | Tercihleri güncelle |

### 5. Migration
`src/Infrastructure/HealthVerse.Infrastructure/Migrations/20251230181523_AddUserNotificationPreferences.cs`

```sql
CREATE TABLE notifications."UserNotificationPreferences" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Category" varchar(50) NOT NULL,
    "PushEnabled" boolean NOT NULL,
    "QuietHoursStart" time,
    "QuietHoursEnd" time,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NOT NULL
);

CREATE UNIQUE INDEX "UX_UserNotificationPreferences_User_Category" 
    ON notifications."UserNotificationPreferences" ("UserId", "Category");
```

---

## Dosya Listesi

### Yeni Dosyalar
```
src/Modules/Notifications/HealthVerse.Notifications.Domain/Entities/
├── NotificationCategory.cs
└── UserNotificationPreference.cs

src/Modules/Notifications/HealthVerse.Notifications.Application/
├── Ports/INotificationPushPolicy.cs
├── Queries/PreferenceQueries.cs
└── Commands/PreferenceCommands.cs

src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/
├── Persistence/UserNotificationPreferenceRepository.cs
└── Services/NotificationPushPolicy.cs

src/Infrastructure/HealthVerse.Infrastructure/
├── Persistence/Configurations/UserNotificationPreferenceConfiguration.cs
└── Migrations/20251230181523_AddUserNotificationPreferences.cs
```

### Güncellenen Dosyalar
```
src/Modules/Notifications/HealthVerse.Notifications.Application/
├── DTOs/NotificationsDtos.cs (preference DTOs eklendi)
└── Ports/INotificationRepositories.cs (IUserNotificationPreferenceRepository eklendi)

src/Modules/Notifications/HealthVerse.Notifications.Infrastructure/
├── DependencyInjection.cs (yeni servisler eklendi)
└── Services/NotificationService.cs (policy entegrasyonu)

src/Infrastructure/HealthVerse.Infrastructure/Persistence/
└── HealthVerseDbContext.cs (DbSet eklendi)

src/Api/HealthVerse.Api/Controllers/
└── NotificationsController.cs (preference endpoint'leri eklendi)

docs/architecture/adr/
└── 0003-notification-delivery-policy.md (Status: Accepted)
```

---

## Test Sonuçları

| Test Suite | Sonuç |
|------------|-------|
| Unit Tests | ✅ 299/299 geçti |
| Integration Tests | ✅ 29/29 geçti |
| Architecture Tests | ✅ 41/41 geçti |

**Toplam: 369 test - Hepsi yeşil!**

---

## API Kullanım Örnekleri

### GET /api/notifications/preferences
```json
{
  "preferences": [
    {
      "category": "Streak",
      "displayName": "Streak Bildirimleri",
      "pushEnabled": true,
      "quietHoursStart": null,
      "quietHoursEnd": null
    },
    {
      "category": "Goal",
      "displayName": "Hedef Bildirimleri",
      "pushEnabled": false,
      "quietHoursStart": null,
      "quietHoursEnd": null
    }
  ]
}
```

### PUT /api/notifications/preferences
```json
{
  "preferences": [
    {
      "category": "Streak",
      "pushEnabled": true,
      "quietHoursStart": "22:00",
      "quietHoursEnd": "08:00"
    },
    {
      "category": "Social",
      "pushEnabled": false
    }
  ]
}
```

---

## Sonraki Adımlar (Opsiyonel)

1. **Unit Tests**: NotificationPushPolicy için unit testler
2. **Integration Tests**: Preference endpoint'leri için testler
3. **Job Audit**: Direct `Notification.Create` kullanan job'ları `INotificationService`'e taşıma
4. **Caching**: Preference lookup'ları için cache layer
