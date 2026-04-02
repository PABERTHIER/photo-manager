# GitHub Copilot Repository Instructions

This project uses shared agent instructions defined in `AGENTS.md` at the repository root.
Copilot reads both this file and `AGENTS.md` automatically.

This file provides Copilot-specific behavioral guidance that complements `AGENTS.md`.

## Copilot-Specific Guidelines

### Code Generation
- Always use explicit types instead of `var` (enforced by `.editorconfig` with `IDE0008` as warning)
- Prefer collection expressions where applicable (`IDE0028`, `IDE0305` as warnings)
- Use `readonly` fields where possible
- Use file-scoped namespaces (`csharp_style_namespace_declarations = file_scoped`)
- Braces are always required (`csharp_prefer_braces = true:warning`)

### Using Directives
- Check `GlobalUsings.cs` in each project before adding any `using` directive
- Unnecessary usings produce warnings and the project treats warnings as errors
- Global usings are defined per-project in `GlobalUsings.cs` files

### When Suggesting Code Changes
- Follow Clean Architecture layer boundaries (see `AGENTS.md` for details)
- Never suggest code that crosses the dependency flow: UI → Application → Domain ← Infrastructure
- Match existing code patterns in the same file/project
- All nullable reference type warnings are treated as errors (CS8600-CS8604)

### Commit Messages
- Use conventional commit format
- Include a clear description of what changed and why

### For Scoped Instructions
- See `.github/instructions/` for file-type-specific rules that apply automatically
- See `.github/prompts/` for reusable prompt templates (invoke with `/` in Copilot Chat)
