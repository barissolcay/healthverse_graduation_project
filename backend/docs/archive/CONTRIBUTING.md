# Contributing to HealthVerse

Bu belge, HealthVerse projesine katkıda bulunurken uyulması gereken kuralları ve süreçleri tanımlar.

## 🏗️ Hexagonal Architecture Kuralları

### Katman Bağımlılıkları (Zorunlu)

```
Domain ← Application ← Infrastructure
                    ← API (Composition Root)
```

| Katman | İzin Verilen Bağımlılıklar | Yasak Bağımlılıklar |
|--------|---------------------------|---------------------|
| Domain | Yok (saf C#) | EF Core, HTTP, Quartz, Framework |
| Application | Domain, SharedKernel, Contracts | Infrastructure, API |
| Infrastructure | Application, Domain, SharedKernel | API |
| API | Hepsi (Composition Root) | - |

### Modül İzolasyonu

- `Modules.*.Application` → başka modülün Application'ına referans veremez
- Cross-module iletişim **HealthVerse.Contracts** üzerinden yapılır
- Her modül kendi Domain'ine sahiptir ve paylaşmaz

### Controller Kuralları

- Controller'lar **thin** olmalı
- Sadece MediatR `Send()` / `Publish()` çağrısı yapmalı
- DbContext veya EF Core kullanmamalı
- Business logic içermemeli

### Job Kuralları

- Job'lar **orchestrator** only
- Business logic Application use-case'lerinde olmalı
- MediatR üzerinden command/query çağırmalı

## 📝 ADR (Architecture Decision Record) Disiplini

### ADR Gereken Durumlar

Aşağıdaki değişiklikler için **mutlaka ADR yazılmalı veya güncellenmelidir**:

1. **Auth/Authorization** değişiklikleri
2. **Migration stratejisi** değişiklikleri
3. **Yeni external adapter** ekleme (Firebase, Quartz, vb.)
4. **Cross-module contract** değişiklikleri
5. **Notification policy** değişiklikleri
6. **Public endpoint** ekleme/kaldırma

### ADR Formatı

Dosya: `docs/architecture/adr/XXXX-kisa-baslik.md`

```markdown
# XXXX - Kısa Başlık

## Durum
[Proposed | Accepted | Deprecated | Superseded]

## Bağlam
Neden bu karara ihtiyaç duyuldu?

## Karar
Ne yapılacak?

## Sonuçlar
Olumlu ve olumsuz etkiler neler?
```

### Mevcut ADR'ler

| ADR | Konu | Durum |
|-----|------|-------|
| 0001 | Auth + Identity (Guid UserId) | ✅ Accepted |
| 0002 | Migrations Strategy (Infrastructure) | ✅ Accepted |
| 0003 | Notification Delivery Policy | ✅ Accepted |

## 🔄 PR Süreci

### PR Açmadan Önce

1. **Branch aç**: `feature/xxx`, `fix/xxx`, `refactor/xxx`
2. **Lokal testleri çalıştır**:
   ```bash
   dotnet build src/HealthVerse.sln -c Release
   dotnet test tests/HealthVerse.UnitTests
   dotnet test tests/HealthVerse.ArchitectureTests
   ```
3. **ADR gerekiyor mu?** Yukarıdaki listeyi kontrol et

### CI Kalite Kapıları

| Gate | Zorunlu? | Açıklama |
|------|----------|----------|
| Fast Gate | ✅ Evet | Build + Unit Tests + Architecture Tests |
| Heavy Gate | ✅ Evet | Integration Tests (Docker/Postgres) |
| Code Quality | ⚠️ Uyarı | Format check + Analyzer warnings |

### Merge Kuralları

- ✅ Fast Gate ve Heavy Gate **mutlaka yeşil** olmalı
- ✅ En az 1 code review approval
- ✅ Architecture-critical dosyalarda CODEOWNERS review
- ⚠️ Migration içeren PR'lar ekstra dikkat gerektirir

## 🧪 Test Yazım Kuralları

### Unit Tests

- `tests/HealthVerse.UnitTests/`
- Modül bazlı klasörleme: `Competition/`, `Gamification/`, vb.
- Mock'lar için NSubstitute
- Business logic'i test et, integration değil

### Architecture Tests

- `tests/HealthVerse.ArchitectureTests/`
- NetArchTest.Rules kullanılır
- Her hexagonal kural için test olmalı
- Yeni kural eklendiğinde test de eklenmeli

### Integration Tests

- `tests/HealthVerse.IntegrationTests/`
- Testcontainers + PostgreSQL
- Gerçek DB ile API endpoint testi
- Test başına izole DB state (Respawn)

## 📁 Dosya Organizasyonu

```
src/
├── Api/HealthVerse.Api/          # Composition Root + HTTP Adapter
├── Infrastructure/               # External Adapters (EF, Firebase, Quartz)
├── Modules/
│   ├── Competition/
│   │   ├── *.Domain/             # Pure business rules
│   │   ├── *.Application/        # Use cases + Ports
│   │   └── *.Infrastructure/     # Adapters
│   └── ...
└── Shared/
    ├── HealthVerse.Contracts/    # Cross-module DTOs/Events/Interfaces
    └── HealthVerse.SharedKernel/ # Common base classes

docs/architecture/
├── adr/                          # Architecture Decision Records
├── phase-reports/                # Hexagonal refactoring phase reports
├── HEXAGONAL_CONTRACT.md         # Architecture rules reference
└── DEPENDENCY_MAP.md             # Project dependency visualization
```

## ❓ Sorular?

Mimari kararlar veya katkı süreci hakkında sorularınız için:
- HEXAGONAL_ROADMAP.md'yi inceleyin
- HEXAGONAL_CONTRACT.md'deki kuralları kontrol edin
- Mevcut ADR'leri okuyun
