---
name: refactor
description: >
  Refactor code following Clean Architecture patterns.
  Use when asked to restructure, extract, move, or reorganize code while preserving behavior.
disable-model-invocation: true
argument-hint: <what to refactor>
---

You are tasked with refactoring: $ARGUMENTS

Follow this workflow in order:

1. **Understand current state**: Read all code involved in the refactoring. Map dependencies
   and understand the call chain across layers (UI → Application → Domain ← Infrastructure).

2. **Verify test coverage**: Before changing anything, check that tests exist for the code
   being refactored. If tests are missing, write them first to lock in current behavior.

3. **Plan the refactoring**: Identify what changes are needed while respecting:
   - Clean Architecture boundaries (Domain must not depend on Infrastructure/UI)
   - Dependency flow: UI → Application → Domain ← Infrastructure
   - Each layer has `{Layer}ServiceCollectionExtensions.cs` for DI registration

4. **Apply changes incrementally**: Make small, focused changes. After each change:
   - Run `dotnet build PhotoManager/PhotoManager.slnx` to verify zero warnings
   - Run relevant tests to verify no regressions

5. **Using directives (NON-NEGOTIABLE)**:
   - Check `GlobalUsings.cs` in each project before adding any `using`
   - Unnecessary usings cause warnings and violate the zero-warnings policy

6. **Update DI registration** if you move or rename services:
   - Check `{Layer}ServiceCollectionExtensions.cs` in the affected project
   - Verify `App.xaml.cs` chains: `AddInfrastructure()` → `AddDomain()` →
     `AddApplication()` → `AddUi()`

7. **Final verification**:
   - Run `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings
   - Run `dotnet test --filter "FullyQualifiedName~AffectedClass" PhotoManager/PhotoManager.slnx`
   - Run `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`

Return a summary of what was refactored and why, with file paths.
