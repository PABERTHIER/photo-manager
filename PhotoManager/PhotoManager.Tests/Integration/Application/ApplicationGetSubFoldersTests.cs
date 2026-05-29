using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetSubFoldersTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetSubFolders_CataloguedAssetsAndParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(assetsDirectory, Directories.NEW_FOLDER_1);
            string parentFolderPath2 = Path.Combine(assetsDirectory, Directories.NEW_FOLDER_2);
            string parentFolderPath3 = Path.Combine(assetsDirectory, Directories.NOT_DUPLICATE);
            string parentFolderPath4 = Path.Combine(assetsDirectory, Directories.PART);
            string parentFolderPath5 = Path.Combine(assetsDirectory, Directories.RESOLUTION);
            string parentFolderPath6 = Path.Combine(assetsDirectory, Directories.THUMBNAIL);

            string childFolderPath1 = Path.Combine(parentFolderPath3, Directories.SAMPLE_1);
            string childFolderPath2 = Path.Combine(parentFolderPath3, Directories.SAMPLE_2);
            string childFolderPath3 = Path.Combine(parentFolderPath3, Directories.SAMPLE_3);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? parentFolder1 = _testableAssetRepository!.GetFolderByPath(parentFolderPath1);
            Folder? parentFolder2 = _testableAssetRepository!.GetFolderByPath(parentFolderPath2);
            Folder? parentFolder3 = _testableAssetRepository!.GetFolderByPath(parentFolderPath3);
            Folder? parentFolder4 = _testableAssetRepository!.GetFolderByPath(parentFolderPath4);
            Folder? parentFolder5 = _testableAssetRepository!.GetFolderByPath(parentFolderPath5);
            Folder? parentFolder6 = _testableAssetRepository!.GetFolderByPath(parentFolderPath6);

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
            Assert.That(parentFolders3.Any(x => x.Path == childFolderPath1));
            Assert.That(parentFolders3.Any(x => x.Path == childFolderPath2));
            Assert.That(parentFolders3.Any(x => x.Path == childFolderPath3));

            Assert.That(parentFolders4, Is.Empty);
            Assert.That(parentFolders5, Is.Empty);
            Assert.That(parentFolders6, Is.Empty);

            Folder? childFolder1 = _testableAssetRepository!.GetFolderByPath(childFolderPath1);
            Folder? childFolder2 = _testableAssetRepository!.GetFolderByPath(childFolderPath2);
            Folder? childFolder3 = _testableAssetRepository!.GetFolderByPath(childFolderPath3);

            Assert.That(childFolder1, Is.Not.Null);
            Assert.That(childFolder2, Is.Not.Null);
            Assert.That(childFolder3, Is.Not.Null);

            Folder[] childFolders1 = _application!.GetSubFolders(childFolder1);
            Folder[] childFolders2 = _application!.GetSubFolders(childFolder2);
            Folder[] childFolders3 = _application!.GetSubFolders(childFolder3);

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(8));
            Assert.That(assetsUpdatedEvents, Has.All.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            string childFolderPath1 = Path.Combine(parentFolderPath1, Directories.TEST_SUB_FOLDER_1);
            string childFolderPath2 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);
            string childFolderPath3 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_3);

            Folder parentFolder1 = _testableAssetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _testableAssetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _testableAssetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _testableAssetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _testableAssetRepository!.AddFolder(childFolderPath3);

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
            Assert.That(parentFolders2.Any(x => x.Path == childFolderPath2));
            Assert.That(parentFolders2.Any(x => x.Path == childFolderPath3));

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder parentFolder1 = _testableAssetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _testableAssetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _application!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _application!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

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
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsNullReferenceException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? parentFolder1 = null;

            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            _testableAssetRepository!.AddFolder(parentFolderPath2);

            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() => _application!.GetSubFolders(parentFolder1!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
