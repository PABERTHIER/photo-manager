# AGENTS.md

This file provides shared guidance for AI coding agents (Claude Code, GitHub Copilot, and others) when working with code in this repository.

---

# UNIVERSAL RULES - Apply to Every Request

## Code Formatting (NON-NEGOTIABLE)

- **Max line length: 120 characters** (from `.editorconfig`)
- All code MUST follow `PhotoManager/.editorconfig` rules
- Indent: 4 spaces, End of line: CRLF, Charset: utf-8
- If a line exceeds 120 chars, wrap it appropriately (except unsplittable strings/URLs)
- When editing files, preserve existing formatting style

## File Encoding (NON-NEGOTIABLE)

- **All files MUST use UTF-8 encoding with BOM** (Byte Order Mark: `EF BB BF`)
- This is the .NET standard as specified in `.editorconfig` (`charset = utf-8`)
- When creating or editing files, ALWAYS ensure the BOM is present
- **Never strip the BOM** when modifying files — preserve the original encoding
- The BOM is critical for proper file encoding detection by .NET tooling and CI/CD pipelines
- If you edit a file and the BOM is lost, re-add it before completing the task

## Performance Optimization Workflow

When optimizing performance:

1. **Create benchmark file first** - `PhotoManager/Benchmarks/PhotoManager.Benchmarks/{Category}/{ClassName}{MethodName}Benchmarks.cs`
2. **Keep original method unchanged** in its source file
3. **Add all optimizations in the benchmark file only**
4. **Run benchmark** with `--filter` for that specific benchmark class
5. **Evaluate**: Speed is priority, but allocation shouldn't increase significantly
6. **Replace original** only if there's clear improvement
7. **Add comment**: `// ClassName.MethodName: X% faster`
8. **Keep all variants** in benchmark file for reference

Example: `PhotoManager/Benchmarks/PhotoManager.Benchmarks/Common/HashingHelperCalculateHashBenchmarks.cs`

## Zero Warnings Policy (NON-NEGOTIABLE)

- **This project contains 0 warnings - keep it that way**
- NEVER introduce compiler warnings when adding/updating code
- Treat warnings as errors is enabled in builds
- Run `dotnet build` to verify no warnings before completing a task

## Test Execution Strategy

- **NEVER run all tests** with `dotnet test PhotoManager/PhotoManager.slnx` without filter
- Run ONLY relevant tests based on what changed:
  - Single test: `dotnet test --filter "FullyQualifiedName~TestClassName"`
  - By namespace: `dotnet test --filter "FullyQualifiedName~HashingHelper"`
  - By project: `dotnet test --filter "FullyQualifiedName~PhotoManager.Common"`
- Always explain which tests are being run and why

## Test File Date Assertions — Stale Build Failure

Some integration tests assert that `asset.FileProperties.Creation.Date` equals `DateTime.Now.Date`. These tests will **fail with a date mismatch** if the test project was last built on a previous day:

```
Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(actualDate))
Expected: 2026-05-15 00:00:00
But was: 2026-05-14 00:00:00
```

**Root cause:** `PhotoManager.Tests.csproj` defines a custom MSBuild task `SetFileDates` (from `MSBuildTask\FileDateTask.dll`) that runs `AfterTargets="PreBuildEvent"` on every build.
It copies all test files under `TestFiles\` to the output directory, and their filesystem creation date is set to the time of that build.
When a day passes without rebuilding, the creation dates of the copied files become yesterday's date, causing assertions against `DateTime.Now.Date` to fail.

**Fix:** Run `dotnet build PhotoManager/PhotoManager.slnx` to trigger the `SetFixedDate` target and refresh the test files in the output directory. Then re-run the failing tests — they will pass.

**This is not a regression introduced by code changes.** If you see only date assertion failures and your code changes are unrelated to file date handling, rebuild first before investigating further.

## TODO Comments (NON-NEGOTIABLE)

- **Never remove a `// TODO` comment** unless the issue it describes has been fully fixed in the same change
- If you touch a file containing TODO comments, leave them untouched unless you are explicitly fixing them

## Before Reporting Completion

