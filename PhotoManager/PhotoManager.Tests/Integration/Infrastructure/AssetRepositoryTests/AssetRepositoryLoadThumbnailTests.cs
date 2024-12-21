using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryLoadThumbnailTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

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
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
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
            Folder = new() { Path = "" },
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
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _testableAssetRepository!.AddAsset(_asset1!, [1, 2, 3]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { fileName, [4, 5, 6]}
            };

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_dataDirectory!));
            Assert.AreEqual(2, thumbnails[_dataDirectory!].Count);
            Assert.IsTrue(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_dataDirectory!][_asset1.FileName]);

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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

            Folder addedFolder1 = testableAssetRepository.AddFolder(_dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = testableAssetRepository.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNull(bitmapImage);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.IsEmpty(thumbnails);

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _database!.WriteBlob([], addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_dataDirectory!));
            Assert.IsEmpty(thumbnails[_dataDirectory!]);

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_dataDirectory!));
            Assert.AreEqual(1, thumbnails[_dataDirectory!].Count);
            Assert.IsTrue(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[_dataDirectory!][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_dataDirectory!));
            Assert.AreEqual(1, thumbnails[_dataDirectory!].Count);
            Assert.IsTrue(thumbnails[_dataDirectory!].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[_dataDirectory!][_asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                _dataDirectory!,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.IsNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_dataDirectory!));
            Assert.IsEmpty(thumbnails[_dataDirectory!]);

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _testableAssetRepository!.LoadThumbnail(
                    directoryName!,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height);
            });

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            string? fileName = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.LoadThumbnail(_dataDirectory!, fileName!, 0, 0));

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_ThumbnailsIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _testableAssetRepository!.AddAsset(_asset1!, [1, 2, 3]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

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

            Assert.IsNotNull(bitmapImage1);
            Assert.IsNotNull(bitmapImage2);
            Assert.IsNotNull(bitmapImage3);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
