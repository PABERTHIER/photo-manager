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
public class AssetRepositoryLoadThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;

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
        _userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, _userConfigurationService, sqlitePersistenceContext, _testLogger);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_ReturnsBitmapImage()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Folder addedFolder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage = _assetRepository!.LoadThumbnail(
                folderPath,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Not.Null);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = folderPath });

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage = _assetRepository!.LoadThumbnail(
                folderPath,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Null);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetAndFolderDoNotExist_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            BitmapImage? bitmapImage = _assetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Null);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_DirectoryNameIsNull_LogsItAndThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Value cannot be null. (Parameter 'key')";

            string? directoryName = null;
            Folder addedFolder = _assetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _assetRepository!.AddAsset(_asset1!, []);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

                ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                {
                    _assetRepository!.LoadThumbnail(
                        directoryName!,
                        _asset1!.FileName,
                        _asset1.Pixel.Thumbnail.Width,
                        _asset1.Pixel.Thumbnail.Height);
                });

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

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
    public void LoadThumbnail_FileNameIsNull_LogsItAndThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Value cannot be null. (Parameter 'key')";

            Folder addedFolder = _assetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            string? fileName = null;

            using (Assert.EnterMultipleScope())
            {
                ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                    _assetRepository!.LoadThumbnail(_dataDirectory!, fileName!, 0, 0));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

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
    public void LoadThumbnail_ThumbnailMissingFromCacheAndDatabase_DeletesOrphanedAssetAndFiresAssetsUpdatedEvent()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "1");

        SqliteConnectionFactory sqliteConnectionFactory = new();
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(sqliteConnectionFactory, sqliteBackupService,
            new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(configurationRootMock);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_pathProviderServiceMock!, imageProcessingService, imageMetadataService,
            userConfigurationService, sqlitePersistenceContext, testLogger);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

            string filePath1 = Path.Combine(folderPath1, _asset1!.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            // Add folder1 + asset1 → thumbnail occupies the single LRU slot
            Folder folder1 = assetRepository.AddFolder(folderPath1);
            _asset1 = _asset1.WithFolder(folder1);
            assetRepository.AddAsset(_asset1, assetData1);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));

            // Add folder2 + asset2 → evicts folder1 from LRU (capacity = 1)
            Folder folder2 = assetRepository.AddFolder(folderPath2);
            Asset asset2 = new()
            {
                Folder = folder2,
                FolderId = folder2.Id,
                FileName = FileNames.IMAGE_9_PNG,
                ImageRotation = Rotation.Rotate0,
                Pixel = new()
                {
                    Asset = new()
                    {
                        Width = PixelWidthAsset.IMAGE_9_PNG,
                        Height = PixelHeightAsset.IMAGE_9_PNG
                    },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_9_PNG,
                        Height = ThumbnailHeightAsset.IMAGE_9_PNG
                    }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_9_PNG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                Hash = Hashes.IMAGE_9_PNG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            assetRepository.AddAsset(asset2, [1, 2, 3]);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            // LRU now: [folder2 only] — folder1 has been evicted

            // Simulate DB inconsistency: remove asset1's thumbnail from the
            // database so that a subsequent load will find neither cache nor DB entry
            sqlitePersistenceContext.Thumbnails.Delete(folder1.Id, _asset1.FileName);

            // asset1 still lives in _assetsByFolderId[folder1.Id],
            // but its thumbnail is now absent from both cache and DB
            BitmapImage? result = assetRepository.LoadThumbnail(folderPath1,
                _asset1.FileName, _asset1.Pixel.Thumbnail.Width, _asset1.Pixel.Thumbnail.Height);

            List<Asset> cataloguedAssets = assetRepository.GetCataloguedAssets();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);

                // One additional event emitted by the orphan-cleanup path
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

                // asset1 removed from catalog; asset2 unaffected
                Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
                Assert.That(cataloguedAssets.Any(a => a.FileName == asset2.FileName), Is.True);
                Assert.That(cataloguedAssets.Any(a => a.FileName == _asset1.FileName), Is.False);

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_ThumbnailsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Folder addedFolder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage1 = null;
            BitmapImage? bitmapImage2 = null;
            BitmapImage? bitmapImage3 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => bitmapImage1 = _assetRepository!.LoadThumbnail(
                    folderPath,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage2 = _assetRepository!.LoadThumbnail(
                    folderPath,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage3 = _assetRepository!.LoadThumbnail(
                    folderPath,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height)
            );

            Assert.That(bitmapImage1, Is.Not.Null);
            Assert.That(bitmapImage2, Is.Not.Null);
            Assert.That(bitmapImage3, Is.Not.Null);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
