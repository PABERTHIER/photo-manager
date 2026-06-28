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
public class AssetRepositoryGetAssetsCounterTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

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
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
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
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("68493435-e299-4bb5-9e02-214da41d0256"))
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithRotation(ImageRotation.Rotate90)
            .WithPixels(PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithCorrupted(false, null)
            .WithRotated(true, "The asset has been rotated")
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("f91b8c81-6938-431a-a689-d86c7c4db126"))
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(true, "The asset is corrupted")
            .WithRotated(true, "The asset has been rotated")
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
    public void GetAssetsCounter_AssetsExist_ReturnsNumberOfAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            int assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.That(assetsCounter, Is.Zero);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _assetRepository.AddAsset(_asset1!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.That(assetsCounter, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.AddAsset(_asset2!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.That(assetsCounter, Is.EqualTo(2));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.AddAsset(_asset3!, []);
            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.That(assetsCounter, Is.EqualTo(3));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.DeleteAsset(folderPath, _asset3.FileName);

            assetsCounter = _assetRepository.GetAssetsCounter();
            Assert.That(assetsCounter, Is.EqualTo(2));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsCounter_NoAsset_Returns0()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            int assetsCounter = _assetRepository!.GetAssetsCounter();

            Assert.That(assetsCounter, Is.Zero);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsCounter_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);

            _assetRepository.AddAsset(_asset1!, []);
            _assetRepository.AddAsset(_asset2!, []);
            _assetRepository.AddAsset(_asset3!, []);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            int assetsCounter1 = 0;
            int assetsCounter2 = 0;
            int assetsCounter3 = 0;

            // Simulate concurrent access
            Parallel.Invoke(
                () => assetsCounter1 = _assetRepository.GetAssetsCounter(),
                () => assetsCounter2 = _assetRepository.GetAssetsCounter(),
                () => assetsCounter3 = _assetRepository.GetAssetsCounter()
            );

            Assert.That(assetsCounter1, Is.EqualTo(3));
            Assert.That(assetsCounter2, Is.EqualTo(3));
            Assert.That(assetsCounter3, Is.EqualTo(3));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
