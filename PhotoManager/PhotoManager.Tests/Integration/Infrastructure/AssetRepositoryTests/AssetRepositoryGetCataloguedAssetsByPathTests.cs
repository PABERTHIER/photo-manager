using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetCataloguedAssetsByPathTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    private Asset? asset1;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRoot = new Mock<IConfigurationRoot>();
        _configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _assetRepository = new AssetRepository(database, _storageService!.Object, userConfigurationService);

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
    public void GetCataloguedAssetsByPath_FolderAndAssetExist_ReturnsMatchingAssets()
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

            List<Asset> cataloguedAssets1 = _assetRepository.GetCataloguedAssetsByPath(folderPath1);
            List<Asset> cataloguedAssets2 = _assetRepository.GetCataloguedAssetsByPath(folderPath2);

            Assert.AreEqual(1, cataloguedAssets1.Count);
            Assert.IsEmpty(cataloguedAssets2);

            Assert.IsTrue(cataloguedAssets1.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetCataloguedAssetsByPath_DirectoryWithNoFolder_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssetsByPath(folderPath2);

            Assert.IsEmpty(cataloguedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetCataloguedAssetsByPath_DirectoryIsNull_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string? folderPath2 = null;

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssetsByPath(folderPath2!);

            Assert.IsEmpty(cataloguedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetCataloguedAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
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

            List<Asset> cataloguedAssets1 = new ();
            List<Asset> cataloguedAssets2 = new();

            // Simulate concurrent access
            Parallel.Invoke(
                () => cataloguedAssets1 = _assetRepository.GetCataloguedAssetsByPath(folderPath1),
                () => cataloguedAssets2 = _assetRepository.GetCataloguedAssetsByPath(folderPath2)
            );

            Assert.AreEqual(1, cataloguedAssets1.Count);
            Assert.IsEmpty(cataloguedAssets2);

            Assert.IsTrue(cataloguedAssets1.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
