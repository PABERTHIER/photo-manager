---
applyTo: "**/.github/workflows/**/*.yml"
---

# CI/CD Workflow Standards for PhotoManager

## Workflow Organization

The project uses separate workflow files per platform for independent badge reporting:

- `build-{os}.yml` — Build, lint, test, and coverage for each OS
- `codeql-{os}.yml` — Security analysis for each OS
- `release-{os}.yml` — Platform-specific packaging and release publishing

## Conventions

- Use emoji prefixes in step names for visual clarity (⤵️, 🛠️, 🔨, 📦, 📂, 💾, ☁️, 🏷️, 📤, 🛡️)
- Always include explicit `permissions` block
- Always cache NuGet packages with `actions/cache@v5`
- Use `env` block for shared variables (PROJECT_NAME, DOTNET_VERSION)
- Artifact names must include the OS/RID suffix for uniqueness

## Platform-Specific Notes

- **Windows**: Uses PowerShell (`Compress-Archive`) for archiving
- **Linux/macOS**: Uses `tar -czf` for archiving (produces `.tar.gz`)
- All platforms use `dotnet format` for linting (cross-platform)
- Release workflows use `gh release create ... || true` for idempotent release creation
- Release workflows use `gh release upload ... --clobber` for safe asset upload

## .NET Version

- Use `10.0.x` for the `dotnet-version` in `actions/setup-dotnet@v5`
- The project targets `net10.0`
