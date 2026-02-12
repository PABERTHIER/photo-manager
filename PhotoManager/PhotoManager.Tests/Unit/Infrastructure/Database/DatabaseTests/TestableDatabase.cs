namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

public class TestableDatabase(
    IObjectListStorage objectListStorage,
    IBlobStorage blobStorage,
    IBackupStorage backupStorage)
    : PhotoManager.Infrastructure.Database.Database(objectListStorage, blobStorage, backupStorage)
{
    public string GetTablesDirectory() => TablesDirectory;
    public string GetBlobsDirectory() => BlobsDirectory;
    public string GetBackupsDirectory() => BackupsDirectory;
    public Dictionary<string, DataTableProperties> GetDataTablePropertiesDictionary() => DataTablePropertiesDictionary;
}
