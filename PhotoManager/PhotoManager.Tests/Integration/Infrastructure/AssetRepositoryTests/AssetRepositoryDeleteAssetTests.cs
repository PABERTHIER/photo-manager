using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteAssetTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

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
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);

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
    }

    [Test]
    public void DeleteAsset_FolderAndAssetExist_ReturnsDeletedAssetAndThumbnailsAndAssetAreDeletedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsFalse(thumbnails.ContainsKey(folderPath2));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset1.FileName]);

            Asset? assetDeleted1 = _testableAssetRepository!.DeleteAsset(folderPath1, _asset1.FileName);
            Asset? assetDeleted2 = _testableAssetRepository!.DeleteAsset(folderPath2, "toto.jpg");

            Assert.IsNotNull(assetDeleted1);
            Assert.AreEqual(_asset1!.FileName, assetDeleted1!.FileName);
            Assert.AreEqual(_asset1!.Hash, assetDeleted1.Hash);
            Assert.AreEqual(_asset1!.FullPath, assetDeleted1.FullPath);

            Assert.IsNull(assetDeleted2);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.IsEmpty(thumbnails[folderPath2]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsFalse(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

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
    public void DeleteAsset_AssetDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
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
            string folderPath = Path.Combine(_dataDirectory!, "TestFolder2");

            testableAssetRepository.AddFolder(folderPath);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? assetDeleted = testableAssetRepository.DeleteAsset(folderPath, "toto.jpg");

            Assert.IsNull(assetDeleted);

            Assert.IsEmpty(thumbnails);

            assets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsTrue(testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FolderIsNull_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset("non_existent_path", _asset1.FileName);

            Assert.IsNull(assetDeleted);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][_asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_ThumbnailsDoNotExistInRepositoryButBinExists_ReturnsDeletedAssetAndThumbnailsAndAssetAreDeletedAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "TestFolder2");
            const string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { fileName, [4, 5, 6]}
            };

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath, _asset1!.FileName);

            Assert.IsNull(assetDeleted);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(fileName));
            Assert.AreEqual(blobToWrite[fileName], thumbnails[folderPath][fileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_DirectoryIsNull_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string? folderPath2 = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath2!, _asset1.FileName);

            Assert.IsNull(assetDeleted);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][_asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FileNameIsNull_ThrowsArgumentNullExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string? assetFileName = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.DeleteAsset(folderPath1, assetFileName!));

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][_asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FolderAndAssetDoNotExist_ReturnsNullAndHasChangesIsFalseAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath1, "toto.jpg");

            Assert.IsNull(assetDeleted);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_ConcurrentAccess_ReturnsDeletedAssetsAndAssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            _asset1!.Folder = addedFolder1;
            _asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsFalse(thumbnails.ContainsKey(folderPath2));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset1.FileName]);

            Asset? assetDeleted1 = null;
            Asset? assetDeleted2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () =>
                { assetDeleted1 = _testableAssetRepository!.DeleteAsset(folderPath1, _asset1.FileName); },
                () =>
                { assetDeleted2 = _testableAssetRepository!.DeleteAsset(folderPath2, "toto.jpg"); }
            );

            Assert.IsNotNull(assetDeleted1);
            Assert.AreEqual(_asset1!.FileName, assetDeleted1!.FileName);
            Assert.AreEqual(_asset1!.Hash, assetDeleted1.Hash);
            Assert.AreEqual(_asset1!.FullPath, assetDeleted1.FullPath);

            Assert.IsNull(assetDeleted2);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.IsEmpty(thumbnails[folderPath2]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsFalse(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

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
