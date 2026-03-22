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

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class AssetRepositoryTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<AssetRepository>? _testLogger;

    private Mock<IDatabase>? _databaseMock;
    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _userConfigurationService = new(_configurationRootMock!.Object);
        _databaseMock = new();

        _databaseMock.Setup(d => d.Initialize(
            It.IsAny<string>(), It.IsAny<char>(), It.IsAny<string>(), It.IsAny<string>()));
        _databaseMock.Setup(d => d.SetDataTableProperties(It.IsAny<DataTableProperties>()));
        _databaseMock.Setup(d => d.ReadObjectList(It.IsAny<string>(), It.IsAny<Func<string[], Folder>>())).Returns([]);
        _databaseMock.Setup(d => d.ReadObjectList(It.IsAny<string>(), It.IsAny<Func<string[], Asset>>())).Returns([]);
        _databaseMock.Setup(d => d.ReadObjectList(
            It.IsAny<string>(), It.IsAny<Func<string[], SyncAssetsDirectoriesDefinition>>())).Returns([]);
        _databaseMock.Setup(d => d.ReadObjectList(It.IsAny<string>(), It.IsAny<Func<string[], string>>())).Returns([]);
        _databaseMock.Setup(d => d.ReadBlob(It.IsAny<string>())).Returns((Dictionary<string, byte[]>?)null);
        _databaseMock.Setup(d => d.BackupExists(It.IsAny<DateTime>())).Returns(false);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());

        _testableAssetRepository = new(_databaseMock.Object, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService, _testLogger);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" },
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
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void AddAsset_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            IOException expectedException = new("Database read error");
            _databaseMock!.Setup(d => d.ReadBlob(It.IsAny<string>())).Throws(expectedException);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() =>
                    _testableAssetRepository!.AddAsset(_asset1!, assetData));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_DatabaseThrowsException_LogsItThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);

            IOException expectedException = new("Failed to write to database");
            _databaseMock!.Setup(d => d.WriteObjectList(It.IsAny<List<Asset>>(), It.IsAny<string>(),
                It.IsAny<Func<Asset, int, object>>())).Throws(expectedException);

            // Add an asset first to ensure hasChanges is true
            _testableAssetRepository!.AddAsset(_asset1!, []);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception =
                    Assert.Throws<IOException>(() => _testableAssetRepository!.SaveCatalog(folder));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            IOException expectedException = new("Failed to access blob storage");
            _databaseMock!.Setup(d => d.IsBlobFileExists(It.IsAny<string>())).Throws(expectedException);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception =
                    Assert.Throws<IOException>(() => _testableAssetRepository!.DeleteFolder(folder));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
