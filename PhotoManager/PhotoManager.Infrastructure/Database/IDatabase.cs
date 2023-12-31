namespace PhotoManager.Infrastructure.Database;

public interface IDatabase
{
    string DataDirectory { get; }
    char Separator { get; }
    Diagnostics Diagnostics { get; }
    void Initialize(string dataDirectory, char separator, string tablesFolderName, string blobsFolderName);
    void SetDataTableProperties(DataTableProperties dataTableProperties);
    List<T> ReadObjectList<T>(string tableName, Func<string[], T> mapObjectFromCsvFields);
    void WriteObjectList<T>(List<T> list, string tableName, Func<T, int, object> mapCsvFieldIndexToCsvField);
    Dictionary<string, byte[]>? ReadBlob(string blobName);
    void WriteBlob(Dictionary<string, byte[]> blob, string blobName);
    bool FolderHasThumbnails(string blobName);
    void DeleteThumbnails(string blobName);
    bool WriteBackup(DateTime backupDate);
    bool BackupExists(DateTime backupDate);
    void DeleteOldBackups(ushort backupsToKeep);
}
