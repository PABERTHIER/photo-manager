---
description: "Investigate and fix a bug with structured workflow"
---

Fix this bug: ${input:bug:Bug description or issue number}

Follow this workflow:

1. **Investigate**: Read relevant code. Trace execution path to find the root cause, not just the symptom.
2. **Reproduce**: Write a failing test demonstrating the bug. Name it `MethodName_BugScenario_ExpectedCorrectBehavior`. Use `TestLogger<T>`.
3. **Fix**: Make the minimal change needed. Do not refactor unrelated code. Check `GlobalUsings.cs` before adding usings.
4. **Verify**:
   - New test now passes
   - All related tests pass: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
   - `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings
5. **Check for similar issues**: Search codebase for the same pattern and fix other occurrences if found.
