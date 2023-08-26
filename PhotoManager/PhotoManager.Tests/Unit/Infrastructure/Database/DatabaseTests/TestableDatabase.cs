using PhotoManager.Infrastructure.Database.Storage;
using PhotoManager.Infrastructure.Database;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

public class TestableDatabase : PhotoManager.Infrastructure.Database.Database
{
    public string GetTablesDirectory() => TablesDirectory;
    public string GetBlobsDirectory() => BlobsDirectory;
    public string GetBackupsDirectory() => BackupsDirectory;
    public Dictionary<string, DataTableProperties> GetDataTablePropertiesDictionary() => DataTablePropertiesDictionary;

    public TestableDatabase(IObjectListStorage objectListStorage, IBlobStorage blobStorage, IBackupStorage backupStorage) : base(objectListStorage, blobStorage, backupStorage)
    {
    }
}
