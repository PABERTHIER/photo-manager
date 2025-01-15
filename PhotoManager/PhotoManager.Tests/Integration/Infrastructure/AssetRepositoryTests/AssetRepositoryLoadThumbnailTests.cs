using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryLoadThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 363888,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_ReturnsBitmapImage()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = [1, 2, 3];

            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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
    public void LoadThumbnail_AssetDoesNotExistButBinExists_ReturnsBitmapImage()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string fileName = "Image2.png";
            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [4, 5, 6];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { fileName, assetData2 }
            };

            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_dataDirectory!), Is.True);
            Assert.That(thumbnails[_dataDirectory!], Has.Count.EqualTo(2));
            Assert.That(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_dataDirectory!][_asset1.FileName], Is.EqualTo(assetData1));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExistsAndRemoveOldThumbnailsDictionaryEntriesIs0_ReturnsNull()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, _storageServiceMock!.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { fileName, [4, 5, 6]}
            };

            Folder addedFolder = testableAssetRepository.AddFolder(_dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            BitmapImage? bitmapImage = testableAssetRepository.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Null);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinNotContainingTheAssetExists_ReturnsNullAndWritesBlobAndDbFiles()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _database!.WriteBlob([], addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_dataDirectory!), Is.True);
            Assert.That(thumbnails[_dataDirectory!], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_ReturnsBitmapImage()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = _dataDirectory! });

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_dataDirectory!), Is.True);
            Assert.That(thumbnails[_dataDirectory!], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_dataDirectory!][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_dataDirectory!), Is.True);
            Assert.That(thumbnails[_dataDirectory!], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_dataDirectory!][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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
    public void LoadThumbnail_AssetAndFolderDoNotExist_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_dataDirectory!), Is.True);
            Assert.That(thumbnails[_dataDirectory!], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_DirectoryNameIsNull_ThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? directoryName = null;
            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _testableAssetRepository!.LoadThumbnail(
                    directoryName!,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height);
            });

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

    [Test]
    public void LoadThumbnail_FileNameIsNull_ThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            string? fileName = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.LoadThumbnail(_dataDirectory!, fileName!, 0, 0));

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

    [Test]
    public void LoadThumbnail_ConcurrentAccess_ThumbnailsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = [1, 2, 3];

            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage1 = null;
            BitmapImage? bitmapImage2 = null;
            BitmapImage? bitmapImage3 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => bitmapImage1 = _testableAssetRepository!.LoadThumbnail(
                    _dataDirectory!,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage2 = _testableAssetRepository!.LoadThumbnail(
                    _dataDirectory!,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage3 = _testableAssetRepository!.LoadThumbnail(
                    _dataDirectory!,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height)
            );

            Assert.That(bitmapImage1, Is.Not.Null);
            Assert.That(bitmapImage2, Is.Not.Null);
            Assert.That(bitmapImage3, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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
