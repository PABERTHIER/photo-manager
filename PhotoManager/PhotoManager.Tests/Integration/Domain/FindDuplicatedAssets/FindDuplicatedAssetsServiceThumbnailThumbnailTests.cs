namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailThumbnailTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;
    private Asset? asset4;
    private Asset? asset5;

    private readonly string originalAssetHash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private readonly string miniAssetHash = "768d0fd37e280f1ef8d2a17702626ca1b290521167c22b0aecadd3cc2f858a88ed59a72f7a4b36c123f487da69842e55e4407de0dd248076947ce7df53e92be4";
    private readonly string shitQualityAssetHash = "0861602a84f5bf56e5f4acf49c1d98da8129542e871208a9c9f25eb497cdc65a75ccf4941874c2be7f4e36309053fe34d61a42b4d1b266910918a0b68f19cfc2";
    private readonly string smallAssetHash = "787146674307ab1e867c0961968dfea28aafc7d45e6cfd83f1562fbd201d029812289cd692c36a4f537c576b25af03c9f6f3cc996a8651f3c69f8240ecb3d5f4";

    private readonly string originalAssetMD5Hash = "bed3d05d409001724e5f6095c2acaa95";
    private readonly string miniAssetMD5Hash = "051a784180ba8c0fce93ce0b81add76c";
    private readonly string shitQualityAssetMD5Hash = "5ec22ad598e3518bf7a5b78344ee8543";
    private readonly string smallAssetMD5Hash = "1f060a67916d271bd37c366d5e1287e8";

    private readonly string originalAssetDHash = "1090783378";
    private readonly string miniAssetDHash = "61348232627230616";
    private readonly string shitQualityAssetDHash = "0";
    private readonly string smallAssetDHash = "0";

    private readonly string originalAssetPHash = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private readonly string miniAssetPHash = "abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f63abd4f8464c8af0d8926261f378b61d61f63";
    private readonly string shitQualityAssetPHash = "abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f69abd4c846458af188927261f388b63261f69";
    private readonly string smallAssetPHash = "abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f58abd60846478ae848929b61f458b66761f58";

    private readonly string miscAssetHash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
    private readonly string miscAssetMD5Hash = "a886f237e1cf6da03fb793588fd79b64";
    private readonly string miscAssetDHash = "23831507941108244";
    private readonly string miscAssetPHash = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
        _configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");
        _configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new (userConfigurationService);

        asset1 = new()
        {
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            FileName = "Image_1336_Mini.JPG",
            FileSize = 1586,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 43,
            PixelHeight = 32,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 149,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            FileName = "Image_1336_Original.JPG",
            FileSize = 4526710,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            FileName = "Image_1336_ShitQuality.JPG",
            FileSize = 370819,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset4 = new()
        {
            FolderId = new Guid("ff140210-e4db-4c2c-96c1-85faed197aa7"),
            FileName = "Image_1336_Small.JPG",
            FileSize = 53227,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 591,
            PixelHeight = 443,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new() // Another asset completely different than the actual asset
        {
            FolderId = new Guid("5b506904-45af-4eaf-b402-53f6d0012075"),
            FileName = "Image 1.jpg",
            FileSize = 29857,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1280,
            PixelHeight = 720,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
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
    [TestCase("128", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = miniAssetHash;
            asset2!.Folder = folder1;
            asset2.Hash = originalAssetHash;
            asset3!.Folder = folder1;
            asset3.Hash = shitQualityAssetHash;
            asset4!.Folder = folder1;
            asset4.Hash = smallAssetHash;
            asset5!.Folder = folder2;
            asset5.Hash = miscAssetHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
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
    [TestCase("32", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = miniAssetMD5Hash;
            asset2!.Folder = folder1;
            asset2.Hash = originalAssetMD5Hash;
            asset3!.Folder = folder1;
            asset3.Hash = shitQualityAssetMD5Hash;
            asset4!.Folder = folder1;
            asset4.Hash = smallAssetMD5Hash;
            asset5!.Folder = folder2;
            asset5.Hash = miscAssetMD5Hash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
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

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = miniAssetDHash;
            asset2!.Folder = folder1;
            asset2.Hash = originalAssetDHash;
            asset3!.Folder = folder1;
            asset3.Hash = shitQualityAssetDHash;
            asset4!.Folder = folder1;
            asset4.Hash = smallAssetDHash;
            asset5!.Folder = folder2;
            asset5.Hash = miscAssetDHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => findDuplicatedAssetsService.GetDuplicatedAssets());

            Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 36/74 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Thumbnail folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("30", 0, new string[] { })]
    [TestCase("40", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG" })]
    [TestCase("50", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG" })]
    [TestCase("60", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("80", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("90", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("100", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG" })]
    [TestCase("120", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("140", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("160", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("180", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    [TestCase("210", 1, new string[] { "Image_1336_Mini.JPG", "Image_1336_Original.JPG", "Image_1336_ShitQuality.JPG", "Image_1336_Small.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailPHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = miniAssetPHash;
            asset2!.Folder = folder1;
            asset2.Hash = originalAssetPHash;
            asset3!.Folder = folder1;
            asset3.Hash = shitQualityAssetPHash;
            asset4!.Folder = folder1;
            asset4.Hash = smallAssetPHash;
            asset5!.Folder = folder2;
            asset5.Hash = miscAssetPHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
