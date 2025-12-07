using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationLoadBitmapHeicImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
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
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
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
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
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
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        AssetRepository assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (assetRepository, storageService, userConfigurationService);
        _application = new (assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
    }

    [Test]
    public async Task LoadBitmapHeicImageFromPath_CataloguedAssets_ReturnsBitmapImage()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            await _application!.CatalogAssetsAsync(_ => {});

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

            Assert.That(assets, Has.Length.EqualTo(4));

            BitmapImage image1 = _application!.LoadBitmapHeicImageFromPath(assets[0].FullPath, assets[0].ImageRotation);
            BitmapImage image2 = _application!.LoadBitmapHeicImageFromPath(assets[1].FullPath, assets[1].ImageRotation);
            BitmapImage image3 = _application!.LoadBitmapHeicImageFromPath(assets[2].FullPath, assets[2].ImageRotation);
            BitmapImage image4 = _application!.LoadBitmapHeicImageFromPath(assets[3].FullPath, assets[3].ImageRotation);

            Assert.That(image1, Is.Not.Null);
            Assert.That(image1.StreamSource, Is.Not.Null);
            Assert.That(image1.Rotation, Is.EqualTo(_asset1!.ImageRotation));
            Assert.That(image1.Width, Is.EqualTo(_asset1.Pixel.Asset.Width));
            Assert.That(image1.Height, Is.EqualTo(_asset1.Pixel.Asset.Height));
            Assert.That(image1.PixelWidth, Is.EqualTo(_asset1.Pixel.Asset.Width));
            Assert.That(image1.PixelHeight, Is.EqualTo(_asset1.Pixel.Asset.Height));
            Assert.That(image1.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image1.DecodePixelHeight, Is.EqualTo(0));

            Assert.That(image2, Is.Not.Null);
            Assert.That(image2.StreamSource, Is.Not.Null);
            Assert.That(image2.Rotation, Is.EqualTo(_asset2!.ImageRotation));
            Assert.That((int)image2.Width, Is.EqualTo(1307)); // Should be _asset2.Pixel.Asset.Width -> 1280 (weird result for png)
            Assert.That((int)image2.Height, Is.EqualTo(735)); // Should be _asset2.Pixel.Asset.Height -> 720 (weird result for png)
            Assert.That(image2.PixelWidth, Is.EqualTo(_asset2.Pixel.Asset.Width));
            Assert.That(image2.PixelHeight, Is.EqualTo(_asset2.Pixel.Asset.Height));
            Assert.That(image2.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image2.DecodePixelHeight, Is.EqualTo(0));

            Assert.That(image3, Is.Not.Null);
            Assert.That(image3.StreamSource, Is.Not.Null);
            Assert.That(image3.Rotation, Is.EqualTo(_asset3!.ImageRotation));
            Assert.That((int)image3.Width, Is.EqualTo(1307)); // Should be _asset3.Pixel.Asset.Width -> 1280 (weird result for png)
            Assert.That((int)image3.Height, Is.EqualTo(735)); // Should be _asset3.Pixel.Asset.Height -> 720 (weird result for png)
            Assert.That(image3.PixelWidth, Is.EqualTo(_asset3.Pixel.Asset.Width));
            Assert.That(image3.PixelHeight, Is.EqualTo(_asset3.Pixel.Asset.Height));
            Assert.That(image3.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image3.DecodePixelHeight, Is.EqualTo(0));

            Assert.That(image4, Is.Not.Null);
            Assert.That(image4.StreamSource, Is.Not.Null);
            Assert.That(image4.Rotation, Is.EqualTo(_asset4!.ImageRotation));
            Assert.That(image4.Width, Is.EqualTo(_asset4.Pixel.Asset.Width));
            Assert.That(image4.Height, Is.EqualTo(_asset4.Pixel.Asset.Height));
            Assert.That(image4.PixelWidth, Is.EqualTo(_asset4.Pixel.Asset.Width));
            Assert.That(image4.PixelHeight, Is.EqualTo(_asset4.Pixel.Asset.Height));
            Assert.That(image4.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image4.DecodePixelHeight, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
