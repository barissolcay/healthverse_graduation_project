# HealthVerse - Hizli Baslangic

## Yeni Sohbette Ne Yapmalisin?

### 1. VS Code'da Proje Ac
```powershell
code "c:\Users\Baris\Desktop\healthverse"
```

### 2. Backend Test Et
```powershell
cd healthverse\backend\tests\HealthVerse.UnitTests
dotnet test
# 322 test passed olmali
```

### 3. Flutter Test Et
```powershell
cd healthverse\mobile
flutter pub get
flutter analyze
# 0 hata olmali
```

### 4. Backend Baslat
```powershell
cd healthverse\backend\src\Api\HealthVerse.Api
dotnet run
```

### 5. Flutter Calistir
```powershell
cd healthverse\mobile
flutter run
```

---

## Onemli Dosyalar

| Dosya | Aciklama |
|-------|----------|
| `mobile/lib/main.dart` | Flutter UI Entry |
| `mobile/lib/features/home/presentation/screens/home_screen_wrapper.dart` | Ana Ekran Wrapper |
| `mobile/lib/core/services/health_sync_service.dart` | Health paketi entegrasyonu |
| `mobile/lib/core/constants/api_constants.dart` | API URL ayarlari |
| `backend/src/Api/HealthVerse.Api/Controllers/HealthController.cs` | Health sync endpoint |
| `backend/docs/architecture/adr/0004-health-data-sync-architecture.md` | Mimari dokumantasyon |

---

## Yapilacaklar (TODO)

### Flutter
- [ ] iOS HealthKit izinleri (`ios/Runner/Info.plist`)
- [ ] Firebase Auth entegrasyonu
- [ ] Offline sync
- [ ] Background periodic sync

### Backend
- [x] Health sync endpoint
- [x] Goals progress updater
- [x] Tasks progress updater
- [x] Duels progress updater
- [x] Partner missions updater
- [x] Global missions updater

---

## Sorun Giderme

### API Baglantisi Kurulamiyorsa
1. Backend calisiyormu kontrol et: `http://localhost:5000/health`
2. Android emulator icin IP: `10.0.2.2:5000`
3. Fiziksel cihaz icin: Bilgisayarin IP adresini kullan

### Health Izinleri Calismiyorsa
1. Android 14+ icin Health Connect yuklu olmali
2. Izinler AndroidManifest.xml'de tanimli olmali
3. Runtime permission istenmeli

---

## Referanslar

- [mobile/README.md](mobile/README.md) - Flutter detaylari
- [backend/README.md](backend/README.md) - .NET API detaylari
- [ADR-0004](backend/docs/architecture/adr/0004-health-data-sync-architecture.md) - Mimari karar
