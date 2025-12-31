# Hexagonal Architecture Contract

Bu doküman, HealthVerse projesinin mimari sınırlarını, katman kurallarını ve izin verilen istisnaları tanımlar.

## Temel Prensipler

1. **Domain Merkezciliği**: İş kuralları dış dünyadan (DB, Web, MQ) bağımsızdır. `Domain` katmanı hiçbir dış projeye referans vermez.
2. **Tek Yönlü Bağımlılık**: Bağımlılıklar dıştan içe doğrudur.
   - `Infrastructure` -> `Application` -> `Domain`
   - `Api` -> `Application` -> `Domain`
3. **Port & Adapter**: Application katmanı "neye ihtiyacı olduğunu" Interface (Port) ile tanımlar; Infrastructure katmanı bunu implemente eder (Adapter).

## Katman Kuralları

### 1. Domain (`Modules.*.Domain`)
- **İçerik**: Entities, Value Objects, Domain Events, Domain Exceptions, Repository Interfaces (opsiyonel, genelde Application'da tutulur ama Domain'de de olabilir).
- **Bağımlılıklar**:
  - ✅ `HealthVerse.SharedKernel` (Base Entity vb. için)
  - ❌ `Application`, `Infrastructure`, `Api`
  - ❌ External Frameworks (EF Core, AspNetCore vb.) - *Saf C#*

### 2. Application (`Modules.*.Application`)
- **İçerik**: Use Cases (MediatR Command/Query Handlers), Port Interfaces (`IRepository`, `IExternalService`), DTOs, Mappers.
- **Bağımlılıklar**:
  - ✅ `Domain` (Kendi modülü)
  - ✅ `SharedKernel`
  - ❌ `Infrastructure` (Kesinlikle yasak - Compile time referansı olmamalı)
  - ❌ `Api`

