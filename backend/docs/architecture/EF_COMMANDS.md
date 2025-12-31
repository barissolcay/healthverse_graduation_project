# Standard EF Core Commands

Bu proje Hexagonal Mimari gereği **Infrastructure** projesini persistence merkezi olarak kullanır.

## Temel Kural
Tüm EF Core komutları şu parametreleri içermelidir:
- **Project**: `src/Infrastructure/HealthVerse.Infrastructure` (Migration oluşturma yeri)
- **Startup Project**: `src/Api/HealthVerse.Api` (Konfigürasyon kaynağı)
- **Context**: `HealthVerseDbContext`

## 1. Migration Ekleme (Yeni Şema Değişikliği)
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure/HealthVerse.Infrastructure \
  --startup-project src/Api/HealthVerse.Api \
  --context HealthVerseDbContext
```

## 2. Veritabanını Güncelleme (Local/Dev)
```bash
dotnet ef database update \
  --project src/Infrastructure/HealthVerse.Infrastructure \
  --startup-project src/Api/HealthVerse.Api \
  --context HealthVerseDbContext
```

## 3. Migration Listeleme
```bash
dotnet ef migrations list \
  --project src/Infrastructure/HealthVerse.Infrastructure \
  --startup-project src/Api/HealthVerse.Api \
  --context HealthVerseDbContext
```

## 4. SQL Script Oluşturma (Prod/Staging için)
```bash
dotnet ef migrations script \
  --project src/Infrastructure/HealthVerse.Infrastructure \
  --startup-project src/Api/HealthVerse.Api \
  --context HealthVerseDbContext \
  --output docs/architecture/db/migration_script.sql
```
