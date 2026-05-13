namespace PhotoManager.Persistence;

/// <summary>
/// Lightweight diagnostics surface for the new persistence layer.
/// Mirrors <c>PhotoManager.Infrastructure.Database.Diagnostics</c> in spirit so logs can correlate
/// what the engine last did when an exception escapes.
/// </summary>
public sealed class PersistenceDiagnostics // TODO: I think there is no need to keep this, it seems useless
// If not, what is the best way to use it ? What would be the need ? In the logs ?
{
    public string? LastDatabasePath { get; internal set; } // TODO: Why no one reads it ?
    public string? LastOperation { get; internal init; }
    public string? LastBackupPath { get; internal init; }
    public string[]? LastDeletedBackupFilePaths { get; internal init; }
}
