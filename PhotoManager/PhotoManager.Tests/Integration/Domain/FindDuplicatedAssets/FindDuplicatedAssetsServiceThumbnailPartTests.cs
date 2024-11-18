namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailPartTests
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
    private Asset? asset6;
    private Asset? asset7;
    private Asset? asset8;
    private Asset? asset9;
    private Asset? asset10;

    private readonly string asset1336_BottomLeftPartHash = "c50e17cee9a52c0235afb410c361d094daf7330c20e4b3f3cb3e3e18683793355e903f84d49bf1db4af7e094980204cc005aa3c503829d14c9e6cd19826e284f";
    private readonly string asset1336_BottomPartHash = "aef59f88b3c54a5fd1bfc098acac1d662bb3799e83220049ee7c3ae5def487a1464112cce7b7384428ee9b69813448d509413a1fc1abcddc272ec2e044c5f876";
    private readonly string asset1336_BottomRightPartHash = "966370f38d7864499453b99934bb51f1654e567033e637b68ded1ec12c338eaa03d42bb4dc74621551c5dfd922180a291f7c58980bf6c94ae114e4d296a484e4";
    private readonly string asset1336_LeftPartHash = "8afa3f673b047d5096ed2a2528b37c7222ab5b44cf780a7920cea35228e93323709b09b1a2ffe1bd2f789061be9bef8ebcb642fc47a2f3b6573a68a8cd5045ee";
    private readonly string asset1336_OriginalHash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private readonly string asset1336_RightPartHash = "0e0e12ea70894ad521c8d3da14b92bbd4e7c7c03010502d779f2e485ea30e917ad6a9bc0435520f70c3448fd96cd9ccf88cf95d2a75a6da669b0f41731aabbe7";
    private readonly string asset1336_TopLeftPartHash = "f59b3913bdd53b6cb485a2c04770d1153350df2caf6dabf38b8b8311fb3fb583becbf7d368a1e0987d3584251a64bca72042d334b6e6c25edb7dfaead2db383d";
    private readonly string asset1336_TopPartHash = "93d6ea1575ea8fd4f540b88ae3ce727d28c657a11388c9b6cf927572abffdcc467d3e9b43e622b34158c18ed680c841d43e17a80c7b544261f106d887b51d2d2";
    private readonly string asset1336_TopRightPartHash = "3fde05f0e85d71a909fa38351411f3d45305857ea7a920ad22bcffaf4cf50136b4a55acb128bdc9cd9d7db600332f83e4157926a511939b0b57f2e50c020e3d7";

    private readonly string asset1336_BottomLeftPartMD5Hash = "1691eb898f26c38b8b5d66b5d5ed71c6";
    private readonly string asset1336_BottomPartMD5Hash = "bac5ead516448cc2a657ea75251ae88d";
    private readonly string asset1336_BottomRightPartMD5Hash = "25830c1fb500800d426c8832d7fabb9e";
    private readonly string asset1336_LeftPartMD5Hash = "55370d738498beff0963693512d2d94f";
    private readonly string asset1336_OriginalMD5Hash = "bed3d05d409001724e5f6095c2acaa95";
    private readonly string asset1336_RightPartMD5Hash = "01b35414fd99b583a5261ff83260b698";
    private readonly string asset1336_TopLeftPartMD5Hash = "58695d954982417e68b68975beff02c4";
    private readonly string asset1336_TopPartMD5Hash = "216eaa3b37a658bd0d288d858022c7e1";
    private readonly string asset1336_TopRightPartMD5Hash = "1ec7dfdec1b6e9aa8ad5f50aae638e0e";

    private readonly string asset1336_BottomLeftPartDHash = "70927157499523007";
    private readonly string asset1336_BottomPartDHash = "70927157499523007";
    private readonly string asset1336_BottomRightPartDHash = "62485089979447164";
    private readonly string asset1336_LeftPartDHash = "72057594037927935";
    private readonly string asset1336_OriginalDHash = "72057594037927935";
    private readonly string asset1336_RightPartDHash = "17023704168856051";
    private readonly string asset1336_TopLeftPartDHash = "72057594037927935";
    private readonly string asset1336_TopPartDHash = "72057594037927935";
    private readonly string asset1336_TopRightPartDHash = "17023704168856051";

    private readonly string asset1336_BottomLeftPartPHash = "a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c45a939484b35885058815f61a1f8a70361c45";
    private readonly string asset1336_BottomPartPHash = "a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2fa3cb9ad75a86e60869ba8da5c876de8dd2f";
    private readonly string asset1336_BottomRightPartPHash = "a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58fa757a847f2873ca86abd8dc848a10a8e58f";
    private readonly string asset1336_LeftPartPHash = "ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d62072ac9c684f6c8797587bc88f6668a37d62072";
    private readonly string asset1336_OriginalPHash = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private readonly string asset1336_RightPartPHash = "ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f286ac49784f3886c9286e8b8dcb9896648f286";
    private readonly string asset1336_TopLeftPartPHash = "ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c362685ade578542a8bf548bb8b6261a8e6c362685";
    private readonly string asset1336_TopPartPHash = "a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a8a9d838286c8b5668b5f3625668cbfa624a8";
    private readonly string asset1336_TopRightPartPHash = "ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270eae3968548a8c4158c2f562b528f01e6270e";

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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_BottomLeftPart.JPG",
            FileSize = 1119873,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 1512,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_BottomPart.JPG",
            FileSize = 2172866,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 1512,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 75,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_BottomRightPart.JPG",
            FileSize = 1061673,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 1512,
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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_LeftPart.JPG",
            FileSize = 1475892,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 100,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_Original.JPG",
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
        asset6 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_RightPart.JPG",
            FileSize = 1437903,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 3024,
            ThumbnailPixelWidth = 100,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset7 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_TopLeftPart.JPG",
            FileSize = 362012,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 1512,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset8 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_TopPart.JPG",
            FileSize = 737269,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 4032,
            PixelHeight = 1512,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 75,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset9 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            FileName = "1336_TopRightPart.JPG",
            FileSize = 383508,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 2016,
            PixelHeight = 1512,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "", // Set in each test
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset10 = new() // Another asset completely different than the actual asset
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

    // The hamming distance is about 113/124 between these hashes
    [Test]
    [Category("Part folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336_BottomLeftPartHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1336_BottomPartHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset1336_BottomRightPartHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset1336_LeftPartHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset1336_OriginalHash;
            asset6!.Folder = folder1;
            asset6.Hash = asset1336_RightPartHash;
            asset7!.Folder = folder1;
            asset7!.Hash = asset1336_TopLeftPartHash;
            asset8!.Folder = folder1;
            asset8!.Hash = asset1336_TopPartHash;
            asset9!.Folder = folder1;
            asset9!.Hash = asset1336_TopRightPartHash;
            asset10!.Folder = folder2;
            asset10.Hash = miscAssetHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);
            _assetRepository.AddAsset(asset9, assetData1);
            _assetRepository.AddAsset(asset10, assetData1);

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
    [Category("Part folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase("5", 0, new string[] { })]
    [TestCase("10", 0, new string[] { })]
    [TestCase("15", 0, new string[] { })]
    [TestCase("20", 0, new string[] { })]
    [TestCase("25", 0, new string[] { })]
    [TestCase("32", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336_BottomLeftPartMD5Hash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1336_BottomPartMD5Hash;
            asset3!.Folder = folder1;
            asset3.Hash = asset1336_BottomRightPartMD5Hash;
            asset4!.Folder = folder1;
            asset4.Hash = asset1336_LeftPartMD5Hash;
            asset5!.Folder = folder1;
            asset5.Hash = asset1336_OriginalMD5Hash;
            asset6!.Folder = folder1;
            asset6.Hash = asset1336_RightPartMD5Hash;
            asset7!.Folder = folder1;
            asset7!.Hash = asset1336_TopLeftPartMD5Hash;
            asset8!.Folder = folder1;
            asset8!.Hash = asset1336_TopPartMD5Hash;
            asset9!.Folder = folder1;
            asset9!.Hash = asset1336_TopRightPartMD5Hash;
            asset10!.Folder = folder2;
            asset10.Hash = miscAssetMD5Hash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);
            _assetRepository.AddAsset(asset9, assetData1);
            _assetRepository.AddAsset(asset10, assetData1);

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

    // The hamming distance is about 14 between these hashes (for some, 0)
    [Test]
    [Category("Part folder, DHash")] // The DHash is a 17-character number
    [TestCase("3", 3, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new string[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new string[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("5", 3, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new string[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new string[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("9", 3, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new string[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new string[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("11", 3, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new string[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new string[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("14", 2, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new string[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" }, new string[] { })]
    [TestCase("17", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartDHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336_BottomLeftPartDHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1336_BottomPartDHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset1336_BottomRightPartDHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset1336_LeftPartDHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset1336_OriginalDHash;
            asset6!.Folder = folder1;
            asset6.Hash = asset1336_RightPartDHash;
            asset7!.Folder = folder1;
            asset7!.Hash = asset1336_TopLeftPartDHash;
            asset8!.Folder = folder1;
            asset8!.Hash = asset1336_TopPartDHash;
            asset9!.Folder = folder1;
            asset9!.Hash = asset1336_TopRightPartDHash;
            asset10!.Folder = folder2;
            asset10.Hash = miscAssetDHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);
            _assetRepository.AddAsset(asset9, assetData1);
            _assetRepository.AddAsset(asset10, assetData1);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 80/100 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Part folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("20", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("30", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("40", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("50", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("60", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("80", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG" }, new string[] { }, new string[] { })]
    [TestCase("90", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG" }, new string[] { }, new string[] { })]
    [TestCase("100", 3, new string[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopRightPart.JPG" }, new string[] { "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_RightPart.JPG" }, new string[] { "1336_LeftPart.JPG", "1336_TopPart.JPG" })]
    [TestCase("120", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("140", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("160", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("180", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("210", 1, new string[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartPHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset1.Hash = asset1336_BottomLeftPartPHash;
            asset2!.Folder = folder1;
            asset2.Hash = asset1336_BottomPartPHash;
            asset3!.Folder = folder1;
            asset3.Hash = asset1336_BottomRightPartPHash;
            asset4!.Folder = folder1;
            asset4.Hash = asset1336_LeftPartPHash;
            asset5!.Folder = folder1;
            asset5.Hash = asset1336_OriginalPHash;
            asset6!.Folder = folder1;
            asset6.Hash = asset1336_RightPartPHash;
            asset7!.Folder = folder1;
            asset7!.Hash = asset1336_TopLeftPartPHash;
            asset8!.Folder = folder1;
            asset8!.Hash = asset1336_TopPartPHash;
            asset9!.Folder = folder1;
            asset9!.Hash = asset1336_TopRightPartPHash;
            asset10!.Folder = folder2;
            asset10.Hash = miscAssetPHash; // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(asset1, assetData1);
            _assetRepository.AddAsset(asset2, assetData1);
            _assetRepository.AddAsset(asset3, assetData1);
            _assetRepository.AddAsset(asset4, assetData1);
            _assetRepository.AddAsset(asset5, assetData1);
            _assetRepository.AddAsset(asset6, assetData1);
            _assetRepository.AddAsset(asset7, assetData1);
            _assetRepository.AddAsset(asset8, assetData1);
            _assetRepository.AddAsset(asset9, assetData1);
            _assetRepository.AddAsset(asset10, assetData1);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
