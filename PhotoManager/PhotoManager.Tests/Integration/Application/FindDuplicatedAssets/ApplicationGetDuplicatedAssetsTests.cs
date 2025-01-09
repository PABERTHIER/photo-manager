namespace PhotoManager.Tests.Integration.Application.FindDuplicatedAssets;

[TestFixture]
public class ApplicationGetDuplicatedAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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
        _asset2 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9.png",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 6000, Height = 6120 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4602393,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
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
        _asset4 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9_duplicate.png",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 6000, Height = 6120 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4602393,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11.heic",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2247285,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
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

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsBasicHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(hash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsDHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplication(100, assetsDirectory, 200, 150, true, false, false, false);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            const string dHash1 = "23831507941108244";
            const string dHash2 = "9346989754624";
            const string dHash3 = "0";
            const string dHash4 = "1090783378";
            const string dHash5 = "27162885008392837";
            const string dHash6 = "9079767575972116";
            const string dHash7 = "6879371593265376";

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(7));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];
            List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets[4];
            List<Asset> sixthDuplicatedAssetsSet = duplicatedAssets[5];
            List<Asset> seventhDuplicatedAssetsSet = duplicatedAssets[6];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(sixthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(seventhDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image_11.heic"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image_11.heic")));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("1337.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1337.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("Image_1336_ShitQuality.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_ShitQuality.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Small.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Small.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(dHash3));

            Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[2].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[3].Hash, Is.EqualTo(dHash4));

            Assert.That(fifthDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336_BottomLeftPart.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_BottomLeftPart.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash5));
            Assert.That(fifthDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_BottomPart.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_BottomPart.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash5));

            Assert.That(sixthDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336_LeftPart.JPG"));
            Assert.That(sixthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_LeftPart.JPG")));
            Assert.That(sixthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash6));
            Assert.That(sixthDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_TopPart.JPG"));
            Assert.That(sixthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_TopPart.JPG")));
            Assert.That(sixthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash6));

            Assert.That(seventhDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336_RightPart.JPG"));
            Assert.That(seventhDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_RightPart.JPG")));
            Assert.That(seventhDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash7));
            Assert.That(seventhDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_TopRightPart.JPG"));
            Assert.That(seventhDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_TopRightPart.JPG")));
            Assert.That(seventhDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash7));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsMD5Hash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, true, false, false);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            const string mD5Hash1 = "a886f237e1cf6da03fb793588fd79b64";
            const string mD5Hash2 = "05d29088572bd5a5970fc370c0ca8751";
            const string mD5Hash3 = "bed3d05d409001724e5f6095c2acaa95";

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(mD5Hash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, true, false);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            const string pHash1 = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";
            const string pHash2 = "afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee0afb2d849b98fd038dc1062da9619b162ee0";
            const string pHash3 = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(pHash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHashAndDetectThumbnails_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, "100");
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200");
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");
        configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            const string pHash1 = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";
            const string pHash2 = "afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee0afb2d849b98fd038dc1062da9619b162ee0";
            const string pHash3 = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";

            const string pHash4First = "abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f62abf0a846dd8aef48929a61f458b6af61f62";
            const string pHash4Second = "abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f62abf0b846dd8aef18929961f458b6af61f62";
            const string pHash4Third = "abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f60abf0f846de8aeee8929b61f478b6b061f60";

            const string pHash5First = "abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f63abd4f8464c8af0d8926261f378b61d61f63";
            const string pHash5Second = "abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f69abd4c846458af188927261f388b63261f69";

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(5));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];
            List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets[4];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(3));
            Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(pHash3));

            Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336_1K.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_1K.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash4First));
            Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_2K.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_2K.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash4Second));
            Assert.That(fourthDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_Thumbnail.JPG"));
            Assert.That(fourthDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_Thumbnail.JPG")));
            Assert.That(fourthDuplicatedAssetsSet[2].Hash, Is.EqualTo(pHash4Third));

            Assert.That(fifthDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image_1336_Mini.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Mini.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash5First));
            Assert.That(fifthDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image_1336_ShitQuality.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_ShitQuality.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash5Second));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssets_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string duplicatesDirectory = Path.Combine(_dataDirectory!, "Duplicates");
            string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, "NewFolder1");
            string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, "NewFolder2");
            string duplicatesNotDuplicateSample1Directory = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
            string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, "Part");
            string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, "Resolution");
            string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, "Thumbnail");

            await _application!.CatalogAssetsAsync(_ => {});

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(5));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];
            List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets[4];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(3));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(3));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(_dataDirectory!, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, "Image 1.jpg")));
            Assert.That(firstDuplicatedAssetsSet[2].FileName, Is.EqualTo("Image 1_duplicate.jpg"));
            Assert.That(firstDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 1_duplicate.jpg")));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 2 duplicated.jpg"));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(_dataDirectory!, "Image 2 duplicated.jpg")));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(_dataDirectory!, "Image 2.jpg")));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(_dataDirectory!, "Image 9.png")));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9.png")));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo("Image 9_duplicate.png"));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image 9_duplicate.png")));

            Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo("Image_11.heic"));
            Assert.That(fourthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(_dataDirectory!, "Image_11.heic")));
            Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo("Image_11.heic"));
            Assert.That(fourthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, "Image_11.heic")));

            Assert.That(fifthDuplicatedAssetsSet[0].FileName, Is.EqualTo("1336.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[0].FullPath, Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, "1336.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[1].FileName, Is.EqualTo("1336_Original.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[1].FullPath, Is.EqualTo(Path.Combine(duplicatesPartDirectory, "1336_Original.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[2].FileName, Is.EqualTo("1336_4K_Original.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[2].FullPath, Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, "1336_4K_Original.JPG")));
            Assert.That(fifthDuplicatedAssetsSet[3].FileName, Is.EqualTo("Image_1336_Original.JPG"));
            Assert.That(fifthDuplicatedAssetsSet[3].FullPath, Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, "Image_1336_Original.JPG")));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string sourcePath1 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");
            string sourcePath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = sourcePath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = sourcePath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset2 = _asset2!.WithFolder(folder2);
            _asset3 = _asset3!.WithFolder(folder2);
            _asset4 = _asset4!.WithFolder(folder2);
            _asset5 = _asset5!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Not.Empty);
            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Asset? duplicatedAsset1 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset1.FileName);
            Asset? duplicatedAsset3 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);

            Assert.That(duplicatedAsset1, Is.Not.Null);
            Assert.That(duplicatedAsset3, Is.Not.Null);
            Assert.That(duplicatedAsset1!.FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));

            Asset? duplicatedAsset2 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
            Asset? duplicatedAsset4 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);

            Assert.That(duplicatedAsset2, Is.Not.Null);
            Assert.That(duplicatedAsset4, Is.Not.Null);
            Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            const string hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960";
            string sourcePath = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset2!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);
            _assetRepository.AddAsset(_asset4!, assetData);
            _assetRepository.AddAsset(_asset5!, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Not.Empty);
            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));

            List<Asset> duplicatedAssetsSet = duplicatedAssets[0];

            Assert.That(duplicatedAssetsSet, Has.Count.EqualTo(4));

            Asset? duplicatedAsset2 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
            Asset? duplicatedAsset3 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);
            Asset? duplicatedAsset4 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);
            Asset? duplicatedAsset5 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset5.FileName);

            Assert.That(duplicatedAsset2, Is.Not.Null);
            Assert.That(duplicatedAsset3, Is.Not.Null);
            Assert.That(duplicatedAsset4, Is.Not.Null);
            Assert.That(duplicatedAsset5, Is.Not.Null);

            Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
            Assert.That(duplicatedAsset5!.FileName, Is.EqualTo(_asset5.FileName));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset3 = _asset3!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset3!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset2 = _asset2!.WithFolder(folder2);
            _asset3 = _asset3!.WithFolder(folder2);
            _asset4 = _asset4!.WithFolder(folder2);
            _asset5 = _asset5!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
