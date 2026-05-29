using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using PhotoManager.Persistence.Repositories;

namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// SQLite-backed implementation of <see cref="IPersistenceContext"/>.
/// Owns the DB file and exposes typed primitive repositories.
/// Thread-safe: every primitive opens its own connection per call.
/// </summary>
public sealed class SqlitePersistenceContext(
    ISqliteConnectionFactory factory,
    ISqliteBackupService backupService,
    ILogger<SqlitePersistenceContext> logger)
    : IPersistenceContext, IDisposable
{
    public const string DATABASE_FILE_NAME = "photomanager.db";
    private const string BACKUPS_SUFFIX = "Backups";

    private string _backupsDirectory = string.Empty;

    public IFolderPersistence Folders { get; private set; } = null!;
    public IAssetPersistence Assets { get; private set; } = null!;
    public IThumbnailPersistence Thumbnails { get; private set; } = null!;
    public IRecentPathsPersistence RecentPaths { get; private set; } = null!;
    public ISyncDefinitionsPersistence SyncDefinitions { get; private set; } = null!;
    public IConfigurationPersistence Configuration { get; private set; } = null!;

    public string DatabaseFilePath => factory.DatabasePath;

    public void Initialize(string databaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(databaseDirectory))
        {
            ArgumentException ex = new("databaseDirectory must not be empty.", nameof(databaseDirectory));
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw ex;
        }

        Directory.CreateDirectory(databaseDirectory);

        string databaseFilePath = Path.Combine(databaseDirectory, DATABASE_FILE_NAME);
        _backupsDirectory = Path.Combine(databaseDirectory, BACKUPS_SUFFIX);
        Directory.CreateDirectory(_backupsDirectory);

        factory.Initialize(databaseFilePath);

        try
        {
            using (SqliteConnection connection = factory.Open())
            {
                SqliteSchema.EnsureCreated(connection);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize SQLite Db at {DatabaseFilePath}", DatabaseFilePath);
            throw;
        }

        Folders = new FolderPersistence(factory);
        Assets = new AssetPersistence(factory);
        Thumbnails = new ThumbnailPersistence(factory);
        RecentPaths = new RecentPathsPersistence(factory);
        SyncDefinitions = new SyncDefinitionsPersistence(factory);
        Configuration = new ConfigurationPersistence(factory);
    }

    public bool WriteBackup(DateTime backupDate)
    {
        EnsureInitialized();

        string backupFilePath = ResolveBackupFilePath(backupDate);

        try
        {
            return backupService.WriteBackup(backupFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while writing backup {BackupFilePath}", backupFilePath);
            throw;
        }
    }

    public bool BackupExists(DateTime backupDate)
    {
        EnsureInitialized();
        return File.Exists(ResolveBackupFilePath(backupDate));
    }

    public void UpsertAssetWithThumbnail(Asset asset, byte[] thumbnailData)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(thumbnailData);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand assetCommand = connection.CreateCommand())
                {
                    assetCommand.Transaction = transaction;
                    assetCommand.CommandText = AssetPersistence.UPSERT_SQL;
                    AssetPersistence.BindAsset(assetCommand, asset);
                    assetCommand.ExecuteNonQuery();
                }

                using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                {
                    thumbnailCommand.Transaction = transaction;
                    thumbnailCommand.CommandText = ThumbnailPersistence.UPSERT_SQL;
                    ThumbnailPersistence.BindUpsert(thumbnailCommand, asset.FolderId, asset.FileName,
                        thumbnailData);
                    thumbnailCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }
    }

    public void UpsertAssetsWithThumbnails(IReadOnlyList<AssetWithThumbnail> assetsWithThumbnails)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(assetsWithThumbnails);

        if (assetsWithThumbnails.Count == 0)
        {
            return;
        }

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand assetCommand = connection.CreateCommand())
                {
                    assetCommand.Transaction = transaction;
                    assetCommand.CommandText = AssetPersistence.UPSERT_SQL;

                    using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                    {
                        thumbnailCommand.Transaction = transaction;
                        thumbnailCommand.CommandText = ThumbnailPersistence.UPSERT_SQL;

                        for (int i = 0; i < assetsWithThumbnails.Count; i++)
                        {
                            AssetWithThumbnail assetWithThumbnail = assetsWithThumbnails[i];
                            assetCommand.Parameters.Clear();
                            AssetPersistence.BindAsset(assetCommand, assetWithThumbnail.Asset);
                            assetCommand.ExecuteNonQuery();

                            thumbnailCommand.Parameters.Clear();
                            ThumbnailPersistence.BindUpsert(
                                thumbnailCommand,
                                assetWithThumbnail.Asset.FolderId,
                                assetWithThumbnail.Asset.FileName,
                                assetWithThumbnail.ThumbnailData);
                            thumbnailCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }

    public void DeleteAssetWithThumbnail(Guid folderId, string fileName)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                ExecuteNonQuery(connection, transaction,
                    "DELETE FROM Thumbnails WHERE FolderId = $folderId AND FileName = $fileName;",
                    folderId, fileName);
                ExecuteNonQuery(connection, transaction,
                    "DELETE FROM Assets WHERE FolderId = $folderId AND FileName = $fileName;",
                    folderId, fileName);

                transaction.Commit();
            }
        }
    }

    public void DeleteAssetsWithThumbnails(Guid folderId, IReadOnlyList<string> fileNames)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(fileNames);

        if (fileNames.Count == 0)
        {
            return;
        }

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                {
                    thumbnailCommand.Transaction = transaction;
                    thumbnailCommand.CommandText =
                        "DELETE FROM Thumbnails WHERE FolderId = $folderId AND FileName = $fileName;";

                    using (SqliteCommand assetCommand = connection.CreateCommand())
                    {
                        assetCommand.Transaction = transaction;
                        assetCommand.CommandText =
                            "DELETE FROM Assets WHERE FolderId = $folderId AND FileName = $fileName;";

                        for (int i = 0; i < fileNames.Count; i++)
                        {
                            string fileName = fileNames[i];
                            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

                            thumbnailCommand.Parameters.Clear();
                            thumbnailCommand.Parameters.AddWithValue("$folderId", folderId);
                            thumbnailCommand.Parameters.AddWithValue("$fileName", fileName);
                            thumbnailCommand.ExecuteNonQuery();

                            assetCommand.Parameters.Clear();
                            assetCommand.Parameters.AddWithValue("$folderId", folderId);
                            assetCommand.Parameters.AddWithValue("$fileName", fileName);
                            assetCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }

    public void DeleteFolderWithAssetsAndThumbnails(Guid folderId)
    {
        EnsureInitialized();

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                ExecuteNonQuery(connection, transaction, "DELETE FROM Thumbnails WHERE FolderId = $folderId;",
                    folderId);
                ExecuteNonQuery(connection, transaction, "DELETE FROM Assets WHERE FolderId = $folderId;", folderId);
                ExecuteNonQuery(connection, transaction, "DELETE FROM Folders WHERE Id = $folderId;", folderId);

                transaction.Commit();
            }
        }
    }

    public void DeleteOldBackups(ushort backupsToKeep)
    {
        EnsureInitialized();

        try
        {
            string[] filesPaths = backupService.GetBackupFilesPaths(_backupsDirectory);

            Array.Sort(filesPaths, StringComparer.Ordinal);

            for (int i = 0; i < filesPaths.Length - backupsToKeep; i++)
            {
                backupService.DeleteBackupFile(filesPaths[i]);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting old backups (keep {BackupsToKeep})", backupsToKeep);
            throw;
        }
    }

    public void Vacuum()
    {
        EnsureInitialized();

        try
        {
            // Close all pooled connections before VACUUM to obtain exclusive access.
            SqliteConnection.ClearAllPools();

            using (SqliteConnection connection = factory.Open())
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "VACUUM;";
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error vacuuming the SQLite database at {DatabaseFilePath}", DatabaseFilePath);
            throw;
        }
    }

    private string ResolveBackupFilePath(DateTime backupDate)
    {
        string fileName = $"{backupDate:yyyyMMdd}.zip";
        return Path.Combine(_backupsDirectory, fileName);
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string commandText,
        Guid folderId, string? fileName = null)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = commandText;
            command.Parameters.AddWithValue("$folderId", folderId);

            if (fileName != null)
            {
                command.Parameters.AddWithValue("$fileName", fileName);
            }

            command.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
    }

    private void EnsureInitialized()
    {
        if (string.IsNullOrEmpty(DatabaseFilePath))
        {
            InvalidOperationException ex = new("Db context has not been initialized.");
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw ex;
        }
    }
}
