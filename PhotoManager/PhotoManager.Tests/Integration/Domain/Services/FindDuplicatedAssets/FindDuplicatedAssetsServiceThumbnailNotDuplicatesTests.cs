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

namespace PhotoManager.Tests.Integration.Domain.Services.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailNotDuplicatesTests
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

    private const string ASSET1336_HASH = Hashes._1336_JPG;
    private const string ASSET1337_HASH = Hashes._1337_JPG;
    private const string ASSET1349_HASH = Hashes._1349_JPG;
    private const string ASSET1350_HASH = Hashes._1350_JPG;
    private const string ASSET1413_HASH = Hashes._1413_JPG;
    private const string ASSET1414_HASH = Hashes._1414_JPG;
    private const string ASSET1415_HASH = Hashes._1415_JPG;

    private const string ASSET1336_MD5_HASH = MD5Hashes._1336_JPG;
    private const string ASSET1337_MD5_HASH = MD5Hashes._1337_JPG;
    private const string ASSET1349_MD5_HASH = MD5Hashes._1349_JPG;
    private const string ASSET1350_MD5_HASH = MD5Hashes._1350_JPG;
    private const string ASSET1413_MD5_HASH = MD5Hashes._1413_JPG;
    private const string ASSET1414_MD5_HASH = MD5Hashes._1414_JPG;
    private const string ASSET1415_MD5_HASH = MD5Hashes._1415_JPG;

    private const string ASSET1336_D_HASH = DHashes._1336_JPG;
    private const string ASSET1337_D_HASH = DHashes._1337_JPG;
    private const string ASSET1349_D_HASH = DHashes._1349_JPG;
    private const string ASSET1350_D_HASH = DHashes._1350_JPG;
    private const string ASSET1413_D_HASH = DHashes._1413_JPG;
    private const string ASSET1414_D_HASH = DHashes._1414_JPG;
    private const string ASSET1415_D_HASH = DHashes._1415_JPG;

    private const string ASSET1336_P_HASH = PHashes._1336_JPG;
    private const string ASSET1337_P_HASH = PHashes._1337_JPG;
    private const string ASSET1349_P_HASH = PHashes._1349_JPG;
    private const string ASSET1350_P_HASH = PHashes._1350_JPG;
    private const string ASSET1413_P_HASH = PHashes._1413_JPG;
    private const string ASSET1414_P_HASH = PHashes._1414_JPG;
    private const string ASSET1415_P_HASH = PHashes._1415_JPG;

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
            .WithFolderId(new("db973547-ed18-4e7d-8dc8-ce441aabef80"))
            .WithFileName(FileNames._1336_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1336_JPG, PixelHeightAsset._1336_JPG,
                ThumbnailWidthAsset._1336_JPG, ThumbnailHeightAsset._1336_JPG)
            .WithFileProperties(FileSize._1336_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("65a65540-586e-4b2e-a62a-ccdbd7d442f2"))
            .WithFileName(FileNames._1337_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1337_JPG, PixelHeightAsset._1337_JPG,
                ThumbnailWidthAsset._1337_JPG, ThumbnailHeightAsset._1337_JPG)
            .WithFileProperties(FileSize._1337_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("65ba6b40-dc3c-46b6-8508-135cb4e1f961"))
            .WithFileName(FileNames._1349_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1349_JPG, PixelHeightAsset._1349_JPG,
                ThumbnailWidthAsset._1349_JPG, ThumbnailHeightAsset._1349_JPG)
            .WithFileProperties(FileSize._1349_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("ee3dcc63-53d8-4937-a38a-45b3189118e6"))
            .WithFileName(FileNames._1350_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1350_JPG, PixelHeightAsset._1350_JPG,
                ThumbnailWidthAsset._1350_JPG, ThumbnailHeightAsset._1350_JPG)
            .WithFileProperties(FileSize._1350_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("044e3039-0b4b-4065-b215-9cf185d1f90f"))
            .WithFileName(FileNames._1413_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1413_JPG, PixelHeightAsset._1413_JPG,
                ThumbnailWidthAsset._1413_JPG, ThumbnailHeightAsset._1413_JPG)
            .WithFileProperties(FileSize._1413_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset6 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("80fa1201-1b9d-4d8e-8730-6efe8e46fcbd"))
            .WithFileName(FileNames._1414_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1414_JPG, PixelHeightAsset._1414_JPG,
                ThumbnailWidthAsset._1414_JPG, ThumbnailHeightAsset._1414_JPG)
            .WithFileProperties(FileSize._1414_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset7 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("4c0b2c7c-3159-4855-850b-27c7e737a96e"))
            .WithFileName(FileNames._1415_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset._1415_JPG, PixelHeightAsset._1415_JPG,
                ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG)
            .WithFileProperties(FileSize._1415_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset8 = AssetBuilder.Create() // Another asset completely different from the actual asset
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5b506904-45af-4eaf-b402-53f6d0012075"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
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

    // The hamming distance is about 121 between these hashes
    [Test]
    [Category(
        "NotDuplicate folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase("20", 0, new string[] { })]
    [TestCase("40", 0, new string[] { })]
    [TestCase("60", 0, new string[] { })]
    [TestCase("80", 0, new string[] { })]
    [TestCase("100", 0, new string[] { })]
    [TestCase("110", 0, new string[] { })]
    [TestCase("128", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateBasicHashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string folderPath2 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_2);
        string folderPath3 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_3);
        string folderPath4 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
        Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
        Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_HASH);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_HASH);
        _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_HASH);
        _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_HASH);
        _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_HASH);
        _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_HASH);
        _asset8 = _asset8!.WithFolder(folder4)
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

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
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
    [TestCase("32", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateMD5HashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string folderPath2 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_2);
        string folderPath3 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_3);
        string folderPath4 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
        Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
        Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_MD5_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_MD5_HASH);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_MD5_HASH);
        _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_MD5_HASH);
        _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_MD5_HASH);
        _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_MD5_HASH);
        _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_MD5_HASH);
        _asset8 = _asset8!.WithFolder(folder4)
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

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase("3", 0, new string[] { })]
    [TestCase("5", 0, new string[] { })]
    [TestCase("9", 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG })]
    [TestCase("11", 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG })]
    [TestCase("14", 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample1DHashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string folderPath4 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_D_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_D_HASH);
        _asset8 = _asset8!.WithFolder(folder4)
            .WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase("3", 0, new string[] { })]
    [TestCase("5", 0, new string[] { })]
    [TestCase("9", 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG })]
    [TestCase("11", 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG })]
    [TestCase("14", 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample2DHashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_2);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1349_D_HASH);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1350_D_HASH);
        _asset8 = _asset8!.WithFolder(folder2)
            .WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase("3", 0, new string[] { })]
    [TestCase("5", 0, new string[] { })]
    [TestCase("9", 0, new string[] { })]
    [TestCase("11", 0, new string[] { })]
    [TestCase("14", 1, new[] { FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample3DHashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_3);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1413_D_HASH);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1414_D_HASH);
        _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1415_D_HASH);
        _asset8 = _asset8!.WithFolder(folder2)
            .WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset5, assetData);
        _testableAssetRepository.AddAsset(_asset6, assetData);
        _testableAssetRepository.AddAsset(_asset7, assetData);
        _testableAssetRepository.AddAsset(_asset8, assetData);

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

        if (expected > 0)
        {
            IList<string> assetsNameList = [.. assetsName];
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsNameList));
        }
    }

    // The hamming distance is about 10/78 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("NotDuplicate folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase("10", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("20", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("30", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("40", 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase("50", 2,
        new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG },
        new[] { FileNames._1413_JPG, FileNames._1414_JPG }, new string[] { })]
    [TestCase("60", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("80", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("90", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("100", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("120", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("140", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("160", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("180", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    [TestCase("210", 1,
        new[]
        {
            FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG,
            FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicatePHashDifferentThresholdValues(
        string thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        _configurationRootMock!.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, thresholdToMock);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository!,
            _fileOperationsService!, userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        string folderPath1 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string folderPath2 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_2);
        string folderPath3 = Path.Combine(_assetsDirectory!,
            Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_3);
        string folderPath4 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
        Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
        Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_P_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_P_HASH);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_P_HASH);
        _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_P_HASH);
        _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_P_HASH);
        _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_P_HASH);
        _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_P_HASH);
        _asset8 = _asset8!.WithFolder(folder4)
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

        Asset[][] duplicatedAssets = findDuplicatedAssetsService.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Length.EqualTo(expected));

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
