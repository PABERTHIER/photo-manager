namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailPartTests
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
    private Asset? _asset9;
    private Asset? _asset10;

    private const string ASSET1336_BOTTOM_LEFT_PART_HASH = "c50e17cee9a52c0235afb410c361d094daf7330c20e4b3f3cb3e3e18683793355e903f84d49bf1db4af7e094980204cc005aa3c503829d14c9e6cd19826e284f";
    private const string ASSET1336_BOTTOM_PART_HASH = "aef59f88b3c54a5fd1bfc098acac1d662bb3799e83220049ee7c3ae5def487a1464112cce7b7384428ee9b69813448d509413a1fc1abcddc272ec2e044c5f876";
    private const string ASSET1336_BOTTOM_RIGHT_PART_HASH = "966370f38d7864499453b99934bb51f1654e567033e637b68ded1ec12c338eaa03d42bb4dc74621551c5dfd922180a291f7c58980bf6c94ae114e4d296a484e4";
    private const string ASSET1336_LEFT_PART_HASH = "8afa3f673b047d5096ed2a2528b37c7222ab5b44cf780a7920cea35228e93323709b09b1a2ffe1bd2f789061be9bef8ebcb642fc47a2f3b6573a68a8cd5045ee";
    private const string ASSET1336_ORIGINAL_HASH = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa";
    private const string ASSET1336_RIGHT_PART_HASH = "0e0e12ea70894ad521c8d3da14b92bbd4e7c7c03010502d779f2e485ea30e917ad6a9bc0435520f70c3448fd96cd9ccf88cf95d2a75a6da669b0f41731aabbe7";
    private const string ASSET1336_TOP_LEFT_PART_HASH = "f59b3913bdd53b6cb485a2c04770d1153350df2caf6dabf38b8b8311fb3fb583becbf7d368a1e0987d3584251a64bca72042d334b6e6c25edb7dfaead2db383d";
    private const string ASSET1336_TOP_PART_HASH = "93d6ea1575ea8fd4f540b88ae3ce727d28c657a11388c9b6cf927572abffdcc467d3e9b43e622b34158c18ed680c841d43e17a80c7b544261f106d887b51d2d2";
    private const string ASSET1336_TOP_RIGHT_PART_HASH = "3fde05f0e85d71a909fa38351411f3d45305857ea7a920ad22bcffaf4cf50136b4a55acb128bdc9cd9d7db600332f83e4157926a511939b0b57f2e50c020e3d7";

    private const string ASSET1336_BOTTOM_LEFT_PART_MD5_HASH = "1691eb898f26c38b8b5d66b5d5ed71c6";
    private const string ASSET1336_BOTTOM_PART_MD5_HASH = "bac5ead516448cc2a657ea75251ae88d";
    private const string ASSET1336_BOTTOM_RIGHT_PART_MD5_HASH = "25830c1fb500800d426c8832d7fabb9e";
    private const string ASSET1336_LEFT_PART_MD5_HASH = "55370d738498beff0963693512d2d94f";
    private const string ASSET1336_ORIGINAL_MD5_HASH = "bed3d05d409001724e5f6095c2acaa95";
    private const string ASSET1336_RIGHT_PART_MD5_HASH = "01b35414fd99b583a5261ff83260b698";
    private const string ASSET1336_TOP_LEFT_PART_MD5_HASH = "58695d954982417e68b68975beff02c4";
    private const string ASSET1336_TOP_PART_MD5_HASH = "216eaa3b37a658bd0d288d858022c7e1";
    private const string ASSET1336_TOP_RIGHT_PART_MD5_HASH = "1ec7dfdec1b6e9aa8ad5f50aae638e0e";

    private const string ASSET1336_BOTTOM_LEFT_PART_D_HASH = "70927157499523007";
    private const string ASSET1336_BOTTOM_PART_D_HASH = "70927157499523007";
    private const string ASSET1336_BOTTOM_RIGHT_PART_D_HASH = "62485089979447164";
    private const string ASSET1336_LEFT_PART_D_HASH = "72057594037927935";
    private const string ASSET1336_ORIGINAL_D_HASH = "72057594037927935";
    private const string ASSET1336_RIGHT_PART_D_HASH = "17023704168856051";
    private const string ASSET1336_TOP_LEFT_PART_D_HASH = "72057594037927935";
    private const string ASSET1336_TOP_PART_D_HASH = "72057594037927935";
    private const string ASSET1336_TOP_RIGHT_PART_D_HASH = "17023704168856051";

    private const string ASSET1336_BOTTOM_LEFT_PART_P_HASH = "a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c45a939484b35885058815f61a1f8a70361c45";
    private const string ASSET1336_BOTTOM_PART_P_HASH = "a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2fa3cb9ad75a86e60869ba8da5c876de8dd2f";
    private const string ASSET1336_BOTTOM_RIGHT_PART_P_HASH = "a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58fa757a847f2873ca86abd8dc848a10a8e58f";
    private const string ASSET1336_LEFT_PART_P_HASH = "ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d62072ac9c684f6c8797587bc88f6668a37d62072";
    private const string ASSET1336_ORIGINAL_P_HASH = "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71";
    private const string ASSET1336_RIGHT_PART_P_HASH = "ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f286ac49784f3886c9286e8b8dcb9896648f286";
    private const string ASSET1336_TOP_LEFT_PART_P_HASH = "ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c362685ade578542a8bf548bb8b6261a8e6c362685";
    private const string ASSET1336_TOP_PART_P_HASH = "a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a8a9d838286c8b5668b5f3625668cbfa624a8";
    private const string ASSET1336_TOP_RIGHT_PART_P_HASH = "ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270eae3968548a8c4158c2f562b528f01e6270e";

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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_BottomLeftPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1119873,
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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_BottomPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 75 }
            },
            FileProperties = new()
            {
                Size = 2172866,
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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_BottomRightPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1061673,
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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_LeftPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 3024 },
                Thumbnail = new() { Width = 100, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1475892,
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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_Original.JPG",
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
        _asset6 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_RightPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 3024 },
                Thumbnail = new() { Width = 100, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1437903,
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
        _asset7 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_TopLeftPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 362012,
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
        _asset8 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_TopPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 75 }
            },
            FileProperties = new()
            {
                Size = 737269,
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
        _asset9 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_TopRightPart.JPG",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 383508,
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
        _asset10 = new() // Another asset completely different from the actual asset
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

    // The hamming distance is about 113/124 between these hashes
    [Test]
    [Category("Part folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartBasicHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);
            _assetRepository.AddAsset(_asset9, assetData1);
            _assetRepository.AddAsset(_asset10, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = assetsName.ToList();
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
    [TestCase("32", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartMD5HashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_MD5_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_MD5_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_MD5_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_MD5_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_MD5_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_MD5_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_MD5_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_MD5_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_MD5_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);
            _assetRepository.AddAsset(_asset9, assetData1);
            _assetRepository.AddAsset(_asset10, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = assetsName.ToList();
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    // The hamming distance is about 14 between these hashes (for some, 0)
    [Test]
    [Category("Part folder, DHash")] // The DHash is a 17-character number
    [TestCase("3", 3, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("5", 3, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("9", 3, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("11", 3, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG" }, new[] { "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" })]
    [TestCase("14", 2, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG" }, new[] { "1336_RightPart.JPG", "1336_TopRightPart.JPG" }, new string[] { })]
    [TestCase("17", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartDHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_D_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_D_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_D_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_D_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_D_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_D_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_D_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_D_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_D_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);
            _assetRepository.AddAsset(_asset9, assetData1);
            _assetRepository.AddAsset(_asset10, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList1 = assetsName1.ToList();
                Assert.That(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = assetsName2.ToList();
                Assert.That(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)), Is.True);
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = assetsName3.ToList();
                Assert.That(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
    [TestCase("80", 1, new[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG" }, new string[] { }, new string[] { })]
    [TestCase("90", 1, new[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG" }, new string[] { }, new string[] { })]
    [TestCase("100", 3, new[] { "1336_BottomLeftPart.JPG", "1336_Original.JPG", "1336_TopLeftPart.JPG", "1336_TopRightPart.JPG" }, new[] { "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_RightPart.JPG" }, new[] { "1336_LeftPart.JPG", "1336_TopPart.JPG" })]
    [TestCase("120", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("140", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("160", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("180", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    [TestCase("210", 1, new[] { "1336_BottomLeftPart.JPG", "1336_BottomPart.JPG", "1336_BottomRightPart.JPG", "1336_LeftPart.JPG", "1336_Original.JPG", "1336_RightPart.JPG", "1336_TopLeftPart.JPG", "1336_TopPart.JPG", "1336_TopRightPart.JPG", "Image 1.jpg" }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartPHashDifferentThresholdValues(string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        try
        {
            _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
            UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
            FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\Part");
            string folderPath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_P_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_P_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_P_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_P_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_P_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_P_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_P_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_P_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_P_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository.AddAsset(_asset2, assetData1);
            _assetRepository.AddAsset(_asset3, assetData1);
            _assetRepository.AddAsset(_asset4, assetData1);
            _assetRepository.AddAsset(_asset5, assetData1);
            _assetRepository.AddAsset(_asset6, assetData1);
            _assetRepository.AddAsset(_asset7, assetData1);
            _assetRepository.AddAsset(_asset8, assetData1);
            _assetRepository.AddAsset(_asset9, assetData1);
            _assetRepository.AddAsset(_asset10, assetData1);

            List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList1 = assetsName1.ToList();
                Assert.That(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = assetsName2.ToList();
                Assert.That(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)), Is.True);
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = assetsName3.ToList();
                Assert.That(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }
}
