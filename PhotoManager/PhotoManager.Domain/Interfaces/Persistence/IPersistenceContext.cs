using PhotoManager.Domain.Interfaces.Persistence.Repositories;

namespace PhotoManager.Domain.Interfaces.Persistence;

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
    /// <paramref name="databaseDirectory"/>. Idempotent.
    /// </summary>
    void Initialize(string databaseDirectory);

    IFolderPersistence Folders { get; }
    IAssetPersistence Assets { get; }
    IThumbnailPersistence Thumbnails { get; }
    IRecentPathsPersistence RecentPaths { get; }
    ISyncDefinitionsPersistence SyncDefinitions { get; }
    IConfigurationPersistence Configuration { get; }

    /// <summary>Inserts or updates one asset and its thumbnail atomically.</summary>
    void UpsertAssetWithThumbnail(Asset asset, byte[] thumbnailData);

    /// <summary>Deletes one asset and its thumbnail atomically.</summary>
    void DeleteAssetWithThumbnail(Guid folderId, string fileName);

    /// <summary>Deletes one folder, its assets, and their thumbnails atomically.</summary>
    void DeleteFolderWithAssetsAndThumbnails(Guid folderId);

    /// <summary>
    /// Reclaims free pages in the database file, reducing its on-disk footprint.
    /// Must not be called inside a transaction.
    /// </summary>
    void Vacuum();

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
