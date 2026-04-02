---
description: "Add or modify tests following project conventions"
---

Add or modify tests for: ${input:target:Class or method to test}

Follow the testing conventions defined in `AGENTS.md`:

1. **Read existing tests** to understand patterns in `PhotoManager.Tests/`.
2. **Use exact test structure**: `[TestFixture]`, `[OneTimeSetUp]`, `[SetUp]`, `[TearDown]` with `TestLogger<T>`.
3. **Check `GlobalUsings.cs`** before adding any `using` directives.
4. **Use existing helpers**: `TestLogger<T>`, `DirectoryHelper`, `ImageHelper`, constants in `PhotoManager.Tests.Unit.Constants`.
5. **Always verify logs**: `_testLogger.AssertLogExceptions([], typeof(ClassName))`.
6. **Always cleanup resources** in `finally` blocks.
7. **Naming**: `MethodName_Situation_ExpectedResult` — NO BDD-style naming.
8. **100% coverage** for all new code — every branch, exception path, and edge case.
9. **Build**: `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings.
10. **Run tests**: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`.
