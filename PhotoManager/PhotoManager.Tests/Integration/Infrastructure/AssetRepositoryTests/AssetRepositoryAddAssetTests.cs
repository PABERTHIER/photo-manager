using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddAssetTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

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
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);

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
    public void AddAsset_FolderAndThumbnailsExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Path = folderPath };

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder2;
            byte[] assetData2 = Array.Empty<byte>();

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(asset2!, assetData2);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);
            Assert.AreEqual(asset2!.FileName, assets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][asset2.FileName]);

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
    public void AddAsset_FolderDoesNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder1 = new() { Path = folderPath };
            Folder folder2 = new() { Path = folderPath };

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder2;
            byte[] assetData2 = Array.Empty<byte>();

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(asset2!, assetData2);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);
            Assert.AreEqual(asset2!.FileName, assets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][asset2.FileName]);

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
    public void AddAsset_ThumbnailDoesNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[] assetData = new byte[] { 1, 2, 3 };

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailDoesNotExistButBinExists_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[] assetData = new byte[] { 1, 2, 3 };

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { asset2!.FileName, Array.Empty<byte>() }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][asset2.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailsDictionaryEntriesToKeepIs0_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, _storageServiceMock!.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = testableAssetRepository.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[] assetData = new byte[] { 1, 2, 3 };

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(asset1!, assetData);

            Assert.IsTrue(testableAssetRepository.HasChanges()); // Due to AddFolder()
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderAndThumbnailsDoNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = new() { Path = folderPath };
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[] assetData = new byte[] { 1, 2, 3 };

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetFolderIsNull_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = new byte[] { 1, 2, 3 };

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData);

            Assert.IsFalse(_testableAssetRepository.HasChanges());
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetIsNull_ThrowsNullReferenceExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Asset? asset = null;
            byte[] assetData = new byte[] { 1, 2, 3 };

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _testableAssetRepository.AddAsset(asset!, assetData));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
            Assert.IsFalse(_testableAssetRepository.HasChanges());
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailDataIsNull_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[]? assetData = null;

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData!);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(null, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ConcurrentAccess_AssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Path = folderPath };

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder2;
            byte[] assetData2 = Array.Empty<byte>();

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository.AddAsset(asset1!, assetData1),
                () => _testableAssetRepository.AddAsset(asset2!, assetData2)
            );

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.IsTrue(assets.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assets.Any(x => x.FileName == asset2.FileName));

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][asset2.FileName]);

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
