namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailThumbnailTests
{
    private string? _dataDirectory;
    private string? _backupPath;
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

    private const string ORIGINAL_ASSET_HASH = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private const string MINI_ASSET_HASH = "768d0fd37e280f1ef8d2a17702626ca1b290521167c22b0aecadd3cc2f858a88ed59a72f7a4b36c123f487da69842e55e4407de0dd248076947ce7df53e92be4";
    private const string SHIT_QUALITY_ASSET_HASH = "0861602a84f5bf56e5f4acf49c1d98da8129542e871208a9c9f25eb497cdc65a75ccf4941874c2be7f4e36309053fe34d61a42b4d1b266910918a0b68f19cfc2";
    private const string SMALL_ASSET_HASH = "787146674307ab1e867c0961968dfea28aafc7d45e6cfd83f1562fbd201d029812289cd692c36a4f537c576b25af03c9f6f3cc996a8651f3c69f8240ecb3d5f4";

    private const string ORIGINAL_ASSET_MD5_HASH = "bed3d05d409001724e5f6095c2acaa95";
    private const string MINI_ASSET_MD5_HASH = "051a784180ba8c0fce93ce0b81add76c";
    private const string SHIT_QUALITY_ASSET_MD5_HASH = "5ec22ad598e3518bf7a5b78344ee8543";
    private const string SMALL_ASSET_MD5_HASH = "1f060a67916d271bd37c366d5e1287e8";

    private const string ORIGINAL_ASSET_D_HASH = "1090783378";
    private const string MINI_ASSET_D_HASH = "61348232627230616";
    private const string SHIT_QUALITY_ASSET_D_HASH = "0";
    private const string SMALL_ASSET_D_HASH = "0";

    private const string ORIGINAL_ASSET_P_HASH = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private const string MINI_ASSET_P_HASH = "abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f63abd4f8464c8af0d8926261f378b61d61f63";
    private const string SHIT_QUALITY_ASSET_P_HASH = "abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f69abd4c846458af188927261f388b63261f69";
    private const string SMALL_ASSET_P_HASH = "abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f58abd60846478ae848929b61f458b66761f58";

    private const string MISC_ASSET_HASH = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
    private const string MISC_ASSET_MD5_HASH = "a886f237e1cf6da03fb793588fd79b64";
    private const string MISC_ASSET_D_HASH = "23831507941108244";
    private const string MISC_ASSET_P_HASH = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
        _configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");
        _configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
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
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_Mini.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 43, Height = 32 },
                Thumbnail = new() { Width = 200, Height = 149 }
            },
            FileProperties = new()
            {
                Size = 1586,
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
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_Original.JPG",
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
        _asset3 = new()
        {
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_ShitQuality.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 370819,
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
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_Small.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 591, Height = 443 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 53227,
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
        _asset5 = new() // Another asset completely different from the actual asset
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

    // The hamming distance is about 120 between these hashes
    [Test]
    [Category("Thumbnail folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_HASH);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList = assetsName.ToList();
                Assert.IsTrue(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 30 between these hashes
    [Test]
    [Category("Thumbnail folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase("5", 0, new string[] { })]
    [TestCase("10", 0, new string[] { })]
    [TestCase("15", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("25", 0, new string[] { })]
    [TestCase("32", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_MD5_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_MD5_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_MD5_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_MD5_HASH);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList = assetsName.ToList();
                Assert.IsTrue(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance cannot be computed for this hashing method because it has not the same length
    [Test]
    [Category("Thumbnail folder, DHash")] // The DHash is a 17-character number
    [TestCase("3")]
    [TestCase("5")]
    [TestCase("9")]
    [TestCase("11")]
    [TestCase("14")]
    [TestCase("17")]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailDHashDifferentThresholdValues(string thresholdToMock)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_D_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_D_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_D_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_D_HASH);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => findDuplicatedAssetsService.GetDuplicatedAssets());

            Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 36/74 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Thumbnail folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("30", 0, new string[] { })]
    [TestCase("40", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG" })]
    [TestCase("50", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG" })]
    [TestCase("60", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("80", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("90", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("100", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("120", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("140", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("160", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("180", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("210", 1, new[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailPHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_P_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_P_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_P_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_P_HASH);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList = assetsName.ToList();
                Assert.IsTrue(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }
}
