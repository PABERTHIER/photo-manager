using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetAssetsCounterTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);

        _asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset2 = new()
        {
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
        _asset3 = new()
        {
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
            FileName = "Image_11.heic",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
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
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _assetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;

            Assert.IsEmpty(assetsUpdatedEvents);

            int assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(0, assetsCounter);

            Assert.IsEmpty(assetsUpdatedEvents);

            _assetRepository.AddAsset(_asset1!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(1, assetsCounter);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.AddAsset(_asset2!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(2, assetsCounter);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            _assetRepository.AddAsset(_asset3!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(3, assetsCounter);

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);

            _assetRepository.DeleteAsset(folderPath, _asset3.FileName);

            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.AreEqual(2, assetsCounter);

            Assert.AreEqual(4, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[3]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsCounter_NoAsset_Returns0()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            int assetsCounter = _assetRepository!.GetAssetsCounter();

            Assert.AreEqual(0, assetsCounter);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsCounter_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _assetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;

            _assetRepository.AddAsset(_asset1!, []);
            _assetRepository.AddAsset(_asset2!, []);
            _assetRepository.AddAsset(_asset3!, []);

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);

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

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
