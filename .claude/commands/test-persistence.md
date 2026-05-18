# Run persistence tests for PhotoManager

Run all persistence-related tests:

```bash
dotnet test --filter "FullyQualifiedName~Persistence" PhotoManager/PhotoManager.slnx
```

If tests fail, analyze the failures and suggest fixes.
If all pass, report the count and confirm coverage.
