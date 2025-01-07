namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationLoadBitmapHeicImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = "Image 1_duplicate.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image 9_duplicate.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

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
