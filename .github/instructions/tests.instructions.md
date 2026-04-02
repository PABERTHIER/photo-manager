---
applyTo: "**/PhotoManager.Tests/**/*.cs"
---

# Test Code Standards for PhotoManager

## Test Framework
- NUnit with NSubstitute for mocking
- Test data in `PhotoManager/PhotoManager.Tests/TestFiles/`

## Structure (Follow Exactly)
- `[TestFixture]` on class
- `[OneTimeSetUp]` to initialize `_dataDirectory`
- `[SetUp]` to create fresh `TestLogger<T>` instance
- `[TearDown]` to call `_testLogger.LoggingAssertTearDown()`
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
