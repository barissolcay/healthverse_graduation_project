## HEXAGONAL_ROADMAP.md

Bu doküman, mevcut HealthVerse kod tabanını **adım adım daha “sıkı” bir hexagonal mimariye** yaklaştırmak için uygulanabilir bir yol haritasıdır.

### Hedef (Net Tanım)
Bu repo için “başarılı hexagonal” hedefini şöyle netliyorum:

- **Domain**: Saf iş kuralları; framework/EF/HTTP/Quartz bağımlılığı yok.
- **Application (Use-case)**: İş akışları tek yerde; dış dünya ile konuşmak için **Port** arayüzleri kullanır. Infrastructure’a compile-time bağımlılık yok.
- **Infrastructure (Adapters)**: EF Core, Firebase/FCM, Quartz gibi dış bağımlılıklar; Application port’larını implemente eder.
- **API (Adapter + Composition Root)**: HTTP adaptörü; mümkün olduğunca “thin controller” + DI wiring. (API’nin Infrastructure projelerini referanslaması “composition root” olarak kabul edilebilir.)
- **Jobs**: Sadece “tetikleyici/orchestrator”; iş kuralı Application use-case’lerinde olmalı.
- **Enforcement**: Mimari kurallar sadece doküman değil; **architecture tests** ile otomatik doğrulanmalı.

---

## Bu dokümanı nasıl kullanmalısın (sıra + çalışma ritmi)
Bu roadmap’i “okuyup kenara bırakma”. Her fazı uygularken **checklist** gibi işaretle ve her faz sonunda kısa bir rapor üret.

### Önerilen uygulama sırası (düşük risk → yüksek risk)
1) **Faz 0**: Guardrails + mimari kurallar + baseline  
2) **Faz 1**: Auth + CurrentUser boundary (Guid userId)  
3) **Faz 3**: Migrations tek zincir (Infrastructure) + data korunumu (bridge/baseline)  
4) **Faz 3.5**: Integration testleri Postgres/Testcontainers’a taşı  
5) **Faz 4**: Notification policy + UserNotificationPreference tablosu  
6) **Faz 2**: API adapter inceltme + API conventions (Status istisnaları dahil)  
7) **Faz 5**: Quartz jobs → orchestrator only  
8) **Faz 6**: Modül izolasyonu + Contracts  
9) **Faz 7**: CI + kalite kapıları (hard gate)

> Neden bu sıra? Kimlik ve DB gerçeği doğru değilse “hexagonal temizlik” kalıcı olmaz. Önce boundary + migration + integration test temeli, sonra refactor/polish.

### Her oturum için standart ritim (sürprizsiz ilerleme)
- [ ] **Branch aç** (Neden: rollback + izole ilerleme)
  - Öneri: `hex/f0-guardrails`, `hex/f1-current-user`, `hex/f3-migrations`, ...
- [ ] **Başlamadan baseline al** (Neden: regresyonu objektif ölçmek)
  - `dotnet build src/HealthVerse.sln -c Release`
  - `dotnet test src/HealthVerse.sln -c Release`
- [ ] **Tek oturum hedefi**: 1 net çıktı + 1 net doğrulama (Neden: büyük refactor riskini düşürür)
  - Örn. “LeagueController’da current user artık claim’den geliyor” + “integration testler yeşil”
- [ ] **Her küçük adım sonrası** ilgili testleri çalıştır (Neden: hata hangi commit’te geldi hemen anlarsın)
- [ ] **Her faz sonunda Phase Report yaz** (Neden: ileride ‘neden böyle?’ sorusuna kanıt)
  - Önerilen klasör: `docs/architecture/phase-reports/`
  - Önerilen dosya: `PHASE_<no>_<short-title>.md`

### Kanıt toplama standardı (özellikle migrations/test için)
- [ ] Komut çıktılarını dosyaya kaydet (Neden: “bende çalışıyordu” yerine kalıcı kanıt)
- [ ] DB snapshot’larını timestamp’li sakla (Neden: bridge/baseline migration sonrası “eşit mi?” kıyası)
  - Öneri: `docs/architecture/db/`

### Minimum araçlar (önceden kur)
- [ ] **.NET SDK 10**
- [ ] **Docker Desktop** (Testcontainers + CI parity)
- [ ] **PostgreSQL client tools** (`psql`, `pg_dump`) (prod/staging şema snapshot için)
- [ ] **DB erişimi** (staging/prod): connection string veya host/user/port bilgileri

---

## AI Otomasyon Modu (Agent Protokolü) — “kusursuzluk” için en yakın pratik yaklaşım
Bu doküman tek başına hiçbir AI’a “%100 kusursuz” kod yazdırmayı **garanti edemez** (özellikle prod DB/migrations, güvenlik ve ürün kararları yüzünden).  
Ama aşağıdaki protokolü ekleyerek hedefi şu noktaya getirir: **AI, hatayı erken yakalayan kapılarla ve deterministik adımlarla yüksek doğrulukla ilerler**.

### Ön kabuller (AI’ın gerçekten uygulayabilmesi için)
- AI kod yazacaksa **repo’ya yazma yetkisi** olmalı (branch/PR açabilmeli).
- AI komut çalıştıracaksa **terminal erişimi** olmalı (en az `dotnet`, `rg`, `docker`, `psql/pg_dump`).
- AI “prod/staging DB”ye bağlanacaksa: **erişim + yetki + güvenlik onayı** gerekir (bu kısım insan onay kapısıdır).

### Preflight (her faza başlamadan önce zorunlu)
Bu komutların çıktısını dosyaya kaydet (Neden: regresyon ve kanıt):
- [ ] `dotnet --info`
- [ ] `dotnet sln src/HealthVerse.sln list`
- [ ] `dotnet build src/HealthVerse.sln -c Release`
- [ ] `dotnet test src/HealthVerse.sln -c Release`
- [ ] EF sanity:
  - `dotnet ef migrations list --project src/Infrastructure/HealthVerse.Infrastructure --startup-project src/Api/HealthVerse.Api --context HealthVerseDbContext`

Önerilen kayıt:
- `docs/architecture/preflight/PREFLIGHT_YYYYMMDD.md`

### Değişmez kurallar (Invariants) — AI bunları asla “kolay olsun” diye delmemeli
- **Auth/UserId**:
  - Canonical kimlik: `Guid UserId` (Q1)
  - Controller’larda “fallback GUID” yok (örn. sabit test user)  
  - Dev/Test bypass varsa: `X-User-Id` **Guid olmalı**, parse edilemiyorsa 401
- **Status endpoints** (Q2):
  - Public: `/status/live`, `/status/ready` (minimal bilgi)
  - Protected: diagnostics/metrics/detailed
- **Migrations** (Q3 + Q6):
  - Migrations source of truth: **Infrastructure**
  - Data korunacak: prod/staging’de “reset” yok
  - Şema değişiklikleri: **incremental**; migration script’inde `DROP TABLE` vb. prod’a gitmez (özel durum hariç, insan onayı gerekir)
- **Integration tests DB** (Q5 + Q7):
  - Integration testler: gerçek Postgres (Testcontainers/Docker)
  - InMemory provider ile “integration test geçti” kabul edilmez
- **Notifications** (Q4 + Q8):
  - In-app ≠ push; push policy rule-based
  - Preferences: ayrı tablo (`UserNotificationPreference`)
- **Hexagonal sınırlar**:
  - `Modules.*.Application` → `Infrastructure` referansı yok
  - Controller’lar mümkünse DbContext kullanmaz (Status allowlist olabilir)
  - Jobs: orchestrator only; iş kuralı Application use-case’lerinde

