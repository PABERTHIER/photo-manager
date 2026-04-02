---
description: "Refactor code following Clean Architecture patterns"
---

Refactor the following: ${input:target:What to refactor}

Follow this workflow:

1. **Understand**: Read all code involved. Map dependencies across layers (UI → Application → Domain ← Infrastructure).
2. **Verify test coverage**: If tests are missing for the code being changed, write them first.
3. **Plan**: Identify changes needed while respecting Clean Architecture boundaries and dependency flow.
4. **Apply changes incrementally**: After each change, build (`dotnet build PhotoManager/PhotoManager.slnx`) and run tests.
5. **Check `GlobalUsings.cs`** before adding any `using` directives.
6. **Update DI registration** in `{Layer}ServiceCollectionExtensions.cs` if services move or rename.
7. **Final verification**:
   - `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings
   - `dotnet test --filter "FullyQualifiedName~AffectedClass" PhotoManager/PhotoManager.slnx`
   - `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
