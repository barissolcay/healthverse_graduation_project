# NotificationDelivery + Push Sender Plan

Amaç: Bildirimler için push delivery outbox’ını tamamlamak; eksik migration’ı eklemek, gönderim işleyicisini tanımlamak ve retry/dnd kurallarını netleştirmek.

## Migration Taslağı (NotificationDeliveries)
- Tablo: `notification.NotificationDeliveries`
- Kolonlar:
  - `Id` (uuid, PK)
  - `NotificationId` (uuid, FK → Notifications.Id, on delete cascade)
  - `UserId` (uuid, FK → Users.Id, on delete cascade)
  - `Channel` (varchar, default 'Push')
  - `Status` (varchar, Pending|Sent|Failed|Cancelled)
  - `ScheduledAt` (timestamp with time zone)
  - `SentAt` (timestamp with time zone, null)
  - `AttemptCount` (int, default 0)
  - `LastError` (text, null)
  - `ProviderMessageId` (varchar, null)
  - `CreatedAt` (timestamptz, default now())
  - `UpdatedAt` (timestamptz, default now())
- Indexler:
  - `IX_NotificationDeliveries_Status_ScheduledAt` (covering pending scan)
  - `IX_NotificationDeliveries_UserId` (user cleanup)
  - Opsiyonel: `IX_NotificationDeliveries_NotificationId`

## Akış
1) **Notification create**: In-app notification yazılırken paralelde `NotificationDelivery.Create(notification.Id, userId, scheduledAt, DeliveryChannel.Push)` eklenir.
2) **Scheduler/Job** (Quartz): `PushDeliveryJob` her X saniyede çalışır, `Pending && ScheduledAt <= now` kayıtlarını batch çeker (örn. 100’lük sayfa).
3) **Push Sender**: Firebase/FCM client ile `UserDevice` tablosundaki aktif token’lara gönderir.
   - Token yoksa: delivery `Failed` + `LastError="NoDeviceToken"` (retry yok).
   - Token expired: remove/disable token, mark failed.
   - Geçici hata: `RecordFailedAttempt(error, retryAt = now + backoff)`.
4) **Başarılı**: `MarkAsSent(sentAt, providerMessageId)` → Status=Sent, SentAt set, AttemptCount artmaz.
5) **Retry Politikası**: Max 3 attempt. Backoff: 1m → 5m → 30m. Daha sonra Status=Failed.
6) **DND / Scheduled**: Quiet hours (örn. 22:00-08:00 TR) için `ScheduledAt` ertelenir; job sadece `ScheduledAt <= now` kayıtları işler.
7) **Idempotency**: Delivery `Id` unique; job idempotent; push provider response id optional `ProviderMessageId`.

## Gerekli Portlar/Servisler
- `INotificationDeliveryRepository`: `GetReadyToSendAsync(now, take)`, `UpdateAsync`, `AddAsync`, `SetFailedAsync`, `SetSentAsync`.
- `IUserDeviceRepository`: Aktif token’ları getir, invalid token’ı devre dışı bırak.
- `IPushSender` (infrastructure): FCM/Expo client; interface ile soyutla.
- `INotificationUnitOfWork`: SaveChanges.

## Diyagram (kısa)
Notification → (create) NotificationDelivery(Pending,ScheduledAt=now) → Quartz `PushDeliveryJob` → `PushSender` → Sent/Failed + retries.

## İş Küçük Adımlar
1) Migration ekle (schema yukarıdaki gibi) + DbContext DbSet.
2) `INotificationDeliveryRepository` + `IUserDeviceRepository` portları tanımla; infra implementasyonlarını ekle.
3) `PushDeliveryJob` (Quartz) yaz; batch çek → send → mark sent/failed.
4) Notification create noktalarında delivery ekle (Social/Duels/Missions/League vb.).
5) DND/backoff ayarlarını appsettings ile parametrik yap.
