# PHASE 2: API Inventory Report

## 2.0 Yasaklı Bağımlılık Taraması
Tarama Tarihi: 2025-12-30
Kapsam: `src/Api/HealthVerse.Api/Controllers`

### 1. DbContext Kullanımı
Komut: `rg "HealthVerseDbContext"`
Bulgular:
- **StatusController.cs**:
  - Line 13: `private readonly HealthVerseDbContext _dbContext;`
  - Line 17: Constructor injection.

**Analiz**: Sadece `StatusController` veritabanına erişiyor. Bu, canlılık (liveness/readiness) kontrolü için yapılıyor olabilir. Roadmap'e göre bu bir "allowlist" adayıdır ancak CQRS'e taşınması (Query) daha temiz bir mimari sunar.

### 2. Domain Logic / LINQ (Include, ToListAsync)
Komutlar: `rg "ToListAsync"`, `rg "Include"`, `rg "ThenInclude"`
Bulgular:
- **HİÇBİR SONUÇ BULUNAMADI.** ✅

**Analiz**: Controller'lar zaten `Mediator` pattern'ini düzgün uyguluyor. Logic sızıntısı yok.

## Sonuç ve Eylem Planı
Refactor edilmesi gereken tek controller: **StatusController**.
Ek olarak: Global Exception Handling (ProblemDetails) standardizasyonu yapılacak.