### Çıktı standardı (AI’ın her PR/phase sonunda üretmesi gereken “kanıt paketi”)
- [ ] **Ne değişti?** (kısa özet + risk)
- [ ] **Hangi dosyalar değişti?** (path list)
- [ ] **Hangi komutlar çalıştırıldı?** (build/test/ef/docker)
- [ ] **Sonuçlar** (çıktı/exit code)
- [ ] **Rollback/geri dönüş notu** (özellikle migrations/auth değişiminde)

Önerilen dosya:
- `docs/architecture/phase-reports/PHASE_<no>_<title>.md`

### Stop conditions (AI durmalı ve insan onayı istemeli)
- Migration senaryosu B3 (drift) çıktıysa ve bridge stratejisi net değilse
- `dotnet test` fail ve fix “muğlak/ürün kararı” gerektiriyorsa
- Prod/staging DB’de DDL çalıştırma noktası geldiyse (script review + onay şart)
- Auth değişimi sonrası endpoint güvenliği belirsizleştiyse (public/protected karıştıysa)
- “Kolay çözüm” olarak hexagonal invariant’larını delme ihtiyacı doğduysa

### İnsan onayı gereken işler (AI tek başına yapmamalı)
- Staging/prod DB’ye bağlanma ve migration uygulama
- Migration dosyalarını silme/yeniden üretme (özellikle prod data varken)
- Auth/authorization policy’lerinde geniş kapsamlı değişiklik (public endpoint listesi dahil)
- Push policy defaultlarının “ürün davranışını” değiştirmesi

### Örnek “AI görev prompt” şablonu (kopyala-yapıştır)

```text
Sen bir AI coding agent’sın. Amaç: HEXAGONAL_ROADMAP.md’deki Faz <X> / Adım <Y>’yi uygula.
Kısıtlar:
- Yukarıdaki Invariants’i asla delme.
- Değişiklikleri küçük PR’lara böl (maks 1–3 ana dosya, 1 net çıktı).
- Her değişiklikten sonra ilgili testleri çalıştır ve çıktıyı rapora yaz.
İstenen çıktı:
- Uygulanan diff
- Çalıştırılan komutlar ve sonuçları
- Phase report (docs/architecture/phase-reports/PHASE_<X>_<title>.md)
Stop conditions:
- Migration drift / prod risk / ürün kararı gerektiren belirsizlikte dur ve insana sor.
```

---

## Mevcut Durumdan Kısa Notlar (Repo’ya göre)
- Controller’ların çoğu MediatR ile CQRS akışında; bu iyi bir temel.
- Bazı kritik sapmalar var:
  - “Current user” çözümlemesi controller’larda header parse + fallback GUID ile dağınık.
  - Migrations iki yerde (API/Infrastructure) ayrışmış.
  - Bildirim üretimi her yerde tek kapıdan geçmiyor (`INotificationService` vs direct `Notification.Create`).
  - Bazı Quartz job’ları iş kuralını direkt DbContext ile yürütüyor.

Bu roadmap bu dört noktayı “hexagonal uyum” için önceliklendirir.

---

## Karar Özeti (Çözüldü)
Aşağıdaki kritik kararlar bu yol haritası için sabitlendi (Q1–Q8 “bloklayıcı” olmaktan çıktı).

- [x] **Q1 — Canonical User Id**: **A** (Sistem içi `Guid UserId` kanonik), Firebase UID = external identity mapping
  - **Neden gerekli**: Domain/Application kimliği dış IdP’ye bağlanmamalı; uzun vadeli bağımlılığı azaltır.
- [x] **Q2 — Production `/status/*` politikası**: **Split**
  - **Public (auth yok)**: `/status/live`, `/status/ready` (minimal OK)
  - **Protected**: detay/diagnostics/metrics (auth veya IP allowlist)
  - **Neden gerekli**: operational health probe ihtiyacı ile güvenlik gereksinimini ayırır.
- [x] **Q3 — Migrations tek kaynağı**: **B** (Infrastructure projesinde migrations)
  - **Neden gerekli**: DB secondary adapter; DbContext + mapping + migrations aynı adaptörde kalmalı.
- [x] **Q4 — Push politikası**: **Hayır** (In-app ≠ push; push rule bazlı)
  - **Neden gerekli**: Push bir secondary adapter; kanal seçimi business/policy ile yönetilmeli.
- [x] **Q5 — Integration test DB stratejisi**: **Gerçek Postgres (Testcontainers/Docker)**
  - **Neden gerekli**: Migration/constraint/query translation doğruluğu InMemory ile yakalanmaz.
- [x] **Q6 — Prod/Staging DB ve data**: **B** (Data korunmalı; reset varsayılan değil)
  - **Neden gerekli**: Prod data geri dönüşü zor bir varlık; migration stratejisi “geriye dönük uyumlu” olmalı.
- [x] **Q7 — CI’da Docker**: **Evet** (Docker + Testcontainers çalıştırılacak)
  - **Neden gerekli**: Secondary adapter (Postgres) doğrulamasını deterministik hale getirir.
- [x] **Q8 — Push preference storage**: **B** (Ayrı tablo: `UserNotificationPreference` benzeri)
  - **Neden gerekli**: JSON metadata kısa vadede hızlı; uzun vadede sorgu/index/validasyon/raporlama/audit için zayıf kalır.

## Faz 3/3.5 için Gerekli Kanıtlar (toplanmadan bazı adımlar BLOKLU olabilir)
Bu maddeler “karar” değil, uygulama sırasında gereken **kanıt/çıktılar**dır. Faz 3’te adım adım toplanacak.

- **DB (staging/prod)**:
  - `__EFMigrationsHistory` içeriği (varsa)
  - `pg_dump --schema-only` (şema snapshot)
  - Kritik tablo/şema varlık kontrolü (örn. `notification.NotificationDeliveries`)
- **CI/Docker**:
  - CI job’unda Docker smoke (örn. `docker version`) + Testcontainers’ın çalıştığını doğrulayan log

---

## Faz 0 — Baseline + Mimari Kontrat + Guardrails

- **Amaç**: Hexagonal hedefini “sözde” değil “kural + test” seviyesine indirmek; refactor’u güvenli ve ölçülebilir yapmak.
- **Kapsam**:
  - **Dahil**: Mimari kuralların yazılması, mimari testlerin genişletilmesi, bağımlılık haritası çıkarma, backlog oluşturma.
  - **Hariç**: Büyük refactor (controller/job/migration değişimleri).
- **Ön koşullar / bağımlılıklar**:
  - Bloklayıcı soru yok.
  - CI yoksa bile lokal komutlarla ölçüm yapılabilir.
- **Adım adım yapılacaklar**:
  - [ ] **“Hexagonal Contract” dokümanı yaz** (Neden: herkesin aynı hedefi uygulaması için “kurallar kitabı” gerekir.)
    - Önerilen dosya: `docs/architecture/HEXAGONAL_CONTRACT.md`
    - İçerik: katman kuralları, izinli bağımlılıklar, istisnalar (örn. Status), naming.
  - [ ] **Bağımlılık haritası çıkar (csproj)** (Neden: modüller arası compile-time coupling nerede bilmeden izolasyon iyileşmez.)
    - Komutlar:
      - `dotnet sln src/HealthVerse.sln list`
      - Her proje için: `dotnet list <csproj> reference`
    - Çıktıyı kaydet: `docs/architecture/DEPENDENCY_MAP.md`
  - [ ] **Baseline test “golden” çıktısı al** (Neden: refactor sonrası regresyonu objektif ölçmek için.)
    - Komutlar:
      - `dotnet build src/HealthVerse.sln -c Release`
      - `dotnet test src/HealthVerse.sln -c Release`
    - Çıktıyı kaydet: `docs/architecture/BASELINE_YYYYMMDD.md`
  - [ ] **Architecture tests’e yeni kurallar için taslak liste ekle** (Neden: Faz 1+’de yapacağın değişimlerin kalıcı olması için.)
    - Önerilen yeni kurallar (fazlarda uygulanacak):
      - Controller’lar DbContext’e doğrudan bağımlı olamaz (allowlist hariç).
      - Job’lar DbContext’e doğrudan bağımlı olamaz (allowlist hariç).
      - Application projeleri Infrastructure’a referans veremez.
      - Module Application → başka module Application referansı “kontrat/proje” ile sınırlandırılır (Faz 6).
