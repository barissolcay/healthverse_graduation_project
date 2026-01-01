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

### iOS (HealthKit) ✅
`ios/Runner/Info.plist` dosyasinda tanimli (31 Aralik 2025):
```xml
<key>NSHealthShareUsageDescription</key>
<string>Saglik verilerinizi senkronize etmek icin izin gerekli</string>
<key>NSHealthUpdateUsageDescription</key>
<string>Saglik verilerinizi guncellemek icin izin gerekli</string>
```

Ayrica `ios/Runner/Runner.entitlements` dosyasinda HealthKit capability aktif.

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

### Tamamlanan Isler (01 Ocak 2026)
- [x] ~~iOS Info.plist HealthKit izinleri~~ (31 Aralik 2025)
- [x] Feature-based klasor yapisi
- [x] State management (ChangeNotifier - basit)
- [x] UI/Tasarim sistemi (AppColors, AppTypography, AppTheme)
- [x] Auth akisi (8 ekran)
- [x] Onboarding anketi (12 ekran)

### Yapilacaklar
- [ ] **SIRADAKI: Home ekrani**
- [ ] Firebase Auth entegrasyonu
- [ ] League/Lig ekrani
- [ ] Tasks/Gorevler ekrani
- [ ] Goals/Hedefler ekrani
- [ ] Offline sync destegi
- [ ] Push notification entegrasyonu

## Ekran Durumu

### Auth Ekranlari (8/8 Tamamlandi)
| Ekran | Dosya | Durum |
|-------|-------|-------|
| Splash | splash_screen.dart | ✅ |
| Auth Secimi | auth_selection_screen.dart | ✅ |
| Email Giris | email_entry_screen.dart | ✅ |
| Email Kayit | email_register_screen.dart | ✅ |
| Email Dogrulama | email_verification_screen.dart | ✅ |
| Takma Ad | username_selection_screen.dart | ✅ |
| Saglik Izni | health_permission_screen.dart | ✅ |
| Sifremi Unuttum | forgot_password_screen.dart | ✅ |

### Onboarding Anketi (12/12 Tamamlandi)
| Ekran | Dosya | Durum |
|-------|-------|-------|
| Hos Geldin | onboarding_welcome_screen.dart | ✅ |
| Dogum Yili | birth_year_screen.dart | ✅ |
| Cinsiyet | gender_screen.dart | ✅ |
| Sehir | city_screen.dart | ✅ |
| Calisma Durumu | employment_screen.dart | ✅ |
| Is Turu | work_type_screen.dart | ✅ |
| Boy & Kilo (BMI) | body_metrics_screen.dart | ✅ |
| Hedefler | goals_screen.dart | ✅ |
| Aktivite Seviyesi | activity_level_screen.dart | ✅ |
| Aktif Saatler | active_hours_screen.dart | ✅ |
| Nereden Duydun | referral_source_screen.dart | ✅ |
| Tamamlandi | onboarding_completion_screen.dart | ✅ |

## Klasor Yapisi (Guncel)

```
lib/
├── main.dart
├── app/
│   └── theme/
│       ├── app_colors.dart      # Renk paleti (#0F9124 primary)
│       ├── app_theme.dart       # MaterialApp tema
│       └── app_typography.dart  # Literata font stilleri
├── core/
│   ├── constants/
│   ├── network/
│   └── services/
└── features/
    ├── auth/
    │   └── presentation/
    │       └── screens/         # 8 auth ekrani
    └── onboarding/
        ├── onboarding.dart      # Barrel export
        └── presentation/
            ├── state/
            │   └── onboarding_state.dart  # Skip counter, BMI
            ├── widgets/
            │   ├── onboarding_scaffold.dart
            │   └── survey_option_tile.dart
            └── screens/         # 12 onboarding ekrani
```

## Test
```bash
flutter test  # Unit testler
flutter analyze  # Kod analizi
```

## Ilgili Dokumanlar

- [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) - Tasarim sistemi ve ekran listesi
- Backend Health Sync: `backend/docs/architecture/adr/0004-flutter-health-integration.md`
- API Inventory: `backend/docs/architecture/phase-reports/PHASE_2_API_INVENTORY.md`

