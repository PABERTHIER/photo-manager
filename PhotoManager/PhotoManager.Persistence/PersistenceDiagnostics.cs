namespace PhotoManager.Persistence;

/// <summary>
/// Lightweight diagnostics snapshot for the persistence layer.
/// Each mutating operation on <see cref="IPersistenceContext"/> creates
/// a fresh instance so the last operation can be inspected in debuggers
/// and diagnostic logs without coupling callers to internal state.
/// </summary>
public sealed class PersistenceDiagnostics
{
    public string? LastOperation { get; internal init; }
    public string? LastBackupPath { get; internal init; }
    public string[]? LastDeletedBackupFilePaths { get; internal init; }
}
