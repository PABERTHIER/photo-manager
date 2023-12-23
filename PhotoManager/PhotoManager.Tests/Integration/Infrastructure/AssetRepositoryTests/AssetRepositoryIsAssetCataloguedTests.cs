using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryIsAssetCataloguedTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;

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

        asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 09),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    [Test]
    public void IsAssetCatalogued_FolderAndAssetExist_ReturnsTrue()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            _assetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            bool isAssetCatalogued1 = _assetRepository!.IsAssetCatalogued(folderPath1, asset1.FileName);
            bool isAssetCatalogued2 = _assetRepository!.IsAssetCatalogued(folderPath2, "toto.jpg");

            Assert.IsTrue(isAssetCatalogued1);
            Assert.IsFalse(isAssetCatalogued2);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void IsAssetCatalogued_DirectoryWithNoFolder_ReturnsFalse()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            bool isAssetCatalogued = _assetRepository.IsAssetCatalogued(folderPath2, "toto.jpg");

            Assert.IsFalse(isAssetCatalogued);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void IsAssetCatalogued_DirectoryNameIsNull_ReturnsFalse()
    {
        try
        {
            string? folderPath = null;

            bool isAssetCatalogued = _assetRepository!.IsAssetCatalogued(folderPath!, "toto.jpg");

            Assert.IsFalse(isAssetCatalogued);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void IsAssetCatalogued_FileNameIsNull_ReturnsFalse()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            string? fileName = null;

            bool isAssetCatalogued = _assetRepository.IsAssetCatalogued(folderPath1, fileName!);

            Assert.IsFalse(isAssetCatalogued);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void IsAssetCatalogued_ConcurrentAccess_AssetsAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            _assetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            bool isAssetCatalogued1 = false;
            bool isAssetCatalogued2 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => isAssetCatalogued1 = _assetRepository!.IsAssetCatalogued(folderPath1, asset1.FileName),
                () => isAssetCatalogued2 = _assetRepository!.IsAssetCatalogued(folderPath2, "toto.jpg")
            );

            Assert.IsTrue(isAssetCatalogued1);
            Assert.IsFalse(isAssetCatalogued2);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
