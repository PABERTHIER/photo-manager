---
name: fix-bug
description: >
  Investigate and fix a bug with a structured workflow.
  Use when asked to fix a bug, debug an issue, or resolve unexpected behavior.
disable-model-invocation: true
argument-hint: <bug description or issue number>
---

You are tasked with fixing this bug: $ARGUMENTS

Follow this workflow in order:

1. **Investigate**: Understand the bug by reading the relevant code. Trace the execution path
   from entry point to the point of failure. Identify the root cause, not just the symptom.

2. **Reproduce**: Write a failing test that demonstrates the bug. Follow the test conventions
   from CLAUDE.md:
   - Test name: `MethodName_BugScenario_ExpectedCorrectBehavior`
   - Use `TestLogger<T>` for logging verification
   - Place in the appropriate test class or create a new one if needed

3. **Fix**: Make the minimal change needed to fix the bug.
   - Do not refactor unrelated code
   - Check `GlobalUsings.cs` before adding any `using` directives
   - Ensure the fix doesn't introduce new warnings

4. **Verify the fix**:
   - Run the new failing test — it should now pass
   - Run all related tests: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
   - Run `dotnet build PhotoManager/PhotoManager.slnx` — zero warnings

5. **Check for similar issues**: Search the codebase for the same pattern that caused the bug.
   If found in other places, fix those too.

Return a summary of the root cause, the fix applied, and test results.