- **Beklenen çıktı**:
  - `docs/architecture/HEXAGONAL_CONTRACT.md`
  - `docs/architecture/DEPENDENCY_MAP.md`
  - `docs/architecture/BASELINE_YYYYMMDD.md`
  - Yeni mimari test kuralları için checklist/backlog
- **Doğrulama/kalite kontrol**:
  - `dotnet test src/HealthVerse.sln` hâlâ yeşil.
  - Bağımlılık haritasında “istenmeyen” referanslar işaretlenmiş.
- **Yaygın hatalar / dikkat**:
  - “Hexagonal yapacağız” deyip kuralları yazmamak → 2 ay sonra drift.
  - Büyük refactor’a guardrail olmadan girmek → testler yeşil kalsa bile mimari geri kayar.

---

## Faz 1 — Auth + CurrentUser Boundary’yi Tekleştir

- **Amaç**: “Kim kullanıcı?” bilgisini tek bir adaptörde çözmek; controller’ların header parse/fallback yapmasını bitirmek; gerçek hexagonal adapter davranışı.
- **Kapsam**:
  - **Dahil**: `CurrentUser` çözümlemesi standardı, middleware/claims, controller helper’larının kaldırılması, test auth uyumu.
  - **Hariç**: Endpoint iş kuralları değişikliği.
- **Ön koşullar / bağımlılıklar**:
  - Q1 sabit: `Guid UserId` kanonik, Firebase UID mapping.
  - Q2 sabit: `/status/live` + `/status/ready` public, geri kalan status/diagnostics protected.
- **Target dosyalar (repo’da mevcut)** (Neden: AI yanlış yerde değişiklik yapmasın):
  - `src/Infrastructure/HealthVerse.Infrastructure/Auth/FirebaseAuthMiddleware.cs`
  - `src/Api/HealthVerse.Api/Controllers/*.cs` (özellikle `StatusController.cs`, `AuthController.cs`, `UsersController.cs` vb.)
  - `tests/HealthVerse.IntegrationTests/TestAuthHandler.cs`
  - `tests/HealthVerse.IntegrationTests/IntegrationTestBase.cs`
  - `tests/HealthVerse.IntegrationTests/CustomWebApplicationFactory.cs`
- **Adım adım yapılacaklar**:
  - [ ] **ADR yaz: Canonical UserId + Auth mapping** (Neden: kimlik modelini kalıcılaştırmak ve drift’i engellemek için.)
    - Dosya: `docs/architecture/adr/0001-auth-identity.md`
    - Net kararlar:
      - Canonical: `Guid UserId`
      - Firebase UID: external identity
      - Middleware sonrası claim: `user_id=<Guid>`
  - [ ] **1.0 Envanter çıkar: “current user” nerelerde çözülüyor?** (Neden: refactor kapsamını netleştirip hiçbir endpoint’i unutma.)
    - [ ] Controller’larda `GetCurrentUserId()` geçen dosyaları listele
      - Komut: `rg "GetCurrentUserId\\(" src/Api/HealthVerse.Api/Controllers`
    - [ ] Fallback GUID kullanımını listele
      - Komut: `rg "00000000-0000-0000-0000-000000000001" src`
    - [ ] `X-User-Id` header okuyan yerleri listele
      - Komut: `rg "X-User-Id" src/Api/HealthVerse.Api/Controllers src/Infrastructure/HealthVerse.Infrastructure`
    - [ ] Çıktıyı kaydet: `docs/architecture/phase-reports/PHASE_1_CURRENTUSER_INVENTORY.md`
  - [ ] **1.1 Claim standardını kodda “tek kaynak” yap** (Neden: her yerde aynı string literal kullanılmasın.)
    - [ ] `user_id` claim adını bir sabitte topla (örn. `Claims.UserId`)
    - [ ] `firebase_uid` gibi claim’ler için de aynı yaklaşımı uygula (opsiyonel)
  - [ ] **API için `ICurrentUser` / `IUserContext` sözleşmesini tanımla** (Neden: controller’lar header/claim parse etmesin.)
    - Küçük parça: sadece `UserId` + `IsAuthenticated` ile başla.
    - [ ] `ICurrentUser` implementasyonunu yaz (API katmanında olabilir)
      - Kaynak: `HttpContext.User` → `user_id` claim → `Guid.Parse`
      - Hata davranışı: claim yoksa 401 (fallback yok)
  - [ ] **Firebase doğrulama sonrası “internal UserId resolve” akışını tasarla** (Neden: token doğrulamak yetmez; domain user’ı bulmak gerekir.)
    - Adımlar:
      - Firebase token doğrula → `firebase_uid`
      - `AuthIdentities` üzerinden `UserId` bul
      - Bulunamazsa: 401/403 (ürün kararına göre) + “register required”
      - Bulunursa: claim `user_id` set et
  - [ ] **1.2 FirebaseAuthMiddleware’ı “user_id claim üretir” hale getir** (Neden: inbound adapter’ın görevi kimliği çözmek ve request context’e koymaktır.)
    - [ ] Production path:
      - Token doğrula → Firebase UID al
      - DB lookup: `AuthIdentities.FirebaseUid == <uid>` → `UserId`
      - Claim set: `user_id=<UserId>`
    - [ ] Dev/Test bypass path:
      - `X-User-Id` header’ı varsa Guid parse et
      - Claim set: `user_id=<Guid>`
      - Guid parse edilemiyorsa 401 (fallback yok)
    - [ ] DB lookup performansı için not:
      - İlk iterasyonda direkt query OK
      - Sonraki iterasyon: in-memory cache (uid → userId) düşünülebilir
  - [ ] **Development bypass’ı standardize et (`X-User-Id` = Guid)** (Neden: dev/test akışı production kimlik modelini taklit etmeli.)
    - Kural: `X-User-Id` sadece Development/Test env’de kabul edilir ve **Guid parse edilemezse 401**.
  - [ ] **Controller’lardaki `GetCurrentUserId()` helper’larını kaldır (dikey slice)** (Neden: kimlik çözümü adapter’a taşınmalı.)
    - Tek oturum hedefi: 1 controller (örn. `LeagueController`) → `ICurrentUser` kullanır
    - Sonra: `UsersController`, `SocialController`, `DuelsController` ... şeklinde ilerle
    - [ ] Her controller refactor’dan sonra:
      - [ ] İlgili integration test(ler)i çalıştır
      - [ ] Swagger smoke (dev) ile 1 istek at (opsiyonel)
  - [ ] **Fallback GUID kullanımını tamamen kaldır** (Neden: gizli “admin/test user” davranışı production’da güvenlik açığıdır.)
    - Kural: UserId resolve edilemiyorsa request fail.
  - [ ] **Integration test auth’u yeni modele uyarla** (Neden: `Guid UserId` standardı gelince testler string “test-user” ile çalışmamalı.)
    - `TestAuthHandler`: `user_id` claim’i Guid set etmeli
    - `IntegrationTestBase`: `X-User-Id` header Guid olmalı
    - [ ] Testlerde user oluşturma yaklaşımını netleştir:
      - Seçenek A: Test başında 2 Guid üret, `dev-register` ile user oluştur, sonra header ile çağır
      - Seçenek B: Test seed helper ile DB’ye user insert (daha hızlı ama adapter gerçekliğini azaltır)
  - [ ] **`/status/live` ve `/status/ready` public olacak şekilde middleware public-list’i güncelle** (Neden: k8s/ingress probe auth header gönderemeyebilir.)
    - Not: `/status/detailed` ve olası `/status/diagnostics` protected kalmalı
  - [ ] **1.3 “No fallback GUID” mimari kuralı ekle** (Neden: ileride tekrar eklenmesini engellemek için.)
    - [ ] Architecture test: `00000000-0000-0000-0000-000000000001` literal’ı yasak (allowlist yok)
    - [ ] CI’da hard-gate yapma (Faz 7), şimdilik localde enforce edebilirsin
