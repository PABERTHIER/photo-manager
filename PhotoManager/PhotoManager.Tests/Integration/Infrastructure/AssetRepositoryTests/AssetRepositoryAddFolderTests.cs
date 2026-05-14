using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddFolderTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly Guid _defaultGuid = Guid.Empty;

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
        SqliteConnectionFactory sqliteConnectionFactory = new();
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
    public void AddFolder_DifferentPaths_AddsFoldersToList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.Not.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(addedFolder1.Id));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

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
    public void AddFolder_SamePath_ReturnsSameFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.EqualTo(addedFolder1.Id));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(1));
            Assert.That(folders[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders[0].Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddFolder_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
            Folder addedFolder2 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => addedFolder1 = _assetRepository!.AddFolder(folderPath1),
                () => addedFolder2 = _assetRepository!.AddFolder(folderPath2)
            );

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(_defaultGuid));

            Assert.That(addedFolder2.Path, Is.Not.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(addedFolder1.Id));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(2));
            Folder? folder1 = folders.FirstOrDefault(x => x.Path == folderPath1);
            Folder? folder2 = folders.FirstOrDefault(x => x.Path == folderPath2);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);

            Assert.That(folder1!.Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folder2!.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
