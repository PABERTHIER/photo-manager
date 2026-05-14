using PhotoManager.Persistence.Repositories;

namespace PhotoManager.Persistence;

/// <summary>
/// High-level facade for the new SQLite-backed persistence engine.
/// All operations are thread-safe (every call opens its own connection
/// and releases it).
/// </summary>
public interface IPersistenceContext
{
    /// <summary>Resolved absolute path to the SQLite database file.</summary>
    string DatabaseFilePath { get; }

    /// <summary>
    /// Opens (and creates if necessary) the SQLite database under
    /// <paramref name="dataDirectory"/>. Idempotent.
    /// </summary>
    void Initialize(string dataDirectory);

    IFolderPersistence Folders { get; }
    IAssetPersistence Assets { get; }
    IThumbnailPersistence Thumbnails { get; }
    IRecentPathsPersistence RecentPaths { get; }
    ISyncDefinitionsPersistence SyncDefinitions { get; }

    /// <summary>
    /// Writes a backup snapshot for <paramref name="backupDate"/>.
    /// File name: <c>yyyyMMdd.zip</c>. Archive contains a single
    /// <c>photomanager.db</c> produced via the SQLite online-backup API.
    /// Returns <c>true</c> on success.
    /// </summary>
    bool WriteBackup(DateTime backupDate);

    /// <summary>True if a backup file already exists for <paramref name="backupDate"/>.</summary>
    bool BackupExists(DateTime backupDate);

    /// <summary>Deletes oldest backup files keeping <paramref name="backupsToKeep"/> most recent.</summary>
    void DeleteOldBackups(ushort backupsToKeep);
}
