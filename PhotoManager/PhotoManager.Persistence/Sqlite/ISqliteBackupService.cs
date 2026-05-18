namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// Produces and manages SQLite backup archives.
/// </summary>
public interface ISqliteBackupService
{
    bool WriteBackup(string backupFilePath);
    string[] GetBackupFilesPaths(string backupDirectory);
    void DeleteBackupFile(string backupFilePath);
}
