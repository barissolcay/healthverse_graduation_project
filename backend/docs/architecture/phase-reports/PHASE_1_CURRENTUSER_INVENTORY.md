# Faz 1 Envanteri: Current User & Auth

Bu belge, "Canonical User Id" refactor'u öncesi kod tabanındaki mevcut durumu belgeler.

## 1. Controller'larda `GetCurrentUserId()` Kullanımı

Aşağıdaki controller'larda `GetCurrentUserId()` ve/veya `X-User-Id` header okuma işlemi tespit edilmiştir:

- `src/Api/HealthVerse.Api/Controllers/AuthController.cs`
- `src/Api/HealthVerse.Api/Controllers/DevicesController.cs`
- `src/Api/HealthVerse.Api/Controllers/DuelsController.cs`
- `src/Api/HealthVerse.Api/Controllers/GlobalMissionsController.cs`
- `src/Api/HealthVerse.Api/Controllers/GoalsController.cs`
- `src/Api/HealthVerse.Api/Controllers/LeaderboardController.cs`
- `src/Api/HealthVerse.Api/Controllers/LeagueController.cs`
- `src/Api/HealthVerse.Api/Controllers/NotificationsController.cs`
- `src/Api/HealthVerse.Api/Controllers/PartnerMissionsController.cs`
- `src/Api/HealthVerse.Api/Controllers/SocialController.cs`
- `src/Api/HealthVerse.Api/Controllers/TasksController.cs`
- `src/Api/HealthVerse.Api/Controllers/UsersController.cs`

**Genel Pattern**:
Çoğu controller, base controller veya helper metodunu çağırarak header'dan ID okuyor ve parse edilemezse sabit bir GUID ("Test User") dönüyor olabilir.

## 2. Sabit GUID Kullanımı

Projede `00000000-0000-0000-0000-000000000001` (veya benzeri) fallback GUID kullanımı yaygın. Bu durum, production'da güvenlik açığı yaratır ve test verisinin karışmasına neden olur.

## 3. Auth Middleware

- `src/Infrastructure/HealthVerse.Infrastructure/Auth/FirebaseAuthMiddleware.cs`
  - Mevcut durumda: Firebase token doğruluyor (varsa).
  - Eksik: Token'dan `UserId` çözümlemesi yapmıyor (bunu controller'a bırakıyor).
  - Hedef: Token -> FirebaseUid -> DB Lookup -> `Claim(UserId)` akışını buraya taşımak.

## Dönüşüm Hedefi

Tüm bu yerlerdeki manuel header parse işlemleri kaldırılacak.
Yerine:
1. `ICurrentUser` servisi inject edilecek.
2. `_currentUser.UserId` (Guid) doğrudan kullanılacak.
3. Fallback/Bypass mantığı sadece Middleware seviyesinde ve sadece `Development` ortamında olacak.
