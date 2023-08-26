using PhotoManager.Infrastructure.Database.Storage;
using System.Data;
using System.Globalization;
using System.IO;

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
    //private readonly IDataTableStorage _dataTableStorage;
    private readonly IBlobStorage _blobStorage;
    private readonly IBackupStorage _backupStorage;

    public Database(IObjectListStorage objectListStorage, IBlobStorage blobStorage, IBackupStorage backupStorage)
    {
        _objectListStorage = objectListStorage;
        //_dataTableStorage = dataTableStorage;
        _blobStorage = blobStorage;
        _backupStorage = backupStorage;
    }

    public void Initialize(string dataDirectory, char separator)
    {
        DataDirectory = dataDirectory;
        TablesDirectory = GetTablesDirectory(dataDirectory);
        BlobsDirectory = GetBlobsDirectory(dataDirectory);
        BackupsDirectory = GetBackupsDirectory(dataDirectory);
        Separator = separator;
        DataTablePropertiesDictionary = new Dictionary<string, DataTableProperties>();
        InitializeDirectory(dataDirectory);
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
            throw new ArgumentException("All column properties should have a ColumnName", nameof(ColumnProperties.ColumnName));
        }

        var group = dataTableProperties.ColumnProperties.GroupBy(c => c.ColumnName).Where(g => g.Count() > 1).FirstOrDefault();
        
        if (group != null)
        {
            throw new ArgumentException("Duplicated column properties.", group.Key);
        }

        DataTablePropertiesDictionary[dataTableProperties.TableName] = dataTableProperties;
    }

    public List<T> ReadObjectList<T>(string tableName, Func<string[], T> mapObjectFromCsvFields)
    {
        string dataFilePath = ResolveTableFilePath(DataDirectory, tableName);
        Diagnostics = new Diagnostics { LastReadFilePath = dataFilePath };
        DataTableProperties? properties = GetDataTableProperties(tableName);
        _objectListStorage.Initialize(properties, Separator);
        return _objectListStorage.ReadObjectList(dataFilePath, mapObjectFromCsvFields, Diagnostics);
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

        string dataFilePath = ResolveTableFilePath(DataDirectory, tableName);
        Diagnostics = new Diagnostics { LastWriteFilePath = dataFilePath };
        DataTableProperties? properties = GetDataTableProperties(tableName);
        _objectListStorage.Initialize(properties, Separator);
        _objectListStorage.WriteObjectList(dataFilePath, list, mapCsvFieldIndexToCsvField, Diagnostics);
    }

    public Dictionary<string, byte[]>? ReadBlob(string blobName) // Key is imageName (string), value is the binary file -> image data (byte[])
    {
        string blobFilePath = ResolveBlobFilePath(DataDirectory, blobName);
        Diagnostics = new Diagnostics { LastReadFilePath = blobFilePath };
        return _blobStorage.ReadFromBinaryFile(blobFilePath);
    }

    public void WriteBlob(Dictionary<string, byte[]> blob, string blobName) // One Blob per folder, Key is imageName (string), value is the binary file -> image data (byte[])
    {
        string blobFilePath = ResolveBlobFilePath(DataDirectory, blobName);
        Diagnostics = new Diagnostics { LastWriteFilePath = blobFilePath, LastWriteFileRaw = blob };
        _blobStorage.WriteToBinaryFile(blob, blobFilePath);
    }

    // This method verifies if the folder has its .bin generated, but not if there are data in it or not
    public bool FolderHasThumbnails(string blobName) // FolderId + ".bin"
    {
        var blobFilePath = ResolveBlobFilePath(DataDirectory, blobName);
        return File.Exists(blobFilePath);
    }

    public void DeleteThumbnails(string blobName) // FolderId + ".bin"
    {
        string thumbnailsFilePath = ResolveBlobFilePath(DataDirectory, blobName);
        File.Delete(thumbnailsFilePath);
    }

    public bool WriteBackup(DateTime backupDate)
    {
        bool written = false;
        string backupFilePath = ResolveBackupFilePath(DataDirectory, backupDate);

        if (!BackupExists(backupDate))
        {
            Diagnostics = new Diagnostics { LastWriteFilePath = backupFilePath };
            _backupStorage.WriteFolderToZipFile(DataDirectory, backupFilePath);
            written = true;
        }

        return written;
    }

    public bool BackupExists(DateTime backupDate)
    {
        string backupFilePath = ResolveBackupFilePath(DataDirectory, backupDate);
        return File.Exists(backupFilePath);
    }

    public void DeleteOldBackups(int backupsToKeep)
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

    private void InitializeDirectory(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(TablesDirectory);
        Directory.CreateDirectory(BlobsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }

    private DataTableProperties? GetDataTableProperties(string tableName)
    {
        return DataTablePropertiesDictionary.ContainsKey(tableName) ?
            DataTablePropertiesDictionary[tableName] : null;
    }

    private static string GetTablesDirectory(string dataDirectory)
    {
        return Path.Combine(dataDirectory, "Tables");
    }

    private static string GetBlobsDirectory(string dataDirectory)
    {
        return Path.Combine(dataDirectory, "Blobs");
    }

    private static string GetBackupsDirectory(string dataDirectory)
    {
        return dataDirectory + "_Backups";
    }

    private static string ResolveTableFilePath(string dataDirectory, string entityName)
    {
        dataDirectory = !string.IsNullOrEmpty(dataDirectory) ? dataDirectory : string.Empty;
        string fileName = string.Format(DATA_FILE_FORMAT, entityName).ToLower();
        return Path.Combine(GetTablesDirectory(dataDirectory), fileName);
    }

    private static string ResolveBackupFilePath(string dataDirectory, DateTime backupDate)
    {
        dataDirectory = !string.IsNullOrEmpty(dataDirectory) ? dataDirectory : string.Empty;
        string fileName = backupDate.ToString("yyyyMMdd") + ".zip";
        return Path.Combine(GetBackupsDirectory(dataDirectory), fileName);
    }

    // TODO: Once Todo in AssetRepository done (implement the deletion here), pass this method in private
    public string ResolveBlobFilePath(string dataDirectory, string blobName)
    {
        return Path.Combine(GetBlobsDirectory(dataDirectory), blobName);
    }

    //public DataTable ReadDataTable(string tableName)
    //{
    //    string dataFilePath = ResolveTableFilePath(DataDirectory, tableName);
    //    Diagnostics = new Diagnostics { LastReadFilePath = dataFilePath };
    //    DataTableProperties? properties = GetDataTableProperties(tableName);
    //    _dataTableStorage.Initialize(properties, Separator);
    //    return _dataTableStorage.ReadDataTable(dataFilePath, tableName, Diagnostics);
    //}

    //public void WriteDataTable(DataTable dataTable)
    //{
    //    if (dataTable == null)
    //        throw new ArgumentNullException(nameof(dataTable));

    //    if (dataTable.Columns.Count == 0)
    //        throw new ArgumentException("DataTable should have at least one column.", nameof(dataTable));

    //    for (int i = 0; i < dataTable.Columns.Count; i++)
    //    {
    //        if (string.IsNullOrWhiteSpace(dataTable.Columns[i].ColumnName))
    //            throw new ArgumentException("All columns should have a name.", nameof(dataTable));
    //    }

    //    string dataFilePath = ResolveTableFilePath(DataDirectory, dataTable.TableName);
    //    Diagnostics = new Diagnostics { LastWriteFilePath = dataFilePath };
    //    DataTableProperties? properties = GetDataTableProperties(dataTable.TableName);
    //    _dataTableStorage.Initialize(properties, Separator);
    //    _dataTableStorage.WriteDataTable(dataFilePath, dataTable, Diagnostics);
    //}

    //public DateTime[] GetBackupDates()
    //{
    //    string[] filesPaths = _backupStorage.GetBackupFilesPaths(BackupsDirectory);

    //    return filesPaths
    //        .Select(f => Path.GetFileName(f))
    //        .Select(f => DateTime.ParseExact(f.Replace(".zip", ""), "yyyyMMdd", CultureInfo.InvariantCulture))
    //        .OrderBy(f => f)
    //        .ToArray();
    //}
}
