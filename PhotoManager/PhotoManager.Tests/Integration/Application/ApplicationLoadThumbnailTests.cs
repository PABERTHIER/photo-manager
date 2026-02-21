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

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationLoadThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset1Temp;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
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
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset1Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.HOMER_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.HOMER_JPG, Height = PixelHeightAsset.HOMER_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_JPG, Height = ThumbnailHeightAsset.HOMER_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.HOMER_JPG,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.HOMER_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = new(configurationRootMock.Object);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_testableAssetRepository, fileOperationsService, _userConfigurationService);
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task LoadThumbnail_CataloguedAssets_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            await _application!.CatalogAssetsAsync(_ => { });

            Folder folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory)!;

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            Assert.That(File.Exists(_asset1!.FullPath), Is.True);
            Assert.That(File.Exists(_asset2!.FullPath), Is.True);
            Assert.That(File.Exists(_asset3!.FullPath), Is.True);
            Assert.That(File.Exists(_asset4!.FullPath), Is.True);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.Not.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);
            _application!.LoadThumbnail(_asset2!);
            _application!.LoadThumbnail(_asset3!);
            _application!.LoadThumbnail(_asset4!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.Not.Empty);

            // True because the catalog has been saved during the CatalogAssetsAsync
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task LoadThumbnail_CataloguedAssetFromVideo_SetsBitmapImageToTheAsset()
    {
        string tempDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_FOLDER);

        ConfigureApplication(100, tempDirectory, 200, 150, false, false, false, true);

        string outputFirstFrameDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            Directory.CreateDirectory(tempDirectory);

            string videoSourcePath = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);
            string videoDestinationPath = Path.Combine(tempDirectory, FileNames.HOMER_MP4);
            File.Copy(videoSourcePath, videoDestinationPath);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder folder = _testableAssetRepository!.GetFolderByPath(outputFirstFrameDirectory)!;

            _asset1Temp = _asset1Temp!.WithFolder(folder);

            Assert.That(File.Exists(_asset1Temp!.FullPath), Is.True);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(outputFirstFrameDirectory), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[outputFirstFrameDirectory].ContainsKey(_asset1Temp.FileName), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory][_asset1Temp.FileName], Is.Not.Empty);

            Assert.That(_asset1Temp.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1Temp);

            Assert.That(_asset1Temp.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1Temp.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(outputFirstFrameDirectory), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[outputFirstFrameDirectory].ContainsKey(_asset1Temp.FileName), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory][_asset1Temp.FileName], Is.Not.Empty);

            // True because the catalog has been saved during the CatalogAssetsAsync
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1Temp.Folder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath1 = Path.Combine(assetsDirectory, _asset1!.FileName);
            string filePath2 = Path.Combine(assetsDirectory, _asset2!.FileName);
            string filePath3 = Path.Combine(assetsDirectory, _asset3!.FileName);
            string filePath4 = Path.Combine(assetsDirectory, _asset4!.FileName);

            byte[] assetData1 = File.ReadAllBytes(filePath1);
            byte[] assetData2 = File.ReadAllBytes(filePath2);
            byte[] assetData3 = File.ReadAllBytes(filePath3);
            byte[] assetData4 = File.ReadAllBytes(filePath4);

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData1);
            _testableAssetRepository!.AddAsset(_asset2!, assetData2);
            _testableAssetRepository!.AddAsset(_asset3!, assetData3);
            _testableAssetRepository!.AddAsset(_asset4!, assetData4);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(assetData3));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(assetData4));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);
            _application!.LoadThumbnail(_asset2!);
            _application!.LoadThumbnail(_asset3!);
            _application!.LoadThumbnail(_asset4!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(assetData3));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(assetData4));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExists_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string fileName = FileNames.NON_EXISTENT_IMAGE_PNG;

            string filePath = Path.Combine(assetsDirectory, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData },
                { fileName, assetData }
            };

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _database!.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(fileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][fileName], Is.EqualTo(assetData));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExistsAndRemoveOldThumbnailsDictionaryEntriesIs0_DoesNotSetBitmapImageToTheAsset()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(database, _pathProviderServiceMock!.Object,
            imageProcessingService, imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(testableAssetRepository, fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] asset1Data = [1, 2, 3];
            byte[] asset2Data = [4, 5, 6];

            const string fileName = FileNames.NON_EXISTENT_IMAGE_PNG;
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, asset1Data },
                { fileName, asset2Data }
            };

            Folder addedFolder = testableAssetRepository.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            database.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            application.LoadThumbnail(_asset1);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinNotContainingTheAssetExists_DoesNotSetBitmapImageToTheAssetAndWritesBlobAndDbFiles()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _database!.WriteBlob([], addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath = Path.Combine(assetsDirectory, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
    public void LoadThumbnail_AssetAndFolderDoNotExist_DoesNotSetBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderPathIsNull_ThrowsArgumentNullException()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = null! });

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _application!.LoadThumbnail(_asset1!);
            });

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            Asset asset = new()
            {
                FolderId = addedFolder.Id,
                Folder = addedFolder,
                FileName = null!,
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
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_1_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            Assert.That(assetsUpdatedEvents, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.LoadThumbnail(asset));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_BitmapImageAreSetToEachAssetSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath1 = Path.Combine(assetsDirectory, _asset1!.FileName);
            string filePath2 = Path.Combine(assetsDirectory, _asset2!.FileName);
            string filePath3 = Path.Combine(assetsDirectory, _asset3!.FileName);
            string filePath4 = Path.Combine(assetsDirectory, _asset4!.FileName);

            byte[] assetData1 = File.ReadAllBytes(filePath1);
            byte[] assetData2 = File.ReadAllBytes(filePath2);
            byte[] assetData3 = File.ReadAllBytes(filePath3);
            byte[] assetData4 = File.ReadAllBytes(filePath4);

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData1);
            _testableAssetRepository!.AddAsset(_asset2!, assetData2);
            _testableAssetRepository!.AddAsset(_asset3!, assetData3);
            _testableAssetRepository!.AddAsset(_asset4!, assetData4);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(assetData3));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(assetData4));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _application!.LoadThumbnail(_asset4!),
                () => _application!.LoadThumbnail(_asset2!),
                () => _application!.LoadThumbnail(_asset1!),
                () => _application!.LoadThumbnail(_asset3!)
            );

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(assetData3));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(assetData4));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
