using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetAssetsCounterTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;

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
        asset2 = new()
        {
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 10),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
            FileName = "Image_11.heic",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
    }

    [Test]
    public void GetAssetsCounter_AssetsExist_ReturnsNumberOfAssets()
    {
        try
        {
            int assetsCounter;

            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _assetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            asset2!.Folder = folder;
            asset2!.FolderId = folder.FolderId;
            asset3!.Folder = folder;
            asset3!.FolderId = folder.FolderId;

            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(0, assetsCounter);

            _assetRepository.AddAsset(asset1!, Array.Empty<byte>());
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(1, assetsCounter);

            _assetRepository.AddAsset(asset2!, Array.Empty<byte>());
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(2, assetsCounter);

            _assetRepository.AddAsset(asset3!, Array.Empty<byte>());
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(3, assetsCounter);

            _assetRepository.DeleteAsset(folderPath, asset3.FileName);

            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(2, assetsCounter);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsCounter_NoAsset_Returns0()
    {
        try
        {
            int assetsCounter = _assetRepository!.GetAssetsCounter();

            Assert.AreEqual(0, assetsCounter);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsCounter_ConcurrentAccess_AssetsAreHandledSafely()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _assetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            asset2!.Folder = folder;
            asset2!.FolderId = folder.FolderId;
            asset3!.Folder = folder;
            asset3!.FolderId = folder.FolderId;

            _assetRepository.AddAsset(asset1!, Array.Empty<byte>());
            _assetRepository.AddAsset(asset2!, Array.Empty<byte>());
            _assetRepository.AddAsset(asset3!, Array.Empty<byte>());

            int assetsCounter1 = 0;
            int assetsCounter2 = 0;
            int assetsCounter3 = 0;

            // Simulate concurrent access
            Parallel.Invoke(
                () => assetsCounter1 = _assetRepository.GetAssetsCounter(),
                () => assetsCounter2 = _assetRepository.GetAssetsCounter(),
                () => assetsCounter3 = _assetRepository.GetAssetsCounter()
            );

            Assert.AreEqual(3, assetsCounter1);
            Assert.AreEqual(3, assetsCounter2);
            Assert.AreEqual(3, assetsCounter3);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
