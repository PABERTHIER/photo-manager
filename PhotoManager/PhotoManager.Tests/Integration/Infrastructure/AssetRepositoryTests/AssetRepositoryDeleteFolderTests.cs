using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Reactive = System.Reactive;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteFolderTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;

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
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        _userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(imageProcessingService, imageMetadataService, _userConfigurationService,
            sqlitePersistenceContext, _testLogger);

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("876283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_1_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void DeleteFolder_FolderExistsWithThumbnails_RemovesThumbnailsAndFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders.Contains(_asset1!.Folder), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnails_RemovesOnlyFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _assetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders.Contains(_asset1!.Folder), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButCreated_RemovesFolderAndThumbnail()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _assetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders.Contains(_asset1!.Folder), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderDoesNotExist_RemovesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        try
        {
            Directory.CreateDirectory(folderPath);

            Folder folder = new()
            {
                Path = folderPath,
                Id = Guid.NewGuid(),
            };

            _assetRepository!.DeleteFolder(folder);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders.Contains(folder), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderIsNull_LogsItAndThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Object reference not set to an instance of an object.";
            Folder? folder = null;

            using (Assert.EnterMultipleScope())
            {
                NullReferenceException? exception =
                    Assert.Throws<NullReferenceException>(() => _assetRepository!.DeleteFolder(folder!));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                Exception expectedException = new(exceptionMessage);
                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_ConcurrentAccess_ThumbnailsAndFoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.DeleteFolder(_asset1!.Folder),
                () => _assetRepository!.DeleteFolder(_asset1!.Folder),
                () => _assetRepository!.DeleteFolder(_asset1!.Folder)
            );

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders.Contains(_asset1!.Folder), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
