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
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private ImageMetadataService? _imageMetadataService;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();
    }

    [SetUp]
    public void SetUp()
    {
        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        _imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            _imageMetadataService, userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
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
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
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
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
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
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            string filePath1 = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder2);
            string filePath2 = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder2);
            string filePath3 = Path.Combine(folderPath2, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].ImageData, Is.Null);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);
            Asset[] assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(2));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath2][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData3));

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
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        BitmapImage? bitmapImage = null;
        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Mock<IImageProcessingService> imageProcessingServiceMock = new();
        imageProcessingServiceMock.Setup(x =>
            x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(bitmapImage!);

        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        TestableAssetRepository testableAssetRepository = new(_database!, pathProviderServiceMock.Object,
            imageProcessingServiceMock.Object, _imageMetadataService!, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

            imageProcessingServiceMock.Verify(x => x.LoadBitmapThumbnailImage(
                It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

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
    public void GetAssetsByPath_ThumbnailsAndFolderExistButBinExists_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            string filePath1 = Path.Combine(folderPath2, _asset3!.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);
            string filePath2 = Path.Combine(folderPath2, _asset2!.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset3!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            _database!.WriteBlob(blobToWrite, _asset3!.Folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset3, assetData1);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(2));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset2.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset3.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath1][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath2][_asset2.FileName], Is.EqualTo(assetData2));

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset3.FileName));

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
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            _asset2 = _asset2!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });
            string filePath = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset2, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);
            Assert.That(thumbnails[folderPath1], Is.Empty);
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2][_asset2.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset2.FileName));

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
    public void GetAssetsByPath_ThumbnailsAndFolderExistButThumbnailsDictionaryEntriesToKeepIs0_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets1 = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets1, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails1 = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails1, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets1, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets1[0].ImageData, Is.Null);

            _testableAssetRepository.SaveCatalog(folder);

            // Create a new repository with ThumbnailsDictionaryEntriesToKeep = 0 using a fresh database
            // pointing to the same directory (data already persisted above via SaveCatalog)
            // This ensures Thumbnails[folder.Path] is loaded then immediately evicted by
            // RemoveOldThumbnailsDictionaryEntries, making Thumbnails.TryGetValue return false
            Mock<IConfigurationRoot> configurationRootMock = new();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

            Mock<IPathProviderService> pathProviderServiceMock = new();
            pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

            UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
            ImageProcessingService imageProcessingService = new();
            FileOperationsService fileOperationsService = new(userConfigurationService);
            ImageMetadataService imageMetadataService = new(fileOperationsService);
            PhotoManager.Infrastructure.Database.Database database = new(
                new ObjectListStorage(), new BlobStorage(), new BackupStorage());
            TestableAssetRepository testableAssetRepository = new(database, pathProviderServiceMock.Object,
                imageProcessingService, imageMetadataService, userConfigurationService);

            List<Asset> cataloguedAssets2 = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets2, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets2[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets2[0].ImageData, Is.Null);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails2 = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails2, Is.Empty);

            Asset[] assets = testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets2[0].ImageData, Is.Null);
            Assert.That(thumbnails2, Is.Empty);
            Assert.That(assets, Is.Empty);

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
    public void GetAssetsByPath_AssetFolderIsDefault_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = _dataDirectory!;
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderDoesNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Guid folderId = Guid.NewGuid();
            Folder folder = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(_dataDirectory!, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

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
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            _testableAssetRepository!.AddFolder(folderPath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExistButBinExists_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_DirectoryIsNull_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? directory = null;
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath1);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(directory!);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

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
    public void GetAssetsByPath_ExceptionThrown_ReturnsAssetsWithPartialDataAndLogsIt()
    {
        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Mock<IImageProcessingService> imageProcessingServiceMock = new();
        imageProcessingServiceMock.Setup(x =>
            x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception());

        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        TestableAssetRepository testableAssetRepository = new(_database!, pathProviderServiceMock.Object,
            imageProcessingServiceMock.Object, _imageMetadataService!, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset3 = _asset3!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset3.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset3, assetData1);
            testableAssetRepository.AddAsset(_asset2, assetData2);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);

            Asset[] assets = testableAssetRepository.GetAssetsByPath(folderPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cataloguedAssets[0].ImageData, Is.Null);
                Assert.That(cataloguedAssets[1].ImageData, Is.Null);

                Assert.That(thumbnails, Has.Count.EqualTo(1));
                Assert.That(thumbnails.ContainsKey(folderPath), Is.True);

                Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));

                Assert.That(thumbnails[folderPath].ContainsKey(_asset3.FileName), Is.True);
                Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);

                Assert.That(thumbnails[folderPath][_asset3.FileName], Is.EqualTo(assetData1));
                Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

                Assert.That(assets, Has.Length.EqualTo(2));
                Assert.That(assets[0].FileName, Is.EqualTo(_asset3.FileName));
                Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));

                imageProcessingServiceMock.Verify(x => x.LoadBitmapThumbnailImage(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

                Exception exception = new("Exception of type 'System.Exception' was thrown.");
                Exception[] expectedExceptions = [exception];
                Type typeOfService = typeof(AssetRepository);

                loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            string filePath1 = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder2);
            string filePath2 = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder2);
            string filePath3 = Path.Combine(folderPath2, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
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
                () => assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1),
                () => assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(2));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath2][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData3));

            Assert.That(assets1, Has.Length.EqualTo(1));
            Assert.That(assets2, Has.Length.EqualTo(2));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[1].FileName, Is.EqualTo(_asset3!.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
