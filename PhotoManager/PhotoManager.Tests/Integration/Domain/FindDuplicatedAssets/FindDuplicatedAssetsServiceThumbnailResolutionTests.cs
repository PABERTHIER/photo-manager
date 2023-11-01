using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailResolutionTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private TestableFindDuplicatedAssetsService? _testableFindDuplicatedAssetsService;
    private IAssetRepository? _assetRepository;
    private IStorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;
    private Asset? asset4;
    private Asset? asset5;
    private Asset? asset6;
    private Asset? asset7;

    private readonly string asset1KHash = "4dcbbf63585dc9fff056a0370ee33efc3d7c57b556bce917f89c5012fbacbc1ee2929f7d3a11ee2f8af426b183212d67167d87e48b80707ab5a1fb4e766d2544";
    private readonly string asset2KHash = "76a8f0b611fdd32b73bff92a186449a7e4bf0b84d618fe8b9666bbf2b022ff6bf259a4a98be42db7d9043ff3c3ff20ae720c5067966c40d16ad3fc67769f514b";
    private readonly string asset3KHash = "98044e6674187219ab629c901dfdf5a72f2535104d83cd167d1bdd9039ded91b7a585b74c5c89860f2e0fab85af9e2181265cba48b908028a42b8ec042265f03";
    private readonly string asset4KOriginalHash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private readonly string asset8KHash = "18289324fb22b8b337b1f4a7d627c605056e04fd308699bc831af0579cbc2d6a5959811dbecce09b26f82c341ea82d02d5c99c1580f50c24106d19c88bd6eba2";
    private readonly string assetThumbnailHash = "0bbabdb2fe28d4ef53b78a917eb89938fe564d758ebb9cb011e5a675dbee0a8e92bc38b8fe0762e279d48e95e7d17f1bb392fd96e8db7006e9e0bb60c5d2ed31";

    private readonly string asset1KMD5Hash = "80bbaf9483ef57ae043863fa954615c6";
    private readonly string asset2KMD5Hash = "83355feec0abe189e3b3f1bcff375151";
    private readonly string asset3KMD5Hash = "02433546c8d0b8a9b76f861077924f85";
    private readonly string asset4KOriginalMD5Hash = "bed3d05d409001724e5f6095c2acaa95";
    private readonly string asset8KMD5Hash = "961b08d1e4b7d9ed07fd9c5b83d23edb";
    private readonly string assetThumbnailMD5Hash = "dd33165a8c3329683ca1ce2bb90ed1b2";

    private readonly string asset1KDHash = "1144041865478144";
    private readonly string asset2KDHash = "3951120804249760";
    private readonly string asset3KDHash = "54473113164973580";
    private readonly string asset4KOriginalDHash = "1090783378";
    private readonly string asset8KDHash = "276890633";
    private readonly string assetThumbnailDHash = "8421922";

    private readonly string asset1KPHash = "abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f62abf0a846dd8aef48929a61f458b6af61f62";
    private readonly string asset2KPHash = "abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f62abf0b846dd8aef18929961f458b6af61f62";
    private readonly string asset3KPHash = "abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f54abe7a846b68aeb78925961f3a8b65a61f54";
    private readonly string asset4KOriginalPHash = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private readonly string asset8KPHash = "abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6aabf6a846f88af0c892c361f4d8b6e661f6a";
    private readonly string assetThumbnailPHash = "abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f60abf0f846de8aeee8929b61f478b6b061f60";

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
        _configurationRootMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new StorageService(userConfigurationService);
        _testableFindDuplicatedAssetsService = new(_assetRepository!, _storageService!);

        asset1 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_1K.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1024,
            PixelHeight = 768,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_2K.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2048,
            PixelHeight = 1536,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_3K.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2880,
            PixelHeight = 2160,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset4 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_4K_Original.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_8K.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 7680,
            PixelHeight = 5760,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset6 = new()
        {
            FolderId = new Guid("fb379e47-0fe4-4ba9-9ff3-cbd779dca559"),
            FileName = "1336_Thumbnail.JPG",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 202,
            PixelHeight = 152,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset7 = new() // Another asset completely different than the actual asset
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

    // The hamming distance is about 117 between these hashes
    [Test]
    [Category("Resolution folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase(20, 0, new string[] { })]
    [TestCase(40, 0, new string[] { })]
    [TestCase(60, 0, new string[] { })]
    [TestCase(80, 0, new string[] { })]
    [TestCase(100, 0, new string[] { })]
    [TestCase(110, 0, new string[] { })]
    [TestCase(128, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionBasicHashDifferentThresholdValues(int threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1KHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset2KHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset3KHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset4KOriginalHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset8KHash;
            asset6!.Folder = folder1;
            asset6.Hash = assetThumbnailHash;
            asset7!.Folder = folder2;
            asset7.Hash = miscAssetHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);

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

    // The hamming distance is about 30 between these hashes
    [Test]
    [Category("Resolution folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase(5, 0, new string[] { })]
    [TestCase(10, 0, new string[] { })]
    [TestCase(15, 0, new string[] { })]
    [TestCase(20, 0, new string[] { })]
    [TestCase(25, 0, new string[] { })]
    [TestCase(32, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionMD5HashDifferentThresholdValues(int threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1KMD5Hash;
            asset2!.Folder = folder1;
            asset2.Hash = asset2KMD5Hash;
            asset3!.Folder = folder1;
            asset3.Hash = asset3KMD5Hash;
            asset4!.Folder = folder1;
            asset4.Hash = asset4KOriginalMD5Hash;
            asset5!.Folder = folder1;
            asset5.Hash = asset8KMD5Hash;
            asset6!.Folder = folder1;
            asset6.Hash = assetThumbnailMD5Hash;
            asset7!.Folder = folder2;
            asset7.Hash = miscAssetMD5Hash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);

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
    [Category("Resolution folder, DHash")] // The DHash is a 17-character number
    [TestCase(3)]
    [TestCase(5)]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(14)]
    [TestCase(17)]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionDHashDifferentThresholdValues(int threshold)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1KDHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset2KDHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset3KDHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset4KOriginalDHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset8KDHash;
            asset6!.Folder = folder1;
            asset6.Hash = assetThumbnailDHash;
            asset7!.Folder = folder2;
            asset7.Hash = miscAssetDHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);

            List<Asset> assets = _assetRepository.GetCataloguedAssets();

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _testableFindDuplicatedAssetsService!.GetDuplicatesBetweenOriginalAndThumbnailTestable(assets, threshold));

            Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 36/74 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Resolution folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase(10, 0, new string[] { })]
    [TestCase(20, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG" })]
    [TestCase(30, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG" })]
    [TestCase(40, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(50, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(60, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(80, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(90, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(100, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG" })]
    [TestCase(120, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase(140, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase(160, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase(180, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    [TestCase(210, 1, new string[] { "1336_1K.JPG", "1336_2K.JPG", "1336_3K.JPG", "1336_4K_Original.JPG", "1336_8K.JPG", "1336_Thumbnail.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ResolutionPHashDifferentThresholdValues(int threshold, int expected, string[] assetsName)
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Resolution");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1KPHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset2KPHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset3KPHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset4KOriginalPHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset8KPHash;
            asset6!.Folder = folder1;
            asset6.Hash = assetThumbnailPHash;
            asset7!.Folder = folder2;
            asset7.Hash = miscAssetPHash; // If this asset is in the set, then the treshold is not good

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);

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
}
