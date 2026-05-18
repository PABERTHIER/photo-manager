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
public class AssetRepositoryGetAssetsByPathTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private ImageMetadataService? _imageMetadataService;
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
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        _imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            _imageMetadataService, userConfigurationService, sqlitePersistenceContext, _testLogger);

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
        _asset3 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new("f91b8c81-6938-431a-a689-d86c7c4db126"),
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
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
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
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
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder1 = _assetRepository!.AddFolder(folderPath1);
            Folder folder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            string filePath1 = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder2);
            string filePath2 = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder2);
            string filePath3 = Path.Combine(folderPath2, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData2);
            _assetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].ImageData, Is.Null);

            Asset[] assets1 = _assetRepository.GetAssetsByPath(folderPath1);
            Asset[] assets2 = _assetRepository.GetAssetsByPath(folderPath2);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(assets1, Has.Length.EqualTo(1));
            Assert.That(assets2, Has.Length.EqualTo(2));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets1[0].ImageData, Is.Not.Null);
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[0].ImageData, Is.Not.Null);
            Assert.That(assets2[1].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets2[1].ImageData, Is.Not.Null);

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

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        BitmapImage? bitmapImage = null;
        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        imageProcessingServiceMock.LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(bitmapImage!);

        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        AssetRepository assetRepository = new(pathProviderServiceMock, imageProcessingServiceMock,
            _imageMetadataService!, userConfigurationService, sqlitePersistenceContext, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = assetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            Asset[] cataloguedAssets = assetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            assetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = assetRepository.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(assets, Is.Empty);

            imageProcessingServiceMock.Received(1).LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetRepository.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _assetRepository!.AddFolder(folderPath1);

            _asset2 = _asset2!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });
            string filePath = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset2, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _assetRepository.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset2.FileName));

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
    public void
        GetAssetsByPath_ThumbnailsAndFolderExistButThumbnailsDictionaryEntriesToKeepIs0_ReturnsAssetsWithThumbnails()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        AssetRepository? assetRepository = null;

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

            IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
            pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

            SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
            SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
            SqlitePersistenceContext sqlitePersistenceContext = new(
                sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
            UserConfigurationService userConfigurationService = new(configurationRootMock);
            ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
            FileOperationsService fileOperationsService = new(userConfigurationService,
                new TestLogger<FileOperationsService>());
            ImageMetadataService imageMetadataService = new(fileOperationsService,
                new TestLogger<ImageMetadataService>());
            assetRepository = new(pathProviderServiceMock, imageProcessingService,
                imageMetadataService, userConfigurationService, sqlitePersistenceContext, _testLogger!);

            Asset[] cataloguedAssets2 = assetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets2, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets2[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets2[0].ImageData, Is.Null);

            Asset[] assets = assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets2[0].ImageData, Is.Not.Null);
            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[0].ImageData, Is.Not.Null);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetRepository?.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_AssetFolderIsDefault_LogsItAndReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _assetsDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Asset[] assets = _assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            string logMessage =
                $"The asset could not be added, folder path is null or empty, asset.FileName: {_asset1.FileName}";
            _testLogger!.AssertLogErrors([logMessage], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderManuallyCreated_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Guid folderId = Guid.NewGuid();
            Folder folder = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(_assetsDirectory!, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Folder? autoCreatedFolder = _assetRepository.GetFolderByPath(folderPath);
            Assert.That(autoCreatedFolder, Is.Not.Null);
            Assert.That(autoCreatedFolder!.Path, Is.EqualTo(folderPath));

            Asset[] assets = _assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            // Because the folder added in the repo has not the same id as the one in the Asset
            Assert.That(assets, Is.Empty);

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
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            _assetRepository!.AddFolder(folderPath);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assets = _assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

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
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assets = _assetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);
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
    public void GetAssetsByPath_DirectoryIsNull_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? directory = null;
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = _assetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath1);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _assetRepository.GetAssetsByPath(directory!);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions(
                [new Exception("Value cannot be null. (Parameter 'key')")],
                typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ExceptionIsThrown_ReturnsEmptyArrayAndLogsIt()
    {
        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        imageProcessingServiceMock.LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
            .Throws(new Exception());

        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        AssetRepository assetRepository = new(pathProviderServiceMock, imageProcessingServiceMock,
            _imageMetadataService!, userConfigurationService, sqlitePersistenceContext, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = assetRepository.AddFolder(folderPath);

            _asset3 = _asset3!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset3.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            Asset[] cataloguedAssets = assetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            assetRepository.AddAsset(_asset3, assetData1);
            assetRepository.AddAsset(_asset2, assetData2);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = assetRepository.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(2));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);

            Asset[] assets = assetRepository.GetAssetsByPath(folderPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cataloguedAssets[0].ImageData, Is.Null);
                Assert.That(cataloguedAssets[1].ImageData, Is.Null);

                Assert.That(assets, Is.Empty);

                imageProcessingServiceMock.Received(1).LoadBitmapThumbnailImage(
                    Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

                Exception exception = new("Exception of type 'System.Exception' was thrown.");
                _testLogger!.AssertLogExceptions([exception], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder1 = _assetRepository!.AddFolder(folderPath1);
            Folder folder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            string filePath1 = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder2);
            string filePath2 = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder2);
            string filePath3 = Path.Combine(folderPath2, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _assetRepository.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData2);
            _assetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _assetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, Has.Length.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].ImageData, Is.Null);

            Asset[] assets1 = [];
            Asset[] assets2 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _assetRepository.GetAssetsByPath(folderPath1),
                () => assets2 = _assetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(assets1, Has.Length.EqualTo(1));
            Assert.That(assets2, Has.Length.EqualTo(2));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[1].FileName, Is.EqualTo(_asset3!.FileName));

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
