using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetSubFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
    }

    [Test]
    public async Task GetSubFolders_CataloguedAssetsAndParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(assetsDirectory, "NewFolder1");
            string parentFolderPath2 = Path.Combine(assetsDirectory, "NewFolder2");
            string parentFolderPath3 = Path.Combine(assetsDirectory, "NotDuplicate");
            string parentFolderPath4 = Path.Combine(assetsDirectory, "Part");
            string parentFolderPath5 = Path.Combine(assetsDirectory, "Resolution");
            string parentFolderPath6 = Path.Combine(assetsDirectory, "Thumbnail");

            string childFolderPath1 = Path.Combine(parentFolderPath3, "Sample1");
            string childFolderPath2 = Path.Combine(parentFolderPath3, "Sample2");
            string childFolderPath3 = Path.Combine(parentFolderPath3, "Sample3");

            await _application!.CatalogAssetsAsync(_ => {});

            Folder? parentFolder1 = _assetRepository!.GetFolderByPath(parentFolderPath1);
            Folder? parentFolder2 = _assetRepository!.GetFolderByPath(parentFolderPath2);
            Folder? parentFolder3 = _assetRepository!.GetFolderByPath(parentFolderPath3);
            Folder? parentFolder4 = _assetRepository!.GetFolderByPath(parentFolderPath4);
            Folder? parentFolder5 = _assetRepository!.GetFolderByPath(parentFolderPath5);
            Folder? parentFolder6 = _assetRepository!.GetFolderByPath(parentFolderPath6);

            Assert.That(parentFolder1, Is.Not.Null);
            Assert.That(parentFolder2, Is.Not.Null);
            Assert.That(parentFolder3, Is.Not.Null);
            Assert.That(parentFolder4, Is.Not.Null);
            Assert.That(parentFolder5, Is.Not.Null);
            Assert.That(parentFolder6, Is.Not.Null);

            Folder[] parentFolders1 = _application!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _application!.GetSubFolders(parentFolder2);
            Folder[] parentFolders3 = _application!.GetSubFolders(parentFolder3);
            Folder[] parentFolders4 = _application!.GetSubFolders(parentFolder4);
            Folder[] parentFolders5 = _application!.GetSubFolders(parentFolder5);
            Folder[] parentFolders6 = _application!.GetSubFolders(parentFolder6);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(parentFolders3, Is.Not.Empty);
            Assert.That(parentFolders3, Has.Length.EqualTo(3));
            Assert.That(parentFolders3[0].Path, Is.EqualTo(childFolderPath1));
            Assert.That(parentFolders3[1].Path, Is.EqualTo(childFolderPath2));
            Assert.That(parentFolders3[2].Path, Is.EqualTo(childFolderPath3));

            Assert.That(parentFolders4, Is.Empty);
            Assert.That(parentFolders5, Is.Empty);
            Assert.That(parentFolders6, Is.Empty);

            Folder? childFolder1 = _assetRepository!.GetFolderByPath(childFolderPath1);
            Folder? childFolder2 = _assetRepository!.GetFolderByPath(childFolderPath2);
            Folder? childFolder3 = _assetRepository!.GetFolderByPath(childFolderPath3);

            Assert.That(childFolder1, Is.Not.Null);
            Assert.That(childFolder2, Is.Not.Null);
            Assert.That(childFolder3, Is.Not.Null);

            Folder[] childFolders1 = _application!.GetSubFolders(childFolder1);
            Folder[] childFolders2 = _application!.GetSubFolders(childFolder2);
            Folder[] childFolders3 = _application!.GetSubFolders(childFolder3);

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(31));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            string childFolderPath1 = Path.Combine(parentFolderPath1, "TestSubFolder1");
            string childFolderPath2 = Path.Combine(parentFolderPath2, "TestSubFolder2");
            string childFolderPath3 = Path.Combine(parentFolderPath2, "TestSubFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _assetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _assetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _assetRepository!.AddFolder(childFolderPath3);

            Folder[] parentFolders1 = _application!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _application!.GetSubFolders(parentFolder2);

            Folder[] childFolders1 = _application!.GetSubFolders(childFolder1);
            Folder[] childFolders2 = _application!.GetSubFolders(childFolder2);
            Folder[] childFolders3 = _application!.GetSubFolders(childFolder3);

            Assert.That(parentFolders1, Is.Not.Empty);
            Assert.That(parentFolders1, Has.Length.EqualTo(1));
            Assert.That(parentFolders1[0].Path, Is.EqualTo(childFolderPath1));

            Assert.That(parentFolders2, Is.Not.Empty);
            Assert.That(parentFolders2, Has.Length.EqualTo(2));
            Assert.That(parentFolders2[0].Path, Is.EqualTo(childFolderPath2));
            Assert.That(parentFolders2[1].Path, Is.EqualTo(childFolderPath3));

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _application!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _application!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderPath1 };
            Folder parentFolder2 = new() { Id = Guid.NewGuid(), Path = parentFolderPath2 };

            Folder[] parentFolders1 = _application!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _application!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsArgumentException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? parentFolder1 = null;

            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            _assetRepository!.AddFolder(parentFolderPath2); // At least one folder to trigger the Where on folders

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _application!.GetSubFolders(parentFolder1!));

            Assert.That(exception?.Message, Is.EqualTo("Delegate to an instance method cannot have null 'this'."));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