### 3. Infrastructure (`Modules.*.Infrastructure`)
- **İçerik**: EF Core DbContext, Repositories, External Service Implementations (Firebase, FCM), Quartz Jobs (Orchestrator).
- **Bağımlılıklar**:
  - ✅ `Application` (Interface'leri implemente etmek için)
  - ✅ `Domain` (Entity'leri persist etmek için)
  - ❌ `Api`

### 4. API (`HealthVerse.Api`)
- **İçerik**: Controllers, Middlewares, Program.cs (Composition Root).
- **Bağımlılıklar**:
  - ✅ `Application` (Command göndermek için)
  - ✅ `Infrastructure` (DI wiring için)

## Modüller Arası İletişim Kuralları (Phase 6 - ✅ Tamamlandı)

- **Doğrudan Çağrı**: Modüller birbirinin `Application` katmanını **doğrudan çağırmamalıdır**.
- **Domain Erişimi**: Modüller birbirinin `Domain` entity'lerine **erişmemelidir**.
- **İletişim Yöntemi**:
  - **Contracts**: Paylaşılan DTO ve Event'ler `HealthVerse.Contracts` içinde tanımlanır.
  - **Events**: Modüller arası asenkron iletişim (MediatR Notifications) tercih edilir.

### HealthVerse.Contracts Kuralları

- **Konum**: `src/Shared/HealthVerse.Contracts/`
- **İçerik**: Sadece DTO, Event ve Interface (sözleşme) içerir
- **Yasak**: İş mantığı (business logic) kesinlikle içermez
- **Referanslar**: Hiçbir `Modules.*` projesine referans vermez
- **Kullanım**: Modüller arası paylaşım bu proje üzerinden yapılır

### Contracts'taki Sözleşmeler

| Sözleşme | Tip | Açıklama |
|----------|-----|----------|
| `INotificationService` | Interface | Bildirim oluşturma kontratı |
| `NotificationCreateRequest` | DTO | Toplu bildirim oluşturma isteği |
| `NotificationType` | Constants | Bildirim tipleri sabitleri |
| `UserPointsEarnedEvent` | Event | Puan kazanma event'i |

## İstisnalar (Allowlist)

Aşağıdaki durumlar, pratik sebeplerle mimari kuralların **bilinçli olarak** esnetildiği yerlerdir:

### 1. StatusController (API)
- **Kural İhlali**: Controller içinde doğrudan `DbContext` kullanımı.
- **Gerekçe**: Health check ve diagnostics için basit, doğrudan DB erişimi gerekliliği. Application katmanını kirletmemek için API'de tutulabilir.
- **Kısıt**: Sadece `/status` endpoint'leri için geçerlidir.

### 2. `EfCore` Bağımlılığı (Domain?)
- **Kural**: Domain katmanında EF Core attribute'ları (`[Key]`, `[Required]` vb.) bulunmamalıdır. Entity configuration `Infrastructure` katmanında (`IEntityTypeConfiguration`) yapılmalıdır.
- **Mevcut Durum**: Eğer varsa temizlenmelidir.

### 3. SharedKernel
- Tüm modüller `SharedKernel`'a erişebilir. Ancak `SharedKernel` içine iş kuralı (business logic) konulmamalı, sadece Building Block'lar (AggregateRoot, Entity base, Result pattern) ve Utility'ler bulunmalıdır.

---

## İlerleme Durumu

| Faz | Kapsam | Durum |
|-----|--------|-------|
| 0 | Guardrails + Architecture Tests | ✅ Tamamlandı |
| 1 | Auth + CurrentUser Boundary | ✅ Tamamlandı |
| 2 | API Thinning | ✅ Tamamlandı |
| 3 | Migrations Consolidation | ✅ Tamamlandı |
| 3.5 | Integration Tests (Testcontainers) | ✅ Tamamlandı |
| 4 | Notification Policy + Preferences | ✅ Tamamlandı |
| 5 | Quartz Jobs → Orchestrator Only | ✅ Tamamlandı |
| 6 | Modül İzolasyonu + Contracts | ✅ Tamamlandı |
| 7 | CI + Kalite Kapıları | ✅ Tamamlandı |

**Son Güncelleme**: 2025-12-31 (WeeklyLeagueFinalizeJob bug fix, dokümantasyon senkronizasyonu)

---

## Mevcut Modül Referans Durumu

| Modül | Durum | Notlar |
|-------|-------|--------|
| Identity | ✅ İzole | Sadece kendi Domain + Contracts |
| Social | ✅ İzole | Sadece kendi Domain + Contracts |
| Tasks | ✅ İzole | Sadece kendi Domain |
| Missions | ✅ İzole | Sadece kendi Domain + Contracts + SharedKernel |
| Notifications | ✅ İzole | Sadece kendi Domain |
| Competition | ⚠️ Kabul Edilebilir | Identity.Domain (IUserRepository port için), Gamification.Domain |
| Gamification | ⚠️ Kabul Edilebilir | Identity.Domain (IUserRepository port için) |

**Not:** Competition ve Gamification'ın Identity.Domain'e referansı, hexagonal mimari açısından kabul edilebilir bir trade-off'tur. `IUserRepository` port interface'i Domain katmanında tanımlı olduğundan bu referans gereklidir.

---

## CI/CD Kalite Kapıları (Phase 7)

### Pipeline Yapısı

| Gate | Zorunlu | İçerik |
|------|---------|--------|
| Fast Gate | ✅ | Build + Unit Tests + Architecture Tests |
| Heavy Gate | ✅ | Integration Tests (Docker + Postgres) |
| Code Quality | ⚠️ Warning | Format check + Analyzers |

### Dosyalar

| Dosya | Açıklama |
|-------|----------|
| `.github/workflows/ci.yml` | GitHub Actions CI pipeline |
| `.github/CODEOWNERS` | Code review ownership rules |
| `.github/PULL_REQUEST_TEMPLATE.md` | PR checklist template |
| `.editorconfig` | Code style configuration |
| `CONTRIBUTING.md` | Contribution guidelines |

### Kalite Kuralları

1. **PR merge için zorunlu**:
   - Fast Gate (build + unit + arch tests) yeşil
   - Heavy Gate (integration tests) yeşil
   - CODEOWNERS approval

2. **ADR gereken durumlar**:
   - Auth/Authorization değişiklikleri
   - Migration stratejisi değişiklikleri
   - Yeni external adapter ekleme
   - Cross-module contract değişiklikleri

---

## Test Metrikleri

| Metrik | Değer |
|--------|-------|
| Unit Tests | 299 |
| Integration Tests | 29 |
| Architecture Tests | 48 |
| **Toplam** | **376** |
| Build Warnings | 0 |
| Build Errors | 0 |
