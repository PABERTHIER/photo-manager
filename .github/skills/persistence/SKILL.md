---
name: persistence
description: >
  SQLite persistence layer reference for PhotoManager.
  Use this skill when working on database code, repositories, backup service, or schema changes.
---

You are working on the PhotoManager persistence layer.

Use the facts below as your authoritative reference.

---

## Project Layout

```
PhotoManager.Persistence/
├── IPersistenceContext.cs            # High-level facade (Initialize, WriteBackup, DeleteOldBackups, Dispose)
├── PersistenceDiagnostics.cs         # Last-operation diagnostics
├── PersistenceServiceCollectionExtensions.cs   # AddPersistence()
├── Sqlite/
│   ├── ISqliteConnectionFactory.cs / SqliteConnectionFactory.cs  # PRAGMAs on every open
│   ├── SqliteSchema.cs              # DDL + PRAGMA user_version
│   ├── ISqliteBackupService.cs / SqliteBackupService.cs  # Online backup + zip
│   └── SqlitePersistenceContext.cs  # IPersistenceContext implementation
├── Repositories/
│   ├── IFolderPersistence.cs / FolderPersistence.cs
│   ├── IAssetPersistence.cs / AssetPersistence.cs
│   ├── IThumbnailPersistence.cs / ThumbnailPersistence.cs
│   ├── IRecentPathsPersistence.cs / RecentPathsPersistence.cs
│   └── ISyncDefinitionsPersistence.cs / SyncDefinitionsPersistence.cs
├── Cache/
│   ├── ILruCache.cs
│   └── LruCache.cs                  # Thread-safe LRU cache
└── GlobalUsings.cs
```

All concrete classes in `Sqlite/` and `Repositories/` are `internal sealed`.

---

## Schema (v1)

Defined in `SqliteSchema.cs`:

| Table              | Primary key              | Notes                                |
|--------------------|--------------------------|--------------------------------------|
| `Folders`          | `Id` (TEXT, GUID)        | Index on `Path`                      |
| `Assets`           | (`FolderId`, `FileName`) | Index on `Hash`. FK → `Folders(Id)`  |
| `Thumbnails`       | (`FolderId`, `FileName`) | `Data BLOB`. FK → `Folders(Id)`      |
| `RecentPaths`      | `Position` (INTEGER)     | Single ordered list                  |
| `SyncDefinitions`  | `Position` (INTEGER)     | Single ordered list                  |

---

## Connection Model

- One connection per operation, never shared across threads
- `SqliteConnectionFactory.Open()` applies PRAGMAs on every connection:
  ```sql
  PRAGMA journal_mode  = WAL;
  PRAGMA synchronous   = NORMAL;
  PRAGMA busy_timeout  = 5000;
  PRAGMA temp_store    = MEMORY;
  PRAGMA foreign_keys  = ON;
  ```
- Bulk operations use `SqliteTransaction` with pre-bound parameters for efficiency

---

## Backup Service

- `WriteBackup(backupFilePath)`: uses SQLite online-backup API → snapshot `.tmp.db` → zip → cleanup
- Backup filename format: `yyyyMMdd.zip` containing `photomanager.db`
- IOException retry: if snapshot file is locked, waits 20ms and retries deletion
- `GetBackupFilesPaths(backupDirectory)`: returns sorted `.zip` file paths
- `DeleteBackupFile(backupFilePath)`: deletes a single backup archive

---

## DI Registration

```csharp
// PersistenceServiceCollectionExtensions.cs
services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
services.AddSingleton<ISqliteBackupService, SqliteBackupService>();
services.AddSingleton<IPersistenceContext, SqlitePersistenceContext>();
```

Called from `InfrastructureServiceCollectionExtensions.AddInfrastructure()`.

---

## Testing Patterns

- Integration tests use real SQLite databases in temp directories
- Call `SqliteConnection.ClearAllPools()` + `TearDownHelper.DeleteTempDbDirectories()` in TearDown
- Unit tests for exception paths use NSubstitute mocks for `ISqliteConnectionFactory` / `ISqliteBackupService`
- Test data comes from `PhotoManager.Tests.Integration.Constants`
- Run: `dotnet test --filter "FullyQualifiedName~Persistence" PhotoManager/PhotoManager.slnx`

---

## Adding a New Table

1. Add DDL to `SqliteSchema.CREATE_SCRIPT` in `SqliteSchema.cs`
2. Bump `SCHEMA_VERSION` and add migration logic in `EnsureCreated()`
3. Create `I{Table}Persistence.cs` interface and `{Table}Persistence.cs` implementation in `Repositories/`
4. Register in `PersistenceServiceCollectionExtensions.cs`
5. Write integration tests in `PhotoManager.Tests/Integration/Persistence/Repositories/`
