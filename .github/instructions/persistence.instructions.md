---
applyTo: "**/PhotoManager.Persistence/**/*.cs"
---

# Persistence Layer Standards for PhotoManager

## Architecture

- All concrete classes in `Sqlite/` and `Repositories/` are `internal sealed`
- Consumers depend only on `IPersistenceContext` and per-table interfaces
- DI registration is in `PersistenceServiceCollectionExtensions.cs`
- Infrastructure calls `AddPersistence()` from `AddInfrastructure()`

## Database Schema

Schema is defined in `SqliteSchema.cs`. Tables: Folders, Assets, Thumbnails, RecentPaths, SyncDefinitions.
When adding tables:

1. Add DDL to `SqliteSchema.CREATE_SCRIPT`
2. Bump `SCHEMA_VERSION` and add migration logic
3. Create repository interface + implementation in `Repositories/`
4. Register in `PersistenceServiceCollectionExtensions`

## Connection Model

- One connection per operation, never shared across threads
- `SqliteConnectionFactory.Open()` applies PRAGMAs on every connection:
  - `journal_mode = WAL`, `synchronous = NORMAL`, `busy_timeout = 5000`
  - `temp_store = MEMORY`, `foreign_keys = ON`
- Bulk operations use `SqliteTransaction` with pre-bound parameters

## Backup Format

- Filename: `yyyyMMdd.zip` containing a single `photomanager.db` entry
- Created via SQLite online-backup API (`SqliteConnection.BackupDatabase`)
- Stored in `<dataDirectory>_Backups/`

## Testing the Persistence Layer

- Integration tests: `PhotoManager.Tests/Integration/Persistence/`
- Unit tests (for exception paths): `PhotoManager.Tests/Unit/Persistence/Sqlite/`
- Always call `SqliteConnection.ClearAllPools()` in TearDown
- Use `TearDownHelper.DeleteTempDbDirectories()` for cleanup
- Test data uses Constants from `PhotoManager.Tests.Integration.Constants`

## Key Patterns

- `IPersistenceContext.Initialize(dataDirectory)` must be called before any repository access
- Repository methods open their own connection via `ISqliteConnectionFactory`
- `LruCache<TKey, TValue>` is thread-safe and bounded by configurable capacity
- `SqliteBackupService.WriteBackup` handles IOException retry for locked snapshot files