- **Beklenen çıktı**:
  - `CurrentUser` standardı + tek implementasyon noktası
  - Controller’larda header parse kodu yok (veya azaltılmış)
  - Integration test auth/headers bu standarda uyumlu
- **Doğrulama/kalite kontrol**:
  - Integration testler: `dotnet test tests/HealthVerse.IntegrationTests/...`
  - “No fallback GUID” kuralı (ör. grep ile): sabit GUID kullanımı kalmamalı.
  - Yeni architecture test: “Controllers do not parse X-User-Id” (regex/NetArchTest ile kısmi kontrol)
- **Yaygın hatalar / dikkat**:
  - Middleware içinde DB’ye gidip her request’te heavy query → caching/optimizasyon gerekebilir.
  - “Dev bypass” ile “Prod auth” davranışını karıştırmak → environment bazlı açık kurallar yaz.

---

## Faz 2 — API Adapter’ı İncelterek “Use-case First” Yap

- **Amaç**: API katmanını “sadece HTTP adaptörü” haline getirmek; controller’lar sadece MediatR send/return yapsın.
- **Kapsam**:
  - **Dahil**: `StatusController` dahil controller refactor (opsiyonel allowlist), hata/exception mapping standardı.
  - **Hariç**: Domain iş kurallarını değiştirme.
- **Ön koşullar / bağımlılıklar**:
  - Faz 1 tamamlanmış olmalı (CurrentUser standardı).
- **Adım adım yapılacaklar**:
  - [x] **2.0 Envanter: controller’larda “yasak bağımlılık” var mı?**
    - [x] DbContext kullanımları tarandı (Sadece StatusController bulundu).
    - [x] Logic sızıntıları tarandı (Temiz).
  - [x] **Controller’lar için “thin controller checklist” yaz**
  - [x] **[Opsiyonel] `StatusController`’ı query/handler’a taşı**
    - `GetSystemStatusQuery` + `SystemCheckService` (Infra) yapısına geçildi.
  - [x] **Global exception → ProblemDetails standardı ekle**
    - `GlobalExceptionHandler` ve `AddProblemDetails` eklendi.
  - [x] **ApiConventionTests’i genişlet**
    - Controller'ların DbContext/EF Core kullanması yasaklandı.
- **Beklenen çıktı**:
  - Controller’lar minimal.
  - Hata cevapları standard.
  - Architecture tests adapter katman kurallarını enforce ediyor.
- **Doğrulama/kalite kontrol**:
  - `dotnet test tests/HealthVerse.ArchitectureTests/...` yeşil.
  - API integration testleri yeşil.
- **Yaygın hatalar / dikkat**:
  - “Thin controller” yaparken domain validation’ı API’ye taşımak → validation Application/Domain’da kalmalı.

---

## Faz 3 — Persistence & Migrations: Tek Kaynak, Tek Zincir

- **Amaç**: DB şemasını yönetme biçimini tekleştirmek; migration drift riskini sıfıra indirmek.
- **Kapsam**:
  - **Dahil**: Migrations konumu kararı, tek zincire konsolidasyon, `dotnet ef` akışı standardı.
  - **Hariç**: Şema değişikliği gerektiren business değişiklikleri (sadece migration organizasyonu).
- **Ön koşullar / bağımlılıklar**:
  - Q3 sabit: Migrations kaynağı = **Infrastructure**
  - Q5 sabit: Integration test DB = **Postgres**
  - Q6 sabit: **Data korunmalı** → baseline/bridge stratejisi uygulanacak
- **Target dosyalar/klasörler (repo’da mevcut)** (Neden: migration konsolidasyonunda doğru “source of truth”u sabitlemek için):
  - API migrations: `src/Api/HealthVerse.Api/Migrations/`
  - Infra migrations: `src/Infrastructure/HealthVerse.Infrastructure/Migrations/`
  - DbContext: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/HealthVerseDbContext.cs`
  - Design-time factory: `src/Infrastructure/HealthVerse.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
  - Host config: `src/Api/HealthVerse.Api/Program.cs`
- **Adım adım yapılacaklar**:
  - [ ] **ADR yaz: Migrations stratejisi (Infrastructure)** (Neden: tek kaynak kararını kalıcılaştırmak için.)
    - Dosya: `docs/architecture/adr/0002-migrations-strategy.md`
    - Net karar: `DbContext + Configurations + Migrations` = Infrastructure
  - [ ] **3.1 DB keşfi: mevcut durumu kanıtla (staging/prod)** (Neden: data korunacağı için “ne var?” sorusu kanıtla yanıtlanmalı.)
    - [ ] **0) Çalışma dizini hazırla** (Neden: çıktıların kaybolmaması ve faz sonunda kanıt sunabilmek için.)
      - [ ] `docs/architecture/db/` klasörü yoksa oluştur
      - [ ] Bugünün çıktıları için bir klasör/başlık seç: `YYYYMMDD` (örn. `20251230`)
    - [ ] **1) DB erişimini “not et ama secret’ı commit etme”** (Neden: aynı komutları staging’de tekrar çalıştırabilmek için.)
      - [ ] DB host/port/db/user/sslmode bilgilerini `docs/architecture/db/DB_CONNECTION_NOTES.md` içine yaz
      - [ ] Şifre/token/secret **repo’ya yazılmayacak** (yerine “nerede saklı” notu: env var / secret manager / ops)
    - [ ] **2) Minimum güvenlik: schema-only dump al (prod/staging)** (Neden: yanlış migration durumunda geri dönüş planın olsun.)
      - Komut şablonu (connection string ile):
        - `pg_dump --schema-only --no-owner --no-privileges --format=plain --file docs/architecture/db/schema-YYYYMMDD.sql "<CONNECTION_STRING>"`
      - Beklenen:
        - `schema-YYYYMMDD.sql` dosyası oluşur ve içinde schema/table/index tanımları vardır
    - [ ] **3) `__EFMigrationsHistory` var mı? İçeriğini al ve kaydet** (Neden: hangi migration id’lerinin “uygulanmış” kabul edildiğini anlamak için.)
      - SQL:
        - `SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";`
      - Çalıştırma şablonu:
        - `psql "<CONNECTION_STRING>" -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";" > docs/architecture/db/ef-history-YYYYMMDD.txt`
      - Beklenen:
        - Satırlarda `MigrationId` ve `ProductVersion` görünür
        - Eğer tablo yoksa psql hata verir → bu durum **Senaryo B2** için sinyaldir (3.2’de sınıflandırılacak)
    - [ ] **4) Kritik tablolar var mı kontrol et** (Neden: code-first model ile prod şema arasında “sessiz eksik” var mı görmek için.)
      - Örnek SQL (Postgres):
        - `SELECT to_regclass('notification."NotificationDeliveries"');`
        - `SELECT to_regclass('notifications."UserDevices"');`
      - Çalıştırma şablonu:
        - `psql "<CONNECTION_STRING>" -c "SELECT to_regclass('notification.\"NotificationDeliveries\"');" > docs/architecture/db/check-deliveries-YYYYMMDD.txt`
    - [ ] **5) Repo migration id listesiyle karşılaştırma için “repo snapshot” al** (Neden: B1/B2/B3 seçimi kanıta dayalı olsun.)
      - [ ] Repo’daki migration dosya adlarını listele ve kaydet:
        - API: `src/Api/HealthVerse.Api/Migrations/`
        - Infra: `src/Infrastructure/HealthVerse.Infrastructure/Migrations/`
      - [ ] Çıktıyı kaydet: `docs/architecture/db/repo-migrations-YYYYMMDD.md`
    - [ ] **6) 3.1 çıktılarının özetini yaz** (Neden: Faz 3 sonunda “neye göre karar verdik” net kalsın.)
      - Dosya: `docs/architecture/db/DB_SNAPSHOT_YYYYMMDD.md`
      - İçerik: connection notes linki + ef-history var/yok + kritik tablo kontrolleri + schema dump dosya adı
  - [x] **3.2 Aktif migration zincirini sınıflandır**
    - Senaryo B3 (Split) tespit edildi.
  - [x] **3.3 (B1) API migration zincirini Infrastructure’a “aynı MigrationId’lerle” taşı**
    - 1-13 nolu API migration'ları Infra'ya taşındı.
    - Infra snapshot korundu.
  - [x] **3.5 (B1/B2 ortak) Runtime migration assembly’i Infrastructure yap**
    - Program.cs güncellendi.
  - [x] **3.8 `dotnet ef` komutlarını standardize et (tek komut seti)**
    - `docs/architecture/EF_COMMANDS.md` oluşturuldu.
