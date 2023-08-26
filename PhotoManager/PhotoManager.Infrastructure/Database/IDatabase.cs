namespace PhotoManager.Infrastructure.Database;

public interface IDatabase
{
    string DataDirectory { get; }
    char Separator { get; }
    Diagnostics Diagnostics { get; }
    void Initialize(string dataDirectory, char separator);
    void SetDataTableProperties(DataTableProperties dataTableProperties);
    //DataTable ReadDataTable(string tableName);
    //void WriteDataTable(DataTable dataTable);
    List<T> ReadObjectList<T>(string tableName, Func<string[], T> mapObjectFromCsvFields);
    void WriteObjectList<T>(List<T> list, string tableName, Func<T, int, object> mapCsvFieldIndexToCsvField);
    Dictionary<string, byte[]>? ReadBlob(string blobName);
    string ResolveBlobFilePath(string dataDirectory, string thumbnailsFileName);
    void WriteBlob(Dictionary<string, byte[]> blob, string blobName);
    bool FolderHasThumbnails(string blobName);
    void DeleteThumbnails(string blobName);
    bool WriteBackup(DateTime backupDate);
    bool BackupExists(DateTime backupDate);
    //DateTime[] GetBackupDates();
    void DeleteOldBackups(int backupsToKeep);
}
