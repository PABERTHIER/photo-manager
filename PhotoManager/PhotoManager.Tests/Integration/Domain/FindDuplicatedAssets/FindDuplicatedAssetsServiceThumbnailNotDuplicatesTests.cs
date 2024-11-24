namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailNotDuplicatesTests
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
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;

    private const string ASSET1336_HASH = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private const string ASSET1337_HASH = "4435b989c27e16be1bf583fb6bb4b3474bd55735447a52e663482703553c5ce38947b17f88a2bfd9662956b24575e7578f7b7d921fdba2273d6046ee765eec36";
    private const string ASSET1349_HASH = "715188ad52e6bd3dc9dfb7c0469038d92638dae94b7818f6326da093505f18b60d277831d451e259809af7dad3149cc2babf7442d1956ceca911f75180365cec";
    private const string ASSET1350_HASH = "a4e02e2ac49b00c7b1b5f53d76d397b88a01148b16d920be66f8f6b7ebe6cb199e964a5129ee41a80db67b699bc3ffa04c6d87e8a2d89cdf46a8130287d11e7d";
    private const string ASSET1413_HASH = "7c2961e6e6842b31a1dea11bfb655c5a2afba42b15c66c2dc155e6b86f2e510c0e2196149fa6d5e94e1c799c8a58503235f6f5f09b9cf618402c927991b6ae4c";
    private const string ASSET1414_HASH = "4d4e53fa11a8e2ffde7e02fa774c959a87c24b9f70ab3dbb8ff6c3384808bfc9a822adb85ae77bef690f93469b2d575bc995317e2af5a63ae3151a04c5df57c7";
    private const string ASSET1415_HASH = "5f122c429a160e0eb1df1a6b77cea6ee4c13b731ebff02c79646314d5bf57070d4fae13a1a8821927d7954f83ae706c937affb452ed32a38dc800e00ef0f6176";

    private const string ASSET1336_MD5_HASH = "bed3d05d409001724e5f6095c2acaa95";
    private const string ASSET1337_MD5_HASH = "49f629639e6a86f43cc6863da60edc16";
    private const string ASSET1349_MD5_HASH = "40b609ff0eafe7c6a767e05daed14850";
    private const string ASSET1350_MD5_HASH = "a0501328ef7e513beadd169965353a0b";
    private const string ASSET1413_MD5_HASH = "c77db382d58e7cecca32bf39c966c8b9";
    private const string ASSET1414_MD5_HASH = "62ae52fd23a004cb2b70fdbf631712bf";
    private const string ASSET1415_MD5_HASH = "8a81b304ab3be092d2e46085fe818782";

    private const string ASSET1336_D_HASH = "1090783378";
    private const string ASSET1337_D_HASH = "0";
    private const string ASSET1349_D_HASH = "69788202038198207";
    private const string ASSET1350_D_HASH = "58456436619323367";
    private const string ASSET1413_D_HASH = "62941001906964359";
    private const string ASSET1414_D_HASH = "58526839426719643";
    private const string ASSET1415_D_HASH = "29520682973338550";

    private const string ASSET1336_P_HASH = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private const string ASSET1337_P_HASH = "aba21844698994389c3061fba8c26061fa362ee062ee062ee062ee062ee062ee062ee0aba21844698994389c3061fba8c26061fa362ee062ee062ee062ee062ee062ee062ee0aba21844698994389c3061fba8c26061fa3aba21844698994389c3061fba8c26061fa3";
    private const string ASSET1349_P_HASH = "ab6ba84445890c1895e961dea8b85961f1662ee062ee062ee062ee062ee062ee062ee0ab6ba84445890c1895e961dea8b85961f1662ee062ee062ee062ee062ee062ee062ee0ab6ba84445890c1895e961dea8b85961f16ab6ba84445890c1895e961dea8b85961f16";
    private const string ASSET1350_P_HASH = "abc9584a4e88a8388f3e61c818b47a61e5862ee062ee062ee062ee062ee062ee062ee0abc9584a4e88a8388f3e61c818b47a61e5862ee062ee062ee062ee062ee062ee062ee0abc9584a4e88a8388f3e61c818b47a61e58abc9584a4e88a8388f3e61c818b47a61e58";
    private const string ASSET1413_P_HASH = "a66038379386f52862528d84e87e738cd2962ee062ee062ee062ee062ee062ee062ee0a66038379386f52862528d84e87e738cd2962ee062ee062ee062ee062ee062ee062ee0a66038379386f52862528d84e87e738cd29a66038379386f52862528d84e87e738cd29";
    private const string ASSET1414_P_HASH = "a610d836af86c76860278de5287c4d8c6fc62ee062ee062ee062ee062ee062ee062ee0a610d836af86c76860278de5287c4d8c6fc62ee062ee062ee062ee062ee062ee062ee0a610d836af86c76860278de5287c4d8c6fca610d836af86c76860278de5287c4d8c6fc";
    private const string ASSET1415_P_HASH = "a74048491d84b8c84917893a186db68b25562ee062ee062ee062ee062ee062ee062ee0a74048491d84b8c84917893a186db68b25562ee062ee062ee062ee062ee062ee062ee0a74048491d84b8c84917893a186db68b255a74048491d84b8c84917893a186db68b255";

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
            FolderId = new Guid("db973547-ed18-4e7d-8dc8-ce441aabef80"),
            Folder = new() { Path = "" },
            FileName = "1336.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset2 = new()
        {
            FolderId = new Guid("65a65540-586e-4b2e-a62a-ccdbd7d442f2"),
            Folder = new() { Path = "" },
            FileName = "1337.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset3 = new()
        {
            FolderId = new Guid("65ba6b40-dc3c-46b6-8508-135cb4e1f961"),
            Folder = new() { Path = "" },
            FileName = "1349.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset4 = new()
        {
            FolderId = new Guid("ee3dcc63-53d8-4937-a38a-45b3189118e6"),
            Folder = new() { Path = "" },
            FileName = "1350.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset5 = new()
        {
            FolderId = new Guid("044e3039-0b4b-4065-b215-9cf185d1f90f"),
            Folder = new() { Path = "" },
            FileName = "1413.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset6 = new()
        {
            FolderId = new Guid("80fa1201-1b9d-4d8e-8730-6efe8e46fcbd"),
            Folder = new() { Path = "" },
            FileName = "1414.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset7 = new()
        {
            FolderId = new Guid("4c0b2c7c-3159-4855-850b-27c7e737a96e"),
            Folder = new() { Path = "" },
            FileName = "1415.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset8 = new() // Another asset completely different from the actual asset
        {
            FolderId = new Guid("5b506904-45af-4eaf-b402-53f6d0012075"),
            Folder = new() { Path = "" },
            FileName = "Image 1.jpg",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileDateTime = new()
            {
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    // The hamming distance is about 121 between these hashes
    [Test]
    [Category("NotDuplicate folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            _asset1!.Folder = folder1;
            _asset1.Hash = ASSET1336_HASH;
            _asset2!.Folder = folder1;
            _asset2.Hash = ASSET1337_HASH;
            _asset3!.Folder = folder2;
            _asset3.Hash = ASSET1349_HASH;
            _asset4!.Folder = folder2;
            _asset4.Hash = ASSET1350_HASH;
            _asset5!.Folder = folder3;
            _asset5.Hash = ASSET1413_HASH;
            _asset6!.Folder = folder3;
            _asset6.Hash = ASSET1414_HASH;
            _asset7!.Folder = folder3;
            _asset7.Hash = ASSET1415_HASH;
            _asset8!.Folder = folder4;
            _asset8.Hash = MISC_ASSET_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.IsTrue(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 26/32 between these hashes
    [Test]
    [Category("NotDuplicate folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase("5", 0, new string[] { })]
    [TestCase("10", 0, new string[] { })]
    [TestCase("15", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("25", 0, new string[] { })]
    [TestCase("32", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            _asset1!.Folder = folder1;
            _asset1.Hash = ASSET1336_MD5_HASH;
            _asset2!.Folder = folder1;
            _asset2.Hash = ASSET1337_MD5_HASH;
            _asset3!.Folder = folder2;
            _asset3.Hash = ASSET1349_MD5_HASH;
            _asset4!.Folder = folder2;
            _asset4.Hash = ASSET1350_MD5_HASH;
            _asset5!.Folder = folder3;
            _asset5.Hash = ASSET1413_MD5_HASH;
            _asset6!.Folder = folder3;
            _asset6.Hash = ASSET1414_MD5_HASH;
            _asset7!.Folder = folder3;
            _asset7.Hash = ASSET1415_MD5_HASH;
            _asset8!.Folder = folder4;
            _asset8.Hash = MISC_ASSET_MD5_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

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
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase("3")]
    [TestCase("5")]
    [TestCase("9")]
    [TestCase("11")]
    [TestCase("14")]
    [TestCase("17")]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample1DHashDifferentThresholdValues(string thresholdToMock)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath4 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder4 = new() { Path = folderPath4 };

            _asset1!.Folder = folder1;
            _asset1.Hash = ASSET1336_D_HASH;
            _asset2!.Folder = folder1;
            _asset2.Hash = ASSET1337_D_HASH;
            _asset8!.Folder = folder4;
            _asset8.Hash = MISC_ASSET_D_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => findDuplicatedAssetsService.GetDuplicatedAssets());

            Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 15 between these hashes
    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase("3", 0, new string[] { })]
    [TestCase("5", 0, new string[] { })]
    [TestCase("9", 0, new string[] { })]
    [TestCase("11", 0, new string[] { })]
    [TestCase("14", 1, new[] { "1349.JPG", "Image 1.jpg" })]
    [TestCase("17", 1, new[] { "1349.JPG", "1350.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample2DHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            _asset3!.Folder = folder1;
            _asset3.Hash = ASSET1349_D_HASH;
            _asset4!.Folder = folder1;
            _asset4.Hash = ASSET1350_D_HASH;
            _asset8!.Folder = folder2;
            _asset8.Hash = MISC_ASSET_D_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

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

    // The hamming distance is about 15 between these hashes
    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase("3", 0, new string[] { }, new string[] { })]
    [TestCase("5", 0, new string[] { }, new string[] { })]
    [TestCase("9", 0, new string[] { }, new string[] { })]
    [TestCase("11", 0, new string[] { }, new string[] { })]
    [TestCase("14", 2, new[] { "1413.JPG", "Image 1.jpg" }, new[] { "1415.JPG", "Image 1.jpg" })] // Weird result
    [TestCase("17", 1, new[] { "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample3DHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            _asset5!.Folder = folder1;
            _asset5.Hash = ASSET1413_D_HASH;
            _asset6!.Folder = folder1;
            _asset6.Hash = ASSET1414_D_HASH;
            _asset7!.Folder = folder1;
            _asset7.Hash = ASSET1415_D_HASH;
            _asset8!.Folder = folder2;
            _asset8.Hash = MISC_ASSET_D_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList1 = assetsName1.ToList();
                Assert.IsTrue(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = assetsName2.ToList();
                Assert.IsTrue(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 76/88 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("NotDuplicate folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("20", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("30", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("40", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("50", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("60", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("80", 3, new[] { "1336.JPG", "1349.JPG" }, new[] { "1337.JPG", "1349.JPG" }, new[] { "1413.JPG", "1414.JPG" })]  // Weird result
    [TestCase("90", 2, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG" }, new[] { "1413.JPG", "1414.JPG" }, new string[] { })]  // Weird result
    [TestCase("100", 2, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG" }, new[] { "1413.JPG", "1414.JPG" }, new string[] { })]  // Weird result
    [TestCase("120", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("140", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("160", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("180", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("210", 1, new[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicatePHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(_dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            _asset1!.Folder = folder1;
            _asset1.Hash = ASSET1336_P_HASH;
            _asset2!.Folder = folder1;
            _asset2.Hash = ASSET1337_P_HASH;
            _asset3!.Folder = folder2;
            _asset3.Hash = ASSET1349_P_HASH;
            _asset4!.Folder = folder2;
            _asset4.Hash = ASSET1350_P_HASH;
            _asset5!.Folder = folder3;
            _asset5.Hash = ASSET1413_P_HASH;
            _asset6!.Folder = folder3;
            _asset6.Hash = ASSET1414_P_HASH;
            _asset7!.Folder = folder3;
            _asset7.Hash = ASSET1415_P_HASH;
            _asset8!.Folder = folder4;
            _asset8.Hash = MISC_ASSET_P_HASH; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.AreEqual(expected, duplicatedAssets.Count);

            if (expected > 0)
            {
                IList<string> assetsNameList1 = assetsName1.ToList();
                Assert.IsTrue(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)));
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = assetsName2.ToList();
                Assert.IsTrue(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)));
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = assetsName3.ToList();
                Assert.IsTrue(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)));
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }
}