- **Beklenen çıktı**:
  - Migrations tek yerde (`HealthVerse.Infrastructure`).
  - `Program.cs` migrations assembly ayarı ile uyumlu.
  - Migration uygulama komutu tek ve güvenilir.
- **Doğrulama/kalite kontrol**:
  - `dotnet ef migrations list` başarıyla tüm listeyi döndü.
  - `__EFMigrationsHistory` beklentisi: prod'da mevcut history ile uyumlu (ID değişmedi).

- **Yaygın hatalar / dikkat**:
  - Migration dosyalarını taşırken namespace/snapshot çatışması.
  - InMemory test DB ile “migration doğrulandı” sanmak (InMemory migrations’ı temsil etmeyebilir).

---

## Faz 3.5 — Integration Tests’i Gerçek Postgres’e Taşı (Testcontainers + CI Docker)

- **Amaç**: Integration testleri “gerçek secondary adapter” olan PostgreSQL üzerinde koşturmak; migration/constraint/query translation sorunlarını erken yakalamak.
- **Kapsam**:
  - **Dahil**: Testcontainers Postgres kurulumu, DbContext’in Npgsql ile testte çalışması, migration apply, DB reset (Respawn), CI’da Docker ile koşum.
  - **Hariç**: Unit testlerin DB’ye bağlanması (unit testler DB’siz kalmalı).
- **Ön koşullar / bağımlılıklar**:
  - Q5 sabit: Postgres integration tests
  - Q7 sabit: CI’da Docker var
  - Faz 3’te migrations “Infrastructure source of truth” olacak (en azından migration assembly netleşmeli)
- **Target dosyalar (repo’da mevcut)** (Neden: AI’ın test altyapısını doğru noktadan kurması için):
  - `tests/HealthVerse.IntegrationTests/HealthVerse.IntegrationTests.csproj`
  - `tests/HealthVerse.IntegrationTests/CustomWebApplicationFactory.cs`
  - `tests/HealthVerse.IntegrationTests/IntegrationTestBase.cs`
  - `tests/HealthVerse.IntegrationTests/TestAuthHandler.cs`
- **Adım adım yapılacaklar**:
  - [x] **0) Docker smoke (lokal)**
    - Docker version check yapıldı (Varsayım).
  - [x] **1) Testcontainers paketlerini ekle**
    - `Testcontainers.PostgreSql` eklendi.
  - [x] **2) Postgres container fixture yaz (IAsyncLifetime)**
    - `CustomWebApplicationFactory` içine gömüldü (Daha pratik).
  - [x] **3) CustomWebApplicationFactory’yi Npgsql’e geçir**
    - InMemory kaldırıldı, Npgsql + MigrateAsync eklendi.
  - [ ] **4) Test başlangıcında migrations apply et** (Neden: schema testte otomatik hazırlanmalı.)
    - Seçenekler:
      - **A) Kod içinden**: `DbContext.Database.Migrate()` (hızlı, tek süreç)
      - **B) Dışarıdan**: `dotnet ef database update` (deploy’a yakın, daha yavaş)
    - Öneri: A (Migrate) ile başla; ileride istersen B’ye geç
    - Kontrol:
      - [ ] Test DB’de `__EFMigrationsHistory` oluşuyor mu?
  - [ ] **5) DB reset stratejisi (Respawn) kur** (Neden: test izolasyonu; “test order” bağımlılığını engeller.)
    - Dosya önerisi: `tests/HealthVerse.IntegrationTests/DatabaseReset.cs` veya fixture içinde helper
    - Postgres özel notlar:
      - [ ] Multiple schema’lar var: `identity`, `social`, `missions`, `competition`, `gamification`, `notifications`, `notification`, `tasks`
      - [ ] Respawn config’te schema’ları dahil etmeyi unutma (aksi halde kirli state kalır)
  - [ ] **6) Quartz’ı Test env’de devre dışı bırak** (Neden: job’lar test DB’yi nondeterministic kirletebilir → flaky test.)
    - En temiz yaklaşım:
      - [ ] `Quartz:Enabled` config flag ekle (Program.cs içinde)
      - [ ] Test environment’te `Quartz:Enabled=false` set et (factory/config override)
    - Kontrol:
      - [ ] Test log’larında scheduler start edilmediği görülür
  - [ ] **7) Integration testleri lokal çalıştır** (Neden: CI’ya gitmeden önce hızlı doğrulama.)
    - Komut:
      - `dotnet test tests/HealthVerse.IntegrationTests/HealthVerse.IntegrationTests.csproj -c Release`
    - Beklenen:
      - Container başlar, migration uygulanır, testler yeşil
  - [ ] **8) CI pipeline’a Docker + integration tests job’u ekle** (Neden: local = CI parity.)
    - CI’de minimum doğrulama:
      - [ ] `docker version`
      - [ ] `dotnet test tests/HealthVerse.IntegrationTests/HealthVerse.IntegrationTests.csproj -c Release`
    - Testcontainers CI ipuçları:
      - [ ] Resource limit’leri (CPU/RAM) yeterli olmalı
      - [ ] Paralel test (xUnit) açılacaksa DB izolasyonu planlanmalı (db-per-testclass gibi)
- **Beklenen çıktı**:
  - Integration testler Postgres container üzerinde koşuyor ve yeşil.
  - Migration/constraint/query translation problemleri integration testte yakalanıyor.
- **Doğrulama/kalite kontrol**:
  - CI’da job yeşil (Docker + Postgres container).
  - Test log’larında InMemory provider kullanılmadığı doğrulanır.
  - `__EFMigrationsHistory` test DB’de beklenen migration zincirini gösterir.
- **Yaygın hatalar / dikkat**:
  - InMemory ile “integration test” sanmak → EF davranışı kaçabilir.
  - Migrations apply etmeden test başlatmak → “table not found” veya yanlış schema.
  - Quartz job’larının test DB’yi kirletmesi → flaky test.

---

## Faz 4 — Notifications: Tek Kapı + Outbox/Delivery Tutarlılığı

