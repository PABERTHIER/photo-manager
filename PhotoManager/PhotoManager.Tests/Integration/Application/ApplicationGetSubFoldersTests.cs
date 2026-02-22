using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetSubFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetSubFolders_CataloguedAssetsAndParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            string childFolderPath1 = Path.Combine(parentFolderPath1, Directories.TEST_SUB_FOLDER_1);
            string childFolderPath2 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);
            string childFolderPath3 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);

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
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

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
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

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

            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

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
