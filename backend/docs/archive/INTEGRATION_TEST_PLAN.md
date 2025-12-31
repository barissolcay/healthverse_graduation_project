# Integration Test Skeleton Plan

Amaç: Modüller arası temel akışları koruyan entegrasyon test iskeletini kurmak. Hedef: API uçları + EF + Quartz job’ları için güvence.

## Proje Yapısı
- Çözümde `tests/HealthVerse.IntegrationTests/` yeni .NET 10 test projesi.
- Paketler: `Microsoft.AspNetCore.Mvc.Testing`, `xunit`, `FluentAssertions`, `Respawn` (DB reset), `DotNetEnv` (appsettings override).
- Fixture: `CustomWebApplicationFactory` (Program.cs’yi kullanır, test Postgres/Sqlite seçeneği, DB seed stub).
- `TestBase` → client + scope provider; `ResetDatabaseAsync()` her test öncesi.

## Konfig
- `appsettings.Test.json`: in-memory JWT bypass, test Postgres connection, Firebase stub.
- `UseTestClock` opsiyonu: `IClock` stub enjekte etmek için DI override.
- Quartz: job scheduler disable veya inline trigger (finalize job senaryosu için manuel tetikleme API’si/DI trigger).

## Senaryo Seti (ilk dilim)
1) **Competition.Join & Leaderboard**
   - Arrange: seed user (tier ISINMA), league configs.
   - Act: POST /api/league/join, GET /api/league/my-room
   - Assert: room created, membership stored, HoursRemaining > 0.

2) **Competition.FinalizeWeek** (domain logic)
   - Arrange: seed room with members + points; mock `IClock` to fixed Sunday 23:59.
   - Act: call service/handler directly or trigger job; SaveChanges.
   - Assert: promoted/demoted counts, UserPointsHistory entries, room processed.

3) **Social.Follow + Mutual + Block**
   - Arrange: two users; POST follow; mutual check; block removes follow.
   - Assert: counters updated, mutual flag, notifications queued (NotificationDelivery pending).

4) **Duels lifecycle**
   - Create → Accept → Poke → history.
   - Assert: status transitions, notifications enqueued, poke limit (1/day) enforced.

5) **Partner Missions pairing**
   - Mutual friends required; pairing creates slots; poke once/day; history returns record.

6) **Global Missions join + detail**
   - Join mission; top contributors ordering; HoursRemaining > 0; participant count increments.

7) **Tasks/Goals**
   - Active tasks returned; expire marks FAILED; claim reward only when COMPLETED.
   - Goals create/list/delete; completed goal cannot be deleted.

8) **Notifications Push Outbox** (pipeline smoke)
   - Arrange: create Notification + Delivery pending; add UserDevice token stub; trigger PushDeliveryJob (DI resolved) with fake sender.
   - Assert: delivery marked Sent; AttemptCount; LastError on failure path.

## Test Data / Helpers
- `SeedBuilder` for users, league configs, tasks templates, missions.
- `ClockStub` implementing `IClock` with adjustable `UtcNow` / `TodayTR`.
- `NotificationSenderStub` capturing outbound push payloads.

## Çalıştırma
- `dotnet test tests/HealthVerse.IntegrationTests` (config: parallel off by default, collection fixtures per module).

## Sonraki Dilim
- Add data-driven tests for DND scheduling (NotificationDelivery), streak jobs, milestone awarding.