- **Amaç**: Bildirim üretimini tek bir use-case/service üzerinden standardize etmek; push delivery/outbox kayıtlarını tutarlı yapmak.
- **Kapsam**:
  - **Dahil**: `INotificationService` kullanımının standart hale gelmesi, job’ların bildirim üretim refactor’u, “push mu değil mi” politikasının netleşmesi.
  - **Hariç**: Yeni bildirim tipleri ekleme (yalnızca altyapı tutarlılığı).
- **Ön koşullar / bağımlılıklar**:
  - Q4 sabit: In-app ve push ayrı; push **rule bazlı**
  - Q8 sabit: Push preference storage = **ayrı tablo**
  - Faz 3 (migrations) idealde tamamlanmış olmalı (delivery tablosu için).
  - Faz 3.5 (Postgres integration tests) önerilir (policy/migration doğrulaması için)
- **Adım adım yapılacaklar**:
  - [ ] **ADR yaz: Notification kanalları ve push policy** (Neden: “hangi tip push olur?” gibi kararlar kodda dağılmasın.)
    - Dosya: `docs/architecture/adr/0003-notification-delivery-policy.md`
    - Minimum politika:
      - In-app: default **her zaman**
      - Push: type-based + device varlığı + quiet hours + rate limit + (varsa) user preference
    - ADR’de netleştir (ileride tartışma çıkmaması için):
      - “Push default açık mı kapalı mı?” (kategori bazlı default listesi)
      - “Quiet hours global mi user-specific mi?” (ilk iterasyonda global bırakmak kabul edilebilir)
      - “Rate limit hangi katmanda?” (creation-time mı send-time mı?)
  - [ ] **Notification kategorilerini netleştir (type → category mapping)** (Neden: preference tablosu kategori bazlı çalışacaksa mapping net olmalı.)
    - Önerilen başlangıç kategorileri (mevcut tip gruplarından): `STREAK`, `DUEL`, `TASK`, `GOAL`, `PARTNER_MISSION`, `GLOBAL_MISSION`, `LEAGUE`, `SOCIAL`, `MILESTONE`, `SYSTEM`
    - [ ] Mapping tablosunu dokümante et (Neden: API/UX ve policy aynı kaynağa baksın.)
      - Dosya: `docs/architecture/notifications/NOTIFICATION_CATEGORIES.md`
  - [ ] **Notification üretim envanteri çıkar** (Neden: direct `Notification.Create` kullanan yerler tek tek bulunmalı.)
    - Özellikle: `src/Infrastructure/HealthVerse.Infrastructure/Jobs/*.cs`
    - Hızlı arama komutları (kendin çalıştır):
      - `rg "Notification\.Create\(" src`
      - `rg "INotificationService" src/Infrastructure/HealthVerse.Infrastructure/Jobs`
  - [ ] **“Tek kapı” kuralını netleştir** (Neden: herkes aynı API’yi kullanmalı.)
    - Kural: “In-app notification creation” tek kapıdan (`INotificationService` veya yeni `INotificationPublisher`) geçer.
    - Push delivery: policy kararına göre aynı kapı **opsiyonel** delivery üretir.
    - “Tek kapı” için pratik karar:
      - [ ] Job/handler/controller içinde doğrudan `Notification.Create(...)` yasak (allowlist yok)
      - [ ] İstisna gerekiyorsa ADR’ye yaz (aksi halde drift)
  - [ ] **UserNotificationPreference modelini tasarla (Domain + Port + Adapter)** (Neden: push kararları user preference’a bağlanacak.)
    - [ ] Domain entity/value object’ler (Notifications.Domain)
    - [ ] Port: `INotificationPreferenceRepository` (Notifications.Application)
    - [ ] Adapter: EF repository + configuration (Notifications.Infrastructure/Infrastructure.Persistence)
    - [ ] Migration: Infrastructure migrations zincirinde yeni tablo
    - [ ] Tablo tasarımını netleştir (Neden: migration üretmeden önce veri modeli kesinleşmeli.)
      - Önerilen minimum tablo (ilk iterasyon):
        - `UserId uuid` (PK parçası)
        - `Category text` (PK parçası; enum/const ile yönet)
        - `PushEnabled boolean`
        - `QuietHoursStart time` (nullable)
        - `QuietHoursEnd time` (nullable)
        - `UpdatedAt timestamptz`
      - Önerilen unique/PK: `(UserId, Category)`
      - Önerilen schema: `notifications` (mevcut `UserDevices` ile aynı schema’da tutmak pratik)
    - [ ] Default davranışı belirle (Neden: preference kaydı yokken ne olacak?)
      - Öneri: “kayıt yoksa default policy uygula” (örn. bazı kategoriler push açık, bazıları kapalı)
    - [ ] Backfill stratejisi yaz (Neden: data korunumu; mevcut kullanıcılar için default kayıt gerekebilir.)
      - Seçenek A: Lazy (ilk istek geldiğinde oluştur)
      - Seçenek B: Migration ile seed/backfill (prod’da daha deterministik)
  - [ ] **Preference yönetimi için API yüzeyi ekle (minimum)** (Neden: user preference’ı nasıl set edeceğini tanımlamazsan policy “hard-coded” kalır.)
    - Örn: `GET/PUT /api/notifications/preferences` veya `UsersController` altında `/me/preferences`
    - [ ] DTO’ları tasarla (Neden: API contract stabil olmalı)
      - GET response: `categories[]` + her biri için `pushEnabled`, `quietHoursStart/end`
      - PUT request: replace-all veya patch (ADR’de seç)
    - [ ] Authorization: sadece “current user” kendi preference’ını değiştirebilmeli (Neden: güvenlik)
  - [ ] **Job’lardan başlayarak direct create → service refactor planı** (Neden: en kritik drift kaynağı job’lar.)
    - Küçük parça: 1 job seç (örn. `WeeklySummaryJob`) ve sadece onu “service-only” yap.
    - Önerilen sıra:
      - `WeeklySummaryJob` (zaten batch create ile iyi aday)
      - `ReminderJob` (çok sayıda notification üretir)
      - `DailyStreakJob` / `ExpireJob` (kritik ve sık çalışır)
  - [ ] **NotificationDelivery doğrulama testleri ekle** (Neden: “bildirim var ama push queue yok” hatası prod’da pahalıdır.)
    - [ ] Postgres integration test: “push enabled category → delivery oluşur”
    - [ ] Postgres integration test: “push disabled category → delivery oluşmaz ama in-app notification olur”
    - [ ] Postgres integration test: “quiet hours içindeyken delivery schedule doğru mu?” (eğer creation-time schedule yapacaksan)
  - [ ] **Policy’yi tek noktaya indir (`INotificationPushPolicy`)** (Neden: push kararı kodda dağılmasın.)
    - [ ] Notifications.Application içine `INotificationPushPolicy` interface’i ekle
    - [ ] Default implementasyon yaz (type/category + device varlığı + preference)
    - [ ] NotificationService sadece bu policy’den çıkan karara göre delivery üretmeli
- **Beklenen çıktı**:
  - Bildirim üretimi tutarlı (tek kapı veya açık ayrım).
  - Push delivery/outbox kayıtları politikaya uygun.
- **Doğrulama/kalite kontrol**:
  - Integration test: “bildirim üretildiğinde delivery kayıtları da var” senaryoları.
  - Architecture test: “Jobs do not call Notification.Create” (policy’ye göre).
- **Yaygın hatalar / dikkat**:
  - “Tek kapı” yaparken performansı düşünmemek (batch create önemli).
  - Delivery tablosu migrate edilmeden push job’u çalıştırmak → runtime hata.

---

## Faz 5 — Quartz Jobs: Orchestrator Only (Business Logic → Application)

- **Amaç**: Jobs’u hexagonal’e uygun hale getirmek: job = trigger, iş kuralı = use-case.
- **Kapsam**:
  - **Dahil**: Her job için Application command/handler, portlar üzerinden DB erişimi, idempotency.
  - **Hariç**: Quartz schedule değiştirme (opsiyonel).
