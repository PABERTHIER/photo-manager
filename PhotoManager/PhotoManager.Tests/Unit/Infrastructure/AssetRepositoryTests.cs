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

    private IDatabase? _databaseMock;
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
        _userConfigurationService = new(_configurationRootMock!);
        _databaseMock = Substitute.For<IDatabase>();

        _databaseMock.ReadObjectList(Arg.Any<string>(), Arg.Any<Func<string[], Folder>>()).Returns([]);
        _databaseMock.ReadObjectList(Arg.Any<string>(), Arg.Any<Func<string[], Asset>>()).Returns([]);
        _databaseMock.ReadObjectList(
            Arg.Any<string>(), Arg.Any<Func<string[], SyncAssetsDirectoriesDefinition>>()).Returns([]);
        _databaseMock.ReadObjectList(Arg.Any<string>(), Arg.Any<Func<string[], string>>()).Returns([]);
        _databaseMock.ReadBlob(Arg.Any<string>()).Returns((Dictionary<string, byte[]>?)null);
        _databaseMock.BackupExists(Arg.Any<DateTime>()).Returns(false);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());

        _testableAssetRepository = new(_databaseMock, _pathProviderServiceMock!, imageProcessingService,
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
            _databaseMock!.ReadBlob(Arg.Any<string>()).Throws(expectedException);

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
            _databaseMock!.When(d => d.WriteObjectList(Arg.Any<List<Asset>>(), Arg.Any<string>(),
                Arg.Any<Func<Asset, int, object>>())).Throw(expectedException);

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
            _databaseMock!.IsBlobFileExists(Arg.Any<string>()).Throws(expectedException);

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
