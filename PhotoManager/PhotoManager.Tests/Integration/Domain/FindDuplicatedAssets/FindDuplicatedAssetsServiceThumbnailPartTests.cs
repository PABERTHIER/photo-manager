using DHashes = PhotoManager.Tests.Integration.Constants.DHashes;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PHashes = PhotoManager.Tests.Integration.Constants.PHashes;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailPartTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private TestableAssetRepository? _testableAssetRepository;
    private FileOperationsService? _fileOperationsService;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

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

    private const string ASSET1336_BOTTOM_LEFT_PART_HASH = Hashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_HASH = Hashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_HASH = Hashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_HASH = Hashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_HASH = Hashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_HASH = Hashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_HASH = Hashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_HASH = Hashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_HASH = Hashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_MD5_HASH = MD5Hashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_MD5_HASH = MD5Hashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_MD5_HASH = MD5Hashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_MD5_HASH = MD5Hashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_MD5_HASH = MD5Hashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_MD5_HASH = MD5Hashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_D_HASH = DHashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_D_HASH = DHashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_D_HASH = DHashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_D_HASH = DHashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_D_HASH = DHashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_D_HASH = DHashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_D_HASH = DHashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_D_HASH = DHashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_D_HASH = DHashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_P_HASH = PHashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_P_HASH = PHashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_P_HASH = PHashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_P_HASH = PHashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_P_HASH = PHashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_P_HASH = PHashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_P_HASH = PHashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_P_HASH = PHashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_P_HASH = PHashes._1336_TOP_RIGHT_PART_JPG;

    private const string MISC_ASSET_HASH = Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_MD5_HASH = MD5Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_D_HASH = DHashes.IMAGE_1_JPG;
    private const string MISC_ASSET_P_HASH = PHashes.IMAGE_1_JPG;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
        _configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");
        _configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        _fileOperationsService = new(userConfigurationService, new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(_fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_BOTTOM_LEFT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG, PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG,
                ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG)
            .WithFileProperties(FileSize._1336_BOTTOM_LEFT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_BOTTOM_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_BOTTOM_PART_JPG, PixelHeightAsset._1336_BOTTOM_PART_JPG,
                ThumbnailWidthAsset._1336_BOTTOM_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_PART_JPG)
            .WithFileProperties(FileSize._1336_BOTTOM_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_BOTTOM_RIGHT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG,
                ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG)
            .WithFileProperties(FileSize._1336_BOTTOM_RIGHT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_LEFT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_LEFT_PART_JPG, PixelHeightAsset._1336_LEFT_PART_JPG,
                ThumbnailWidthAsset._1336_LEFT_PART_JPG, ThumbnailHeightAsset._1336_LEFT_PART_JPG)
            .WithFileProperties(FileSize._1336_LEFT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_ORIGINAL_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
                ThumbnailWidthAsset._1336_ORIGINAL_JPG, ThumbnailHeightAsset._1336_ORIGINAL_JPG)
            .WithFileProperties(FileSize._1336_ORIGINAL_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset6 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_RIGHT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_RIGHT_PART_JPG, PixelHeightAsset._1336_RIGHT_PART_JPG,
                ThumbnailWidthAsset._1336_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_RIGHT_PART_JPG)
            .WithFileProperties(FileSize._1336_RIGHT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset7 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_TOP_LEFT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_TOP_LEFT_PART_JPG, PixelHeightAsset._1336_TOP_LEFT_PART_JPG,
                ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG)
            .WithFileProperties(FileSize._1336_TOP_LEFT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset8 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_TOP_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_TOP_PART_JPG, PixelHeightAsset._1336_TOP_PART_JPG,
                ThumbnailWidthAsset._1336_TOP_PART_JPG, ThumbnailHeightAsset._1336_TOP_PART_JPG)
            .WithFileProperties(FileSize._1336_TOP_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset9 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5126101a-c970-4c87-92ee-ab415ff659f3"))
            .WithFileName(FileNames._1336_TOP_RIGHT_PART_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset._1336_TOP_RIGHT_PART_JPG, PixelHeightAsset._1336_TOP_RIGHT_PART_JPG,
                ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG)
            .WithFileProperties(FileSize._1336_TOP_RIGHT_PART_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset10 = AssetBuilder.Create() // Another asset completely different from the actual asset
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5b506904-45af-4eaf-b402-53f6d0012075"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
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
    [TestCase("128", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartBasicHashDifferentThresholdValues(string thresholdToMock,
        int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.PART);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

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
        _asset10 = _asset10!.WithFolder(folder2)
            .WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);
        _testableAssetRepository.AddAsset(_asset6, assetData);
        _testableAssetRepository.AddAsset(_asset7, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);
        _testableAssetRepository.AddAsset(_asset9, assetData);
        _testableAssetRepository.AddAsset(_asset10, assetData);

        List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
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
    [TestCase("32", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartMD5HashDifferentThresholdValues(string thresholdToMock,
        int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.PART);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

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
        _asset10 = _asset10!.WithFolder(folder2)
            .WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);
        _testableAssetRepository.AddAsset(_asset6, assetData);
        _testableAssetRepository.AddAsset(_asset7, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);
        _testableAssetRepository.AddAsset(_asset9, assetData);
        _testableAssetRepository.AddAsset(_asset10, assetData);

        List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
        }
    }

    [Test]
    [Category("Part folder, DHash")] // The DHash is a 14-hex digits
    [TestCase("3", 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG },
        new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG },
        new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG }, new string[] { })]
    [TestCase("5", 4, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG },
        new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG },
        new[] { FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG },
        new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase("9", 3,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_TOP_LEFT_PART_JPG
        }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_PART_JPG },
        new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG }, new string[] { })]
    [TestCase("11", 2,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_LEFT_PART_JPG,
            FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG
        }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG }, new string[] { },
        new string[] { })]
    [TestCase("14", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartDHashDifferentThresholdValues(string thresholdToMock,
        int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3, string[] assetsName4)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.PART);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

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
        _asset10 = _asset10!.WithFolder(folder2)
            .WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);
        _testableAssetRepository.AddAsset(_asset6, assetData);
        _testableAssetRepository.AddAsset(_asset7, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);
        _testableAssetRepository.AddAsset(_asset9, assetData);
        _testableAssetRepository.AddAsset(_asset10, assetData);

        List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList1 = [.. assetsName1];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList1));
        }
        if (expected > 1)
        {
            IList<string> assetsNameList2 = [.. assetsName2];
            Assert.That(duplicatedAssets[1].Select(y => y.FileName), Is.EquivalentTo(assetsNameList2));
        }
        if (expected > 2)
        {
            IList<string> assetsNameList3 = [.. assetsName3];
            Assert.That(duplicatedAssets[2].Select(y => y.FileName), Is.EquivalentTo(assetsNameList3));
        }
        if (expected > 3)
        {
            IList<string> assetsNameList4 = [.. assetsName4];
            Assert.That(duplicatedAssets[3].Select(y => y.FileName), Is.EquivalentTo(assetsNameList4));
        }
    }

    // The hamming distance is about 40/60 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Part folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("20", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("30", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("40", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("50", 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG },
        new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_RIGHT_PART_JPG },
        new[] { FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase("60", 2,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG
        },
        new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_RIGHT_PART_JPG }, new string[] { })]
    [TestCase("80", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("90", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("100", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("120", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("140", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("160", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("180", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("210", 1,
        new[]
        {
            FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG,
            FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG,
            FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG,
            FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartPHashDifferentThresholdValues(string thresholdToMock,
        int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.PART);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

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
        _asset10 = _asset10!.WithFolder(folder2)
            .WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);
        _testableAssetRepository.AddAsset(_asset6, assetData);
        _testableAssetRepository.AddAsset(_asset7, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);
        _testableAssetRepository.AddAsset(_asset9, assetData);
        _testableAssetRepository.AddAsset(_asset10, assetData);

        List<List<Asset>> duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList1 = [.. assetsName1];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList1));
        }
        if (expected > 1)
        {
            IList<string> assetsNameList2 = [.. assetsName2];
            Assert.That(duplicatedAssets[1].Select(y => y.FileName), Is.EquivalentTo(assetsNameList2));
        }
        if (expected > 2)
        {
            IList<string> assetsNameList3 = [.. assetsName3];
            Assert.That(duplicatedAssets[2].Select(y => y.FileName), Is.EquivalentTo(assetsNameList3));
        }
    }
}