- **Ön koşullar / bağımlılıklar**:
  - Faz 4 (notification policy) netleşmiş olmalı.
- **Adım adım yapılacaklar**:
  - [ ] **5.0 Job envanteri + “ne yapıyor?” dökümü çıkar** (Neden: hangi job hangi modül sınırını aşıyor görmeden doğru parçalayamazsın.)
    - Kaynak klasör: `src/Infrastructure/HealthVerse.Infrastructure/Jobs/`
    - Her job için şu şablonla kısa not çıkar:
      - Schedule (cron/interval)
      - Okuduğu tablolar (DbSet’ler)
      - Yazdığı tablolar
      - Ürettiği notification tipleri
      - Idempotent mi? (tekrar çalışınca ne olur?)
    - Çıktıyı kaydet: `docs/architecture/phase-reports/PHASE_5_JOBS_INVENTORY.md`
  - [ ] **Job-by-job refactor backlog’u çıkar** (Neden: 10 job’u birden taşımak riskli; sıraya koy.)
    - Önerilen sıra (risk/etki):
      1) `WeeklyLeagueFinalizeJob` (tutarsızlık var)
      2) `WeeklySummaryJob` (batch notification)
      3) `ReminderJob` (çok kural)
      4) `ExpireJob` (state transitions)
      5) `DailyStreakJob`
      6) `MilestoneCheckJob`
      7) `GlobalMissionFinalizeJob`
      8) `PartnerMissionFinalizeJob`
      9) `StreakReminderJob`
      10) `PushDeliveryJob` (infra-heavy; en sona)
  - [ ] **5.1 “Doğru hedef katman” kararını standardize et** (Neden: job logic’i nereye taşınacak net olmalı.)
    - Kural: Job logic’i **Infrastructure’a taşınmaz**, **modül Application** içine taşınır.
    - Job sınıfı (Quartz) sadece orchestrator:
      - `await _mediator.Send(...)`
      - minimal logging + error handling
  - [ ] **5.2 Job → Modül Command parçalama şablonu** (Neden: tek bir job birden fazla modül kuralı çalıştırıyor olabilir.)
    - Örnek yaklaşım:
      - `ExpireJob` → `Tasks.Application` içinde `ExpireUserTasksCommand` + `Social.Application` içinde `ExpireDuelInvitationsCommand`/`FinishExpiredDuelsCommand`
      - Job sınıfı sadece bu komutları sırayla çağırır
  - [ ] **5.3 İlk “dikey slice” seç ve tamamla (1 job)** (Neden: pattern’i oturtup sonra hızlanmak için.)
    - Öneri: `WeeklySummaryJob` veya `ExpireJob` (etkisi yüksek, test edilebilir)
    - Tek oturum hedefi:
      - [ ] 1 yeni Command + Handler
      - [ ] Job sınıfından DbContext kodu kaldır
      - [ ] En az 1 test (unit veya integration) ile doğrula
  - [ ] **5.4 Handler’ların port’larını netleştir** (Neden: Application bağımsız kalmalı.)
    - Kural: Handler içinde `HealthVerseDbContext` yok
    - Gerekirse yeni port ekle (örn. `IUserTaskRepository`, `IDuelRepository` zaten bazı modüllerde var)
  - [ ] **5.5 Idempotency / re-run güvenliği ekle** (Neden: Quartz job’lar retry/yeniden çalıştırma yaşayabilir.)
    - Her command için:
      - “Zaten expire olmuş task” tekrar expire edilince ne olur?
      - “Zaten gönderilmiş reminder” tekrar gönderilir mi? (idempotency key / recent check)
      - “Finalize” tekrar koşarsa double reward/notification var mı?
  - [ ] **5.6 Mimari kural: Jobs DbContext bağımlılığı yok** (Neden: drift’i engellemek.)
    - Architecture test:
      - `src/Infrastructure/HealthVerse.Infrastructure/Jobs/*` içinde `HealthVerseDbContext` referansı yasak (geçiş sürecinde allowlist ile)
  - [ ] **5.7 Ölçüm** (Neden: ilerlemeyi sayısal takip etmek için.)
    - [ ] “DbContext kullanan job sayısı” metriğini yaz ve her job dönüşümünde güncelle
- **Beklenen çıktı**:
  - Job sınıfları “thin”.
  - İş kuralı Application’da.
  - Infrastructure sadece scheduling + adapter.
- **Doğrulama/kalite kontrol**:
  - Integration testlerde Quartz başlatma/stop stabil.
  - Yeni unit test: handler’lar (job use-case) deterministic.
  - Architecture test: Jobs do not depend on DbContext (allowlist hariç).
- **Yaygın hatalar / dikkat**:
  - Batch işlemleri handler’a taşırken “N+1 query” üretmek.
  - “Sadece taşıdım” deyip transaction/UoW sınırını bozmak.

---

## Faz 6 — Modül İzolasyonu: Cross-module Contracts ile Bağımlılıkları Sadeleştir

- **Amaç**: Modüller arası bağımlılığı “kontrat” seviyesine indirip application-to-application coupling’i azaltmak.
- **Kapsam**:
  - **Dahil**: Cross-module event/contract projesi, csproj referanslarının temizlenmesi, architecture test kuralları.
  - **Hariç**: Modülleri ayrı servislere bölmek (bu roadmap modüler monolith içinde kalır).
- **Ön koşullar / bağımlılıklar**:
  - Faz 0’daki dependency map hazır olmalı.
