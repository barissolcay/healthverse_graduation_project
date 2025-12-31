# HealthVerse Mobile (Flutter)

Flutter ile gelistirilen mobil saglik uygulamasi. iOS HealthKit ve Android Health Connect entegrasyonu icin `health` paketi kullanilmaktadir.

## Proje Yapisi

```
mobile/
 lib/
    main.dart                    # Ana uygulama ve UI
    core/
        constants/
           api_constants.dart   # API endpoint sabitleri
        network/
           api_client.dart      # Dio HTTP client
        services/
            health_sync_service.dart  # Saglik verisi senkronizasyonu
 android/
    app/src/main/AndroidManifest.xml  # Health Connect izinleri
 pubspec.yaml                     # Bagimliliklar
```

## Kurulum

### 1. Flutter SDK Kontrolu
```bash
flutter --version
# Flutter 3.35.7 veya uzeri olmali
```

### 2. Bagimliliklari Yukle
```bash
cd mobile
flutter pub get
```

### 3. Kod Analizi
```bash
flutter analyze
# 0 hata olmali
```

## Calistirma

### Backend Baslat (Terminal 1)
```bash
cd backend/src/Api/HealthVerse.Api
dotnet run
# http://localhost:5000 adresinde baslar
```

### Flutter Uygulama Baslat (Terminal 2)
```bash
cd mobile

# Emulator listele
flutter emulators

# Emulator baslat
flutter emulators --launch <emulator_id>

# Veya bagil cihazlari listele
flutter devices

# Uygulamayi calistir
flutter run
```

## API Baglantisi

### Android Emulator
`api_constants.dart` dosyasinda:
```dart
static const String baseUrl = 'http://10.0.2.2:5000';
// 10.0.2.2 Android emulator icin localhost'a yonlendirir
```

### iOS Simulator
```dart
static const String baseUrl = 'http://localhost:5000';
```

### Fiziksel Cihaz
```dart
static const String baseUrl = 'http://<BILGISAYAR_IP>:5000';
// ornek: http://192.168.1.100:5000
```

## Saglik Izinleri

### Android (Health Connect)
`android/app/src/main/AndroidManifest.xml` dosyasinda tanimli:
- `READ_STEPS` - Adim sayisi
- `READ_DISTANCE` - Yurume/kosma mesafesi  
- `READ_TOTAL_CALORIES_BURNED` - Kalori
- `READ_ACTIVE_CALORIES_BURNED` - Aktif kalori
- `READ_HEART_RATE` - Nabiz
- `READ_SLEEP` - Uyku
- `READ_EXERCISE` - Egzersiz
- `ACTIVITY_RECOGNITION` - Aktivite tanima

### iOS (HealthKit)
`ios/Runner/Info.plist` dosyasina eklenecek (henuz yapilmadi):
```xml
<key>NSHealthShareUsageDescription</key>
<string>Saglik verilerinizi senkronize etmek icin izin gerekli</string>
<key>NSHealthUpdateUsageDescription</key>
<string>Saglik verilerinizi guncellemek icin izin gerekli</string>
```

## Uygulama Akisi

1. **Dev Login** - Test kullanicisi olusturur (`POST /api/auth/dev-login`)
2. **Saglik Izinleri** - Health Connect/HealthKit izni ister
3. **Veri Senkronizasyonu** - Gunluk verileri backend'e gonderir (`POST /api/health/sync`)

## Backend API Entegrasyonu

### Health Sync Endpoint
```
POST /api/health/sync
Header: X-User-Id: <user_guid>

Body:
{
  "activities": [
    {
      "activityType": "WALKING",
      "targetMetric": "STEPS",
      "value": 8500,
      "recordingMethod": "AUTOMATIC"
    }
  ]
}

Response:
{
  "success": true,
  "message": "Sync basarili",
  "totalSteps": 8500,
  "stepPointsEarned": 85,
  "taskPointsEarned": 50,
  "goalsCompleted": 1,
  "tasksCompleted": 2,
  "duelsUpdated": 1
}
```

### Recording Method Kurallari
| Method | Kabul | Aciklama |
|--------|-------|----------|
| AUTOMATIC |  | Cihaz tarafindan otomatik |
| ACTIVE |  | Kullanici aktif olarak kaydetti |
| MANUAL |  | Manuel giris (hile onleme) |
| UNKNOWN |  | Bilinmeyen kaynak |

## Gelistirme Notlari

### health Paketi (v13.2.1)
- iOS: HealthKit wrapper
- Android: Health Connect wrapper (Android 14+)
- Eski Android: Google Fit (deprecated)

### API Client
- Dio HTTP client kullanilir
- `X-User-Id` header ile auth (dev mode)
- flutter_secure_storage ile token saklama

### Veri Donusumu
Flutter `HealthDataPoint` -> Backend `HealthActivityData`:
```dart
// health_sync_service.dart icinde _convertToActivities()
// HealthDataType.STEPS -> "STEPS"
// HealthWorkoutActivityType.RUNNING -> "RUNNING"
// RecordingMethod.automatic -> "AUTOMATIC"
```

## Sonraki Adimlar

### Yapilacaklar
- [ ] iOS Info.plist HealthKit izinleri
- [ ] Firebase Auth entegrasyonu (dev-login yerine)
- [ ] Offline sync destegi
- [ ] Background sync (periodic)
- [ ] Push notification entegrasyonu

### Test
```bash
flutter test  # Unit testler
flutter analyze  # Kod analizi
```

## Ilgili Dokumanlar

- Backend Health Sync: `backend/docs/architecture/adr/0004-flutter-health-integration.md`
- API Inventory: `backend/docs/architecture/phase-reports/PHASE_2_API_INVENTORY.md`
- Hexagonal Contract: `backend/docs/architecture/HEXAGONAL_CONTRACT.md`
