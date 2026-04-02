---
applyTo: "**/*.cs"
---

# C# Code Standards for PhotoManager

## Formatting
- Max line length: 120 characters
- Indent: 4 spaces
- End of line: CRLF
- Use Allman-style braces (opening brace on new line for all constructs)

## Type Usage
- Use explicit types, NOT `var` (e.g., `string name = "value"` not `var name = "value"`)
- Use predefined type keywords (`string`, `int`, `bool`) not BCL names (`String`, `Int32`)
- Use collection expressions where applicable

## Naming Conventions
- PascalCase for public members, types, and methods
- `_camelCase` for private fields (with underscore prefix)
- `I` prefix for interfaces (e.g., `IAssetRepository`)
- File-scoped namespaces only

## Null Safety
- Nullable reference types are enabled project-wide
- All CS8600-CS8604 nullable warnings are treated as errors
- Always handle nullable references properly

## Other Rules
- Braces are always required, even for single-line blocks
- Do not qualify members with `this.` or `Me.`
- Mark fields `readonly` where possible
- Use pattern matching over `as`/`is` with null checks
- Check `GlobalUsings.cs` before adding `using` directives

## TODO Comments
- **Never remove a `// TODO` comment** unless the issue it describes is fully fixed in the same change
- If you touch a file that contains TODO comments, leave them untouched unless you are explicitly fixing them
