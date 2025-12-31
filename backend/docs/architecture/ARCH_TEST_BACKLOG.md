# Architecture Test Backlog

Bu dosya, `Hexagonal Contract` kurallarını otomatize etmek için `tests/HealthVerse.ArchitectureTests` projesine eklenecek yeni test kurallarını listeler.

## Faz 1: Auth & API

- [ ] **Controller Design**: Controller sınıfları `DbContext`'e doğrudan bağımlı olmamalıdır.
  - *İstisna*: `StatusController`
- [ ] **Current User**: Controller sınıfları `HttpContext.Request.Headers["X-User-Id"]` okumamalıdır. (Tüm user resolution `ICurrentUser` üzerinden olmalıdır).
- [ ] **Hardcoded Guids**: Kod içinde (özellikle Controller/Auth) `00000000-0000-0000-0000-000000000001` dizesi bulunmamalıdır.

## Faz 2: API & Thin Controller

- [ ] **MediatR Only**: Controller metotları sadece `IMediator` kullanmalıdır (servis logic çağrılmamalı).
- [ ] **No Domain Logic**: API katmanında domain entity'leri oluşturulmamalı/manipüle edilmemelidir.

## Faz 3: Infrastructure

- [ ] **Migration Assembly**: `Program.cs` içinde migration assembly olarak `HealthVerse.Infrastructure` ayarlanmalıdır (bunu kodla test etmek zor olabilir, convention testi ile `DbContextOptions` kontrol edilebilir).

## Faz 4: Notifications

- [ ] **Unified Gateway**: `INotificationService` haricinde hiçbir sınıf (özellikle Job'lar) `Notification.Create` factory metodunu çağırmamalıdır.
- [ ] **Job Rules**: Job sınıfları (Infrastructure/Jobs namespace) `Notification` entity'sini doğrudan `DbContext.Add` ile eklememelidir.

## Faz 5: Jobs

- [ ] **Thin Jobs**: `Quartz` Job sınıfları `DbContext`'e bağımlı olmamalıdır. Sadece `IMediator` üzerinden komut göndermelidir.

## Faz 6: Module Isolation

- [ ] **App-to-App Restriction**: `Modules.*.Application` assembly'leri diğer `Modules.*.Application` assembly'lerine referans vermemelidir.
- [ ] **App-to-Domain Restriction**: `Modules.*.Application` assembly'leri diğer `Modules.*.Domain` assembly'lerine referans vermemelidir.
- [ ] **Infrastructure Restriction**: `Modules.*.Application` katmanı `Modules.*.Infrastructure` katmanına referans veremez (bu kural zaten var, sıkılaştırılacak).
