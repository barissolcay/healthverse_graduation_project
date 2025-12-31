# ADR-0004: Health Data Sync Architecture (Flutter Health Integration)

**Tarih**: 31 Aralık 2025  
**Durum**: Kabul Edildi  
**Karar Vericiler**: @barissolcay

## Bağlam

Mobil uygulama (Flutter) tarafında kullanıcıların sağlık verileri (adım, mesafe, kalori, süre vb.) **Flutter Health** paketi aracılığıyla toplanmaktadır. Bu veriler iOS'ta HealthKit, Android'de Health Connect API'leri üzerinden okunur.

Mevcut backend sadece `POST /api/health/sync-steps` endpoint'i ile günlük adım sayısını alıyordu. Ancak proje kapsamı çok daha geniş:

- **UserGoals**: Kullanıcının kişisel hedefleri (ActivityType + TargetMetric + TargetValue)
- **UserTasks**: Günlük/haftalık görevler (template bazlı, puan ödüllü)
- **Duels**: İki kullanıcı arasındaki rekabet (ActivityType + TargetMetric + TargetValue)
- **WeeklyPartnerMissions**: Haftalık partner görevleri (ortak hedef)
- **GlobalMissions**: Topluluk görevleri (herkesin katkı yaptığı havuz)

Tüm bu yapılar `ActivityType` (WALKING, RUNNING, CYCLING vb.) ve `TargetMetric` (STEPS, DISTANCE, CALORIES, DURATION) kombinasyonlarını kullanır.

## Sorun

1. **Parçalı Senkronizasyon**: Her modül için ayrı endpoint (sync-steps, sync-distance, sync-duel vb.) oluşturmak karmaşıklığı artırır.
2. **Tekrarlayan Kod**: Her endpoint'te aynı validasyon, idempotency ve kullanıcı kontrolü mantığı tekrarlanır.
3. **Cross-module Koordinasyon**: Tek bir sağlık verisi birden fazla modülü etkileyebilir (örn: 5000 adım hem Goal'u hem Task'ı hem de Duel'i güncellemeli).
4. **Recording Method Filtreleme**: Manuel girilen veriler (hile riski) kabul edilmemeli.

## Karar

**Tek akıllı endpoint** yaklaşımı benimsenmiştir:

```
POST /api/health/sync
```

### Mimari Yapı

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HealthController                              │
│                     POST /api/health/sync                            │
└──────────────────────────┬──────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   SyncHealthDataCommand                              │
│              (Gamification.Application)                              │
│                                                                      │
│  1. Validate activities (reject MANUAL/UNKNOWN)                      │
│  2. Get user from IUserRepository                                    │
│  3. Process step points (idempotent)                                 │
│  4. Run all IHealthProgressUpdater implementations (ordered)         │
│  5. Save changes (single transaction)                                │
│  6. Publish HealthDataSyncedEvent                                    │
└──────────────────────────┬──────────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┬───────────────┐
           ▼               ▼               ▼               ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
    │  Goals      │ │  Tasks      │ │  Duels      │ │  Missions   │
    │  Updater    │ │  Updater    │ │  Updater    │ │  Updaters   │
    │  (Order=20) │ │  (Order=30) │ │  (Order=40) │ │  (Order=50) │
    └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘
```

### Contracts (Cross-module)

Hexagonal architecture'a uygun olarak, modüller arası iletişim `HealthVerse.Contracts` projesi üzerinden sağlanır:

```
HealthVerse.Contracts/Health/
├── HealthActivityData.cs       # DTO: ActivityType, TargetMetric, Value, RecordingMethod
├── HealthSyncResponse.cs       # Response: tüm modül güncellemeleri
├── HealthProgressResult.cs     # Modül sonucu: UpdatedCount, CompletedCount, PointsEarned
├── IHealthProgressUpdater.cs   # Interface: Order, UpdateProgressAsync
├── HealthConstants.cs          # ActivityTypes, TargetMetrics sabitleri
└── HealthDataSyncedEvent.cs    # Domain event (INotification)
```

### IHealthProgressUpdater Interface

```csharp
public interface IHealthProgressUpdater
{
    /// <summary>
    /// Execution order. Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Updates module progress based on health activities.
    /// </summary>
    Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default);
}
```

### Updater Sırası

| Order | Updater | Modül | Açıklama |
|-------|---------|-------|----------|
| 20 | GoalsProgressUpdater | Tasks.Infrastructure | UserGoal.CurrentValue günceller |
| 30 | TasksProgressUpdater | Tasks.Infrastructure | UserTask.CurrentValue günceller, puan hesaplar |
| 40 | DuelsProgressUpdater | Social.Infrastructure | Duel skorlarını günceller, erken bitişi kontrol eder |
| 50 | PartnerMissionsProgressUpdater | Missions.Infrastructure | Partner progress günceller |
| 60 | GlobalMissionsProgressUpdater | Missions.Infrastructure | Idempotent contribution ekler |

### Recording Method Filtresi

Flutter Health paketi her veri için `recordingMethod` bilgisi sağlar:

| Method | Kabul? | Açıklama |
|--------|--------|----------|
| `AUTOMATIC` | ✅ | Sensör/cihaz tarafından otomatik kaydedilmiş |
| `ACTIVE` | ✅ | Kullanıcı workout başlatmış (timer ile) |
| `MANUAL` | ❌ | Kullanıcı manuel girmiş (hile riski) |
| `UNKNOWN` | ❌ | Belirsiz kaynak |

### Eşleştirme Mantığı

Her updater, gelen aktiviteleri kendi entity'leriyle eşleştirir:

```csharp
// Örnek: Duel eşleştirmesi
foreach (var duel in activeDuels)
{
    var match = activities.FirstOrDefault(a =>
        a.TargetMetric.Equals(duel.TargetMetric, StringComparison.OrdinalIgnoreCase) &&
        a.ActivityType.Equals(duel.ActivityType, StringComparison.OrdinalIgnoreCase));
    
    if (match != null)
    {
        // Update duel score
    }
}
```

**Kural**: Hem `ActivityType` hem `TargetMetric` eşleşmeli. Bazı entity'ler (Goals, Tasks) ActivityType'ı NULL olarak tutabilir; bu durumda sadece TargetMetric kontrolü yapılır.

### Idempotency

1. **Daily Steps**: `IdempotencyKey.ForDailySteps(userId, date)` - Aynı gün için tekrar puan verilmez
2. **Global Mission Contributions**: `MISSION:{missionId}:{userId}:{date}:{activityType}:{metric}` formatında unique key

## API Request/Response

### Request
```json
POST /api/health/sync
Authorization: Bearer {firebase_token}

