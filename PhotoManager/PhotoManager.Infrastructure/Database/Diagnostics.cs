namespace PhotoManager.Infrastructure.Database;

public class Diagnostics
{
    public string? LastReadFilePath { get; internal init; }
    public string? LastReadFileRaw { get; internal set; }
    public string? LastWriteFilePath { get; internal init; }
    public object? LastWriteFileRaw { get; internal set; }
    public string[]? LastDeletedBackupFilePaths { get; init; }
}
