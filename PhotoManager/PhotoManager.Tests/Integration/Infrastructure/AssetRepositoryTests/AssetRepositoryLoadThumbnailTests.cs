using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Reactive = System.Reactive;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryLoadThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

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
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);

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

    [Test]
    public void LoadThumbnail_ThumbnailExists_ReturnsBitmapImage()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Folder addedFolder = _testableAssetRepository!.AddFolder(folderPath);

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
                folderPath,
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

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs,
                    _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string fileName = FileNames.NON_EXISTENT_IMAGE_PNG;

            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData },
                { fileName, assetData }
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
            Assert.That(thumbnails[_dataDirectory!][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs,
                    addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object,
            imageProcessingService,
            imageMetadataService, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string fileName = FileNames.NON_EXISTENT_IMAGE_PNG;
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3] },
                { fileName, [4, 5, 6] }
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

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs,
                    addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.True);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void
        LoadThumbnail_AssetDoesNotExistButBinNotContainingTheAssetExists_ReturnsNullAndRemovesBlobAndWritesDbFile()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            using (Assert.EnterMultipleScope())
            {
                Assert.That(bitmapImage, Is.Null);

                List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
                Assert.That(assets, Is.Empty);

                Assert.That(thumbnails, Is.Empty);

                Assert.That(
                    File.Exists(Path.Combine(_databasePath!,
                        _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs,
                        addedFolder.ThumbnailsFilename)), Is.False);

                Assert.That(
                    File.Exists(Path.Combine(_databasePath!,
                        _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                    Is.True);
                Assert.That(
                    File.Exists(Path.Combine(_databasePath!,
                        _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                    Is.True);
                Assert.That(
                    File.Exists(Path.Combine(_databasePath!,
                        _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                        Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
                Assert.That(
                    File.Exists(Path.Combine(_databasePath!,
                        _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                        Tables.RECENT_TARGET_PATHS_DB)), Is.True);

                Assert.That(assetsUpdatedEvents, Is.Empty);
            }
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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = folderPath });

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(
                folderPath,
                _asset1!.FileName,
                _asset1.Pixel.Thumbnail.Width,
                _asset1.Pixel.Thumbnail.Height);

            Assert.That(bitmapImage, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            string? fileName = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _testableAssetRepository!.LoadThumbnail(_dataDirectory!, fileName!, 0, 0));

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
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Folder addedFolder = _testableAssetRepository!.AddFolder(folderPath);

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
                    folderPath,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage2 = _testableAssetRepository!.LoadThumbnail(
                    folderPath,
                    _asset1!.FileName,
                    _asset1.Pixel.Thumbnail.Width,
                    _asset1.Pixel.Thumbnail.Height),
                () => bitmapImage3 = _testableAssetRepository!.LoadThumbnail(
                    folderPath,
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

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs,
                    _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)),
                Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(
                File.Exists(Path.Combine(_databasePath!,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
