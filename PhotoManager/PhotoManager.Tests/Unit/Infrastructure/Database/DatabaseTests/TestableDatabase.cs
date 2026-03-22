using Microsoft.Extensions.Logging;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

public class TestableDatabase(
    IObjectListStorage objectListStorage,
    IBlobStorage blobStorage,
    IBackupStorage backupStorage,
    ILogger<PhotoManager.Infrastructure.Database.Database> logger)
    : PhotoManager.Infrastructure.Database.Database(objectListStorage, blobStorage, backupStorage, logger)
{
    public string GetTablesDirectory() => TablesDirectory;
    public string GetBlobsDirectory() => BlobsDirectory;
    public string GetBackupsDirectory() => BackupsDirectory;
    public Dictionary<string, DataTableProperties> GetDataTablePropertiesDictionary() => DataTablePropertiesDictionary;
}
