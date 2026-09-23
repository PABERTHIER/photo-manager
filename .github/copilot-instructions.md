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
- Never suggest code that crosses the dependency flow: UI → Application → Domain ← Infrastructure ← Persistence
- Match existing code patterns in the same file/project
- All nullable reference type warnings are treated as errors (CS8600-CS8604)
- Tests run on Windows, Linux, and macOS CI and must pass on all three — never assert on a Windows-only absolute path (`@"C:\..."`) routed through a `Path` API; use the `PathHelper` helpers and `Path.Combine` (see **Cross-Platform Test Compliance** in `AGENTS.md` and `.github/instructions/tests.instructions.md`)

## Git Policy

**Never run any git write command without explicit user instruction.**

Forbidden without a direct user request:

- `git commit`, `git push`, `git reset`, `git rebase`, `git merge`
- `git cherry-pick`, `git revert`, `git stash`, `git tag`
- `git branch -D`, `git am`

Allowed: `git status`, `git diff`, `git log`, `git show` (read-only inspection).

If asked to commit or push, **ask for confirmation first** and show what will be committed. Never commit speculatively at the end of a task.

### Commit Messages

- Use conventional commit format
- Include a clear description of what changed and why

### For Scoped Instructions

- See `.github/instructions/` for file-type-specific rules that apply automatically
- See `.github/prompts/` for reusable prompt templates (invoke with `/` in Copilot Chat)

## Shared Skills

Detailed workflows are shared with Claude Code in `.agents/skills/`. Use the relevant skill for Avalonia UI,
feature development, bug fixing, performance, persistence, refactoring, or testing:

| Skill       | File                                                                             | When to consult                                      |
| ----------- | -------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Avalonia    | [../.agents/skills/avalonia/SKILL.md](../.agents/skills/avalonia/SKILL.md)       | Working on Avalonia UI, MVVM, or image handling      |
| Feature     | [../.agents/skills/feature/SKILL.md](../.agents/skills/feature/SKILL.md)         | Adding a new PhotoManager feature                    |
| Fix bug     | [../.agents/skills/fix-bug/SKILL.md](../.agents/skills/fix-bug/SKILL.md)         | Investigating or fixing unexpected behavior          |
| Performance | [../.agents/skills/perf/SKILL.md](../.agents/skills/perf/SKILL.md)               | Optimizing speed, allocations, or runtime behavior   |
| Persistence | [../.agents/skills/persistence/SKILL.md](../.agents/skills/persistence/SKILL.md) | Changing SQLite persistence, repositories, or schema |
| Refactoring | [../.agents/skills/refactor/SKILL.md](../.agents/skills/refactor/SKILL.md)       | Restructuring code while preserving behavior         |
| Testing     | [../.agents/skills/test/SKILL.md](../.agents/skills/test/SKILL.md)               | Adding or modifying tests                            |

## Copilot Customizations

Additional Copilot-specific instructions and prompts:

### File Instructions

| Instruction | File                                                                                 | When applied                |
| ----------- | ------------------------------------------------------------------------------------ | --------------------------- |
| Avalonia    | [instructions/avalonia.instructions.md](instructions/avalonia.instructions.md)       | Editing Avalonia UI files   |
| Benchmarks  | [instructions/benchmarks.instructions.md](instructions/benchmarks.instructions.md)   | Editing benchmark C# files  |
| CI          | [instructions/ci.instructions.md](instructions/ci.instructions.md)                   | Working on CI configuration |
| C#          | [instructions/csharp.instructions.md](instructions/csharp.instructions.md)           | Editing C# files            |
| Persistence | [instructions/persistence.instructions.md](instructions/persistence.instructions.md) | Working on persistence code |
| Tests       | [instructions/tests.instructions.md](instructions/tests.instructions.md)             | Editing test C# files       |

### Prompts

| Prompt      | File                                                           | Use case                              |
| ----------- | -------------------------------------------------------------- | ------------------------------------- |
| Avalonia    | [prompts/avalonia.prompt.md](prompts/avalonia.prompt.md)       | Plan or implement Avalonia UI changes |
| Build       | [prompts/build.prompt.md](prompts/build.prompt.md)             | Build and validate the solution       |
| Feature     | [prompts/feature.prompt.md](prompts/feature.prompt.md)         | Implement a new feature               |
| Fix bug     | [prompts/fix-bug.prompt.md](prompts/fix-bug.prompt.md)         | Investigate and fix a bug             |
| Fix issue   | [prompts/fix-issue.prompt.md](prompts/fix-issue.prompt.md)     | Work through a tracked issue          |
| Format      | [prompts/format.prompt.md](prompts/format.prompt.md)           | Apply or verify formatting            |
| Performance | [prompts/perf.prompt.md](prompts/perf.prompt.md)               | Benchmark and optimize performance    |
| Persistence | [prompts/persistence.prompt.md](prompts/persistence.prompt.md) | Work on SQLite persistence            |
| Refactoring | [prompts/refactor.prompt.md](prompts/refactor.prompt.md)       | Refactor while preserving behavior    |
| Testing     | [prompts/test.prompt.md](prompts/test.prompt.md)               | Add or modify tests                   |
