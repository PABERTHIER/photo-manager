---
applyTo: "**/PhotoManager.Tests/**/*.cs"
---

# Test Code Standards for PhotoManager

## Test Framework

- NUnit with NSubstitute for mocking
- Test data in `PhotoManager/PhotoManager.Tests/TestFiles/`

## Structure (Follow Exactly)

- `[TestFixture]` on class
- `[OneTimeSetUp]` to initialize `_assetsDirectory` and `_databaseDirectory`
- `[SetUp]` to create fresh `TestLogger<T>` instance
- `[TearDown]` to call `_testableAssetRepository?.Dispose()`, `TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!)` and `_testLogger.LoggingAssertTearDown()`
- For static methods being tested, use `TestLogger<ClassNameTests>` instead

## Naming (NON-NEGOTIABLE)

- Files: `{ClassName}Tests.cs`
- Classes: `{ClassName}Tests`
- Methods: `{MethodName}_{Scenario}_{ExpectedResult}`
- Do NOT use BDD-style naming (Given/When/Should)

## Required Helpers (Never Duplicate)

- `TestLogger<T>` — logging verification (AssertLogInfos, AssertLogErrors, AssertLogExceptions)
- `DirectoryHelper` — directory access control (DenyWriteAccess, AllowWriteAccess, DenyAccess, AllowAccess)
- `ImageHelper` — creating invalid test images (CreateInvalidImage)
- Constants in `PhotoManager.Tests.Unit.Constants` namespace

## Assertions

- Always verify logs: `_testLogger.AssertLogExceptions([], typeof(ClassName))`
- Always cleanup resources in `finally` blocks

## Coverage

- 100% coverage required for new code
- Every branch, exception path, and edge case must be tested

## Cross-Platform Compatibility (NON-NEGOTIABLE)

Tests run on Windows, Linux, and macOS CI — every test must pass on all three. On Linux/macOS `\` is **not** a path separator and `C:\...` paths are **not** rooted, so `Path.*` calls return different results than on Windows (e.g. `Path.GetFileName(@"C:\a\b.png")` returns the whole string, `Path.GetDirectoryName(...)` returns `""`).

- Never hardcode a Windows-only absolute path (`@"C:\..."`, drive letters) that is passed to `Path.GetFileName`/`GetDirectoryName`/`GetExtension`/`Path.Combine` and then asserted.
- Use `PathHelper.ToPlatformAbsolutePath(@"C:\Dir")` for absolute test paths fed to a `Path` API or to production path logic; use `PathHelper.ToResolvedConfigPath(@"...")` when asserting a path resolved by `UserConfigurationService`.
- Build expected paths exactly as production does (`Path.Combine(resolvedDir, name)`), never `Path.GetFullPath(@"...\name")`.
- Opaque path strings (stored/returned verbatim and asserted against the same literal — config values, sync definitions) are portable as-is; leave them unchanged.
- Build real paths with `Path.Combine`, never literal separators. File names are case-sensitive on Linux — match the on-disk case. Do not assume `\r\n` / `Environment.NewLine`.

## Visibility and Testability (NON-NEGOTIABLE)

- **Never use `InternalsVisibleTo`** to expose internal members for testing
- If a method cannot be tested through the public API, it is a design smell — refactor instead:
  - Test the behavior through a higher-level public method that exercises it
  - Extract the logic into a new class with a public interface
  - Reconsider whether the method belongs in its current layer
- Granting internal visibility to the test project couples the tests to implementation details and undermines maintainability
