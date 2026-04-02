---
name: test
description: >
  Add or modify tests following project conventions.
  Use when asked to write tests, add test coverage, or fix failing tests.
disable-model-invocation: true
argument-hint: <class or method to test>
---

You are tasked with adding or modifying tests for: $ARGUMENTS

Follow this workflow in order:

1. **Read existing tests** to understand patterns. Good references:
   - `PhotoManager.Tests/` directory structure
   - Look for `*Tests.cs` files matching the class you are testing

2. **Follow this exact test structure**:

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
       public void MethodName_Situation_ExpectedResult()
       {
           // Arrange / Act / Assert
           _testLogger.AssertLogExceptions([], typeof(ClassName));
       }
   }
   ```

   For static methods, use `TestLogger<ClassNameTests>` instead.

3. **Using directives (NON-NEGOTIABLE)**:
   - Check `PhotoManager.Tests/GlobalUsings.cs` BEFORE adding any `using`
   - Only add if the namespace is NOT already in global usings
   - Unnecessary usings cause warnings and violate the zero-warnings policy

4. **Use existing helpers (NEVER duplicate)**:
   - `TestLogger<T>` — for all logging verification
   - `DirectoryHelper` — for directory access control tests
   - `ImageHelper` — for creating invalid test images
   - Constants in `PhotoManager.Tests.Unit.Constants` namespace

5. **Always verify log output** in assertions:
   - `_testLogger.AssertLogInfos(messages, typeof(Service))`
   - `_testLogger.AssertLogErrors(messages, typeof(Service))`
   - `_testLogger.AssertLogExceptions(exceptions, typeof(Service))`

6. **Always cleanup resources** with `finally` blocks:

   ```csharp
   try
   {
       Directory.CreateDirectory(testDir);
       // test code
   }
   finally
   {
       Directory.Delete(testDir, true);
   }
   ```

7. **Test naming (NON-NEGOTIABLE)**: `MethodName_Situation_ExpectedResult`
   - Do NOT use BDD-style naming (Given/When/Should)
   - Examples: `CalculateHash_ValidFile_ReturnsHashValue`,
     `DeleteAsset_AssetExists_RemovesAssetFromDatabase`

8. **100% coverage required** for all new code — every branch, exception path,
   and edge case must be tested.

9. **Build**: Run `dotnet build PhotoManager/PhotoManager.slnx` and ensure zero warnings.

10. **Run tests**: Run `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
    using the appropriate class name filter.

Return a summary of tests added/modified with test results.
