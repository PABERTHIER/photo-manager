---
name: refactor
description: >
  Refactor PhotoManager code following Clean Architecture patterns.
  Use this skill when asked to restructure, extract, move, or reorganize code while preserving behavior.
---

You are refactoring PhotoManager code. Preserve all existing behavior.

Follow this workflow in order:

1. **Understand**: Read all code involved. Map dependencies across layers
   (UI → Application → Domain ← Infrastructure).

2. **Verify coverage**: Before changing anything, check that tests exist for the code being
   refactored. Write missing tests first to lock in current behavior.

3. **Plan**: Identify changes needed while respecting Clean Architecture boundaries.
   Domain must NOT depend on Infrastructure or UI.

4. **Apply changes incrementally**: After each change:
   - `dotnet build PhotoManager/PhotoManager.slnx` — verify zero warnings
   - Run relevant tests to verify no regressions

5. **Check `GlobalUsings.cs`** before adding any `using` directives.

6. **Update DI registration** in `{Layer}ServiceCollectionExtensions.cs` if services move or rename.
   Verify the chain in `App.xaml.cs`: `AddInfrastructure()` → `AddDomain()` → `AddApplication()` → `AddUi()`

7. **Final verification**:
   - `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings
   - `dotnet test --filter "FullyQualifiedName~AffectedClass" PhotoManager/PhotoManager.slnx`
   - `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
