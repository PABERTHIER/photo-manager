using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void GetFolders_Folders_ReturnsCorrectFolders()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(2));
            Assert.That(folders.Any(x => x.Path == folderPath1 && x.Id == addedFolder1.Id), Is.True);
            Assert.That(folders.Any(x => x.Path == folderPath2 && x.Id == addedFolder2.Id), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolders_NoFolders_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolders_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders1 = [];
            Folder[] folders2 = [];
            Folder[] folders3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => folders1 = _assetRepository!.GetFolders(),
                () => folders2 = _assetRepository!.GetFolders(),
                () => folders3 = _assetRepository!.GetFolders()
            );

            Assert.That(folders1, Has.Length.EqualTo(2));
            Folder? firstFolder1 = folders1.FirstOrDefault(x => x.Path == folderPath1);
            Folder? secondFolder1 = folders1.FirstOrDefault(x => x.Path == folderPath2);
            Assert.That(firstFolder1, Is.Not.Null);
            Assert.That(secondFolder1, Is.Not.Null);
            Assert.That(firstFolder1!.Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(secondFolder1!.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(folders2, Has.Length.EqualTo(2));
            Folder? firstFolder2 = folders2.FirstOrDefault(x => x.Path == folderPath1);
            Folder? secondFolder2 = folders2.FirstOrDefault(x => x.Path == folderPath2);
            Assert.That(firstFolder2, Is.Not.Null);
            Assert.That(secondFolder2, Is.Not.Null);
            Assert.That(firstFolder2!.Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(secondFolder2!.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(folders3, Has.Length.EqualTo(2));
            Folder? firstFolder3 = folders3.FirstOrDefault(x => x.Path == folderPath1);
            Folder? secondFolder3 = folders3.FirstOrDefault(x => x.Path == folderPath2);
            Assert.That(firstFolder3, Is.Not.Null);
            Assert.That(secondFolder3, Is.Not.Null);
            Assert.That(firstFolder3!.Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(secondFolder3!.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
