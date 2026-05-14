# PhotoManager.Persistence

The persistence layer for PhotoManager backed by **SQLite**
(`Microsoft.Data.Sqlite` + `SQLitePCLRaw.bundle_e_sqlite3`). It provides all
data storage: asset cataloging, thumbnail caching, folder tracking, sync
definitions, and automated backups.

---

## 1. Why SQLite?

We migrated away from the custom CSV+blob format because it was not designed
for multi-threaded ingestion of thousands of images:

| Option                | OS-independent | Multi-thread | ACID  | File-based | Reason rejected            |
|-----------------------|----------------|--------------|-------|------------|----------------------------|
| Legacy CSV+blob       | ✅              | ❌            | ❌     | ✅          | What we are replacing.     |
| LiteDB (BSON)         | ✅              | ⚠ writer-lock| ✅     | ✅          | Writer-only locking, slower bulk inserts, smaller community. |
| LMDB (Lightning DB)   | ✅              | ✅            | ✅     | ✅          | Single-writer, fixed mmap size, low-level API. |
| **SQLite (chosen)**   | **✅**          | **✅ WAL**   | **✅** | **✅**      | Battle-tested, WAL gives concurrent reads + 1 writer, native online backup, rich ecosystem. |
| RocksDB               | ✅              | ✅            | ✅     | ✅ (dir)    | Native deps not first-class on .NET, KV-only (no relational queries). |
| Server DB (Pg/MySQL)  | ✅              | ✅            | ✅     | ❌          | Requires external server — unacceptable for an .exe-distributed desktop app. |

`Microsoft.Data.Sqlite` ships native binaries via `SQLitePCLRaw.bundle_e_sqlite3`
which is auto-deployed alongside the published executable on Windows / Linux /
macOS — supporting our future cross-platform plan with no extra work.

---

## 2. Project layout

```
PhotoManager.Persistence/
├── IPersistenceContext.cs            # high-level facade (replaces IDatabase)
├── PersistenceDiagnostics.cs         # last-operation diagnostics
├── PersistenceServiceCollectionExtensions.cs   # AddPersistence()
├── Sqlite/
│   ├── SqliteConnectionFactory.cs    # PRAGMAs on every open
│   ├── SqliteSchema.cs               # DDL + PRAGMA user_version
│   ├── SqliteBackupService.cs        # online backup + zip
│   └── SqlitePersistenceContext.cs   # IPersistenceContext impl
├── Repositories/                     # primitive table accessors
│   ├── IFolderPersistence.cs / FolderPersistence.cs
│   ├── IAssetPersistence.cs  / AssetPersistence.cs
│   ├── IThumbnailPersistence.cs / ThumbnailPersistence.cs
│   ├── IRecentPathsPersistence.cs / RecentPathsPersistence.cs
│   └── ISyncDefinitionsPersistence.cs / SyncDefinitionsPersistence.cs
├── Cache/
│   ├── ILruCache.cs
│   └── LruCache.cs                   # thread-safe LRU
└── GlobalUsings.cs
```

All concrete classes inside `Sqlite/` and `Repositories/` are `internal sealed`
— consumers depend only on `IPersistenceContext` and the per-table interfaces.

---

## 3. Schema (v1)

`SqliteSchema.cs` creates these tables and indexes on first open and stamps
`PRAGMA user_version = 1`:

| Table              | Primary key                  | Notes                                    |
|--------------------|------------------------------|------------------------------------------|
| `Folders`          | `Id` (TEXT, GUID)            | Index on `Path`. Duplicate paths allowed (legacy quirk). |
| `Assets`           | (`FolderId`, `FileName`)     | Index on `Hash`. FK on `FolderId` → `Folders(Id)` ON DELETE CASCADE. |
| `Thumbnails`       | (`FolderId`, `FileName`)     | `Data BLOB`. FK on `FolderId` ON DELETE CASCADE. |
| `RecentPaths`      | `Position` (INTEGER)         | Single ordered list.                     |
| `SyncDefinitions`  | `Position` (INTEGER)         | Single ordered list.                     |

The schema is **logically identical** to the legacy one (same fields, same
semantics) so the migration is mechanical when the time comes.

---

## 4. Connection model

We use **one connection per operation, never shared across threads**.
`SqliteConnectionFactory.Open()` applies the following pragmas on every open
because they are connection-scoped, not database-scoped:

```sql
PRAGMA journal_mode  = WAL;        -- concurrent readers + 1 writer
PRAGMA synchronous   = NORMAL;     -- crash-safe with WAL, fast
PRAGMA busy_timeout  = 5000;       -- 5s busy retry (multi-thread ingestion)
PRAGMA temp_store    = MEMORY;
PRAGMA foreign_keys  = ON;
```

Bulk operations (`AssetPersistence.UpsertMany`,
`ThumbnailPersistence.ReplaceForFolder`, `RecentPathsPersistence.Replace`,
`SyncDefinitionsPersistence.Replace`) wrap their work in a `SqliteTransaction`
with pre-bound parameters re-used across rows to avoid allocations.

