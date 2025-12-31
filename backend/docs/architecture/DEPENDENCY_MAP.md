# Dependency Map — 31 Aralık 2025

Bu harita, `Modules.*.Application` projelerinin diğer modüllere ve katmanlara olan compile-time referanslarını belgeler.  
**Son Güncelleme:** Phase 6 (Modül İzolasyonu) sonrası + Phase 7 CI sonrası

## Özet Tablo

| Modül (Application) | Cross-Module Domain Refs | Cross-Module Application Refs | Durum |
|----------------------|--------------------------|-------------------------------|-------|
| **Competition** | Identity.Domain, Gamification.Domain | - | ⚠️ Domain refs var (Port kullanımı) |
| **Gamification** | Identity.Domain | - | ⚠️ Domain refs var (Port kullanımı) |
| **Identity** | - | - | ✅ Temiz |
| **Missions** | - | - | ✅ Temiz |
| **Notifications** | - | - | ✅ Temiz |
| **Social** | - | - | ✅ Temiz |
| **Tasks** | - | - | ✅ Temiz |

**Not:** Competition ve Gamification modüllerindeki Identity.Domain referansları, `IUserRepository` port interface'ini kullanmak için gereklidir. Bu, hexagonal mimari açısından kabul edilebilir bir trade-off'tur (Port interface'i Domain katmanında tanımlı).

---

## Detaylı Referans Listesi

`dotnet list reference` çıktısından derlenmiştir.

### 1. HealthVerse.Competition.Application
- **Internal**: `Competition.Domain`
- **Shared**: `SharedKernel`, `Contracts`
- **Cross-Module**: `Identity.Domain` (IUserRepository port için), `Gamification.Domain`

### 2. HealthVerse.Gamification.Application
- **Internal**: `Gamification.Domain`
- **Shared**: `Contracts`
- **Cross-Module**: `Identity.Domain` (IUserRepository port için)

### 3. HealthVerse.Identity.Application
- **Internal**: `Identity.Domain`
- **Shared**: `Contracts`
- **Durum**: ✅ Temiz

### 4. HealthVerse.Missions.Application
- **Internal**: `Missions.Domain`
- **Shared**: `SharedKernel`, `Contracts`
- **Durum**: ✅ Temiz

### 5. HealthVerse.Notifications.Application
- **Internal**: `Notifications.Domain`
- **Durum**: ✅ Temiz

### 6. HealthVerse.Social.Application
- **Internal**: `Social.Domain`
- **Shared**: `Contracts`
- **Durum**: ✅ Temiz

### 7. HealthVerse.Tasks.Application
- **Internal**: `Tasks.Domain`
- **Durum**: ✅ Temiz

---

## Phase 6 Sonrası İyileştirmeler

✅ **Tamamlanan:**
- `Application → Application` referansları kaldırıldı
- Cross-module iletişim `HealthVerse.Contracts` üzerinden sağlanıyor
- `INotificationService` Contracts'a taşındı
- `UserPointsEarnedEvent` Contracts'a taşındı

⚠️ **Kabul Edilen Trade-off:**
- Competition ve Gamification'ın `Identity.Domain`'e referansı devam ediyor
- Sebep: `IUserRepository` interface'i Identity.Domain.Ports'ta tanımlı
- Bu, leaderboard ve kullanıcı bilgisi sorgulama için gerekli
- Alternatif (ileride): Read-only DTO'ları Contracts'a taşımak
