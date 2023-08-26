using NUnit.Framework;
using PhotoManager.Constants;
using PhotoManager.Infrastructure.Database.Storage;
using System.IO;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseFolderHasThumbnailsTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        string directoryName = Path.GetDirectoryName(typeof(DatabaseFolderHasThumbnailsTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void FolderHasThumbnails_BlobExists_ReturnsTrue()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            _database!.WriteBlob(blobToWrite, blobName);

            bool folderHasThumbnails = _database!.FolderHasThumbnails(blobName);

            Assert.IsTrue(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void FolderHasThumbnails_BlobDoesNotExist_ReturnsFalse()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            bool folderHasThumbnails = _database!.FolderHasThumbnails(blobName);

            Assert.IsFalse(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
