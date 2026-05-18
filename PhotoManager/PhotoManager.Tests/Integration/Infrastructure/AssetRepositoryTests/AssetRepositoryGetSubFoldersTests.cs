using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetSubFoldersTests
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
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            string childFolderPath1 = Path.Combine(parentFolderPath1, Directories.TEST_SUB_FOLDER_1);
            string childFolderPath2 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);
            string childFolderPath3 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _assetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _assetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _assetRepository!.AddFolder(childFolderPath3);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Folder[] childFolders1 = _assetRepository!.GetSubFolders(childFolder1);
            Folder[] childFolders2 = _assetRepository!.GetSubFolders(childFolder2);
            Folder[] childFolders3 = _assetRepository!.GetSubFolders(childFolder3);

            Assert.That(parentFolders1, Is.Not.Empty);
            Assert.That(parentFolders1, Has.Length.EqualTo(1));
            Assert.That(parentFolders1[0].Path, Is.EqualTo(childFolderPath1));

            Assert.That(parentFolders2, Is.Not.Empty);
            Assert.That(parentFolders2, Has.Length.EqualTo(1));
            Assert.That(parentFolders2.Any(x => x.Path == childFolderPath2), Is.True);
            Assert.That(parentFolders2.Any(x => x.Path == childFolderPath3), Is.True);

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderPath1 };
            Folder parentFolder2 = new() { Id = Guid.NewGuid(), Path = parentFolderPath2 };

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? parentFolder1 = null;

            string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            _assetRepository!.AddFolder(parentFolderPath2); // At least one folder to trigger the loop

            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() => _assetRepository!.GetSubFolders(parentFolder1!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
