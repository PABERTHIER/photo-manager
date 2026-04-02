---
name: feature
description: >
  Implement a new feature for PhotoManager with full workflow.
  Use when asked to add new functionality, implement a user story, or add a new capability.
disable-model-invocation: true
argument-hint: <feature description>
---

You are implementing a new feature for PhotoManager described as: $ARGUMENTS

Follow this workflow in order:

1. **Understand**: Read relevant existing code to understand patterns and conventions used in the
   affected layers.

2. **Plan**: Briefly outline the approach — which layers are affected, what new files/classes are
   needed, and what existing code must change.

3. **Architecture**: Determine if new database tables (in `TablesConfig/`), settings classes
   (in `PhotoManager.Domain/UserConfigurationSettings/`), or service interfaces are needed.

4. **Implement**: Write the code following Clean Architecture layer order:
   - Domain first (entities, interfaces)
   - Infrastructure (implementations)
   - Application (orchestration)
   - UI (ViewModels, Views) — only if needed

5. **Using directives (NON-NEGOTIABLE)**:
   - Check existing `GlobalUsings.cs` files in each project BEFORE adding any `using`
   - Only add a `using` if the namespace is NOT already in global usings
   - Unnecessary usings cause warnings and violate the zero-warnings policy

6. **Build**: Run `dotnet build PhotoManager/PhotoManager.slnx` and fix any errors or warnings.
   The project has zero warnings — do not introduce any.

7. **Test**: Write comprehensive tests using NUnit + NSubstitute following the conventions in CLAUDE.md.
   Run ONLY relevant tests: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`

8. **Format**: Run `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
   to verify code style compliance.

Return a summary of all changes made with file paths.
