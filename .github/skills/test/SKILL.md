---
name: test
description: >
  Add or modify tests for PhotoManager following project conventions.
  Use this skill when asked to write tests, add test coverage, or fix failing tests.
---

You are adding or modifying tests for PhotoManager.

Follow this workflow in order:

1. **Read existing tests** to understand patterns in `PhotoManager.Tests/`.

2. **Use this exact test structure**:

   ```csharp
   [TestFixture]
   public class ClassNameTests
   {
       private string? _assetsDirectory;
       private string? _databaseDirectory;

       private TestLogger<ClassName> _testLogger = new();

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
       public void MethodName_Situation_ExpectedResult()
       {
           // Arrange / Act / Assert
           _testLogger.AssertLogExceptions([], typeof(ClassName));
       }
   }
   ```

   For static methods, use `TestLogger<ClassNameTests>` instead.

3. **Check `GlobalUsings.cs`** before adding any `using` directives.

4. **Use existing helpers only** — never duplicate:
   - `TestLogger<T>` — logging verification
   - `DirectoryHelper` — directory access control
   - `ImageHelper` — creating invalid test images
   - Constants in `PhotoManager.Tests.Unit.Constants`

5. **Test naming (NON-NEGOTIABLE)**: `MethodName_Situation_ExpectedResult`
   - Do NOT use BDD-style naming (Given/When/Should)
   - Examples: `CalculateHash_ValidFile_ReturnsHashValue`, `DeleteAsset_AssetExists_RemovesAssetFromDatabase`

6. **Always verify logs** in assertions and **always cleanup** in `finally` blocks.

7. **100% coverage** for all new code — every branch, exception path, and edge case.

8. **Cross-platform (NON-NEGOTIABLE)**: tests run on Windows, Linux, and macOS CI and must pass on all three. On Linux/macOS `\` is not a path separator and `C:\...` is not rooted, so `Path.*` calls diverge from Windows.
   Never assert on a Windows-only absolute path routed through a `Path` API — wrap it with `PathHelper.ToPlatformAbsolutePath(@"C:\Dir")` (or `PathHelper.ToResolvedConfigPath(...)` for a resolved config path), build expected paths with `Path.Combine` exactly as production does, and reference test files with their exact on-disk case.
   Opaque path strings (stored/asserted verbatim, e.g. config values or sync definitions) are fine.

9. **Build**: `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings.

10. **Run**: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
