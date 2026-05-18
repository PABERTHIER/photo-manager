---
description: "Work with the SQLite persistence layer (schema, repositories, backup)"
---

# Persistence Layer Task

Perform the following task on the PhotoManager persistence layer: ${input:task:Describe what you need (e.g., add a column, create a repository, fix a backup issue)}

## Context

The persistence layer lives in `PhotoManager.Persistence/` and uses SQLite via `Microsoft.Data.Sqlite`.

Key files:
- Schema DDL: `PhotoManager.Persistence/Sqlite/SqliteSchema.cs`
- Context (Initialize/Backup/Dispose): `PhotoManager.Persistence/Sqlite/SqlitePersistenceContext.cs`
- Connection factory (PRAGMAs): `PhotoManager.Persistence/Sqlite/SqliteConnectionFactory.cs`
- Backup service: `PhotoManager.Persistence/Sqlite/SqliteBackupService.cs`
- Repositories: `PhotoManager.Persistence/Repositories/`
- DI: `PhotoManager.Persistence/PersistenceServiceCollectionExtensions.cs`

## Workflow

1. Read the relevant persistence code to understand the current state.
2. Make changes following existing patterns (internal sealed classes, interface + implementation).
3. Check `GlobalUsings.cs` before adding any `using` directives.
4. Build: `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings.
5. Test: `dotnet test --filter "FullyQualifiedName~Persistence" PhotoManager/PhotoManager.slnx`.
6. Format: `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`.
