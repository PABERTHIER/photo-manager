using Microsoft.Extensions.Logging;
using PhotoManager.Persistence.Repositories;

namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// SQLite-backed implementation of <see cref="IPersistenceContext"/>.
/// Owns the DB file and exposes typed primitive repositories.
/// Thread-safe: every primitive opens its own connection per call.
/// </summary>
public sealed class SqlitePersistenceContext(ILogger<SqlitePersistenceContext> logger)
    : IPersistenceContext, IDisposable
{
    public const string DATABASE_FILE_NAME = "photomanager.db";
    private const string BACKUPS_SUFFIX = "_Backups";

    private string _backupsDirectory = string.Empty;

    private SqliteConnectionFactory? _factory; // TODO: Why not using interface to inject it properly ?
    private SqliteBackupService? _backupService; // TODO: Why not using interface to inject it properly ?

    public IFolderPersistence Folders { get; private set; } = null!;
    public IAssetPersistence Assets { get; private set; } = null!;
    public IThumbnailPersistence Thumbnails { get; private set; } = null!;
    public IRecentPathsPersistence RecentPaths { get; private set; } = null!;
    public ISyncDefinitionsPersistence SyncDefinitions { get; private set; } = null!;

    public string DatabaseFilePath { get; private set; } = string.Empty;
    public PersistenceDiagnostics Diagnostics { get; private set; } = new();

    public void Initialize(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            ArgumentException ex = new("dataDirectory must not be empty.", nameof(dataDirectory));
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw ex;
        }

        Directory.CreateDirectory(dataDirectory);
        DatabaseFilePath = Path.Combine(dataDirectory, DATABASE_FILE_NAME);
        _backupsDirectory = dataDirectory + BACKUPS_SUFFIX;
        Directory.CreateDirectory(_backupsDirectory);

        // TODO: DatabaseFilePath can be init in SqliteConnectionFactory and used as a public field instead
        // To add to that, DatabasePath in the SqliteConnectionFactory is already public, so easy to do.
        _factory = new SqliteConnectionFactory(DatabaseFilePath);
        _backupService = new SqliteBackupService(_factory);

        try
        {
            using (SqliteConnection connection = _factory.Open())
            {
                SqliteSchema.EnsureCreated(connection);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize SQLite persistence at {DatabaseFilePath}", DatabaseFilePath);
            throw;
        }

        Folders = new FolderPersistence(_factory);
        Assets = new AssetPersistence(_factory);
        Thumbnails = new ThumbnailPersistence(_factory);
        RecentPaths = new RecentPathsPersistence(_factory);
        SyncDefinitions = new SyncDefinitionsPersistence(_factory);

        Diagnostics = new PersistenceDiagnostics
        {
            LastDatabasePath = DatabaseFilePath,
            LastOperation = "Initialize"
        };
    }

    public bool WriteBackup(DateTime backupDate)
    {
        EnsureInitialized();

        string backupFilePath = ResolveBackupFilePath(backupDate);

        Diagnostics = new PersistenceDiagnostics
        {
            LastDatabasePath = DatabaseFilePath,
            LastBackupPath = backupFilePath,
            LastOperation = "WriteBackup"
        };

        try
        {
            return _backupService!.WriteBackup(backupFilePath);
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
            string[] filesPaths = _backupService!.GetBackupFilesPaths(_backupsDirectory);
            // TODO: Why do we need to sort ?
            Array.Sort(filesPaths, StringComparer.Ordinal);

            List<string> deletedBackupFilePaths = [];

            for (int i = 0; i < filesPaths.Length - backupsToKeep; i++)
            {
                _backupService.DeleteBackupFile(filesPaths[i]);
                deletedBackupFilePaths.Add(filesPaths[i]);
            }

            Diagnostics = new PersistenceDiagnostics
            {
                LastDatabasePath = DatabaseFilePath,
                LastOperation = "DeleteOldBackups",
                LastDeletedBackupFilePaths = [.. deletedBackupFilePaths]
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting old backups (keep {BackupsToKeep})", backupsToKeep);
            throw;
        }
    }

    private string ResolveBackupFilePath(DateTime backupDate)
    {
        string fileName = backupDate.ToString("yyyyMMdd") + ".zip";
        return Path.Combine(_backupsDirectory, fileName);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
    }

    // TODO: By using interface to inject them properly, then they cannot be null, this code is useless ?
    private void EnsureInitialized()
    {
        if (_factory == null || _backupService == null)
        {
            InvalidOperationException ex = new("SqlitePersistenceContext has not been initialized.");
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw ex;
        }
    }
}
