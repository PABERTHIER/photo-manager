using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryFolderHasThumbnailsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
    }

    [Test]
    public void FolderHasThumbnails_ThumbnailsExist_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
            _database!.WriteBlob([], folder.ThumbnailsFilename);

            bool folderHasThumbnails = _assetRepository!.FolderHasThumbnails(folder);

            Assert.That(folderHasThumbnails, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void FolderHasThumbnails_ThumbnailsDoNotExist_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            bool folderHasThumbnails = _assetRepository!.FolderHasThumbnails(folder);

            Assert.That(folderHasThumbnails, Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void FolderHasThumbnails_FolderIsNull_ThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? folder = null;

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _assetRepository!.FolderHasThumbnails(folder!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
