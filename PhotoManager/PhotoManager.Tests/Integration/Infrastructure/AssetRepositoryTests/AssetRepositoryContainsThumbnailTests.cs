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
public class AssetRepositoryContainsThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

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
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);

        _asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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

    [Test]
    public void ContainsThumbnail_ThumbnailsExistInRepository_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ConcurrentAccess_ThumbnailsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            bool isContainingThumbnail1 = false;
            bool isContainingThumbnail2 = false;
            bool isContainingThumbnail3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => isContainingThumbnail1 = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName),
                () => isContainingThumbnail2 = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName),
                () => isContainingThumbnail3 = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName)
            );

            Assert.That(isContainingThumbnail1, Is.True);
            Assert.That(isContainingThumbnail2, Is.True);
            Assert.That(isContainingThumbnail3, Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExistButAdded_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                Path = folderPath
            };

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExist_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                Path = folderPath
            };

            _asset1 = _asset1!.WithFolder(folder);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Is.Empty);
            Assert.That(isContainingThumbnail, Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_ReturnsFalse()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object,
            imageProcessingService, imageMetadataService, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                Path = folderPath
            };

            _asset1 = _asset1!.WithFolder(folder);

            testableAssetRepository.AddAsset(_asset1!, []);

            Assert.That(testableAssetRepository.HasChanges(), Is.True); // Set by AddFolder method, called by AddAsset method

            Assert.That(assetsUpdatedEvents, Is.Empty);

            bool isContainingThumbnail = testableAssetRepository.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ThumbnailsDoNotExistInRepository_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Is.Empty);
            Assert.That(isContainingThumbnail, Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ThumbnailsDoNotExistInRepositoryButBinExists_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [4, 5, 6];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { FileNames.IMAGE_2_PNG, assetData2 }
            };

            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1.Folder.Path], Has.Count.EqualTo(2));
            Assert.That(thumbnails[_asset1.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1.Folder.Path][_asset1.FileName], Is.EqualTo(assetData1));

            Assert.That(isContainingThumbnail, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_DifferentFileName_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            const string fileName = FileNames.NON_EXISTENT_IMAGE_JPG;
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, fileName);

            Assert.That(isContainingThumbnail, Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_NullFileName_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            string? fileName = null;
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, fileName!);

            Assert.That(isContainingThumbnail, Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_NullDirectoryName_ThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            string? directoryName = null;
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.ContainsThumbnail(directoryName!, _asset1!.FileName));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
