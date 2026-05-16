using Microsoft.Extensions.Logging;
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

    public string DatabaseFilePath => factory.DatabasePath;

    public void Initialize(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            ArgumentException ex = new("dataDirectory must not be empty.", nameof(dataDirectory));
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw ex;
        }

        Directory.CreateDirectory(dataDirectory);

        string databaseFilePath = Path.Combine(dataDirectory, DATABASE_FILE_NAME);
        _backupsDirectory = Path.Combine(dataDirectory, BACKUPS_SUFFIX);
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

    private string ResolveBackupFilePath(DateTime backupDate)
    {
        string fileName = $"{backupDate:yyyyMMdd}.zip";
        return Path.Combine(_backupsDirectory, fileName);
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
