using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure.Database;

public class Database(
    IObjectListStorage objectListStorage,
    IBlobStorage blobStorage,
    IBackupStorage backupStorage,
    ILogger<Database> logger)
    : IDatabase
{
    private const string DATA_FILE_FORMAT = "{0}.db";

    public string DataDirectory { get; private set; } = string.Empty;
    public char Separator { get; private set; }
    public Diagnostics Diagnostics { get; private set; } = new();
    protected string TablesDirectory { get; private set; } = string.Empty;
    protected string BlobsDirectory { get; private set; } = string.Empty;
    protected string BackupsDirectory { get; private set; } = string.Empty;
    protected Dictionary<string, DataTableProperties> DataTablePropertiesDictionary { get; private set; } = [];

    public void Initialize(string dataDirectory, char separator, string tablesFolderName, string blobsFolderName)
    {
        DataDirectory = dataDirectory;
        TablesDirectory = GetTablesDirectory(tablesFolderName);
        BlobsDirectory = GetBlobsDirectory(blobsFolderName);
        BackupsDirectory = GetBackupsDirectory();
        Separator = separator;
        DataTablePropertiesDictionary = [];
        InitializeDirectory();
    }

    public void SetDataTableProperties(DataTableProperties dataTableProperties)
    {
        ArgumentNullException.ThrowIfNull(dataTableProperties);

        if (dataTableProperties.ColumnProperties.Length == 0)
        {
            ArgumentException exception = new("Column properties must not be empty.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }

        if (dataTableProperties.ColumnProperties.Any(c => string.IsNullOrWhiteSpace(c.ColumnName)))
        {
            ArgumentNullException exception = new(nameof(ColumnProperties.ColumnName),
                "All column properties should have a ColumnName");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }

        IGrouping<string, ColumnProperties>? group = dataTableProperties.ColumnProperties.GroupBy(c => c.ColumnName)
            .FirstOrDefault(g => g.Count() > 1);

        if (group != null)
        {
            ArgumentException exception = new("Duplicated column properties.", group.Key);
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }

        DataTablePropertiesDictionary[dataTableProperties.TableName] = dataTableProperties;
    }

    public List<T> ReadObjectList<T>(string tableName, Func<string[], T> mapObjectFromCsvFields)
    {
        try
        {
            string dataFilePath = ResolveTableFilePath(tableName);
            Diagnostics = new() { LastReadFilePath = dataFilePath };
            DataTableProperties? properties = GetDataTableProperties(tableName);
            objectListStorage.Initialize(properties, Separator);
            return objectListStorage.ReadObjectList(dataFilePath, mapObjectFromCsvFields, Diagnostics);
        }
        catch (Exception ex)
        {
            ArgumentException exception = new($"Error while trying to read data table {tableName}.\n" +
                                              $"DataDirectory: {DataDirectory}\n" +
                                              $"Separator: {Separator}\n" +
                                              $"LastReadFilePath: {Diagnostics.LastReadFilePath}\n" +
                                              $"LastReadFileRaw: {Diagnostics.LastReadFileRaw}",
                ex);
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    public void WriteObjectList<T>(List<T> list, string tableName, Func<T, int, object> mapCsvFieldIndexToCsvField)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName));
        }

        try
        {
            string dataFilePath = ResolveTableFilePath(tableName);
            Diagnostics = new() { LastWriteFilePath = dataFilePath };
            DataTableProperties? properties = GetDataTableProperties(tableName);
            objectListStorage.Initialize(properties, Separator);
            objectListStorage.WriteObjectList(dataFilePath, list, mapCsvFieldIndexToCsvField, Diagnostics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error while trying to write data table {TableName}. DataDirectory: {DataDirectory}, Separator: {Separator}, LastWriteFilePath: {LastWriteFilePath}",
                tableName, DataDirectory, Separator, Diagnostics.LastWriteFilePath);
            throw;
        }
    }

    // Key is imageName (string), value is the binary file -> image data (byte[])
    public Dictionary<string, byte[]>? ReadBlob(string blobName)
    {
        try
        {
            string blobFilePath = ResolveBlobFilePath(blobName);
            Diagnostics = new() { LastReadFilePath = blobFilePath };
            return blobStorage.ReadFromBinaryFile(blobFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to read blob {BlobName}. LastReadFilePath: {LastReadFilePath}",
                blobName, Diagnostics.LastReadFilePath);
            throw;
        }
    }

    // One Blob per folder, Key is imageName (string), value is the binary file -> image data (byte[])
    public void WriteBlob(Dictionary<string, byte[]> blob, string blobName)
    {
        try
        {
            string blobFilePath = ResolveBlobFilePath(blobName);
            Diagnostics = new() { LastWriteFilePath = blobFilePath, LastWriteFileRaw = blob };
            blobStorage.WriteToBinaryFile(blob, blobFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to write blob {BlobName}. LastWriteFilePath: {LastWriteFilePath}",
                blobName, Diagnostics.LastWriteFilePath);
            throw;
        }
    }

    public bool IsBlobFileExists(string blobName) // Folder.Id + ".bin"
    {
        string blobFilePath = ResolveBlobFilePath(blobName);
        return File.Exists(blobFilePath);
    }

    public void DeleteBlobFile(string blobName) // Folder.Id + ".bin"
    {
        try
        {
            string blobFilePath = ResolveBlobFilePath(blobName);
            File.Delete(blobFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to delete blob {BlobName}.", blobName);
            throw;
        }
    }

    public bool WriteBackup(DateTime backupDate)
    {
        try
        {
            string backupFilePath = ResolveBackupFilePath(backupDate);

            if (BackupExists(backupDate))
            {
                File.Delete(backupFilePath);
            }

            Diagnostics = new() { LastWriteFilePath = backupFilePath };
            backupStorage.WriteFolderToZipFile(DataDirectory, backupFilePath);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error while trying to write backup for date {BackupDate:yyyyMMdd}. LastWriteFilePath: {LastWriteFilePath}",
                backupDate, Diagnostics.LastWriteFilePath);
            throw;
        }
    }

    public bool BackupExists(DateTime backupDate)
    {
        string backupFilePath = ResolveBackupFilePath(backupDate);
        return File.Exists(backupFilePath);
    }

    public void DeleteOldBackups(ushort backupsToKeep)
    {
        try
        {
            string[] filesPaths = backupStorage.GetBackupFilesPaths(BackupsDirectory);
            filesPaths = [.. filesPaths.OrderBy(f => f)];
            List<string> deletedBackupFilePaths = [];

            for (int i = 0; i < filesPaths.Length - backupsToKeep; i++)
            {
                backupStorage.DeleteBackupFile(filesPaths[i]);
                deletedBackupFilePaths.Add(filesPaths[i]);
            }

            Diagnostics = new() { LastDeletedBackupFilePaths = [.. deletedBackupFilePaths] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to delete old backups. Backups to keep: {BackupsToKeep}",
                backupsToKeep);
            throw;
        }
    }

    private void InitializeDirectory()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(TablesDirectory);
        Directory.CreateDirectory(BlobsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }

    private DataTableProperties? GetDataTableProperties(string tableName)
    {
        return DataTablePropertiesDictionary.GetValueOrDefault(tableName);
    }

    private string GetTablesDirectory(string tablesFolderName)
    {
        return Path.Combine(DataDirectory, tablesFolderName);
    }

    private string GetBlobsDirectory(string blobsFolderName)
    {
        return Path.Combine(DataDirectory, blobsFolderName);
    }

    private string GetBackupsDirectory()
    {
        return DataDirectory + "_Backups";
    }

    private string ResolveTableFilePath(string entityName)
    {
        string fileName = string.Format(DATA_FILE_FORMAT, entityName).ToLower();
        return Path.Combine(TablesDirectory, fileName);
    }

    private string ResolveBackupFilePath(DateTime backupDate)
    {
        string fileName = backupDate.ToString("yyyyMMdd") + ".zip";
        return Path.Combine(BackupsDirectory, fileName);
    }

    private string ResolveBlobFilePath(string blobName)
    {
        return Path.Combine(BlobsDirectory, blobName);
    }
}
