namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailResolutionTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;

    private const string ASSET1_K_HASH = "4dcbbf63585dc9fff056a0370ee33efc3d7c57b556bce917f89c5012fbacbc1ee2929f7d3a11ee2f8af426b183212d67167d87e48b80707ab5a1fb4e766d2544";
    private const string ASSET2_K_HASH = "76a8f0b611fdd32b73bff92a186449a7e4bf0b84d618fe8b9666bbf2b022ff6bf259a4a98be42db7d9043ff3c3ff20ae720c5067966c40d16ad3fc67769f514b";
    private const string ASSET3_K_HASH = "98044e6674187219ab629c901dfdf5a72f2535104d83cd167d1bdd9039ded91b7a585b74c5c89860f2e0fab85af9e2181265cba48b908028a42b8ec042265f03";
    private const string ASSET4_K_ORIGINAL_HASH = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private const string ASSET8_K_HASH = "18289324fb22b8b337b1f4a7d627c605056e04fd308699bc831af0579cbc2d6a5959811dbecce09b26f82c341ea82d02d5c99c1580f50c24106d19c88bd6eba2";
    private const string ASSET_THUMBNAIL_HASH = "0bbabdb2fe28d4ef53b78a917eb89938fe564d758ebb9cb011e5a675dbee0a8e92bc38b8fe0762e279d48e95e7d17f1bb392fd96e8db7006e9e0bb60c5d2ed31";

    private const string ASSET1_K_MD5_HASH = "80bbaf9483ef57ae043863fa954615c6";
    private const string ASSET2_K_MD5_HASH = "83355feec0abe189e3b3f1bcff375151";
    private const string ASSET3_K_MD5_HASH = "02433546c8d0b8a9b76f861077924f85";
    private const string ASSET4_K_ORIGINAL_MD5_HASH = "bed3d05d409001724e5f6095c2acaa95";
    private const string ASSET8_K_MD5_HASH = "961b08d1e4b7d9ed07fd9c5b83d23edb";
    private const string ASSET_THUMBNAIL_MD5_HASH = "dd33165a8c3329683ca1ce2bb90ed1b2";

    private const string ASSET1_K_D_HASH = "1144041865478144";
    private const string ASSET2_K_D_HASH = "3951120804249760";
    private const string ASSET3_K_D_HASH = "54473113164973580";
    private const string ASSET4_K_ORIGINAL_D_HASH = "1090783378";
    private const string ASSET8_K_D_HASH = "276890633";
    private const string ASSET_THUMBNAIL_D_HASH = "8421922";

    private const string ASSET1_K_P_HASH = "abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f62abf0a846dd8aef48929a61f458b6af61f62";
    private const string ASSET2_K_P_HASH = "abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f62abf0b846dd8aef18929961f458b6af61f62";
    private const string ASSET3_K_P_HASH = "abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f54abe7a846b68aeb78925961f3a8b65a61f54";
    private const string ASSET4_K_ORIGINAL_P_HASH = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private const string ASSET8_K_P_HASH = "abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6aabf6a846f88af0c892c361f4d8b6e661f6a";
    private const string ASSET_THUMBNAIL_P_HASH = "abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f60abf0f846de8aeee8929b61f478b6b061f60";

    private const string MISC_ASSET_HASH = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
    private const string MISC_ASSET_MD5_HASH = "a886f237e1cf6da03fb793588fd79b64";
    private const string MISC_ASSET_D_HASH = "23831507941108244";
    private const string MISC_ASSET_P_HASH = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
        _configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");
        _configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new (userConfigurationService);

        _asset1 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_1K.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1024, Height = 768 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 311067,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_2K.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2048, Height = 1536 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1154116,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_3K.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2880, Height = 2160 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2098339,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_4K_Original.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4526710,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_8K.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 7680, Height = 5760 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 8855421,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset6 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_Thumbnail.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 202, Height = 152 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 19472,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset7 = new() // Another asset completely different from the actual asset
        {
            FolderId = new Guid("5b506904-45af-4eaf-b402-53f6d0012075"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
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
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    // The hamming distance is about 117 between these hashes
    [Test]
    [Category("Resolution folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1_K_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET2_K_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET3_K_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET4_K_ORIGINAL_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET8_K_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET_THUMBNAIL_HASH);
            _asset7 = _asset7!.WithFolder(folder2).WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 30 between these hashes
    [Test]
    [Category("Resolution folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase("5", 0, new string[] { })]
    [TestCase("10", 0, new string[] { })]
    [TestCase("15", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("25", 0, new string[] { })]
    [TestCase("32", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1_K_MD5_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET2_K_MD5_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET3_K_MD5_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET4_K_ORIGINAL_MD5_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET8_K_MD5_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET_THUMBNAIL_MD5_HASH);
            _asset7 = _asset7!.WithFolder(folder2).WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance cannot be computed for this hashing method because it has not the same length
    [Test]
    [Category("Resolution folder, DHash")] // The DHash is a 17-character number
    [TestCase("3")]
    [TestCase("5")]
    [TestCase("9")]
    [TestCase("11")]
    [TestCase("14")]
    [TestCase("17")]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionDHashDifferentThresholdValues(string thresholdToMock)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1_K_D_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET2_K_D_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET3_K_D_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET4_K_ORIGINAL_D_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET8_K_D_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET_THUMBNAIL_D_HASH);
            _asset7 = _asset7!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => findDuplicatedAssetsService.GetDuplicatedAssets());

            Assert.That(exception?.Message, Is.EqualTo("Invalid arguments for hamming distance calculation."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 36/74 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Resolution folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { })]
    [TestCase("20", 1, new[] { "1336_1K.JPG", "1336_2K.JPG" })]
    [TestCase("30", 1, new[] { "1336_1K.JPG", "1336_2K.JPG" })]
    [TestCase("40", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("50", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("60", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("80", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("90", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("100", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase("120", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase("140", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase("160", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase("180", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase("210", 1, new[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionPHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1_K_P_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET2_K_P_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET3_K_P_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET4_K_ORIGINAL_P_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET8_K_P_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET_THUMBNAIL_P_HASH);
            _asset7 = _asset7!.WithFolder(folder2).WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
