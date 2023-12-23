using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryFolderExistsTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void FolderExists_FolderExists_ReturnsTrue()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath);

            bool folderExists = _assetRepository!.FolderExists(folderPath);

            Assert.IsTrue(folderExists);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void FolderExists_FolderDoesNotExist_ReturnsFalse()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            _assetRepository!.AddFolder(folderPath1);

            bool folderExists = _assetRepository!.FolderExists(folderPath2);

            Assert.IsFalse(folderExists);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void FolderExists_ConcurrentAccess_FoldersAreHandledSafely()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath);

            bool folderExists1 = false;
            bool folderExists2 = false;
            bool folderExists3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => folderExists1 = _assetRepository!.FolderExists(folderPath),
                () => folderExists2 = _assetRepository!.FolderExists(folderPath),
                () => folderExists3 = _assetRepository!.FolderExists(folderPath)
            );

            Assert.IsTrue(folderExists1);
            Assert.IsTrue(folderExists2);
            Assert.IsTrue(folderExists3);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