{
  "activities": [
    {
      "activityType": "WALKING",
      "targetMetric": "STEPS",
      "value": 8500,
      "recordingMethod": "AUTOMATIC"
    },
    {
      "activityType": "RUNNING",
      "targetMetric": "DISTANCE",
      "value": 5200,
      "recordingMethod": "ACTIVE"
    },
    {
      "activityType": "RUNNING",
      "targetMetric": "CALORIES",
      "value": 320,
      "recordingMethod": "AUTOMATIC"
    }
  ]
}
```

### Response
```json
{
  "success": true,
  "message": "8,500 adım, 85 puan kazanıldı, streak korundu, 1 hedef tamamlandı.",
  "stepPointsEarned": 85,
  "taskPointsEarned": 50,
  "currentTotalPoints": 12450,
  "totalSteps": 8500,
  "streakSecured": true,
  "goalsUpdated": 2,
  "goalsCompleted": 1,
  "tasksUpdated": 3,
  "tasksCompleted": 1,
  "duelsUpdated": 1,
  "duelsFinished": 0,
  "partnerMissionsUpdated": 1,
  "globalMissionsContributed": 2,
  "alreadyProcessed": false,
  "rejectedActivities": 0
}
```

## Alternatifler (Değerlendirilip Reddedilen)

### A. Her modül için ayrı endpoint
- ❌ N+1 API call sorunu (her aktivite türü için ayrı istek)
- ❌ Transaction bütünlüğü sağlanamaz
- ❌ Client tarafında karmaşık orchestration gerekir

### B. Event-driven (async) güncelleme
- ❌ Kullanıcıya anlık feedback verilemez
- ❌ Eventual consistency sorunları
- ❌ Retry/failure handling karmaşıklığı

### C. Batch queue + worker
- ❌ Gerçek zamanlı UX beklentisiyle uyumsuz
- ❌ Operasyonel karmaşıklık

## Sonuçlar

### Olumlu
- ✅ Tek endpoint, tek transaction
- ✅ Tüm modüller senkronize güncellenir
- ✅ Recording method filtresi ile hile önleme
- ✅ Idempotent tasarım
- ✅ Hexagonal architecture'a uyumlu (Contracts üzerinden bağımlılık)
- ✅ Kolay genişletilebilir (yeni IHealthProgressUpdater ekle, DI'a kaydet)
- ✅ Test edilebilir (her updater izole test edilebilir)

### Dikkat Edilmesi Gerekenler
- ⚠️ Uzun süren bir sync işlemi timeout'a neden olabilir (monitoring gerekli)
- ⚠️ Büyük activity listesi performans sorunu yaratabilir (max 100 limit konuldu)
- ⚠️ Tüm updater'lar aynı DbContext transaction'ını paylaşır; birinin hatası tüm işlemi geri alır

## Dosya Listesi

### Contracts
- `src/Shared/HealthVerse.Contracts/Health/HealthActivityData.cs`
- `src/Shared/HealthVerse.Contracts/Health/HealthSyncResponse.cs`
- `src/Shared/HealthVerse.Contracts/Health/HealthProgressResult.cs`
- `src/Shared/HealthVerse.Contracts/Health/IHealthProgressUpdater.cs`
- `src/Shared/HealthVerse.Contracts/Health/HealthConstants.cs`
- `src/Shared/HealthVerse.Contracts/Health/HealthDataSyncedEvent.cs`

### Application
- `src/Modules/Gamification/HealthVerse.Gamification.Application/Commands/SyncHealthDataCommand.cs`

### Infrastructure (Updaters)
- `src/Modules/Tasks/HealthVerse.Tasks.Infrastructure/Services/GoalsProgressUpdater.cs`
- `src/Modules/Tasks/HealthVerse.Tasks.Infrastructure/Services/TasksProgressUpdater.cs`
- `src/Modules/Social/HealthVerse.Social.Infrastructure/Services/DuelsProgressUpdater.cs`
- `src/Modules/Missions/HealthVerse.Missions.Infrastructure/Services/PartnerMissionsProgressUpdater.cs`
- `src/Modules/Missions/HealthVerse.Missions.Infrastructure/Services/GlobalMissionsProgressUpdater.cs`

### API
- `src/Api/HealthVerse.Api/Controllers/HealthController.cs` (güncellendi)

### Tests
- `tests/HealthVerse.UnitTests/Gamification/SyncHealthDataCommandHandlerTests.cs` (12 test)

## Flutter Tarafı Kullanım Örneği

```dart
import 'package:health/health.dart';