- **Adım adım yapılacaklar**:
  - [ ] **6.0 Mevcut “compile-time coupling” fotoğrafını çek** (Neden: hedefi ölçmek için önce mevcut durumu sayısallaştır.)
    - [ ] Solution projelerini listele ve kaydet:
      - `dotnet sln src/HealthVerse.sln list`
    - [ ] Her `Modules.*.Application` projesinin referanslarını çıkart:
      - `dotnet list <csproj> reference`
    - [ ] Hızlı grep:
      - `rg "<ProjectReference" src/Modules -g"*.csproj"`
    - [ ] Çıktıları kaydet: `docs/architecture/phase-reports/PHASE_6_DEPENDENCY_BASELINE.md`
    - Not (repo’da bugün görünen örnek coupling’ler):
      - `src/Modules/Competition/HealthVerse.Competition.Application/HealthVerse.Competition.Application.csproj` → `HealthVerse.Gamification.Application` referansı var
      - `src/Modules/Identity/HealthVerse.Identity.Application/HealthVerse.Identity.Application.csproj` → `HealthVerse.Notifications.Application` referansı var
  - [ ] **Cross-module etkileşim envanteri çıkar** (Neden: hangi modül hangi modülden ne alıyor bilmek şart.)
    - Örn. Gamification → Competition: “points earned” sinyali.
    - Social/Missions → Notifications: notification port.
    - [ ] Envanteri “3 tipe” ayır (Neden: çözüm yolu tipe göre değişir):
      - **Event**: “şu oldu” (publish/subscribe)
      - **Query**: “şunu bana ver” (read model / projection)
      - **Command/Service**: “şunu yap” (use-case çağrısı)
  - [ ] **Contracts stratejisi seç** (Neden: “Application projesi başka module Application’a referans vermez” hedefi.)
    - Öneri: `src/Shared/HealthVerse.Contracts/` gibi yeni proje (veya SharedKernel altında alt proje).
    - [ ] Net kural yaz (HEXAGONAL_CONTRACT’a da ekle):
      - `HealthVerse.Contracts` **sadece** “DTO/Event/Interface” içerir (iş kuralı içermez)
      - `HealthVerse.Contracts` hiçbir `Modules.*` assembly’sine referans vermez
      - Modüller arası paylaşım “Contracts üzerinden” yapılır
  - [ ] **6.1 `HealthVerse.Contracts` projesini oluştur** (Neden: tek konuşma dili ve compile-time coupling’i azaltma aracı.)
    - Konum: `src/Shared/HealthVerse.Contracts/HealthVerse.Contracts.csproj`
    - Target: `net10.0`
    - Referanslar:
      - (Opsiyonel) `HealthVerse.SharedKernel`
      - Başka hiçbir modül referansı yok
    - [ ] Solution’a ekle: `dotnet sln src/HealthVerse.sln add src/Shared/HealthVerse.Contracts/HealthVerse.Contracts.csproj`
  - [ ] **6.2 İlk “dikey slice” seç: 1 coupling’i kaldır** (Neden: pattern’i oturtup sonra hızlanmak.)
    - Önerilen başlangıç (mevcut duruma göre):
      - `Identity.Application` → `Notifications.Application` referansını kaldıracak slice
      - veya `Competition.Application` → `Gamification.Application` referansını kaldıracak slice
    - Dikey slice şablonu:
      - [ ] Referansı kullanan types’ları bul (`rg "HealthVerse\\.<OtherModule>\\.Application"`)
      - [ ] Bu types’lardan “kontrat” olanları `HealthVerse.Contracts` içine taşı (DTO/event/interface)
      - [ ] Referansı kaldır, yerine Contracts referansı ekle
      - [ ] DI wiring gerekiyorsa: caller module Application’da port/interface tanımla; callee module Infrastructure’da adapter implemente et
  - [ ] **6.3 Modüller arası çağrıyı “event-first” düşün** (Neden: sıkı coupling’i azaltmanın en sağlam yolu genelde event’lerdir.)
    - Kural:
      - “A modülü B modülünün use-case’ini direkt çağırıyor” → önce “A bir event yayınlayabilir mi?” diye sor
      - “B kendi içinde bu event’i handle eder” yaklaşımı çoğu kez daha hexagonal
  - [ ] **Events/DTO contracts’ı taşı (1 event ile başla)** (Neden: incremental ilerleme)
    - Örn. `UserPointsEarnedEvent` gibi cross-module event’leri Contracts’a taşı.
  - [ ] **Architecture tests ile enforce et** (Neden: drift’i engellemek için)
    - Kural: `Modules.*.Application` yalnızca kendi Domain + SharedKernel(+Contracts) bağımlı olabilir.
    - [ ] Mevcut testleri genişlet:
      - `tests/HealthVerse.ArchitectureTests/LayerDependencyTests.cs` (Application->Infrastructure zaten var)
      - `tests/HealthVerse.ArchitectureTests/ModuleIsolationTests.cs` (Domain isolation var)
    - [ ] Yeni kural ekle (öneri):
      - “Application assemblies should not depend on other module Application assemblies”  
        (Allowed: `HealthVerse.Contracts`, `HealthVerse.SharedKernel`)
- **Beklenen çıktı**:
  - Cross-module bağımlılıklar azalır.
  - Contracts projesi tek “paylaşılan konuşma dili”.
- **Doğrulama/kalite kontrol**:
  - `dotnet build` yeşil.
  - Architecture tests “module isolation” kuralları yeşil.
  - `dotnet list <csproj> reference` çıktılarında “başka modül Application referansı” kalmamalı.
- **Yaygın hatalar / dikkat**:
  - Contracts içine “iş kuralı” kaçırmak → contracts sadece veri/olay sözleşmesi olmalı.
  - Contracts büyüyüp “God module” olması → isimlendirme + ownership kuralı koy.

---

## Faz 7 — Süreklilik: CI, Kalite Kapıları, Mimari Drift Önleme

- **Amaç**: Hexagonal mimariyi “korunan” hale getirmek; yeni özellik eklerken mimari bozulmasın.
- **Kapsam**:
  - **Dahil**: CI pipeline, zorunlu kalite kapıları, otomasyon.
  - **Hariç**: Prod deploy (istersen ayrı roadmap).
- **Ön koşullar / bağımlılıklar**:
  - En az Faz 1-3 bitmiş olmalı (aksi halde kurallar çok kırılır).
- **Adım adım yapılacaklar**:
  - [ ] **7.0 CI hedefini ikiye böl: hızlı gate + ağır gate** (Neden: PR hızını korurken kaliteyi düşürmemek.)
    - **Fast gate (her PR)**:
      - `dotnet build src/HealthVerse.sln -c Release`
      - `dotnet test src/HealthVerse.sln -c Release` (unit + architecture dahil)
    - **Heavy gate (her PR veya nightly)**:
      - `dotnet test tests/HealthVerse.IntegrationTests/HealthVerse.IntegrationTests.csproj -c Release`
      - Docker/Testcontainers şart (Q7)
  - [ ] **7.1 CI sağlayıcısına göre pipeline dosyasını koy** (Neden: repo içinde “tek doğru CI tanımı” olmalı.)
    - GitHub Actions kullanıyorsan: `.github/workflows/ci.yml`
    - GitLab kullanıyorsan: `.gitlab-ci.yml`
    - Azure DevOps kullanıyorsan: `azure-pipelines.yml`
    - Not: Bu dosyalar repo’da yoksa sen ekleyeceksin (provider’a göre).
  - [ ] **7.2 Docker/Testcontainers doğrulamasını CI’da görünür yap** (Neden: “Docker yokmuş” sürprizi yaşamamak.)
    - CI step:
      - `docker version`
      - (Opsiyonel) `docker info`
  - [ ] **7.3 Architecture tests’i hard gate yap** (Neden: mimari drift’i otomatik durdurur.)
    - `dotnet test tests/HealthVerse.ArchitectureTests/HealthVerse.ArchitectureTests.csproj -c Release`
  - [ ] **7.4 Kod kalite kapıları (opsiyonel ama önerilir)** (Neden: refactor sürecinde standardı korumak.)
    - [ ] `dotnet format --verify-no-changes` (uygunsa)
    - [ ] Analyzer/nullable warning seviyesi hedefi belirle (örn. warning-as-error kademeli)
  - [ ] **7.5 ADR disiplinini CI ile bağla** (Neden: büyük kararlar “konuşulup unutulmasın”.)
    - Kural:
      - Auth/Migrations/Notification policy gibi değişikliklerde ilgili ADR güncellenmeden PR merge edilmez (code review check)
  - [ ] **7.6 CI’da artefact/log saklama** (Neden: fail olduğunda debug hızlı olsun.)
    - Test raporları, failing logs, coverage (istersen) artefact olarak saklanır
- **Beklenen çıktı**:
  - PR’lar otomatik kalite kapılarından geçmeden merge olamaz.
  - Mimari kurallar süreklilik kazanır.
- **Doğrulama/kalite kontrol**:
  - CI yeşil, ana branch her zaman deploy edilebilir.
- **Yaygın hatalar / dikkat**:
  - Kuralları çok agresif yapıp “dev hızını” öldürmek → allowlist ve fazlı sertleştirme uygula.

---

## Ek: “Tek oturum” çalışma önerisi (pratik)
Her çalışma oturumunu şu şablonda tut:

- [ ] **1 küçük hedef** (örn. “LeagueController current user refactor”)
- [ ] **1-3 dosya** değişsin
- [ ] **1 yeni test/kurala** bağlansın
- [ ] **dotnet test** yeşil kalsın

Neden: Hexagonal refactor maraton değil; **küçük, güvenli, ölçülebilir** adımlar.

---

## Ek: Yeni cevap beklediğim sorular (şimdilik yok)
Q1–Q8 kararları net. Faz 3.1’de DB snapshot çıktıları alındıktan sonra, “3.2 Senaryo B1/B2/B3” seçimi fiilen kesinleşecek.