---

## 5. Backup format

The new format is **filename-compatible** (`yyyyMMdd.zip`) but **not
content-compatible** with the legacy format. The archive contains a single
entry, `photomanager.db`, produced via the SQLite online-backup API
(`SqliteConnection.BackupDatabase`) which yields a consistent snapshot even
under concurrent writes.

Why not preserve the legacy `Tables/` + `Blobs/` structure? Because:

- The new engine is one file, splitting it into a 7-table CSV facade would
  require keeping a CSV writer alive — defeating the migration.
- Backups are consumed only by the application, so format compatibility is not
  a user-facing concern.
- The `yyyyMMdd.zip` filename pattern is preserved so backup retention rules
  and external scripts (e.g. cleanup, archival) continue to work unchanged.

A future "import legacy backup" tool is documented as out-of-scope for this
PR — not blocking for the migration since both engines coexist.

---

## 6. Threading & observers

`AssetRepository` uses a reentrant `Lock` (`Monitor`) for in-memory
state (indexes, cache, change flag). We **deliberately avoided
`ReaderWriterLockSlim`** because subscribers of `AssetsUpdated` may call back
into the repository synchronously, and `ReaderWriterLockSlim` is not reentrant
across read↔write modes — that is a well-documented deadlock trap.

To prevent reentrancy with subscribers, every mutation captures whether it
needs to fire `OnNext(Unit.Default)` and **fires it after releasing the lock**.

The persistence layer itself is stateless from a threading perspective: every
call opens its own connection and closes it. SQLite WAL handles the
read/write concurrency.

---

## 7. AssetRepository — what's new

`PhotoManager.Infrastructure.AssetRepository` is a drop-in
`IAssetRepository`. Public contract is **byte-compatible** with the legacy
repository (including legacy quirks: duplicate folders allowed,
`LoadThumbnail` repair-on-miss). The internals are different:

- `Dictionary<string, Folder>` indexed by **path** (O(1) `GetFolderByPath`).
- `Dictionary<Guid, Folder>` indexed by **id** (O(1) parent/child lookups).
- `Dictionary<Guid, Dictionary<string, Asset>>` (folderId → fileName → asset)
  for O(1) `IsAssetCatalogued` / `DeleteAsset`.
- `Dictionary<string, List<Asset>>` indexed by **hash** — new capability.
- `LruCache<string, Dictionary<string, byte[]>>` for thumbnail dictionaries,
  bounded by `ThumbnailsDictionaryEntriesToKeep` (matches legacy semantic).
- Span-based path utilities (no allocations on hot paths).
- `SaveCatalog` flushes via transactional `UpsertMany`/`Replace` per table.

What we deliberately did **not** do:

- Inline arrays / unsafe code — measured to be a non-bottleneck for this
  workload, and would complicate the codebase.
- A frozen-collection rebuild on every change — the cost dwarfs the benefit
  given write frequency.

---

## 8. DI wiring

```csharp
// PhotoManager.Infrastructure/InfrastructureServiceCollectionExtensions.cs
services.AddPersistence();  // registers SqliteConnectionFactory, SqliteBackupService, SqlitePersistenceContext
services.AddSingleton<IAssetRepository, AssetRepository>();
```

In `App.xaml.cs`, the call chain remains
`AddInfrastructure().AddDomain().AddApplication().AddUi()` — `AddPersistence()`
is wired transitively through `AddInfrastructure`.

The `SqlitePersistenceContext` calls `Initialize(dataDirectory)` lazily on
first repository read; data goes to
`<dataDirectory>/photomanager.db` and backups to `<dataDirectory>_Backups/`.

---

## 9. Tests

Tests live under two directories:

**Integration tests** (`PhotoManager.Tests/Integration/Persistence/`):

- `Cache/LruCacheTests.cs` — full LRU semantics with real file data.
- `SqlitePersistenceContextTests.cs` — initialise / backup write /
  backup retention / schema version / overwrite assertions.
- `SqliteConnectionFactoryTests.cs` — database path, initialize, open
  (PRAGMA verification, multiple connections, error paths).
- `SqliteBackupServiceTests.cs` — WriteBackup, GetBackupFilesPaths,
  DeleteBackupFile with real SQLite databases.
- `Repositories/AssetPersistenceTests.cs` — CRUD on Assets table with
  real asset data from Constants.
- `Repositories/ThumbnailPersistenceTests.cs` — CRUD on Thumbnails
  with real image bytes.

**Unit tests** (`PhotoManager.Tests/Unit/Persistence/Sqlite/`):

- `SqlitePersistenceContextTests.cs` — exception catch block paths
  using NSubstitute mocks.
- `SqliteBackupServiceTests.cs` — pre-existing snapshot cleanup,
  IOException retry logic with file locking.

Run them with:

```bash
dotnet test --filter "FullyQualifiedName~Persistence" PhotoManager/PhotoManager.slnx
```
