# Phase 6: Module Isolation - Dependency Baseline

**Tarih**: 2024-12-30  
**Amaç**: Mevcut cross-module compile-time coupling fotoğrafı

## Cross-Module Application Referansları (Problem)

Aşağıdaki referanslar hexagonal kuralları ihlal ediyor:  
**Kural**: `Modules.*.Application` başka modülün `Application` katmanına referans vermemeli.

| Source Module | Target Module | Kullanım Amacı |
|--------------|---------------|----------------|
| `Competition.Application` | `Gamification.Application` | `UserPointsEarnedEvent` (MediatR event) |
| `Identity.Application` | `Notifications.Application` | `INotificationService` port |
| `Missions.Application` | `Notifications.Application` | `INotificationService` port |
| `Social.Application` | `Notifications.Application` | `INotificationService` port |

## Detaylı Kullanım Envanteri

### 1. Competition → Gamification.Application

**Dosya**: `Competition.Application/EventHandlers/UpdateLeaguePointsHandler.cs`
```csharp
using HealthVerse.Gamification.Application.Events;
// INotificationHandler<UserPointsEarnedEvent>
```
**İhtiyaç**: `UserPointsEarnedEvent` event sözleşmesi

### 2. Identity → Notifications.Application

**Dosyalar**:
- `Identity.Application/Commands/RegisterCommand.cs`
- `Identity.Application/Commands/DevRegisterCommand.cs`

```csharp
using HealthVerse.Notifications.Application.Ports;
// INotificationService kullanımı - welcome notification göndermek için
```
**İhtiyaç**: `INotificationService` interface + `NotificationCreateRequest` DTO

### 3. Missions → Notifications.Application

**Dosyalar**:
- `Missions.Application/Commands/JoinGlobalMissionCommand.cs`
- `Missions.Application/Commands/PairWithFriendCommand.cs`
- `Missions.Application/Commands/PokePartnerCommand.cs`

**İhtiyaç**: `INotificationService` interface

### 4. Social → Notifications.Application

**Dosyalar**:
- `Social.Application/Commands/PokeDuelCommand.cs`
- `Social.Application/Commands/FollowUserCommand.cs`
- `Social.Application/Commands/DuelDecisionCommands.cs`

**İhtiyaç**: `INotificationService` interface

## Çözüm Stratejisi

### HealthVerse.Contracts Projesi

Yeni proje: `src/Shared/HealthVerse.Contracts/`

**Taşınacak sözleşmeler**:

1. **INotificationService** (Interface/Port)
   - Kaynak: `Notifications.Application/Ports/INotificationService.cs`
   - Hedef: `Contracts/Notifications/INotificationService.cs`

2. **NotificationCreateRequest** (DTO)
   - Kaynak: `Notifications.Application/Ports/INotificationService.cs` (aynı dosyada)
   - Hedef: `Contracts/Notifications/NotificationCreateRequest.cs`

3. **UserPointsEarnedEvent** (Event)
   - Kaynak: `Gamification.Application/Events/UserPointsEarnedEvent.cs`
   - Hedef: `Contracts/Gamification/UserPointsEarnedEvent.cs`

### Referans Değişiklikleri (Sonrası)

| Module | Eski Referans | Yeni Referans |
|--------|--------------|---------------|
| Competition.Application | Gamification.Application | HealthVerse.Contracts |
| Identity.Application | Notifications.Application | HealthVerse.Contracts |
| Missions.Application | Notifications.Application | HealthVerse.Contracts |
| Social.Application | Notifications.Application | HealthVerse.Contracts |
| Gamification.Application | (değişiklik yok) | + HealthVerse.Contracts |
| Notifications.Application | (değişiklik yok) | + HealthVerse.Contracts |

## Metrics (Before)

- Cross-module Application referansları: **4**
- İhlal eden dosya sayısı: **9**
- Etkilenen modüller: Competition, Identity, Missions, Social

## Hedef Metrics (After)

- Cross-module Application referansları: **0**
- Tüm modüller `HealthVerse.Contracts` üzerinden iletişim kuracak
