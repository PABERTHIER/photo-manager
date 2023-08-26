namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteThumbnailsTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void DeleteThumbnails_BlobExists_DeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, "Blobs", blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            _database!.WriteBlob(blobToWrite, blobName);

            Assert.IsTrue(File.Exists(blobFilePath));

            _database!.DeleteThumbnails(blobName);

            Assert.IsFalse(File.Exists(blobFilePath));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void DeleteThumbnails_BlobDoesNotExist_DoesNotDeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, "Blobs", blobName);

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            Assert.IsFalse(File.Exists(blobFilePath));

            _database!.DeleteThumbnails(blobName);

            Assert.IsFalse(File.Exists(blobFilePath));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
