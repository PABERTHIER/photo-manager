# CLAUDE.md

This file loads shared project context for Claude Code. See AGENTS.md for the full reference.

Claude Code should use the shared skills in `.agents/skills/` for Avalonia UI, feature development, bug fixing,
performance, persistence, refactoring, and testing. `AGENTS.md` contains the complete shared skill index.

## Shared Skills

| Skill       | File                                                                       | When to consult                                      |
|-------------|----------------------------------------------------------------------------|------------------------------------------------------|
| Avalonia    | [.agents/skills/avalonia/SKILL.md](.agents/skills/avalonia/SKILL.md)       | Working on Avalonia UI, MVVM, or image handling      |
| Feature     | [.agents/skills/feature/SKILL.md](.agents/skills/feature/SKILL.md)         | Adding a new PhotoManager feature                    |
| Fix bug     | [.agents/skills/fix-bug/SKILL.md](.agents/skills/fix-bug/SKILL.md)         | Investigating or fixing unexpected behavior          |
| Performance | [.agents/skills/perf/SKILL.md](.agents/skills/perf/SKILL.md)               | Optimizing speed, allocations, or runtime behavior   |
| Persistence | [.agents/skills/persistence/SKILL.md](.agents/skills/persistence/SKILL.md) | Changing SQLite persistence, repositories, or schema |
| Refactoring | [.agents/skills/refactor/SKILL.md](.agents/skills/refactor/SKILL.md)       | Restructuring code while preserving behavior         |
| Testing     | [.agents/skills/test/SKILL.md](.agents/skills/test/SKILL.md)               | Adding or modifying tests                            |

## Claude Code Customizations

Claude-specific agents, commands, and settings live under `.claude/`.

### Agents

| Agent         | File                                                               | Purpose                                               |
|---------------|--------------------------------------------------------------------|-------------------------------------------------------|
| Code reviewer | [.claude/agents/code-reviewer.md](.claude/agents/code-reviewer.md) | Review code quality, architecture, and .NET practices |

### Commands

| Command           | File                                                                         | Purpose                              |
|-------------------|------------------------------------------------------------------------------|--------------------------------------|
| Avalonia          | [.claude/commands/avalonia.md](.claude/commands/avalonia.md)                 | Work with Avalonia UI code           |
| Build             | [.claude/commands/build.md](.claude/commands/build.md)                       | Build the solution                   |
| Fix issue         | [.claude/commands/fix-issue.md](.claude/commands/fix-issue.md)               | Investigate and fix a reported issue |
| Format            | [.claude/commands/format.md](.claude/commands/format.md)                     | Format and validate code style       |
| Persistence tests | [.claude/commands/test-persistence.md](.claude/commands/test-persistence.md) | Run persistence-focused tests        |

### Settings

| File                                           | Purpose                      |
|------------------------------------------------|------------------------------|
| [.claude/settings.json](.claude/settings.json) | Claude Code project settings |

@AGENTS.md
