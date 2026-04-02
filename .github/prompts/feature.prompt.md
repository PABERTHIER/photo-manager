---
description: "Implement a new feature following Clean Architecture workflow"
---

Implement the following feature for PhotoManager: ${input:feature:Describe the feature to implement}

Follow this workflow in order:

1. **Understand**: Read relevant existing code to understand patterns and conventions.
2. **Plan**: Outline which layers are affected and what changes are needed.
3. **Architecture**: Check if new database tables (`TablesConfig/`), settings classes (`PhotoManager.Domain/UserConfigurationSettings/`), or service interfaces are needed.
4. **Implement**: Write code following Clean Architecture layer order: Domain → Infrastructure → Application → UI.
5. **Using directives**: Check `GlobalUsings.cs` before adding any `using`. Unnecessary usings cause build failures (warnings = errors).
6. **Build**: Run `dotnet build PhotoManager/PhotoManager.slnx` — must produce zero warnings.
7. **Test**: Write tests using NUnit + NSubstitute following conventions in `AGENTS.md`. Run with `--filter`.
8. **Format**: Run `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`.
