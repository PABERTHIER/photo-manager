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

## TODO Comments (NON-NEGOTIABLE)
- **Never remove a `// TODO` comment** unless the issue it describes has been fully fixed in the same change
- If you touch a file containing TODO comments, leave them untouched unless you are explicitly fixing them

## Before Reporting Completion
1. Verify no compiler warnings (run `dotnet build` and check output)
2. Verify no line exceeds 120 characters (use `dotnet format` to check)
3. Run only relevant tests for changes made
4. If code was modified, ensure tests pass
5. Verify all `// TODO` comments in modified files are still present (do not silently remove them)

---

## Project Overview

PhotoManager is a WPF desktop application for managing photo/video collections, detecting duplicates, and syncing assets across folders. It targets .NET 10.0 and uses a Clean Architecture approach with a custom file-based database for local persistence.

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
PhotoManager/test-with-coverage.bat
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
├── PhotoManager.Infrastructure/# External concerns (file system, image processing, database)
├── PhotoManager.Common/        # Shared utilities (hashing, image/video helpers)
├── PhotoManager.UI/            # WPF presentation layer (ViewModels, Views)
├── PhotoManager.Tests/         # NUnit tests with NSubstitute
└── PhotoManager.Benchmarks/    # BenchmarkDotNet performance benchmarks
```

### Dependency Flow
UI → Application → Domain ← Infrastructure

### Key Architectural Patterns

1. **Custom File-Based Database**: The project uses a custom database implementation (`PhotoManager.Infrastructure.Database`) that stores data in CSV-like `.db` files with blobs in a separate folder. Configuration is in `TablesConfig/`.

2. **Service Collection Extensions**: Each layer has a `{Layer}ServiceCollectionExtensions.cs` that registers its services. The UI layer (`App.xaml.cs`) chains them: `AddInfrastructure()` → `AddDomain()` → `AddApplication()` → `AddUi()`.

3. **Hash-Based Duplicate Detection**: The `HashingHelper` supports multiple algorithms (PHash, DHash, MD5, SHA512). PHash is the most advanced and can detect duplicates between rotated images, thumbnails, and different resolutions. Controlled by `HashSettings` in `appsettings.json`.

4. **Microsoft Extensions Logging**: The application uses `ILogger<T>` throughout. The UI layer configures Serilog for file logging and console output in `App.xaml.cs`.

5. **Configuration**: All settings are in `appsettings.json` with strongly-typed settings classes in `PhotoManager.Domain/UserConfigurationSettings/` (AssetSettings, HashSettings, PathSettings, StorageSettings, ProjectSettings).

## Important Files

- `PhotoManager/PhotoManager.UI/App.xaml.cs` - Application startup, DI container setup
- `PhotoManager/PhotoManager.Application/Application.cs` - Main application facade
- `PhotoManager/PhotoManager.Domain/Asset.cs` - Core domain entity
- `PhotoManager/PhotoManager.Infrastructure/Database/Database.cs` - Custom database implementation
- `PhotoManager/PhotoManager.Common/HashingHelper.cs` - Hash algorithms for duplicate detection
- `PhotoManager/PhotoManager.slnx` - Solution file

## Code Style

- **Indent**: 4 spaces
- **Max line length**: 120
- **End of line**: CRLF
- Treat warnings as errors is enabled in builds
- Run `dotnet format` before committing to verify code style
- Global usings are defined in each project's `GlobalUsings.cs`

## Testing Conventions

### Test Structure (Follow this pattern exactly)
```csharp
[TestFixture]
public class ClassNameTests
{
    private string? _dataDirectory;

    private TestLogger<ClassName> _testLogger = new(); // That would be ClassNameTests for static method only

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new TestLogger<ClassName>();
    }

    [TearDown]
    public void TearDown()
    {
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
string testDirectory = Path.Combine(_dataDirectory!, Directories.TEST_DIR);
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

## External Dependencies

- **Magick.NET** (ImageMagick) - Image processing
- **FFMpegCore** - Video processing (FFmpeg executables are extracted from `.rar` files on build via `FileExtractionTask.dll`)
- **Serilog** - File logging
- **Microsoft.Extensions.Logging** - Logging abstraction
- **NSubstitute** - Test mocking
- **BenchmarkDotNet** - Performance benchmarking
- **coverlet** - Code coverage

## Working with the Database

The custom database stores:
- Tables as `.db` files in `{BackupPath}/{StorageVersion}/{TablesFolderName}/`
- Blobs in `{BackupPath}/{StorageVersion}/{BlobsFolderName}/`
- Backups in `{BackupPath}/{StorageVersion}_Backups/`

To add a new table, create a `DataTableProperties` in `TablesConfig/` and register it with `Database.SetDataTableProperties()`.

## HEIC/HEVC Support

The application requires HEIF Image Extensions and HEVC Video Extensions to be installed. In CI (`build.yml`), these are installed via `winget` and `Add-AppxPackage`.
