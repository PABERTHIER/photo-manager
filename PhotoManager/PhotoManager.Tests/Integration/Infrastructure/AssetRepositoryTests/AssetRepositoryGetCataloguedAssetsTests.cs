﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetCataloguedAssetsTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
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
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
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
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            asset1!.Folder = new()
            {
                Path = folderPath1
            };
            asset2!.Folder = new()
            {
                Path = folderPath2
            };

            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());
            _assetRepository!.AddAsset(asset2!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();

            Assert.IsNotEmpty(assets);
            Assert.AreEqual(2, assets.Count);
            Assert.IsTrue(assets.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
            Assert.IsTrue(assets.FirstOrDefault(x => x.Hash == asset2.Hash)?.FileName == asset2.FileName);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetCataloguedAssets_NoAssetCatalogued_ReturnsEmptyList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<Asset> assets = _assetRepository!.GetCataloguedAssets();

            Assert.IsEmpty(assets);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetCataloguedAssets_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            asset1!.Folder = new()
            {
                Path = folderPath1
            };
            asset2!.Folder = new()
            {
                Path = folderPath2
            };

            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());
            _assetRepository!.AddAsset(asset2!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            List<Asset> assets1 = new();
            List<Asset> assets2 = new();
            List<Asset> assets3 = new();

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _assetRepository!.GetCataloguedAssets(),
                () => assets2 = _assetRepository!.GetCataloguedAssets(),
                () => assets3 = _assetRepository!.GetCataloguedAssets()
            );

            Assert.IsNotEmpty(assets1);
            Assert.AreEqual(2, assets1.Count);
            Assert.IsTrue(assets1.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
            Assert.IsTrue(assets1.FirstOrDefault(x => x.Hash == asset2.Hash)?.FileName == asset2.FileName);

            Assert.IsNotEmpty(assets2);
            Assert.AreEqual(2, assets2.Count);
            Assert.IsTrue(assets2.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
            Assert.IsTrue(assets2.FirstOrDefault(x => x.Hash == asset2.Hash)?.FileName == asset2.FileName);

            Assert.IsNotEmpty(assets3);
            Assert.AreEqual(2, assets3.Count);
            Assert.IsTrue(assets3.FirstOrDefault(x => x.Hash == asset1.Hash)?.FileName == asset1.FileName);
            Assert.IsTrue(assets3.FirstOrDefault(x => x.Hash == asset2.Hash)?.FileName == asset2.FileName);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
