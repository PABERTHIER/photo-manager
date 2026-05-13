using System.IO.Compression;

namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// Produces and manages SQLite backup archives.
/// File naming is filename-compatible with the legacy backup format
/// (<c>yyyyMMdd.zip</c>) but the archive content differs: a single
/// <c>photomanager.db</c> snapshot produced via the SQLite online backup API.
/// </summary>
internal sealed class SqliteBackupService(ISqliteConnectionFactory connectionFactory)
{
    private const string ENTRY_NAME = "photomanager.db";

    public bool WriteBackup(string backupFilePath)
    {
        string? backupDirectory = Path.GetDirectoryName(backupFilePath);

        if (!string.IsNullOrEmpty(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }

        string snapshotPath = backupFilePath + ".tmp.db";

        if (File.Exists(snapshotPath))
        {
            File.Delete(snapshotPath);
        }

        try
        {
            // Online backup -> consistent snapshot of the live DB even with concurrent writes.
            using (SqliteConnection sourceConnection = connectionFactory.Open())
            {
                SqliteConnectionStringBuilder destinationBuilder = new()
                {
                    DataSource = snapshotPath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Pooling = false
                };

                using (SqliteConnection destinationConnection = new(destinationBuilder.ConnectionString))
                {
                    destinationConnection.Open();
                    sourceConnection.BackupDatabase(destinationConnection);
                    destinationConnection.Close();
                }
            }

            using (FileStream zipStream = new(backupFilePath, FileMode.CreateNew))
            {
                using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(snapshotPath, ENTRY_NAME, CompressionLevel.Fastest);
                }
            }

            return true;
        }
        finally
        {
            if (File.Exists(snapshotPath))
            {
                try
                {
                    File.Delete(snapshotPath);
                }
                catch (IOException)
                {
                    // SQLite may keep a brief lock on the file even with pooling disabled;
                    // a single retry after a short delay clears it deterministically.
                    Thread.Sleep(20);
                    File.Delete(snapshotPath);
                }
            }
        }
    }

    public string[] GetBackupFilesPaths(string backupDirectory)
    {
        return Directory.Exists(backupDirectory) ? Directory.GetFiles(backupDirectory, "*.zip") : [];
    }

    public void DeleteBackupFile(string backupFilePath) => File.Delete(backupFilePath);
}
