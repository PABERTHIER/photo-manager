# Fix GitHub issue #$ARGUMENTS

1. Read the issue description from GitHub (use `gh issue view $ARGUMENTS` if the GitHub CLI is available, otherwise ask the user to paste the issue details)
2. Understand the requirements
3. Investigate the relevant code
4. Implement the fix following the project's Clean Architecture patterns
5. Write tests for the fix
6. Run `dotnet build PhotoManager/PhotoManager.slnx` — verify zero warnings
7. Run relevant tests with `--filter` to verify the fix works
8. Run `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
