# Verify code formatting compliance for the PhotoManager project

```bash
dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes
```

If formatting issues are found, fix them by running:

```bash
dotnet format PhotoManager/PhotoManager.slnx --severity warn
```

Then verify the fix:

```bash
dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes
```
