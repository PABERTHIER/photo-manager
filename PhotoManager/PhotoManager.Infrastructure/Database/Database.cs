namespace PhotoManager.Infrastructure.Database;

public class Database : IDatabase
{
    private const string DATA_FILE_FORMAT = "{0}.db";

    public string DataDirectory { get; private set; }
    public char Separator { get; private set; }
    public Diagnostics Diagnostics { get; private set; }
    protected string TablesDirectory { get; private set; }
    protected string BlobsDirectory { get; private set; }
    protected string BackupsDirectory { get; private set; }
    protected Dictionary<string, DataTableProperties> DataTablePropertiesDictionary { get; private set; }

    private readonly IObjectListStorage _objectListStorage;
    private readonly IBlobStorage _blobStorage;
    private readonly IBackupStorage _backupStorage;

    public Database(IObjectListStorage objectListStorage, IBlobStorage blobStorage, IBackupStorage backupStorage)
    {
        _objectListStorage = objectListStorage;
        _blobStorage = blobStorage;
        _backupStorage = backupStorage;
        DataDirectory = string.Empty;
        TablesDirectory = string.Empty;
        BlobsDirectory = string.Empty;
        BackupsDirectory = string.Empty;
        Diagnostics = new Diagnostics();
        DataTablePropertiesDictionary = new();
    }

    public void Initialize(string dataDirectory, char separator, string tablesFolderName, string blobsFolderName)
    {
        DataDirectory = dataDirectory;
        TablesDirectory = GetTablesDirectory(tablesFolderName);
        BlobsDirectory = GetBlobsDirectory(blobsFolderName);
        BackupsDirectory = GetBackupsDirectory();
        Separator = separator;
        DataTablePropertiesDictionary = new Dictionary<string, DataTableProperties>();
        InitializeDirectory();
    }

    public void SetDataTableProperties(DataTableProperties dataTableProperties)
    {
        if (dataTableProperties == null)
        {
            throw new ArgumentNullException(nameof(dataTableProperties));
        }

        if (dataTableProperties.ColumnProperties == null || dataTableProperties.ColumnProperties.Length == 0)
        {
            throw new ArgumentException("Column properties must not be empty.");
        }

        if (dataTableProperties.ColumnProperties.Any(c => string.IsNullOrWhiteSpace(c.ColumnName)))
        {
            throw new ArgumentNullException(nameof(ColumnProperties.ColumnName), "All column properties should have a ColumnName");
        }

        IGrouping<string, ColumnProperties>? group = dataTableProperties.ColumnProperties.GroupBy(c => c.ColumnName).Where(g => g.Count() > 1).FirstOrDefault();

        if (group != null)
        {
            throw new ArgumentException("Duplicated column properties.", group.Key);
        }

        DataTablePropertiesDictionary[dataTableProperties.TableName] = dataTableProperties;
    }

    public List<T> ReadObjectList<T>(string tableName, Func<string[], T> mapObjectFromCsvFields)
    {
        try
        {
            string dataFilePath = ResolveTableFilePath(tableName);
            Diagnostics = new Diagnostics { LastReadFilePath = dataFilePath };
            DataTableProperties? properties = GetDataTableProperties(tableName);
            _objectListStorage.Initialize(properties, Separator);
            return _objectListStorage.ReadObjectList(dataFilePath, mapObjectFromCsvFields, Diagnostics);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error while trying to read data table {tableName}.\n" +
                $"DataDirectory: {DataDirectory}\n" +
                $"Separator: {Separator}\n" +
                $"LastReadFilePath: {Diagnostics.LastReadFilePath}\n" +
                $"LastReadFileRaw: {Diagnostics.LastReadFileRaw}",
                ex);
        }
    }

    public void WriteObjectList<T>(List<T> list, string tableName, Func<T, int, object> mapCsvFieldIndexToCsvField)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName));
        }

        string dataFilePath = ResolveTableFilePath(tableName);
        Diagnostics = new Diagnostics { LastWriteFilePath = dataFilePath };
        DataTableProperties? properties = GetDataTableProperties(tableName);
        _objectListStorage.Initialize(properties, Separator);
        _objectListStorage.WriteObjectList(dataFilePath, list, mapCsvFieldIndexToCsvField, Diagnostics);
    }

    public Dictionary<string, byte[]>? ReadBlob(string blobName) // Key is imageName (string), value is the binary file -> image data (byte[])
    {
        string blobFilePath = ResolveBlobFilePath(blobName);
        Diagnostics = new Diagnostics { LastReadFilePath = blobFilePath };
        return _blobStorage.ReadFromBinaryFile(blobFilePath);
    }

    public void WriteBlob(Dictionary<string, byte[]> blob, string blobName) // One Blob per folder, Key is imageName (string), value is the binary file -> image data (byte[])
    {
        string blobFilePath = ResolveBlobFilePath(blobName);
        Diagnostics = new Diagnostics { LastWriteFilePath = blobFilePath, LastWriteFileRaw = blob };
        _blobStorage.WriteToBinaryFile(blob, blobFilePath);
    }

    // TODO: This method verifies if the folder has its .bin generated, but not if there are data in it or not
    public bool FolderHasThumbnails(string blobName) // FolderId + ".bin"
    {
        string blobFilePath = ResolveBlobFilePath(blobName);
        return File.Exists(blobFilePath);
    }

    public void DeleteThumbnails(string blobName) // FolderId + ".bin"
    {
        string thumbnailsFilePath = ResolveBlobFilePath(blobName);
        File.Delete(thumbnailsFilePath);
    }

    public bool WriteBackup(DateTime backupDate)
    {
        string backupFilePath = ResolveBackupFilePath(backupDate);

        if (BackupExists(backupDate))
        {
            File.Delete(backupFilePath);
        }

        Diagnostics = new Diagnostics { LastWriteFilePath = backupFilePath };
        _backupStorage.WriteFolderToZipFile(DataDirectory, backupFilePath);

        return true;
    }

    public bool BackupExists(DateTime backupDate)
    {
        string backupFilePath = ResolveBackupFilePath(backupDate);
        return File.Exists(backupFilePath);
    }

    public void DeleteOldBackups(ushort backupsToKeep)
    {
        string[] filesPaths = _backupStorage.GetBackupFilesPaths(BackupsDirectory);
        filesPaths = filesPaths.OrderBy(f => f).ToArray();
        List<string> deletedBackupFilePaths = new();

        for (int i = 0; i < filesPaths.Length - backupsToKeep; i++)
        {
            _backupStorage.DeleteBackupFile(filesPaths[i]);
            deletedBackupFilePaths.Add(filesPaths[i]);
        }

        Diagnostics = new Diagnostics { LastDeletedBackupFilePaths = deletedBackupFilePaths.ToArray() };
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
        return DataTablePropertiesDictionary.ContainsKey(tableName) ?
            DataTablePropertiesDictionary[tableName] : null;
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
