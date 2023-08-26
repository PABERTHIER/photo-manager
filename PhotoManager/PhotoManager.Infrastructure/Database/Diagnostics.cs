namespace PhotoManager.Infrastructure.Database;

public class Diagnostics
{
    public string LastReadFilePath { get; internal set; }
    public string LastReadFileRaw { get; internal set; }
    public string LastWriteFilePath { get; internal set; }
    public object LastWriteFileRaw { get; internal set; }
    public string[] LastDeletedBackupFilePaths { get; set; }
}
