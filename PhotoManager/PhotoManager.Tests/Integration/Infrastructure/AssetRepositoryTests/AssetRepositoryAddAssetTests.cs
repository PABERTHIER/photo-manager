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
public class AssetRepositoryAddAssetTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;

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
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, _testLogger);

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
        _asset2 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = FileNames.IMAGE_9_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
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
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void AddAsset_FolderAndThumbnailsExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder1 = _assetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _assetRepository.AddAsset(_asset2!, assetData2);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderDoesNotExist_AssetIsAddedAndFolderIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _assetRepository.AddAsset(_asset2!, assetData2);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(folderPath);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(folderPath));
            Assert.That(folder.Name, Is.EqualTo("NewFolder"));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailDoesNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _assetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailsDictionaryEntriesToKeepIs0_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new(configurationRootMock);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        TestableAssetRepository testableAssetRepository = new(_pathProviderServiceMock!,
            imageProcessingService, imageMetadataService, userConfigurationService, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = testableAssetRepository.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            testableAssetRepository.AddAsset(_asset1!, assetData);

            assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

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
    public void AddAsset_FolderAndThumbnailsDoNotExist_AssetIsAddedAndFolderIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Folder? folderFromRepository = _assetRepository!.GetFolderByPath(folderPath);
            Assert.That(folderFromRepository, Is.Not.Null);
            Assert.That(folderFromRepository.Path, Is.EqualTo(folderPath));
            Assert.That(folderFromRepository.Name, Is.EqualTo("NewFolder"));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetFolderIsDefault_AssetIsNotAddedAndAssetsUpdatedIsNotUpdatedAndLogsIt()
    {
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        TestableAssetRepository testableAssetRepository = new(_pathProviderServiceMock!,
            imageProcessingService, imageMetadataService, userConfigurationService, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            testableAssetRepository.AddAsset(_asset1!, assetData);

            assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            string[] messages =
            [
                $"The asset could not be added, folder path is null or empty, asset.FileName: {_asset1!.FileName}"
            ];

            _testLogger!.AssertLogErrors(messages, typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetIsNull_LogsItAndThrowsNullReferenceExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Object reference not set to an instance of an object.";

            Asset? asset = null;
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            using (Assert.EnterMultipleScope())
            {
                NullReferenceException? exception =
                    Assert.Throws<NullReferenceException>(() => _assetRepository.AddAsset(asset!, assetData));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                assets = _assetRepository.GetCataloguedAssets();
                Assert.That(assets, Is.Empty);

                Assert.That(assetsUpdatedEvents, Is.Empty);

                Exception expectedException = new(exceptionMessage);
                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailDataIsNull_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _assetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[]? assetData = null;

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData!);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ConcurrentAccess_AssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder1 = _assetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository.AddAsset(_asset1!, assetData1),
                () => _assetRepository.AddAsset(_asset2!, assetData2)
            );

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets.Any(x => x.FileName == _asset1.FileName), Is.True);
            Assert.That(assets.Any(x => x.FileName == _asset2.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