1. Verify no compiler warnings (run `dotnet build` and check output)
2. Verify no line exceeds 120 characters (use `dotnet format` to check)
3. Verify all modified files have UTF-8 BOM encoding (first 3 bytes should be `EF BB BF`)
4. Run only relevant tests for changes made
5. If code was modified, ensure tests pass
6. Verify all `// TODO` comments in modified files are still present (do not silently remove them)

---

## Project Overview

PhotoManager is a cross-platform desktop application (Windows, Linux, macOS) built with Avalonia UI for managing photo/video collections, detecting duplicates, and syncing assets across folders. It targets .NET 10.0 and uses a Clean Architecture approach with a SQLite database for local persistence.

## Build and Test Commands

**Build:**

```bash
dotnet build PhotoManager/PhotoManager.slnx
```

**Build Release:**

```bash
dotnet build --configuration Release PhotoManager/PhotoManager.slnx
```

**Restore dependencies:**

```bash
dotnet restore PhotoManager/PhotoManager.slnx
```

**Run tests:**

```bash
dotnet test PhotoManager/PhotoManager.slnx
```

**Run tests with coverage (local):**

```bash
pwsh PhotoManager/test-with-coverage.ps1
```

**Format/Verify code style:**

```bash
dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes
```

**Run single test:**

```bash
dotnet test --filter "FullyQualifiedName~TestClassName" PhotoManager/PhotoManager.slnx
```

**Run the UI application:**

```bash
dotnet run --project PhotoManager/PhotoManager.UI/PhotoManager.UI.csproj
```

**Run benchmarks:**

```bash
dotnet run --project PhotoManager/Benchmarks/PhotoManager.Benchmarks/PhotoManager.Benchmarks.csproj -c Release
```

**Run specific benchmark:**

```bash
dotnet run --project PhotoManager/Benchmarks/PhotoManager.Benchmarks/PhotoManager.Benchmarks.csproj -c Release -- --filter "BenchmarkClassName"
```

## Architecture

The solution follows Clean Architecture with the following layers:

```
PhotoManager.slnx
├── PhotoManager.Domain/        # Core domain logic, entities, service interfaces
├── PhotoManager.Application/   # Application orchestration (IApplication facade)
├── PhotoManager.Infrastructure/# External concerns (file system, image processing)
├── PhotoManager.Persistence/   # SQLite database, repositories, backup service
├── PhotoManager.Common/        # Shared utilities (hashing, image/video helpers)
├── PhotoManager.UI/            # Avalonia UI presentation layer (ViewModels, Views)
├── PhotoManager.Tests/         # NUnit tests with NSubstitute
└── PhotoManager.Benchmarks/    # BenchmarkDotNet performance benchmarks
```

### Dependency Flow

UI → Application → Domain ← Infrastructure ← Persistence

### Key Architectural Patterns

1. **SQLite Persistence**: The project uses SQLite (`Microsoft.Data.Sqlite` + `SQLitePCLRaw.bundle_e_sqlite3`) in the `PhotoManager.Persistence` project. The database is a single `photomanager.db` file with WAL mode for concurrent reads. Schema is defined in `SqliteSchema.cs` with tables: Folders, Assets, Thumbnails, RecentPaths, SyncDefinitions.

2. **Service Collection Extensions**: Each layer has a `{Layer}ServiceCollectionExtensions.cs` that registers its services. The UI layer (`App.axaml.cs`) chains them: `AddInfrastructure()` → `AddDomain()` → `AddApplication()` → `AddUi()`. `AddInfrastructure()` calls `AddPersistence()` internally.

3. **Hash-Based Duplicate Detection**: The `HashingHelper` supports multiple algorithms (PHash, DHash, MD5, SHA512). PHash is the most advanced and can detect duplicates between rotated images, thumbnails, and different resolutions. Controlled by `HashSettings` in `appsettings.json`.

4. **Microsoft Extensions Logging**: The application uses `ILogger<T>` throughout. The UI layer configures Serilog for file logging and console output in `App.axaml.cs`.

5. **Configuration**: All settings are in `appsettings.json` with strongly-typed settings classes in `PhotoManager.Domain/UserConfigurationSettings/` (AssetSettings, HashSettings, PathSettings, StorageSettings, ProjectSettings). The database directory is **not** configurable — it is always resolved at runtime as `AppContext.BaseDirectory + "Database"` by `PathProviderService`.

