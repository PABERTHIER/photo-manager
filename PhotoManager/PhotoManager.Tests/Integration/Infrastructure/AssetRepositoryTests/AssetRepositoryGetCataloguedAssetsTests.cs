using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetCataloguedAssetsTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset2 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 6000, Height = 6120 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
    }

    [Test]
    public void GetCataloguedAssets_AssetsCatalogued_ReturnsEmptyList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            _asset1!.Folder = new()
            {
                Path = folderPath1
            };
            _asset2!.Folder = new()
            {
                Path = folderPath2
            };

            _assetRepository!.AddAsset(_asset1!, []);
            _assetRepository!.AddAsset(_asset2!, []);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();

            Assert.IsNotEmpty(assets);
            Assert.AreEqual(2, assets.Count);
            Assert.IsTrue(assets.FirstOrDefault(x => x.Hash == _asset1.Hash)?.FileName == _asset1.FileName);
            Assert.IsTrue(assets.FirstOrDefault(x => x.Hash == _asset2.Hash)?.FileName == _asset2.FileName);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetCataloguedAssets_NoAssetCatalogued_ReturnsEmptyList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<Asset> assets = _assetRepository!.GetCataloguedAssets();

            Assert.IsEmpty(assets);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetCataloguedAssets_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            _asset1!.Folder = new()
            {
                Path = folderPath1
            };
            _asset2!.Folder = new()
            {
                Path = folderPath2
            };

            _assetRepository!.AddAsset(_asset1!, []);
            _assetRepository!.AddAsset(_asset2!, []);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            List<Asset> assets1 = [];
            List<Asset> assets2 = [];
            List<Asset> assets3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _assetRepository!.GetCataloguedAssets(),
                () => assets2 = _assetRepository!.GetCataloguedAssets(),
                () => assets3 = _assetRepository!.GetCataloguedAssets()
            );

            Assert.IsNotEmpty(assets1);
            Assert.AreEqual(2, assets1.Count);
            Assert.IsTrue(assets1.FirstOrDefault(x => x.Hash == _asset1.Hash)?.FileName == _asset1.FileName);
            Assert.IsTrue(assets1.FirstOrDefault(x => x.Hash == _asset2.Hash)?.FileName == _asset2.FileName);

            Assert.IsNotEmpty(assets2);
            Assert.AreEqual(2, assets2.Count);
            Assert.IsTrue(assets2.FirstOrDefault(x => x.Hash == _asset1.Hash)?.FileName == _asset1.FileName);
            Assert.IsTrue(assets2.FirstOrDefault(x => x.Hash == _asset2.Hash)?.FileName == _asset2.FileName);

            Assert.IsNotEmpty(assets3);
            Assert.AreEqual(2, assets3.Count);
            Assert.IsTrue(assets3.FirstOrDefault(x => x.Hash == _asset1.Hash)?.FileName == _asset1.FileName);
            Assert.IsTrue(assets3.FirstOrDefault(x => x.Hash == _asset2.Hash)?.FileName == _asset2.FileName);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
