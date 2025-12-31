# ADR 0002: Migrations Location Strategy

## Status
Accepted

## Context
The project currently has a split migration history:
- `src/Api/HealthVerse.Api/Migrations`: Contains the initial chain (13 migrations).
- `src/Infrastructure/HealthVerse.Infrastructure/Migrations`: Contains a recent migration (`AddNotificationDeliveries`).

This split causes confusion about the "source of truth", complicates `dotnet ef` commands, and risks migration drift between environments.
Hexagonal Architecture dictates that persistence details (including schema management) belong to the **Infrastructure** layer (Secondary Adapter).

## Decision
We will consolidate all migrations into the **Infrastructure** project.

1.  **Source of Truth**: `src/Infrastructure/HealthVerse.Infrastructure` will contain all migration files.
2.  **Runtime Assembly**: The API (Host) will be configured to load migrations from the `HealthVerse.Infrastructure` assembly.
3.  **Command Standardization**: All `dotnet ef` commands will target the `Infrastructure` project for migrations.

## Consequences
### Positive
- **Single Source of Truth**: No ambiguity about where schema definitions live.
- **Layer Compliance**: API layer becomes purely a "Gateway/Host", decoupling it from DB mechanics.
- **CI/CD Simplification**: Pipeline only needs to look at one folder for schema changes.

### Negative
- **Migration Move Effort**: Existing migrations must be carefully moved to preserve `MigrationId`s (crucial for valid `__EFMigrationsHistory` in existing DBs).
- **Refactoring**: Namespaces of moved migrations might need adjustment (or kept as is to avoid breaking history). *Decision: Keep original namespaces if possible, or bulk update if `__EFMigrationsHistory` allows.* (Ideally, namespace doesn't matter for EF Core identity, only Class Name/ID matters, but we'll be careful).

## Compliance
- `dotnet ef migrations add` MUST be run along with specific arguments to target Infrastructure.
- `HealthVerseDbContext` MUST be configured with `b.MigrationsAssembly("HealthVerse.Infrastructure")`.