## Important Files

- `PhotoManager/PhotoManager.UI/App.axaml.cs` - Application startup, DI container setup
- `PhotoManager/PhotoManager.Application/Application.cs` - Main application facade
- `PhotoManager/PhotoManager.Domain/Asset.cs` - Core domain entity
- `PhotoManager/PhotoManager.Persistence/Sqlite/SqlitePersistenceContext.cs` - Database initialization, backup, dispose
- `PhotoManager/PhotoManager.Persistence/Sqlite/SqliteSchema.cs` - DDL schema definition
- `PhotoManager/PhotoManager.Persistence/Sqlite/SqliteConnectionFactory.cs` - Connection creation with PRAGMAs
- `PhotoManager/PhotoManager.Persistence/Sqlite/SqliteBackupService.cs` - Online backup + zip archive
- `PhotoManager/PhotoManager.Common/HashingHelper.cs` - Hash algorithms for duplicate detection
- `PhotoManager/PhotoManager.slnx` - Solution file

## Code Style

- **Indent**: 4 spaces
- **Max line length**: 120
- **End of line**: CRLF
- **File encoding**: UTF-8 with BOM for all files
- Treat warnings as errors is enabled in builds
- Run `dotnet format` to apply right file encoding, and code style
- Global usings are defined in each project's `GlobalUsings.cs`

## Testing Conventions

### Test Structure (Follow this pattern exactly)

```csharp
[TestFixture]
public class ClassNameTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private TestLogger<ClassName> _testLogger = new(); // That would be ClassNameTests for static method only

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new TestLogger<ClassName>();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    [TestCase("value1", "expected1")]
    [TestCase("value2", "expected2")]
    public void MethodName_Situation_ExpectedResult(string input, string expected)
    {
        // Arrange / Act / Assert
        _testLogger.AssertLogExceptions([], typeof(ClassName));
    }
}
```

### Naming Conventions

- Test files: `{ClassName}Tests.cs`
- Test class: `{ClassName}Tests`
- Test methods: `{MethodName}_{Scenario}_{ExpectedResult}`

### Required Test Helpers (Don't duplicate - USE THESE)

- `TestLogger<T>` - For all logging verification in tests
  - `_testLogger.AssertLogInfos(messages, typeof(Service))` - Verify info logs
  - `_testLogger.AssertLogErrors(messages, typeof(Service))` - Verify error logs
  - `_testLogger.AssertLogExceptions(exceptions, typeof(Service))` - Verify exceptions logged
  - `_testLogger.LoggingAssertTearDown()` - Clear logs in TearDown
- `DirectoryHelper` - For directory access control tests
  - `DenyWriteAccess()`, `AllowWriteAccess()`, `DenyAccess()`, `AllowAccess()`
- `ImageHelper` - For creating invalid test images
  - `CreateInvalidImage(validPath, invalidPath)`

### Resource Cleanup Pattern

```csharp
string testDirectory = Path.Combine(_assetsDirectory!, Directories.TEST_DIR);
try
{
    Directory.CreateDirectory(testDirectory);
    // Test code here
}
finally
{
    Directory.Delete(testDirectory, true);
}
```

### Key Points

- ALWAYS use `TestLogger<T>` for each test class to verify log output
- ALWAYS verify logs in assertions: `_testLogger.AssertLogExceptions([], typeof(Type))`
- ALWAYS cleanup in `finally` blocks or `TearDown`
- Use existing test helpers - don't recreate functionality
- Test data files in `PhotoManager/PhotoManager.Tests/TestFiles/`
- Constants for test data in `PhotoManager.Tests.Unit.Constants` namespace

### Visibility and Testability (NON-NEGOTIABLE)

- **Never use `InternalsVisibleTo`** to expose internal members for testing
- If a method cannot be tested through the public API, it is a design smell — refactor instead:
  - Test the behavior through a higher-level public method that exercises it
  - Extract the logic into a new class with a public interface
  - Reconsider whether the method belongs in its current layer
- Granting internal visibility to the test project couples tests to implementation details and undermines maintainability

## External Dependencies

