---
description: "Verify and fix code formatting"
---

Verify code formatting compliance for the PhotoManager project.

1. Check: `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
2. If issues found, fix: `dotnet format PhotoManager/PhotoManager.slnx --severity warn`
3. Verify fix: `dotnet format PhotoManager/PhotoManager.slnx --severity warn --verify-no-changes`
