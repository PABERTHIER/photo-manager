namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailNotDuplicatesTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private TestableFindDuplicatedAssetsService? _testableFindDuplicatedAssetsService;
    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;
    private Asset? asset4;
    private Asset? asset5;
    private Asset? asset6;
    private Asset? asset7;
    private Asset? asset8;

    private readonly string asset1336Hash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private readonly string asset1337Hash = "4435b989c27e16be1bf583fb6bb4b3474bd55735447a52e663482703553c5ce38947b17f88a2bfd9662956b24575e7578f7b7d921fdba2273d6046ee765eec36";
    private readonly string asset1349Hash = "715188ad52e6bd3dc9dfb7c0469038d92638dae94b7818f6326da093505f18b60d277831d451e259809af7dad3149cc2babf7442d1956ceca911f75180365cec";
    private readonly string asset1350Hash = "a4e02e2ac49b00c7b1b5f53d76d397b88a01148b16d920be66f8f6b7ebe6cb199e964a5129ee41a80db67b699bc3ffa04c6d87e8a2d89cdf46a8130287d11e7d";
    private readonly string asset1413Hash = "7c2961e6e6842b31a1dea11bfb655c5a2afba42b15c66c2dc155e6b86f2e510c0e2196149fa6d5e94e1c799c8a58503235f6f5f09b9cf618402c927991b6ae4c";
    private readonly string asset1414Hash = "4d4e53fa11a8e2ffde7e02fa774c959a87c24b9f70ab3dbb8ff6c3384808bfc9a822adb85ae77bef690f93469b2d575bc995317e2af5a63ae3151a04c5df57c7";
    private readonly string asset1415Hash = "5f122c429a160e0eb1df1a6b77cea6ee4c13b731ebff02c79646314d5bf57070d4fae13a1a8821927d7954f83ae706c937affb452ed32a38dc800e00ef0f6176";

    private readonly string asset1336MD5Hash = "bed3d05d409001724e5f6095c2acaa95";
    private readonly string asset1337MD5Hash = "49f629639e6a86f43cc6863da60edc16";
    private readonly string asset1349MD5Hash = "40b609ff0eafe7c6a767e05daed14850";
    private readonly string asset1350MD5Hash = "a0501328ef7e513beadd169965353a0b";
    private readonly string asset1413MD5Hash = "c77db382d58e7cecca32bf39c966c8b9";
    private readonly string asset1414MD5Hash = "62ae52fd23a004cb2b70fdbf631712bf";
    private readonly string asset1415MD5Hash = "8a81b304ab3be092d2e46085fe818782";

    private readonly string asset1336DHash = "1090783378";
    private readonly string asset1337DHash = "0";
    private readonly string asset1349DHash = "69788202038198207";
    private readonly string asset1350DHash = "58456436619323367";
    private readonly string asset1413DHash = "62941001906964359";
    private readonly string asset1414DHash = "58526839426719643";
    private readonly string asset1415DHash = "29520682973338550";

    private readonly string asset1336PHash = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private readonly string asset1337PHash = "aba21844698994389c3061fba8c26061fa362ee062ee062ee062ee062ee062ee062ee0aba21844698994389c3061fba8c26061fa362ee062ee062ee062ee062ee062ee062ee0aba21844698994389c3061fba8c26061fa3aba21844698994389c3061fba8c26061fa3";
    private readonly string asset1349PHash = "ab6ba84445890c1895e961dea8b85961f1662ee062ee062ee062ee062ee062ee062ee0ab6ba84445890c1895e961dea8b85961f1662ee062ee062ee062ee062ee062ee062ee0ab6ba84445890c1895e961dea8b85961f16ab6ba84445890c1895e961dea8b85961f16";
    private readonly string asset1350PHash = "abc9584a4e88a8388f3e61c818b47a61e5862ee062ee062ee062ee062ee062ee062ee0abc9584a4e88a8388f3e61c818b47a61e5862ee062ee062ee062ee062ee062ee062ee0abc9584a4e88a8388f3e61c818b47a61e58abc9584a4e88a8388f3e61c818b47a61e58";
    private readonly string asset1413PHash = "a66038379386f52862528d84e87e738cd2962ee062ee062ee062ee062ee062ee062ee0a66038379386f52862528d84e87e738cd2962ee062ee062ee062ee062ee062ee062ee0a66038379386f52862528d84e87e738cd29a66038379386f52862528d84e87e738cd29";
    private readonly string asset1414PHash = "a610d836af86c76860278de5287c4d8c6fc62ee062ee062ee062ee062ee062ee062ee0a610d836af86c76860278de5287c4d8c6fc62ee062ee062ee062ee062ee062ee062ee0a610d836af86c76860278de5287c4d8c6fca610d836af86c76860278de5287c4d8c6fc";
    private readonly string asset1415PHash = "a74048491d84b8c84917893a186db68b25562ee062ee062ee062ee062ee062ee062ee0a74048491d84b8c84917893a186db68b25562ee062ee062ee062ee062ee062ee062ee0a74048491d84b8c84917893a186db68b255a74048491d84b8c84917893a186db68b255";

    private readonly string miscAssetHash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
    private readonly string miscAssetMD5Hash = "a886f237e1cf6da03fb793588fd79b64";
    private readonly string miscAssetDHash = "23831507941108244";
    private readonly string miscAssetPHash = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new (userConfigurationService);
        _testableFindDuplicatedAssetsService = new(_assetRepository!, _storageService!, userConfigurationService);

        // TODO: Update the assets !!
        asset1 = new()
        {
            FolderId = new Guid("65a65540-586e-4b2e-a62a-ccdbd7d442f2"),
            FileName = "1336.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("65a65540-586e-4b2e-a62a-ccdbd7d442f2"),
            FileName = "1337.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("ee3dcc63-53d8-4937-a38a-45b3189118e6"),
            FileName = "1349.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset4 = new()
        {
            FolderId = new Guid("ee3dcc63-53d8-4937-a38a-45b3189118e6"),
            FileName = "1350.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new()
        {
            FolderId = new Guid("4c0b2c7c-3159-4855-850b-27c7e737a96e"),
            FileName = "1413.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset6 = new()
        {
            FolderId = new Guid("4c0b2c7c-3159-4855-850b-27c7e737a96e"),
            FileName = "1414.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset7 = new()
        {
            FolderId = new Guid("4c0b2c7c-3159-4855-850b-27c7e737a96e"),
            FileName = "1415.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 10, 25, 10, 49, 38),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset8 = new() // Another asset completely different than the actual asset
        {
            FolderId = new Guid("5b506904-45af-4eaf-b402-53f6d0012075"),
            FileName = "Image 1.jpg",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1280,
            PixelHeight = 720,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
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
    [TestCase((ushort)20, 0, new string[] { })]
    [TestCase((ushort)40, 0, new string[] { })]
    [TestCase((ushort)60, 0, new string[] { })]
    [TestCase((ushort)80, 0, new string[] { })]
    [TestCase((ushort)100, 0, new string[] { })]
    [TestCase((ushort)110, 0, new string[] { })]
    [TestCase((ushort)128, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateBasicHashDifferentThresholdValues(ushort threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336Hash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1337Hash;
            asset3!.Folder = folder2;
            asset3.Hash = asset1349Hash;
            asset4!.Folder = folder2;
            asset4.Hash = asset1350Hash;
            asset5!.Folder = folder3;
            asset5.Hash = asset1413Hash;
            asset6!.Folder = folder3;
            asset6.Hash = asset1414Hash;
            asset7!.Folder = folder3;
            asset7.Hash = asset1415Hash;
            asset8!.Folder = folder4;
            asset8.Hash = miscAssetHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            List<List<Asset>> duplicatedAssets = _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold);

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

    // The hamming distance is about 26/32 between these hashes
    [Test]
    [Category("NotDuplicate folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase((ushort)5, 0, new string[] { })]
    [TestCase((ushort)10, 0, new string[] { })]
    [TestCase((ushort)15, 0, new string[] { })]
    [TestCase((ushort)20, 0, new string[] { })]
    [TestCase((ushort)25, 0, new string[] { })]
    [TestCase((ushort)32, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateMD5HashDifferentThresholdValues(ushort threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336MD5Hash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1337MD5Hash;
            asset3!.Folder = folder2;
            asset3.Hash = asset1349MD5Hash;
            asset4!.Folder = folder2;
            asset4.Hash = asset1350MD5Hash;
            asset5!.Folder = folder3;
            asset5.Hash = asset1413MD5Hash;
            asset6!.Folder = folder3;
            asset6.Hash = asset1414MD5Hash;
            asset7!.Folder = folder3;
            asset7.Hash = asset1415MD5Hash;
            asset8!.Folder = folder4;
            asset8.Hash = miscAssetMD5Hash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            List<List<Asset>> duplicatedAssets = _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold);

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
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase((ushort)3)]
    [TestCase((ushort)5)]
    [TestCase((ushort)9)]
    [TestCase((ushort)11)]
    [TestCase((ushort)14)]
    [TestCase((ushort)17)]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample1DHashDifferentThresholdValues(ushort threshold)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath4 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder4 = new() { Path = folderPath4 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336DHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1337DHash;
            asset8!.Folder = folder4;
            asset8.Hash = miscAssetDHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold));

            Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 15 between these hashes
    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase((ushort)3, 0, new string[] { })]
    [TestCase((ushort)5, 0, new string[] { })]
    [TestCase((ushort)9, 0, new string[] { })]
    [TestCase((ushort)11, 0, new string[] { })]
    [TestCase((ushort)14, 1, new string[] { "1349.JPG", "Image 1.jpg" })]
    [TestCase((ushort)17, 1, new string[] { "1349.JPG", "1350.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample2DHashDifferentThresholdValues(ushort threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset3!.Folder = folder1;
            asset3.Hash = asset1349DHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset1350DHash;
            asset8!.Folder = folder2;
            asset8.Hash = miscAssetDHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            List<List<Asset>> duplicatedAssets = _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold);

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

    // The hamming distance is about 15 between these hashes
    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 17-character number
    [TestCase((ushort)3, 0, new string[] { }, new string[] { })]
    [TestCase((ushort)5, 0, new string[] { }, new string[] { })]
    [TestCase((ushort)9, 0, new string[] { }, new string[] { })]
    [TestCase((ushort)11, 0, new string[] { }, new string[] { })]
    [TestCase((ushort)14, 2, new string[] { "1413.JPG", "Image 1.jpg" }, new string[] { "1415.JPG", "Image 1.jpg" })] // Weird result
    [TestCase((ushort)17, 1, new string[] { "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample3DHashDifferentThresholdValues(ushort threshold, int expected, string[] assetsName1, string[] assetsName2)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset5!.Folder = folder1;
            asset5.Hash = asset1413DHash;
            asset6!.Folder = folder1;
            asset6.Hash = asset1414DHash;
            asset7!.Folder = folder1;
            asset7.Hash = asset1415DHash;
            asset8!.Folder = folder2;
            asset8.Hash = miscAssetDHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            List<List<Asset>> duplicatedAssets = _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 76/88 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("NotDuplicate folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase((ushort)10, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)20, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)30, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)40, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)50, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)60, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase((ushort)80, 3, new string[] { "1336.JPG", "1349.JPG" }, new string[] { "1337.JPG", "1349.JPG" }, new string[] { "1413.JPG", "1414.JPG" })]  // Weird result
    [TestCase((ushort)90, 2, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG" }, new string[] { "1413.JPG", "1414.JPG" }, new string[] { })]  // Weird result
    [TestCase((ushort)100, 2, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG" }, new string[] { "1413.JPG", "1414.JPG" }, new string[] { })]  // Weird result
    [TestCase((ushort)120, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase((ushort)140, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase((ushort)160, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase((ushort)180, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase((ushort)210, 1, new string[] { "1336.JPG", "1337.JPG", "1349.JPG", "1350.JPG", "1413.JPG", "1414.JPG", "1415.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicatePHashDifferentThresholdValues(ushort threshold, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample1");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample2");
            string folderPath3 = Path.Combine(dataDirectory!, "Duplicates\\NotDuplicate\\Sample3");
            string folderPath4 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };
            Folder folder3 = new() { Path = folderPath3 };
            Folder folder4 = new() { Path = folderPath4 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336PHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1337PHash;
            asset3!.Folder = folder2;
            asset3.Hash = asset1349PHash;
            asset4!.Folder = folder2;
            asset4.Hash = asset1350PHash;
            asset5!.Folder = folder3;
            asset5.Hash = asset1413PHash;
            asset6!.Folder = folder3;
            asset6.Hash = asset1414PHash;
            asset7!.Folder = folder3;
            asset7.Hash = asset1415PHash;
            asset8!.Folder = folder4;
            asset8.Hash = miscAssetPHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            List<List<Asset>> duplicatedAssets = _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