- **Avalonia UI** - Cross-platform desktop UI framework (Windows, Linux, macOS)
- **Microsoft.Data.Sqlite** + **SQLitePCLRaw.bundle_e_sqlite3** - SQLite database
- **Magick.NET** (ImageMagick) - Image processing
- **FFMpegCore** - Video processing; versioned FFmpeg runtime packages copy app-local binaries at build time
- **SkiaSharp** - Cross-platform image decoding (thumbnails, EXIF, hashing)
- **Serilog** - File logging
- **Microsoft.Extensions.Logging** - Logging abstraction
- **NSubstitute** - Test mocking
- **BenchmarkDotNet** - Performance benchmarking
- **coverlet** - Code coverage

## Working with the Database

The persistence layer uses **SQLite** (via `Microsoft.Data.Sqlite`) with a single database file:

- Database file: `<AppContext.BaseDirectory>/Database/photomanager.db` (auto-created on first run, always next to the executable)
- Backups: `<AppContext.BaseDirectory>/Database/Backups/yyyyMMdd.zip` (contains a `photomanager.db` snapshot)
- Connection model: one connection per operation, WAL journal mode, `busy_timeout = 5000`

### Schema (v1 — defined in `SqliteSchema.cs`)

| Table              | Primary key              | Notes                                |
|--------------------|--------------------------|--------------------------------------|
| `Folders`          | `Id` (TEXT, GUID)        | Index on `Path`                      |
| `Assets`           | (`FolderId`, `FileName`) | Index on `Hash`. FK → `Folders(Id)`  |
| `Thumbnails`       | (`FolderId`, `FileName`) | `Data BLOB`. FK → `Folders(Id)`      |
| `RecentPaths`      | `Position` (INTEGER)     | Single ordered list                  |
| `SyncDefinitions`  | `Position` (INTEGER)     | Single ordered list                  |

### Key classes (all in `PhotoManager.Persistence`)

- `IPersistenceContext` / `SqlitePersistenceContext` — facade: Initialize, WriteBackup, DeleteOldBackups, Dispose
- `ISqliteConnectionFactory` / `SqliteConnectionFactory` — opens connections with PRAGMAs applied
- `ISqliteBackupService` / `SqliteBackupService` — online backup API + zip compression
- `IAssetPersistence` / `AssetPersistence` — CRUD for Assets table
- `IThumbnailPersistence` / `ThumbnailPersistence` — CRUD for Thumbnails table
- `IFolderPersistence` / `FolderPersistence` — CRUD for Folders table
- `IRecentPathsPersistence` / `RecentPathsPersistence` — Replace-all semantics
- `ISyncDefinitionsPersistence` / `SyncDefinitionsPersistence` — Replace-all semantics
- `ILruCache<TKey, TValue>` / `LruCache<TKey, TValue>` — thread-safe bounded cache

### Adding a new table

1. Add the DDL to `SqliteSchema.cs` `CREATE_SCRIPT`
2. Bump `SCHEMA_VERSION` and add migration logic in `EnsureCreated()`
3. Create a repository interface + implementation in `Repositories/`
4. Register in `PersistenceServiceCollectionExtensions.cs`

## HEIC/HEVC Support

HEIC/HEVC image handling is provided by Magick.NET; CI does not install OS codec extensions.

## Git Policy — AI Agents Must Never Commit or Push

**AI agents (Claude Code, GitHub Copilot, and any other tool) are strictly forbidden from running any destructive or history-altering git commands without explicit user instruction.**

### Permanently forbidden without explicit user request

- `git commit` — never commit on behalf of the user
- `git push` — never push to any remote
- `git reset` — never alter HEAD or the index
- `git rebase` — never rebase branches
- `git merge` — never merge branches
- `git cherry-pick` — never cherry-pick commits
- `git revert` — never create revert commits
- `git stash` — never stash changes
- `git tag` — never create or delete tags
- `git branch -D` — never delete branches
- `git am` — never apply patches

### Allowed read-only git operations

- `git status`, `git diff`, `git log`, `git show` — inspection only

### Rule

If the user says "commit the changes" or "push", **ask for confirmation first** and show exactly what will be committed/pushed before running the command. Never commit speculatively at the end of a task.
