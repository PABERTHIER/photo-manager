---
name: fix-bug
description: >
  Investigate and fix a bug in PhotoManager with a structured workflow.
  Use this skill when asked to fix a bug, debug an issue, or resolve unexpected behavior.
---

You are fixing a bug in PhotoManager. Find the root cause, not just the symptom.

Follow this workflow in order:

1. **Investigate**: Read relevant code. Trace the execution path from entry point to the
   point of failure to identify the root cause.

2. **Reproduce**: Write a failing test that demonstrates the bug before fixing it.
   - Name: `MethodName_BugScenario_ExpectedCorrectBehavior`
   - Use `TestLogger<T>` for logging verification

3. **Fix**: Make the minimal change needed. Do not refactor unrelated code.
   Check `GlobalUsings.cs` before adding any `using` directives.
   Ensure the fix introduces no new warnings.

4. **Verify**:
   - The new test now passes
   - All related tests pass: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
   - `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings

5. **Check for similar issues**: Search the codebase for the same pattern that caused the bug
   and fix other occurrences if found.
