# HealthVerse

Saglik ve fitness takip uygulamasi - Monorepo yapisi.

## Proje Yapisi

```
healthverse/
 backend/          # .NET 10 Backend API
    src/          # Kaynak kod
    tests/        # Unit, Integration, Architecture testleri
    docs/         # Dokumantasyon ve ADR'ler

 mobile/           # Flutter Mobil Uygulama
     lib/          # Dart kaynak kod
     android/      # Android platform kodu
     ios/          # iOS platform kodu
```

## Hizli Baslangic

### Gereksinimler
- .NET 10 SDK
- Flutter 3.35+ SDK
- PostgreSQL (Docker ile veya local)
- Android Studio / Xcode (mobil gelistirme icin)

### 1. Backend Baslat
```bash
cd backend/src/Api/HealthVerse.Api
dotnet run
# http://localhost:5000
```

### 2. Flutter Uygulama Baslat
```bash
cd mobile
flutter pub get
flutter run
```

## Dokumantasyon

| Dokuman | Icerik |
|---------|--------|
| [Backend README](backend/README.md) | .NET API detaylari |
| [Mobile README](mobile/README.md) | Flutter uygulama detaylari |
| [ADR'ler](backend/docs/architecture/adr/) | Mimari kararlar |
| [Hexagonal Contract](backend/docs/architecture/HEXAGONAL_CONTRACT.md) | Katman kurallari |

## Test

### Backend
```bash
cd backend/tests/HealthVerse.UnitTests
dotnet test  # 322 test

cd backend/tests/HealthVerse.ArchitectureTests
dotnet test  # 48 test
```

### Mobile
```bash
cd mobile
flutter analyze  # Kod analizi
flutter test     # Unit testler
```

## Mimari

### Backend - Hexagonal Architecture
- **Domain**: Is mantigi ve entity'ler
- **Application**: Use case'ler ve CQRS handler'lar
- **Infrastructure**: DB, API, external servisler
- **Api**: HTTP endpoint'ler

### Mobile - Clean Architecture + Feature-First
- **app/theme/**: Renk, tipografi, tema (Primary: #0F9124)
- **core/**: Ortak servisler ve utility'ler
- **features/auth/**: 8 kimlik dogrulama ekrani ✅
- **features/onboarding/**: 12 anket ekrani ✅
- **Siradaki**: Home ekrani

## Onemli Endpoint'ler

| Endpoint | Metod | Aciklama |
|----------|-------|----------|
| `/api/health/sync` | POST | Saglik verisi senkronizasyonu |
| `/api/auth/dev-login` | POST | Development login |
| `/api/tasks/goals` | GET | Kullanici hedefleri |
| `/api/duels` | GET/POST | Duello islemleri |
| `/api/missions/partner` | GET | Partner gorevleri |

## Lisans

Bu proje ozel bir mezuniyet projesidir.
