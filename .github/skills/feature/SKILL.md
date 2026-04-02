---
name: feature
description: >
  Implement a new feature for PhotoManager following Clean Architecture workflow.
  Use this skill when asked to add new functionality, implement a user story, or add a new capability.
---

You are implementing a new feature for PhotoManager.

Follow this workflow in order:

1. **Understand**: Read relevant existing code to understand patterns and conventions used in
   the affected layers.

2. **Plan**: Briefly outline the approach — which layers are affected, what new files/classes
   are needed, and what existing code must change.

3. **Architecture**: Determine if new database tables (in `TablesConfig/`), settings classes
   (in `PhotoManager.Domain/UserConfigurationSettings/`), or service interfaces are needed.

4. **Implement**: Write code following Clean Architecture layer order:
   - Domain first (entities, interfaces)
   - Infrastructure (implementations)
   - Application (orchestration)
   - UI (ViewModels, Views) — only if needed

5. **Using directives (NON-NEGOTIABLE)**:
   - Check existing `GlobalUsings.cs` files in each project BEFORE adding any `using`
   - Only add a `using` if the namespace is NOT already in global usings
   - Unnecessary usings cause warnings and the project treats warnings as errors

6. **Build**: Run `dotnet build PhotoManager/PhotoManager.slnx` — must produce zero warnings.

7. **Test**: Write tests using NUnit + NSubstitute following conventions in `AGENTS.md`.
   Run with: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`

8. **Format**: Run `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`.

Return a summary of all changes made with file paths.
