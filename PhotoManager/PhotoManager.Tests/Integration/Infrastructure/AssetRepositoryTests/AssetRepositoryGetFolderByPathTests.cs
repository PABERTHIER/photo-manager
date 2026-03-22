using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFolderByPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
            new BackupStorage(), new TestLogger<PhotoManager.Infrastructure.Database.Database>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(database, _pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void GetFolderByPath_DifferentPaths_ReturnsCorrectFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_SamePath_ReturnsFirstFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath2.Id, Is.Not.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_PathNotRegistered_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Null);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_PathIsNull_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? folderPath1 = null;

            _assetRepository!.AddFolder(Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1!);

            Assert.That(folderByPath1, Is.Null);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
            Folder? folderByPath2 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1),
                () => folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2)
            );

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
