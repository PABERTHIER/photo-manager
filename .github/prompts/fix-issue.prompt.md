---
description: "Fix a GitHub issue by number"
---

Fix GitHub issue #${input:issueNumber:Issue number to fix}.

1. Read the issue description (use `gh issue view ${input:issueNumber}` if GitHub CLI is available, otherwise ask for details).
2. Understand the requirements.
3. Investigate the relevant code.
4. Implement the fix following Clean Architecture patterns.
5. Write tests for the fix.
6. Build: `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings.
7. Run relevant tests: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`.
8. Format: `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`.
