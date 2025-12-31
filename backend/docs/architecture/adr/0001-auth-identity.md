# ADR 0001: Canonical User Identity & Mapping

**Durum**: Kabul Edildi  
**Tarih**: 30 Aralık 2025  
**Faz**: 1

## Bağlam

HealthVerse projesi kimlik doğrulama (Authentication) için Firebase kullanıyor, ancak domain içinde kendi `Guid UserId` yapısına sahip. Mevcut durumda:
1. Controller'lar `X-User-Id` header'ını manuel parse ediyor.
2. Firebase Token (JWT) içindeki UID (string) ile Domain UserId (Guid) arasındaki bağlantı controller'larda veya dağınık şekillerde çözülüyor.
3. Production ortamında bile header tabanlı "user impersonation" riski var (middleware sıkı değilse).

## Karar

1. **Canonical UserId**: Sistem içindeki tek geçerli kimlik `Guid UserId` olacaktır. Tüm application/domain logic bu ID'yi kullanacaktır.
2. **External Identity**: Firebase UID (`user_id` claim in JWT) sadece "giriş kapısında" (Auth Middleware) kullanılır.
3. **Single Point of Resolution (Middleware)**:
   - `FirebaseAuthMiddleware`, kimlik çözümlemenin **tek yeri** olacaktır.
   - Akış:
     - JWT doğrula -> FirebaseUID al.
     - DB (`AuthIdentities` tablosu) sorgula: `FirebaseUid` -> `UserId`.
     - `UserId` bulunursa: `HttpContext.User` içine `user_id` claim'i (Guid) ekle.
     - Bulunamazsa: 401/403 (veya Registration Required).
4. **Development Bypass**:
   - Sadece `ASPNETCORE_ENVIRONMENT=Development` (veya `Test`) durumunda `X-User-Id` header'ı kabul edilir.
   - Bu durumda direkt olarak header'daki GUID claim olarak eklenir.
   - Production'da bu header **asla** işlenmez.
5. **ICurrentUser**:
   - API/Application katmanları `HttpContext`'e dokunmaz.
   - `ICurrentUser` arayüzü `UserId` (Guid) döner. Eğer kullanıcı yoksa exception fırlatır (Null object yok, fallback yok).

## Sonuçlar

### Olumlu
- **Güvenlik**: Production'da kimlik sahteciliği (header spoofing) engellenir.
- **Tutarlılık**: Tüm modüller aynı `Guid`'i kullanır.
- **Temizlik**: Controller'lardan auth logic temizlenir.

### Olumsuz
- **Performans**: Middleware her istekte DB'ye (`AuthIdentities`) gidebilir. (İleride cache (Redis/Memory) ile çözülebilir).
- **Migration**: Mevcut testler/client'lar `X-User-Id` gönderiyorsa, Development ortamında çalıştıkları sürece sorun olmaz; ancak Production client'ı token göndermek zorundadır.