Future<void> syncHealthData() async {
  final health = Health();
  
  // Request permissions
  await health.requestAuthorization([
    HealthDataType.STEPS,
    HealthDataType.DISTANCE_WALKING_RUNNING,
    HealthDataType.ACTIVE_ENERGY_BURNED,
  ]);
  
  // Get today's data
  final now = DateTime.now();
  final midnight = DateTime(now.year, now.month, now.day);
  
  final healthData = await health.getHealthDataFromTypes(
    types: [
      HealthDataType.STEPS,
      HealthDataType.DISTANCE_WALKING_RUNNING,
      HealthDataType.ACTIVE_ENERGY_BURNED,
    ],
    startTime: midnight,
    endTime: now,
  );
  
  // Convert to API format
  final activities = healthData.map((e) => {
    'activityType': _mapWorkoutType(e.workoutActivityType),
    'targetMetric': _mapDataType(e.type),
    'value': e.value.toInt(),
    'recordingMethod': e.recordingMethod.name.toUpperCase(),
  }).toList();
  
  // Send to backend
  await api.post('/api/health/sync', body: {'activities': activities});
}
```

## Referanslar

- [Flutter Health Package (pub.dev)](https://pub.dev/packages/health)
- [Apple HealthKit Documentation](https://developer.apple.com/documentation/healthkit)
- [Android Health Connect](https://developer.android.com/health-and-fitness/guides/health-connect)
- ADR-0001: Auth Identity (Firebase UID mapping)
- `docs/architecture/HEXAGONAL_CONTRACT.md`


---

## Flutter Uygulama Implementasyonu

### Proje Konumu
Flutter uygulaması `mobile/` klasöründe bulunmaktadır.

### Dosya Yapısı
```
mobile/lib/
 main.dart                           # UI ve state management
 core/
     constants/api_constants.dart    # API endpoint sabitleri
     network/api_client.dart         # Dio HTTP client
     services/health_sync_service.dart   # Health paketi entegrasyonu
```

### Kurulum Adımları
1. `cd mobile && flutter pub get`
2. Backend'i başlat: `cd backend/src/Api/HealthVerse.Api && dotnet run`
3. `flutter run`

### Platform Konfigürasyonu

#### Android (Health Connect)
`android/app/src/main/AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.health.READ_STEPS"/>
<uses-permission android:name="android.permission.health.READ_DISTANCE"/>
<uses-permission android:name="android.permission.ACTIVITY_RECOGNITION"/>
```

#### iOS (HealthKit) - TODO
`ios/Runner/Info.plist`:
```xml
<key>NSHealthShareUsageDescription</key>
<string>Sağlık verilerinizi senkronize etmek için izin gerekli</string>
```

### Bağımlılıklar (pubspec.yaml)
```yaml
dependencies:
  health: ^13.2.1
  dio: ^5.9.0
  flutter_secure_storage: ^10.0.0
```

### API Bağlantısı
- Android Emulator: `http://10.0.2.2:5000` (localhost'a yönlendirir)
- iOS Simulator: `http://localhost:5000`
- Fiziksel cihaz: `http://<PC_IP>:5000`
