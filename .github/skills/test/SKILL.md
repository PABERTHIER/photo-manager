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
       private string? _dataDirectory;
       private TestLogger<ClassName> _testLogger = new();

       [OneTimeSetUp]
       public void OneTimeSetUp()
       {
           _dataDirectory = Path.Combine(
               TestContext.CurrentContext.TestDirectory,
               Directories.TEST_FILES);
       }

       [SetUp]
       public void SetUp() { _testLogger = new TestLogger<ClassName>(); }

       [TearDown]
       public void TearDown() { _testLogger.LoggingAssertTearDown(); }

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
   No BDD-style naming (Given/When/Should).

6. **Always verify logs** in assertions and **always cleanup** in `finally` blocks.

7. **100% coverage** for all new code — every branch, exception path, and edge case.

8. **Build**: `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings.

9. **Run**: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
