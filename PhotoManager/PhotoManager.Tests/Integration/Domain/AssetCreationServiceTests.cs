using System.Diagnostics.CodeAnalysis;
using DHashes = PhotoManager.Tests.Integration.Constants.DHashes;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ImageByteSizes = PhotoManager.Tests.Integration.Constants.ImageByteSizes;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PHashes = PhotoManager.Tests.Integration.Constants.PHashes;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class AssetCreationServiceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetCreationService? _assetCreationService;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    private void ConfigureAssetCreationService(int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash,
        bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY,
            _dataDirectory!); // Only needed for videos
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        _assetCreationService = new(_testableAssetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG,
        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, Rotation.Rotate0, Hashes.IMAGE_1_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, FileSize.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
        PixelWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG, Rotation.Rotate90, Hashes.IMAGE_1_90_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, FileSize.IMAGE_1_180_DEG_JPG, PixelHeightAsset.IMAGE_1_180_DEG_JPG,
        PixelWidthAsset.IMAGE_1_180_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_180_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_180_DEG_JPG, Rotation.Rotate180, Hashes.IMAGE_1_180_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, FileSize.IMAGE_1_270_DEG_JPG, PixelHeightAsset.IMAGE_1_270_DEG_JPG,
        PixelWidthAsset.IMAGE_1_270_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_270_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_270_DEG_JPG, Rotation.Rotate270, Hashes.IMAGE_1_270_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, FileSize.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG, PixelWidthAsset.IMAGE_2_JPG,
        ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG, Rotation.Rotate0, Hashes.IMAGE_2_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, FileSize.IMAGE_2_DUPLICATED_JPG,
        PixelHeightAsset.IMAGE_2_DUPLICATED_JPG, PixelWidthAsset.IMAGE_2_DUPLICATED_JPG,
        ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG, Rotation.Rotate0,
        Hashes.IMAGE_2_DUPLICATED_JPG, false, null, false, null, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, FileSize.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG, PixelWidthAsset.IMAGE_3_JPG,
        ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG, Rotation.Rotate0, Hashes.IMAGE_3_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, FileSize.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG, PixelWidthAsset.IMAGE_4_JPG,
        ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG, Rotation.Rotate0, Hashes.IMAGE_4_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, FileSize.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG, PixelWidthAsset.IMAGE_5_JPG,
        ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG, Rotation.Rotate0, Hashes.IMAGE_5_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, FileSize.IMAGE_6_JPG, PixelHeightAsset.IMAGE_6_JPG, PixelWidthAsset.IMAGE_6_JPG,
        ThumbnailWidthAsset.IMAGE_6_JPG, ThumbnailHeightAsset.IMAGE_6_JPG, Rotation.Rotate0, Hashes.IMAGE_6_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, FileSize.IMAGE_7_JPG, PixelHeightAsset.IMAGE_7_JPG, PixelWidthAsset.IMAGE_7_JPG,
        ThumbnailWidthAsset.IMAGE_7_JPG, ThumbnailHeightAsset.IMAGE_7_JPG, Rotation.Rotate0, Hashes.IMAGE_7_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, FileSize.IMAGE_8_JPEG, PixelHeightAsset.IMAGE_8_JPEG,
        PixelWidthAsset.IMAGE_8_JPEG, ThumbnailWidthAsset.IMAGE_8_JPEG, ThumbnailHeightAsset.IMAGE_8_JPEG,
        Rotation.Rotate0, Hashes.IMAGE_8_JPEG, false, null, false, null, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG,
        ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG, Rotation.Rotate0, Hashes.IMAGE_9_PNG, false,
        null, false, null, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, FileSize.IMAGE_10_PORTRAIT_PNG, PixelHeightAsset.IMAGE_10_PORTRAIT_PNG,
        PixelWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG,
        ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG, Rotation.Rotate0, Hashes.IMAGE_10_PORTRAIT_PNG, false, null, false,
        null, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_11_HEIC, FileSize.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
        PixelWidthAsset.IMAGE_11_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        Rotation.Rotate0, Hashes.IMAGE_11_HEIC, false, null, false, null, ImageByteSizes.IMAGE_11_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, Hashes.IMAGE_11_90_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_90_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, Hashes.IMAGE_11_180_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_180_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, Hashes.IMAGE_11_270_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_270_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, FileSize.IMAGE_WITH_UPPERCASE_NAME_JPG,
        PixelHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, PixelWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        ThumbnailWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, ThumbnailHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        Rotation.Rotate0, Hashes.IMAGE_WITH_UPPERCASE_NAME_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG, "")]
    [TestCase(FileNames.HOMER_GIF, FileSize.HOMER_GIF, PixelHeightAsset.HOMER_GIF, PixelWidthAsset.HOMER_GIF,
        ThumbnailWidthAsset.HOMER_GIF, ThumbnailHeightAsset.HOMER_GIF, Rotation.Rotate0, Hashes.HOMER_GIF, false, null,
        false, null, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, FileSize._1336_BOTTOM_LEFT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG, PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG,
        Rotation.Rotate0, Hashes._1336_BOTTOM_LEFT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, FileSize._1336_BOTTOM_PART_JPG, PixelHeightAsset._1336_BOTTOM_PART_JPG,
        PixelWidthAsset._1336_BOTTOM_PART_JPG, ThumbnailWidthAsset._1336_BOTTOM_PART_JPG,
        ThumbnailHeightAsset._1336_BOTTOM_PART_JPG, Rotation.Rotate0, Hashes._1336_BOTTOM_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_BOTTOM_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileSize._1336_BOTTOM_RIGHT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG, PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG,
        Rotation.Rotate0, Hashes._1336_BOTTOM_RIGHT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, FileSize._1336_LEFT_PART_JPG, PixelHeightAsset._1336_LEFT_PART_JPG,
        PixelWidthAsset._1336_LEFT_PART_JPG, ThumbnailWidthAsset._1336_LEFT_PART_JPG,
        ThumbnailHeightAsset._1336_LEFT_PART_JPG, Rotation.Rotate0, Hashes._1336_LEFT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, FileSize._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
        PixelWidthAsset._1336_ORIGINAL_JPG, ThumbnailWidthAsset._1336_ORIGINAL_JPG,
        ThumbnailHeightAsset._1336_ORIGINAL_JPG, Rotation.Rotate0, Hashes._1336_ORIGINAL_JPG, false, null, false, null,
        ImageByteSizes._1336_ORIGINAL_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, FileSize._1336_RIGHT_PART_JPG, PixelHeightAsset._1336_RIGHT_PART_JPG,
        PixelWidthAsset._1336_RIGHT_PART_JPG, ThumbnailWidthAsset._1336_RIGHT_PART_JPG,
        ThumbnailHeightAsset._1336_RIGHT_PART_JPG, Rotation.Rotate0, Hashes._1336_RIGHT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, FileSize._1336_TOP_LEFT_PART_JPG,
        PixelHeightAsset._1336_TOP_LEFT_PART_JPG, PixelWidthAsset._1336_TOP_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG, Rotation.Rotate0,
        Hashes._1336_TOP_LEFT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, FileSize._1336_TOP_PART_JPG, PixelHeightAsset._1336_TOP_PART_JPG,
        PixelWidthAsset._1336_TOP_PART_JPG, ThumbnailWidthAsset._1336_TOP_PART_JPG,
        ThumbnailHeightAsset._1336_TOP_PART_JPG, Rotation.Rotate0, Hashes._1336_TOP_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_TOP_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, FileSize._1336_TOP_RIGHT_PART_JPG,
        PixelHeightAsset._1336_TOP_RIGHT_PART_JPG, PixelWidthAsset._1336_TOP_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG, Rotation.Rotate0,
        Hashes._1336_TOP_RIGHT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, FileSize._1336_1_K_JPG, PixelHeightAsset._1336_1_K_JPG,
        PixelWidthAsset._1336_1_K_JPG, ThumbnailWidthAsset._1336_1_K_JPG, ThumbnailHeightAsset._1336_1_K_JPG,
        Rotation.Rotate0, Hashes._1336_1_K_JPG, false, null, false, null, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, FileSize._1336_2_K_JPG, PixelHeightAsset._1336_2_K_JPG,
        PixelWidthAsset._1336_2_K_JPG, ThumbnailWidthAsset._1336_2_K_JPG, ThumbnailHeightAsset._1336_2_K_JPG,
        Rotation.Rotate0, Hashes._1336_2_K_JPG, false, null, false, null, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, FileSize._1336_3_K_JPG, PixelHeightAsset._1336_3_K_JPG,
        PixelWidthAsset._1336_3_K_JPG, ThumbnailWidthAsset._1336_3_K_JPG, ThumbnailHeightAsset._1336_3_K_JPG,
        Rotation.Rotate0, Hashes._1336_3_K_JPG, false, null, false, null, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, FileSize._1336_4_K_ORIGINAL_JPG,
        PixelHeightAsset._1336_4_K_ORIGINAL_JPG, PixelWidthAsset._1336_4_K_ORIGINAL_JPG,
        ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG, Rotation.Rotate0,
        Hashes._1336_4_K_ORIGINAL_JPG, false, null, false, null, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, FileSize._1336_8_K_JPG, PixelHeightAsset._1336_8_K_JPG,
        PixelWidthAsset._1336_8_K_JPG, ThumbnailWidthAsset._1336_8_K_JPG, ThumbnailHeightAsset._1336_8_K_JPG,
        Rotation.Rotate0, Hashes._1336_8_K_JPG, false, null, false, null, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, FileSize._1336_THUMBNAIL_JPG, PixelHeightAsset._1336_THUMBNAIL_JPG,
        PixelWidthAsset._1336_THUMBNAIL_JPG, ThumbnailWidthAsset._1336_THUMBNAIL_JPG,
        ThumbnailHeightAsset._1336_THUMBNAIL_JPG, Rotation.Rotate0, Hashes._1336_THUMBNAIL_JPG, false, null, false,
        null, ImageByteSizes._1336_THUMBNAIL_JPG, $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, FileSize.IMAGE_1336_MINI_JPG, PixelHeightAsset.IMAGE_1336_MINI_JPG,
        PixelWidthAsset.IMAGE_1336_MINI_JPG, ThumbnailWidthAsset.IMAGE_1336_MINI_JPG,
        ThumbnailHeightAsset.IMAGE_1336_MINI_JPG, Rotation.Rotate0, Hashes.IMAGE_1336_MINI_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_MINI_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, FileSize.IMAGE_1336_ORIGINAL_JPG,
        PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG, PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG,
        ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG, Rotation.Rotate0,
        Hashes.IMAGE_1336_ORIGINAL_JPG, false, null, false, null, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileSize.IMAGE_1336_SHIT_QUALITY_JPG,
        PixelHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG, PixelWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        ThumbnailWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, ThumbnailHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        Rotation.Rotate0, Hashes.IMAGE_1336_SHIT_QUALITY_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, FileSize.IMAGE_1336_SMALL_JPG, PixelHeightAsset.IMAGE_1336_SMALL_JPG,
        PixelWidthAsset.IMAGE_1336_SMALL_JPG, ThumbnailWidthAsset.IMAGE_1336_SMALL_JPG,
        ThumbnailHeightAsset.IMAGE_1336_SMALL_JPG, Rotation.Rotate0, Hashes.IMAGE_1336_SMALL_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_SMALL_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, FileSize._1337_JPG, PixelHeightAsset._1337_JPG, PixelWidthAsset._1337_JPG,
        ThumbnailWidthAsset._1337_JPG, ThumbnailHeightAsset._1337_JPG, Rotation.Rotate0, Hashes._1337_JPG, false, null,
        false, null, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, FileSize._1349_JPG, PixelHeightAsset._1349_JPG, PixelWidthAsset._1349_JPG,
        ThumbnailWidthAsset._1349_JPG, ThumbnailHeightAsset._1349_JPG, Rotation.Rotate0, Hashes._1349_JPG, false, null,
        false, null, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, FileSize._1350_JPG, PixelHeightAsset._1350_JPG, PixelWidthAsset._1350_JPG,
        ThumbnailWidthAsset._1350_JPG, ThumbnailHeightAsset._1350_JPG, Rotation.Rotate0, Hashes._1350_JPG, false, null,
        false, null, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, FileSize._1413_JPG, PixelHeightAsset._1413_JPG, PixelWidthAsset._1413_JPG,
        ThumbnailWidthAsset._1413_JPG, ThumbnailHeightAsset._1413_JPG, Rotation.Rotate0, Hashes._1413_JPG, false, null,
        false, null, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, FileSize._1414_JPG, PixelHeightAsset._1414_JPG, PixelWidthAsset._1414_JPG,
        ThumbnailWidthAsset._1414_JPG, ThumbnailHeightAsset._1414_JPG, Rotation.Rotate0, Hashes._1414_JPG, false, null,
        false, null, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, FileSize._1415_JPG, PixelHeightAsset._1415_JPG, PixelWidthAsset._1415_JPG,
        ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG, Rotation.Rotate270, Hashes._1415_JPG, false,
        null, true, "The asset has been rotated", ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void CreateAsset_PictureAndBasicHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.That(asset, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                AssertAssetPropertyValidity(
                    asset!,
                    fileName,
                    imagePath,
                    folderPath,
                    folder,
                    fileSize,
                    pixelWidth,
                    pixelHeight,
                    thumbnailPixelWidth,
                    thumbnailPixelHeight,
                    ModificationDate.Default,
                    imageRotation,
                    hash,
                    isCorrupted,
                    corruptedMessage,
                    isRotated,
                    rotatedMessage);

                AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG,
        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, Rotation.Rotate0, PHashes.IMAGE_1_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, FileSize.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
        PixelWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG, Rotation.Rotate90, PHashes.IMAGE_1_90_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, FileSize.IMAGE_1_180_DEG_JPG, PixelHeightAsset.IMAGE_1_180_DEG_JPG,
        PixelWidthAsset.IMAGE_1_180_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_180_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_180_DEG_JPG, Rotation.Rotate180, PHashes.IMAGE_1_180_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, FileSize.IMAGE_1_270_DEG_JPG, PixelHeightAsset.IMAGE_1_270_DEG_JPG,
        PixelWidthAsset.IMAGE_1_270_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_270_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_270_DEG_JPG, Rotation.Rotate270, PHashes.IMAGE_1_270_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, FileSize.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG, PixelWidthAsset.IMAGE_2_JPG,
        ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG, Rotation.Rotate0, PHashes.IMAGE_2_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, FileSize.IMAGE_2_DUPLICATED_JPG,
        PixelHeightAsset.IMAGE_2_DUPLICATED_JPG, PixelWidthAsset.IMAGE_2_DUPLICATED_JPG,
        ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG, Rotation.Rotate0,
        PHashes.IMAGE_2_DUPLICATED_JPG, false, null, false, null, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, FileSize.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG, PixelWidthAsset.IMAGE_3_JPG,
        ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG, Rotation.Rotate0, PHashes.IMAGE_3_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, FileSize.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG, PixelWidthAsset.IMAGE_4_JPG,
        ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG, Rotation.Rotate0, PHashes.IMAGE_4_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, FileSize.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG, PixelWidthAsset.IMAGE_5_JPG,
        ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG, Rotation.Rotate0, PHashes.IMAGE_5_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, FileSize.IMAGE_6_JPG, PixelHeightAsset.IMAGE_6_JPG, PixelWidthAsset.IMAGE_6_JPG,
        ThumbnailWidthAsset.IMAGE_6_JPG, ThumbnailHeightAsset.IMAGE_6_JPG, Rotation.Rotate0, PHashes.IMAGE_6_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, FileSize.IMAGE_7_JPG, PixelHeightAsset.IMAGE_7_JPG, PixelWidthAsset.IMAGE_7_JPG,
        ThumbnailWidthAsset.IMAGE_7_JPG, ThumbnailHeightAsset.IMAGE_7_JPG, Rotation.Rotate0, PHashes.IMAGE_7_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, FileSize.IMAGE_8_JPEG, PixelHeightAsset.IMAGE_8_JPEG,
        PixelWidthAsset.IMAGE_8_JPEG, ThumbnailWidthAsset.IMAGE_8_JPEG, ThumbnailHeightAsset.IMAGE_8_JPEG,
        Rotation.Rotate0, PHashes.IMAGE_8_JPEG, false, null, false, null, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG,
        ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG, Rotation.Rotate0, PHashes.IMAGE_9_PNG, false,
        null, false, null, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, FileSize.IMAGE_10_PORTRAIT_PNG, PixelHeightAsset.IMAGE_10_PORTRAIT_PNG,
        PixelWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG,
        ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG, Rotation.Rotate0, PHashes.IMAGE_10_PORTRAIT_PNG, false, null, false,
        null, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_11_HEIC, FileSize.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
        PixelWidthAsset.IMAGE_11_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        Rotation.Rotate0, PHashes.IMAGE_11_HEIC, false, null, false, null, ImageByteSizes.IMAGE_11_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, PHashes.IMAGE_11_90_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_90_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, PHashes.IMAGE_11_180_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_180_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, PHashes.IMAGE_11_270_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_270_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, FileSize.IMAGE_WITH_UPPERCASE_NAME_JPG,
        PixelHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, PixelWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        ThumbnailWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, ThumbnailHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        Rotation.Rotate0, PHashes.IMAGE_WITH_UPPERCASE_NAME_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG, "")]
    [TestCase(FileNames.HOMER_GIF, FileSize.HOMER_GIF, PixelHeightAsset.HOMER_GIF, PixelWidthAsset.HOMER_GIF,
        ThumbnailWidthAsset.HOMER_GIF, ThumbnailHeightAsset.HOMER_GIF, Rotation.Rotate0, PHashes.HOMER_GIF, false, null,
        false, null, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, FileSize._1336_BOTTOM_LEFT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG, PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG,
        Rotation.Rotate0, PHashes._1336_BOTTOM_LEFT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, FileSize._1336_BOTTOM_PART_JPG, PixelHeightAsset._1336_BOTTOM_PART_JPG,
        PixelWidthAsset._1336_BOTTOM_PART_JPG, ThumbnailWidthAsset._1336_BOTTOM_PART_JPG,
        ThumbnailHeightAsset._1336_BOTTOM_PART_JPG, Rotation.Rotate0, PHashes._1336_BOTTOM_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_BOTTOM_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileSize._1336_BOTTOM_RIGHT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG, PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG,
        Rotation.Rotate0, PHashes._1336_BOTTOM_RIGHT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, FileSize._1336_LEFT_PART_JPG, PixelHeightAsset._1336_LEFT_PART_JPG,
        PixelWidthAsset._1336_LEFT_PART_JPG, ThumbnailWidthAsset._1336_LEFT_PART_JPG,
        ThumbnailHeightAsset._1336_LEFT_PART_JPG, Rotation.Rotate0, PHashes._1336_LEFT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, FileSize._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
        PixelWidthAsset._1336_ORIGINAL_JPG, ThumbnailWidthAsset._1336_ORIGINAL_JPG,
        ThumbnailHeightAsset._1336_ORIGINAL_JPG, Rotation.Rotate0, PHashes._1336_ORIGINAL_JPG, false, null, false, null,
        ImageByteSizes._1336_ORIGINAL_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, FileSize._1336_RIGHT_PART_JPG, PixelHeightAsset._1336_RIGHT_PART_JPG,
        PixelWidthAsset._1336_RIGHT_PART_JPG, ThumbnailWidthAsset._1336_RIGHT_PART_JPG,
        ThumbnailHeightAsset._1336_RIGHT_PART_JPG, Rotation.Rotate0, PHashes._1336_RIGHT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, FileSize._1336_TOP_LEFT_PART_JPG,
        PixelHeightAsset._1336_TOP_LEFT_PART_JPG, PixelWidthAsset._1336_TOP_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG, Rotation.Rotate0,
        PHashes._1336_TOP_LEFT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, FileSize._1336_TOP_PART_JPG, PixelHeightAsset._1336_TOP_PART_JPG,
        PixelWidthAsset._1336_TOP_PART_JPG, ThumbnailWidthAsset._1336_TOP_PART_JPG,
        ThumbnailHeightAsset._1336_TOP_PART_JPG, Rotation.Rotate0, PHashes._1336_TOP_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_TOP_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, FileSize._1336_TOP_RIGHT_PART_JPG,
        PixelHeightAsset._1336_TOP_RIGHT_PART_JPG, PixelWidthAsset._1336_TOP_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG, Rotation.Rotate0,
        PHashes._1336_TOP_RIGHT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, FileSize._1336_1_K_JPG, PixelHeightAsset._1336_1_K_JPG,
        PixelWidthAsset._1336_1_K_JPG, ThumbnailWidthAsset._1336_1_K_JPG, ThumbnailHeightAsset._1336_1_K_JPG,
        Rotation.Rotate0, PHashes._1336_1_K_JPG, false, null, false, null, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, FileSize._1336_2_K_JPG, PixelHeightAsset._1336_2_K_JPG,
        PixelWidthAsset._1336_2_K_JPG, ThumbnailWidthAsset._1336_2_K_JPG, ThumbnailHeightAsset._1336_2_K_JPG,
        Rotation.Rotate0, PHashes._1336_2_K_JPG, false, null, false, null, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, FileSize._1336_3_K_JPG, PixelHeightAsset._1336_3_K_JPG,
        PixelWidthAsset._1336_3_K_JPG, ThumbnailWidthAsset._1336_3_K_JPG, ThumbnailHeightAsset._1336_3_K_JPG,
        Rotation.Rotate0, PHashes._1336_3_K_JPG, false, null, false, null, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, FileSize._1336_4_K_ORIGINAL_JPG,
        PixelHeightAsset._1336_4_K_ORIGINAL_JPG, PixelWidthAsset._1336_4_K_ORIGINAL_JPG,
        ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG, Rotation.Rotate0,
        PHashes._1336_4_K_ORIGINAL_JPG, false, null, false, null, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, FileSize._1336_8_K_JPG, PixelHeightAsset._1336_8_K_JPG,
        PixelWidthAsset._1336_8_K_JPG, ThumbnailWidthAsset._1336_8_K_JPG, ThumbnailHeightAsset._1336_8_K_JPG,
        Rotation.Rotate0, PHashes._1336_8_K_JPG, false, null, false, null, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, FileSize._1336_THUMBNAIL_JPG, PixelHeightAsset._1336_THUMBNAIL_JPG,
        PixelWidthAsset._1336_THUMBNAIL_JPG, ThumbnailWidthAsset._1336_THUMBNAIL_JPG,
        ThumbnailHeightAsset._1336_THUMBNAIL_JPG, Rotation.Rotate0, PHashes._1336_THUMBNAIL_JPG, false, null, false,
        null, ImageByteSizes._1336_THUMBNAIL_JPG, $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, FileSize.IMAGE_1336_MINI_JPG, PixelHeightAsset.IMAGE_1336_MINI_JPG,
        PixelWidthAsset.IMAGE_1336_MINI_JPG, ThumbnailWidthAsset.IMAGE_1336_MINI_JPG,
        ThumbnailHeightAsset.IMAGE_1336_MINI_JPG, Rotation.Rotate0, PHashes.IMAGE_1336_MINI_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_MINI_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, FileSize.IMAGE_1336_ORIGINAL_JPG,
        PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG, PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG,
        ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG, Rotation.Rotate0,
        PHashes.IMAGE_1336_ORIGINAL_JPG, false, null, false, null, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileSize.IMAGE_1336_SHIT_QUALITY_JPG,
        PixelHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG, PixelWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        ThumbnailWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, ThumbnailHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        Rotation.Rotate0, PHashes.IMAGE_1336_SHIT_QUALITY_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, FileSize.IMAGE_1336_SMALL_JPG, PixelHeightAsset.IMAGE_1336_SMALL_JPG,
        PixelWidthAsset.IMAGE_1336_SMALL_JPG, ThumbnailWidthAsset.IMAGE_1336_SMALL_JPG,
        ThumbnailHeightAsset.IMAGE_1336_SMALL_JPG, Rotation.Rotate0, PHashes.IMAGE_1336_SMALL_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_SMALL_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, FileSize._1337_JPG, PixelHeightAsset._1337_JPG, PixelWidthAsset._1337_JPG,
        ThumbnailWidthAsset._1337_JPG, ThumbnailHeightAsset._1337_JPG, Rotation.Rotate0, PHashes._1337_JPG, false, null,
        false, null, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, FileSize._1349_JPG, PixelHeightAsset._1349_JPG, PixelWidthAsset._1349_JPG,
        ThumbnailWidthAsset._1349_JPG, ThumbnailHeightAsset._1349_JPG, Rotation.Rotate0, PHashes._1349_JPG, false, null,
        false, null, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, FileSize._1350_JPG, PixelHeightAsset._1350_JPG, PixelWidthAsset._1350_JPG,
        ThumbnailWidthAsset._1350_JPG, ThumbnailHeightAsset._1350_JPG, Rotation.Rotate0, PHashes._1350_JPG, false, null,
        false, null, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, FileSize._1413_JPG, PixelHeightAsset._1413_JPG, PixelWidthAsset._1413_JPG,
        ThumbnailWidthAsset._1413_JPG, ThumbnailHeightAsset._1413_JPG, Rotation.Rotate0, PHashes._1413_JPG, false, null,
        false, null, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, FileSize._1414_JPG, PixelHeightAsset._1414_JPG, PixelWidthAsset._1414_JPG,
        ThumbnailWidthAsset._1414_JPG, ThumbnailHeightAsset._1414_JPG, Rotation.Rotate0, PHashes._1414_JPG, false, null,
        false, null, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, FileSize._1415_JPG, PixelHeightAsset._1415_JPG, PixelWidthAsset._1415_JPG,
        ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG, Rotation.Rotate270, PHashes._1415_JPG, false,
        null, true, "The asset has been rotated", ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void CreateAsset_PictureAndPHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, true, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.That(asset, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                AssertAssetPropertyValidity(
                    asset!,
                    fileName,
                    imagePath,
                    folderPath,
                    folder,
                    fileSize,
                    pixelWidth,
                    pixelHeight,
                    thumbnailPixelWidth,
                    thumbnailPixelHeight,
                    ModificationDate.Default,
                    imageRotation,
                    hash,
                    isCorrupted,
                    corruptedMessage,
                    isRotated,
                    rotatedMessage);

                AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG,
        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, Rotation.Rotate0, DHashes.IMAGE_1_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, FileSize.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
        PixelWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG, Rotation.Rotate90, DHashes.IMAGE_1_90_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, FileSize.IMAGE_1_180_DEG_JPG, PixelHeightAsset.IMAGE_1_180_DEG_JPG,
        PixelWidthAsset.IMAGE_1_180_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_180_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_180_DEG_JPG, Rotation.Rotate180, DHashes.IMAGE_1_180_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, FileSize.IMAGE_1_270_DEG_JPG, PixelHeightAsset.IMAGE_1_270_DEG_JPG,
        PixelWidthAsset.IMAGE_1_270_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_270_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_270_DEG_JPG, Rotation.Rotate270, DHashes.IMAGE_1_270_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, FileSize.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG, PixelWidthAsset.IMAGE_2_JPG,
        ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG, Rotation.Rotate0, DHashes.IMAGE_2_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, FileSize.IMAGE_2_DUPLICATED_JPG,
        PixelHeightAsset.IMAGE_2_DUPLICATED_JPG, PixelWidthAsset.IMAGE_2_DUPLICATED_JPG,
        ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG, Rotation.Rotate0,
        DHashes.IMAGE_2_DUPLICATED_JPG, false, null, false, null, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, FileSize.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG, PixelWidthAsset.IMAGE_3_JPG,
        ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG, Rotation.Rotate0, DHashes.IMAGE_3_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, FileSize.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG, PixelWidthAsset.IMAGE_4_JPG,
        ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG, Rotation.Rotate0, DHashes.IMAGE_4_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, FileSize.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG, PixelWidthAsset.IMAGE_5_JPG,
        ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG, Rotation.Rotate0, DHashes.IMAGE_5_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, FileSize.IMAGE_6_JPG, PixelHeightAsset.IMAGE_6_JPG, PixelWidthAsset.IMAGE_6_JPG,
        ThumbnailWidthAsset.IMAGE_6_JPG, ThumbnailHeightAsset.IMAGE_6_JPG, Rotation.Rotate0, DHashes.IMAGE_6_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, FileSize.IMAGE_7_JPG, PixelHeightAsset.IMAGE_7_JPG, PixelWidthAsset.IMAGE_7_JPG,
        ThumbnailWidthAsset.IMAGE_7_JPG, ThumbnailHeightAsset.IMAGE_7_JPG, Rotation.Rotate0, DHashes.IMAGE_7_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, FileSize.IMAGE_8_JPEG, PixelHeightAsset.IMAGE_8_JPEG,
        PixelWidthAsset.IMAGE_8_JPEG, ThumbnailWidthAsset.IMAGE_8_JPEG, ThumbnailHeightAsset.IMAGE_8_JPEG,
        Rotation.Rotate0, DHashes.IMAGE_8_JPEG, false, null, false, null, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG,
        ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG, Rotation.Rotate0, DHashes.IMAGE_9_PNG, false,
        null, false, null, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, FileSize.IMAGE_10_PORTRAIT_PNG, PixelHeightAsset.IMAGE_10_PORTRAIT_PNG,
        PixelWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG,
        ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG, Rotation.Rotate0, DHashes.IMAGE_10_PORTRAIT_PNG, false, null, false,
        null, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_11_HEIC, FileSize.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
        PixelWidthAsset.IMAGE_11_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        Rotation.Rotate0, DHashes.IMAGE_11_HEIC, false, null, false, null, ImageByteSizes.IMAGE_11_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, DHashes.IMAGE_11_90_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_90_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, DHashes.IMAGE_11_180_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_180_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, DHashes.IMAGE_11_270_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_270_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, FileSize.IMAGE_WITH_UPPERCASE_NAME_JPG,
        PixelHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, PixelWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        ThumbnailWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, ThumbnailHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        Rotation.Rotate0, DHashes.IMAGE_WITH_UPPERCASE_NAME_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG, "")]
    [TestCase(FileNames.HOMER_GIF, FileSize.HOMER_GIF, PixelHeightAsset.HOMER_GIF, PixelWidthAsset.HOMER_GIF,
        ThumbnailWidthAsset.HOMER_GIF, ThumbnailHeightAsset.HOMER_GIF, Rotation.Rotate0, DHashes.HOMER_GIF, false, null,
        false, null, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, FileSize._1336_BOTTOM_LEFT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG, PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG,
        Rotation.Rotate0, DHashes._1336_BOTTOM_LEFT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, FileSize._1336_BOTTOM_PART_JPG, PixelHeightAsset._1336_BOTTOM_PART_JPG,
        PixelWidthAsset._1336_BOTTOM_PART_JPG, ThumbnailWidthAsset._1336_BOTTOM_PART_JPG,
        ThumbnailHeightAsset._1336_BOTTOM_PART_JPG, Rotation.Rotate0, DHashes._1336_BOTTOM_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_BOTTOM_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileSize._1336_BOTTOM_RIGHT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG, PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG,
        Rotation.Rotate0, DHashes._1336_BOTTOM_RIGHT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, FileSize._1336_LEFT_PART_JPG, PixelHeightAsset._1336_LEFT_PART_JPG,
        PixelWidthAsset._1336_LEFT_PART_JPG, ThumbnailWidthAsset._1336_LEFT_PART_JPG,
        ThumbnailHeightAsset._1336_LEFT_PART_JPG, Rotation.Rotate0, DHashes._1336_LEFT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, FileSize._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
        PixelWidthAsset._1336_ORIGINAL_JPG, ThumbnailWidthAsset._1336_ORIGINAL_JPG,
        ThumbnailHeightAsset._1336_ORIGINAL_JPG, Rotation.Rotate0, DHashes._1336_ORIGINAL_JPG, false, null, false, null,
        ImageByteSizes._1336_ORIGINAL_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, FileSize._1336_RIGHT_PART_JPG, PixelHeightAsset._1336_RIGHT_PART_JPG,
        PixelWidthAsset._1336_RIGHT_PART_JPG, ThumbnailWidthAsset._1336_RIGHT_PART_JPG,
        ThumbnailHeightAsset._1336_RIGHT_PART_JPG, Rotation.Rotate0, DHashes._1336_RIGHT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, FileSize._1336_TOP_LEFT_PART_JPG,
        PixelHeightAsset._1336_TOP_LEFT_PART_JPG, PixelWidthAsset._1336_TOP_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG, Rotation.Rotate0,
        DHashes._1336_TOP_LEFT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, FileSize._1336_TOP_PART_JPG, PixelHeightAsset._1336_TOP_PART_JPG,
        PixelWidthAsset._1336_TOP_PART_JPG, ThumbnailWidthAsset._1336_TOP_PART_JPG,
        ThumbnailHeightAsset._1336_TOP_PART_JPG, Rotation.Rotate0, DHashes._1336_TOP_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_TOP_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, FileSize._1336_TOP_RIGHT_PART_JPG,
        PixelHeightAsset._1336_TOP_RIGHT_PART_JPG, PixelWidthAsset._1336_TOP_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG, Rotation.Rotate0,
        DHashes._1336_TOP_RIGHT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, FileSize._1336_1_K_JPG, PixelHeightAsset._1336_1_K_JPG,
        PixelWidthAsset._1336_1_K_JPG, ThumbnailWidthAsset._1336_1_K_JPG, ThumbnailHeightAsset._1336_1_K_JPG,
        Rotation.Rotate0, DHashes._1336_1_K_JPG, false, null, false, null, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, FileSize._1336_2_K_JPG, PixelHeightAsset._1336_2_K_JPG,
        PixelWidthAsset._1336_2_K_JPG, ThumbnailWidthAsset._1336_2_K_JPG, ThumbnailHeightAsset._1336_2_K_JPG,
        Rotation.Rotate0, DHashes._1336_2_K_JPG, false, null, false, null, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, FileSize._1336_3_K_JPG, PixelHeightAsset._1336_3_K_JPG,
        PixelWidthAsset._1336_3_K_JPG, ThumbnailWidthAsset._1336_3_K_JPG, ThumbnailHeightAsset._1336_3_K_JPG,
        Rotation.Rotate0, DHashes._1336_3_K_JPG, false, null, false, null, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, FileSize._1336_4_K_ORIGINAL_JPG,
        PixelHeightAsset._1336_4_K_ORIGINAL_JPG, PixelWidthAsset._1336_4_K_ORIGINAL_JPG,
        ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG, Rotation.Rotate0,
        DHashes._1336_4_K_ORIGINAL_JPG, false, null, false, null, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, FileSize._1336_8_K_JPG, PixelHeightAsset._1336_8_K_JPG,
        PixelWidthAsset._1336_8_K_JPG, ThumbnailWidthAsset._1336_8_K_JPG, ThumbnailHeightAsset._1336_8_K_JPG,
        Rotation.Rotate0, DHashes._1336_8_K_JPG, false, null, false, null, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, FileSize._1336_THUMBNAIL_JPG, PixelHeightAsset._1336_THUMBNAIL_JPG,
        PixelWidthAsset._1336_THUMBNAIL_JPG, ThumbnailWidthAsset._1336_THUMBNAIL_JPG,
        ThumbnailHeightAsset._1336_THUMBNAIL_JPG, Rotation.Rotate0, DHashes._1336_THUMBNAIL_JPG, false, null, false,
        null, ImageByteSizes._1336_THUMBNAIL_JPG, $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, FileSize.IMAGE_1336_MINI_JPG, PixelHeightAsset.IMAGE_1336_MINI_JPG,
        PixelWidthAsset.IMAGE_1336_MINI_JPG, ThumbnailWidthAsset.IMAGE_1336_MINI_JPG,
        ThumbnailHeightAsset.IMAGE_1336_MINI_JPG, Rotation.Rotate0, DHashes.IMAGE_1336_MINI_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_MINI_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, FileSize.IMAGE_1336_ORIGINAL_JPG,
        PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG, PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG,
        ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG, Rotation.Rotate0,
        DHashes.IMAGE_1336_ORIGINAL_JPG, false, null, false, null, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileSize.IMAGE_1336_SHIT_QUALITY_JPG,
        PixelHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG, PixelWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        ThumbnailWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, ThumbnailHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        Rotation.Rotate0, DHashes.IMAGE_1336_SHIT_QUALITY_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, FileSize.IMAGE_1336_SMALL_JPG, PixelHeightAsset.IMAGE_1336_SMALL_JPG,
        PixelWidthAsset.IMAGE_1336_SMALL_JPG, ThumbnailWidthAsset.IMAGE_1336_SMALL_JPG,
        ThumbnailHeightAsset.IMAGE_1336_SMALL_JPG, Rotation.Rotate0, DHashes.IMAGE_1336_SMALL_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_SMALL_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, FileSize._1337_JPG, PixelHeightAsset._1337_JPG, PixelWidthAsset._1337_JPG,
        ThumbnailWidthAsset._1337_JPG, ThumbnailHeightAsset._1337_JPG, Rotation.Rotate0, DHashes._1337_JPG, false, null,
        false, null, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, FileSize._1349_JPG, PixelHeightAsset._1349_JPG, PixelWidthAsset._1349_JPG,
        ThumbnailWidthAsset._1349_JPG, ThumbnailHeightAsset._1349_JPG, Rotation.Rotate0, DHashes._1349_JPG, false, null,
        false, null, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, FileSize._1350_JPG, PixelHeightAsset._1350_JPG, PixelWidthAsset._1350_JPG,
        ThumbnailWidthAsset._1350_JPG, ThumbnailHeightAsset._1350_JPG, Rotation.Rotate0, DHashes._1350_JPG, false, null,
        false, null, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, FileSize._1413_JPG, PixelHeightAsset._1413_JPG, PixelWidthAsset._1413_JPG,
        ThumbnailWidthAsset._1413_JPG, ThumbnailHeightAsset._1413_JPG, Rotation.Rotate0, DHashes._1413_JPG, false, null,
        false, null, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, FileSize._1414_JPG, PixelHeightAsset._1414_JPG, PixelWidthAsset._1414_JPG,
        ThumbnailWidthAsset._1414_JPG, ThumbnailHeightAsset._1414_JPG, Rotation.Rotate0, DHashes._1414_JPG, false, null,
        false, null, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, FileSize._1415_JPG, PixelHeightAsset._1415_JPG, PixelWidthAsset._1415_JPG,
        ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG, Rotation.Rotate270, DHashes._1415_JPG, false,
        null, true, "The asset has been rotated", ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void CreateAsset_PictureAndDHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, true, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.That(asset, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                AssertAssetPropertyValidity(
                    asset!,
                    fileName,
                    imagePath,
                    folderPath,
                    folder,
                    fileSize,
                    pixelWidth,
                    pixelHeight,
                    thumbnailPixelWidth,
                    thumbnailPixelHeight,
                    ModificationDate.Default,
                    imageRotation,
                    hash,
                    isCorrupted,
                    corruptedMessage,
                    isRotated,
                    rotatedMessage);

                AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG,
        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_1_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, FileSize.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
        PixelWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG, Rotation.Rotate90, MD5Hashes.IMAGE_1_90_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, FileSize.IMAGE_1_180_DEG_JPG, PixelHeightAsset.IMAGE_1_180_DEG_JPG,
        PixelWidthAsset.IMAGE_1_180_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_180_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_180_DEG_JPG, Rotation.Rotate180, MD5Hashes.IMAGE_1_180_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, FileSize.IMAGE_1_270_DEG_JPG, PixelHeightAsset.IMAGE_1_270_DEG_JPG,
        PixelWidthAsset.IMAGE_1_270_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_270_DEG_JPG,
        ThumbnailHeightAsset.IMAGE_1_270_DEG_JPG, Rotation.Rotate270, MD5Hashes.IMAGE_1_270_DEG_JPG, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, FileSize.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG, PixelWidthAsset.IMAGE_2_JPG,
        ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_2_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, FileSize.IMAGE_2_DUPLICATED_JPG,
        PixelHeightAsset.IMAGE_2_DUPLICATED_JPG, PixelWidthAsset.IMAGE_2_DUPLICATED_JPG,
        ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG, Rotation.Rotate0,
        MD5Hashes.IMAGE_2_DUPLICATED_JPG, false, null, false, null, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, FileSize.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG, PixelWidthAsset.IMAGE_3_JPG,
        ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_3_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, FileSize.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG, PixelWidthAsset.IMAGE_4_JPG,
        ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_4_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, FileSize.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG, PixelWidthAsset.IMAGE_5_JPG,
        ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_5_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, FileSize.IMAGE_6_JPG, PixelHeightAsset.IMAGE_6_JPG, PixelWidthAsset.IMAGE_6_JPG,
        ThumbnailWidthAsset.IMAGE_6_JPG, ThumbnailHeightAsset.IMAGE_6_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_6_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, FileSize.IMAGE_7_JPG, PixelHeightAsset.IMAGE_7_JPG, PixelWidthAsset.IMAGE_7_JPG,
        ThumbnailWidthAsset.IMAGE_7_JPG, ThumbnailHeightAsset.IMAGE_7_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_7_JPG,
        false, null, false, null, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, FileSize.IMAGE_8_JPEG, PixelHeightAsset.IMAGE_8_JPEG,
        PixelWidthAsset.IMAGE_8_JPEG, ThumbnailWidthAsset.IMAGE_8_JPEG, ThumbnailHeightAsset.IMAGE_8_JPEG,
        Rotation.Rotate0, MD5Hashes.IMAGE_8_JPEG, false, null, false, null, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG,
        ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG, Rotation.Rotate0, MD5Hashes.IMAGE_9_PNG,
        false, null, false, null, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, FileSize.IMAGE_10_PORTRAIT_PNG, PixelHeightAsset.IMAGE_10_PORTRAIT_PNG,
        PixelWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG,
        ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG, Rotation.Rotate0, MD5Hashes.IMAGE_10_PORTRAIT_PNG, false, null,
        false, null, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_11_HEIC, FileSize.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
        PixelWidthAsset.IMAGE_11_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        Rotation.Rotate0, MD5Hashes.IMAGE_11_HEIC, false, null, false, null, ImageByteSizes.IMAGE_11_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, MD5Hashes.IMAGE_11_90_DEG_HEIC, false, null, true,
        "The asset has been rotated", ImageByteSizes.IMAGE_11_90_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, MD5Hashes.IMAGE_11_180_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_180_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, MD5Hashes.IMAGE_11_270_DEG_HEIC, false, null,
        true, "The asset has been rotated", ImageByteSizes.IMAGE_11_270_DEG_HEIC, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, FileSize.IMAGE_WITH_UPPERCASE_NAME_JPG,
        PixelHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, PixelWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        ThumbnailWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, ThumbnailHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG,
        Rotation.Rotate0, MD5Hashes.IMAGE_WITH_UPPERCASE_NAME_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG, "")]
    [TestCase(FileNames.HOMER_GIF, FileSize.HOMER_GIF, PixelHeightAsset.HOMER_GIF, PixelWidthAsset.HOMER_GIF,
        ThumbnailWidthAsset.HOMER_GIF, ThumbnailHeightAsset.HOMER_GIF, Rotation.Rotate0, MD5Hashes.HOMER_GIF, false,
        null, false, null, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, FileSize._1336_BOTTOM_LEFT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG, PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG,
        Rotation.Rotate0, MD5Hashes._1336_BOTTOM_LEFT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, FileSize._1336_BOTTOM_PART_JPG, PixelHeightAsset._1336_BOTTOM_PART_JPG,
        PixelWidthAsset._1336_BOTTOM_PART_JPG, ThumbnailWidthAsset._1336_BOTTOM_PART_JPG,
        ThumbnailHeightAsset._1336_BOTTOM_PART_JPG, Rotation.Rotate0, MD5Hashes._1336_BOTTOM_PART_JPG, false, null,
        false, null, ImageByteSizes._1336_BOTTOM_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileSize._1336_BOTTOM_RIGHT_PART_JPG,
        PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG, PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG,
        Rotation.Rotate0, MD5Hashes._1336_BOTTOM_RIGHT_PART_JPG, false, null, false, null,
        ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, FileSize._1336_LEFT_PART_JPG, PixelHeightAsset._1336_LEFT_PART_JPG,
        PixelWidthAsset._1336_LEFT_PART_JPG, ThumbnailWidthAsset._1336_LEFT_PART_JPG,
        ThumbnailHeightAsset._1336_LEFT_PART_JPG, Rotation.Rotate0, MD5Hashes._1336_LEFT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_LEFT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, FileSize._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
        PixelWidthAsset._1336_ORIGINAL_JPG, ThumbnailWidthAsset._1336_ORIGINAL_JPG,
        ThumbnailHeightAsset._1336_ORIGINAL_JPG, Rotation.Rotate0, MD5Hashes._1336_ORIGINAL_JPG, false, null, false,
        null, ImageByteSizes._1336_ORIGINAL_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, FileSize._1336_RIGHT_PART_JPG, PixelHeightAsset._1336_RIGHT_PART_JPG,
        PixelWidthAsset._1336_RIGHT_PART_JPG, ThumbnailWidthAsset._1336_RIGHT_PART_JPG,
        ThumbnailHeightAsset._1336_RIGHT_PART_JPG, Rotation.Rotate0, MD5Hashes._1336_RIGHT_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_RIGHT_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, FileSize._1336_TOP_LEFT_PART_JPG,
        PixelHeightAsset._1336_TOP_LEFT_PART_JPG, PixelWidthAsset._1336_TOP_LEFT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG, Rotation.Rotate0,
        MD5Hashes._1336_TOP_LEFT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, FileSize._1336_TOP_PART_JPG, PixelHeightAsset._1336_TOP_PART_JPG,
        PixelWidthAsset._1336_TOP_PART_JPG, ThumbnailWidthAsset._1336_TOP_PART_JPG,
        ThumbnailHeightAsset._1336_TOP_PART_JPG, Rotation.Rotate0, MD5Hashes._1336_TOP_PART_JPG, false, null, false,
        null, ImageByteSizes._1336_TOP_PART_JPG, $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, FileSize._1336_TOP_RIGHT_PART_JPG,
        PixelHeightAsset._1336_TOP_RIGHT_PART_JPG, PixelWidthAsset._1336_TOP_RIGHT_PART_JPG,
        ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG, Rotation.Rotate0,
        MD5Hashes._1336_TOP_RIGHT_PART_JPG, false, null, false, null, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, FileSize._1336_1_K_JPG, PixelHeightAsset._1336_1_K_JPG,
        PixelWidthAsset._1336_1_K_JPG, ThumbnailWidthAsset._1336_1_K_JPG, ThumbnailHeightAsset._1336_1_K_JPG,
        Rotation.Rotate0, MD5Hashes._1336_1_K_JPG, false, null, false, null, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, FileSize._1336_2_K_JPG, PixelHeightAsset._1336_2_K_JPG,
        PixelWidthAsset._1336_2_K_JPG, ThumbnailWidthAsset._1336_2_K_JPG, ThumbnailHeightAsset._1336_2_K_JPG,
        Rotation.Rotate0, MD5Hashes._1336_2_K_JPG, false, null, false, null, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, FileSize._1336_3_K_JPG, PixelHeightAsset._1336_3_K_JPG,
        PixelWidthAsset._1336_3_K_JPG, ThumbnailWidthAsset._1336_3_K_JPG, ThumbnailHeightAsset._1336_3_K_JPG,
        Rotation.Rotate0, MD5Hashes._1336_3_K_JPG, false, null, false, null, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, FileSize._1336_4_K_ORIGINAL_JPG,
        PixelHeightAsset._1336_4_K_ORIGINAL_JPG, PixelWidthAsset._1336_4_K_ORIGINAL_JPG,
        ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG, Rotation.Rotate0,
        MD5Hashes._1336_4_K_ORIGINAL_JPG, false, null, false, null, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, FileSize._1336_8_K_JPG, PixelHeightAsset._1336_8_K_JPG,
        PixelWidthAsset._1336_8_K_JPG, ThumbnailWidthAsset._1336_8_K_JPG, ThumbnailHeightAsset._1336_8_K_JPG,
        Rotation.Rotate0, MD5Hashes._1336_8_K_JPG, false, null, false, null, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, FileSize._1336_THUMBNAIL_JPG, PixelHeightAsset._1336_THUMBNAIL_JPG,
        PixelWidthAsset._1336_THUMBNAIL_JPG, ThumbnailWidthAsset._1336_THUMBNAIL_JPG,
        ThumbnailHeightAsset._1336_THUMBNAIL_JPG, Rotation.Rotate0, MD5Hashes._1336_THUMBNAIL_JPG, false, null, false,
        null, ImageByteSizes._1336_THUMBNAIL_JPG, $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, FileSize.IMAGE_1336_MINI_JPG, PixelHeightAsset.IMAGE_1336_MINI_JPG,
        PixelWidthAsset.IMAGE_1336_MINI_JPG, ThumbnailWidthAsset.IMAGE_1336_MINI_JPG,
        ThumbnailHeightAsset.IMAGE_1336_MINI_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_1336_MINI_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_MINI_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, FileSize.IMAGE_1336_ORIGINAL_JPG,
        PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG, PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG,
        ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG, Rotation.Rotate0,
        MD5Hashes.IMAGE_1336_ORIGINAL_JPG, false, null, false, null, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileSize.IMAGE_1336_SHIT_QUALITY_JPG,
        PixelHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG, PixelWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        ThumbnailWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, ThumbnailHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG,
        Rotation.Rotate0, MD5Hashes.IMAGE_1336_SHIT_QUALITY_JPG, false, null, false, null,
        ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, FileSize.IMAGE_1336_SMALL_JPG, PixelHeightAsset.IMAGE_1336_SMALL_JPG,
        PixelWidthAsset.IMAGE_1336_SMALL_JPG, ThumbnailWidthAsset.IMAGE_1336_SMALL_JPG,
        ThumbnailHeightAsset.IMAGE_1336_SMALL_JPG, Rotation.Rotate0, MD5Hashes.IMAGE_1336_SMALL_JPG, false, null, false,
        null, ImageByteSizes.IMAGE_1336_SMALL_JPG, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, FileSize._1337_JPG, PixelHeightAsset._1337_JPG, PixelWidthAsset._1337_JPG,
        ThumbnailWidthAsset._1337_JPG, ThumbnailHeightAsset._1337_JPG, Rotation.Rotate0, MD5Hashes._1337_JPG, false,
        null, false, null, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, FileSize._1349_JPG, PixelHeightAsset._1349_JPG, PixelWidthAsset._1349_JPG,
        ThumbnailWidthAsset._1349_JPG, ThumbnailHeightAsset._1349_JPG, Rotation.Rotate0, MD5Hashes._1349_JPG, false,
        null, false, null, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, FileSize._1350_JPG, PixelHeightAsset._1350_JPG, PixelWidthAsset._1350_JPG,
        ThumbnailWidthAsset._1350_JPG, ThumbnailHeightAsset._1350_JPG, Rotation.Rotate0, MD5Hashes._1350_JPG, false,
        null, false, null, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, FileSize._1413_JPG, PixelHeightAsset._1413_JPG, PixelWidthAsset._1413_JPG,
        ThumbnailWidthAsset._1413_JPG, ThumbnailHeightAsset._1413_JPG, Rotation.Rotate0, MD5Hashes._1413_JPG, false,
        null, false, null, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, FileSize._1414_JPG, PixelHeightAsset._1414_JPG, PixelWidthAsset._1414_JPG,
        ThumbnailWidthAsset._1414_JPG, ThumbnailHeightAsset._1414_JPG, Rotation.Rotate0, MD5Hashes._1414_JPG, false,
        null, false, null, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, FileSize._1415_JPG, PixelHeightAsset._1415_JPG, PixelWidthAsset._1415_JPG,
        ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG, Rotation.Rotate270, MD5Hashes._1415_JPG, false,
        null, true, "The asset has been rotated", ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void CreateAsset_PictureAndMD5HashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, true, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.That(asset, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                AssertAssetPropertyValidity(
                    asset!,
                    fileName,
                    imagePath,
                    folderPath,
                    folder,
                    fileSize,
                    pixelWidth,
                    pixelHeight,
                    thumbnailPixelWidth,
                    thumbnailPixelHeight,
                    ModificationDate.Default,
                    imageRotation,
                    hash,
                    isCorrupted,
                    corruptedMessage,
                    isRotated,
                    rotatedMessage);

                AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [TestCase(FileNames._1415_JPG, FileSize._1415_JPG, PixelHeightAsset._1415_JPG, PixelWidthAsset._1415_JPG,
        ThumbnailWidthAsset._1415_JPG, ThumbnailHeightAsset._1415_JPG, Rotation.Rotate270, Hashes._1415_JPG, false,
        null, true, "The asset has been rotated", ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void CreateAsset_PreviouslyRotatedPictureAndBasicHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.That(asset, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                AssertAssetPropertyValidity(
                    asset!,
                    fileName,
                    imagePath,
                    folderPath,
                    folder,
                    fileSize,
                    pixelWidth,
                    pixelHeight,
                    thumbnailPixelWidth,
                    thumbnailPixelHeight,
                    ModificationDate.Default,
                    imageRotation,
                    hash,
                    isCorrupted,
                    corruptedMessage,
                    isRotated,
                    rotatedMessage);

                AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, 150, 84, 1653)]
    [TestCase(200, 150, ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, ImageByteSizes.IMAGE_1_JPG)]
    [TestCase(150, 200, 150, 84, 1653)]
    [TestCase(0, 0, 0, 0, 30773)]
    [TestCase(1000, 10, 1000, 562, 23730)]
    [TestCase(10, 10000, 10, 6, 662)]
    [TestCase(150, 10000000, 150, 84, 1653)]
    [TestCase(-100, -100, -100, -56, 1232)]
    public void CreateAsset_PictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = expectedThumbnailPixelWidth, Height = expectedThumbnailPixelHeight }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG,
        ImageByteSizes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(200, 150, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG, ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG,
        ImageByteSizes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(150, 200, 112, 200, 11218)]
    [TestCase(0, 0, 0, 0, 142863)]
    [TestCase(1000, 10, 6, 10, 282)]
    [TestCase(10, 10000, 5625, 10000, 14790272)]
    [TestCase(10000000, 100, 56, 100, 2501)]
    [TestCase(-100, -100, -56, -100, 2490)]
    public void CreateAsset_PortraitPictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_10_PORTRAIT_PNG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_10_PORTRAIT_PNG,
                    Height = PixelHeightAsset.IMAGE_10_PORTRAIT_PNG
                },
                Thumbnail = new() { Width = expectedThumbnailPixelWidth, Height = expectedThumbnailPixelHeight }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_10_PORTRAIT_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_10_PORTRAIT_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        ImageByteSizes.IMAGE_11_HEIC)]
    [TestCase(200, 150, ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC,
        ImageByteSizes.IMAGE_11_HEIC)]
    [TestCase(150, 200, 150, 200, 9594)]
    [TestCase(0, 0, 0, 0, 634)]
    [TestCase(1000, 10, 8, 10, 704)]
    [TestCase(10, 10000, 7500, 10000, 7267008)]
    [TestCase(100000, 100, 75, 100, 3187)]
    public void CreateAsset_HeicPictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = expectedThumbnailPixelWidth, Height = expectedThumbnailPixelHeight }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_PictureAndBasicHashTypeAndCreatingTwiceSameImageInDifferentLocations_ReturnsAssetAndCreatesBothAssets()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        Asset expectedAsset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        const int imageByteSize = ImageByteSizes.IMAGE_1_JPG;

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            string newSameAssetFolderPath =
                Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder newSameAssetFolder =
                _testableAssetRepository!.AddFolder(newSameAssetFolderPath); // Set above, not in this method

            string newSameImagePath = Path.Combine(newSameAssetFolderPath, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            Asset? newSameAsset = _assetCreationService!.CreateAsset(newSameAssetFolderPath, expectedAsset.FileName);

            Assert.That(newSameAsset, Is.Not.Null);

            AssertAssetPropertyValidity(
                newSameAsset!,
                expectedAsset.FileName,
                newSameImagePath,
                newSameAssetFolderPath,
                newSameAssetFolder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            Asset firstAssetFromRepository = assetsFromRepository[0];

            Assert.That(firstAssetFromRepository.FileName, Is.EqualTo(asset!.FileName));
            Assert.That(firstAssetFromRepository.FolderId, Is.EqualTo(asset.FolderId));
            Assert.That(firstAssetFromRepository.Hash, Is.EqualTo(asset.Hash));
            Assert.That(firstAssetFromRepository.ImageData, Is.Null);

            Asset secondAssetFromRepository = assetsFromRepository[1];

            Assert.That(secondAssetFromRepository.FileName, Is.EqualTo(newSameAsset!.FileName));
            Assert.That(secondAssetFromRepository.FolderId, Is.EqualTo(newSameAsset.FolderId));
            Assert.That(secondAssetFromRepository.Hash, Is.EqualTo(newSameAsset.Hash));
            Assert.That(secondAssetFromRepository.ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(asset.Folder.Path), Is.True);
            Assert.That(thumbnails.ContainsKey(newSameAsset.Folder.Path), Is.True);

            Assert.That(thumbnails[folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folder.Path].ContainsKey(asset.FileName), Is.True);
            Assert.That(thumbnails[folder.Path][asset.FileName], Is.Not.Null);
            Assert.That(thumbnails[folder.Path][asset.FileName], Has.Length.EqualTo(imageByteSize));

            Assert.That(thumbnails[newSameAssetFolder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[newSameAssetFolder.Path].ContainsKey(newSameAsset.FileName), Is.True);
            Assert.That(thumbnails[newSameAssetFolder.Path][newSameAsset.FileName], Is.Not.Null);
            Assert.That(thumbnails[newSameAssetFolder.Path][newSameAsset.FileName], Has.Length.EqualTo(imageByteSize));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndCreatingTwiceSameImage_DoesNotAddTheSecondOneAndReturnsNull()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        Asset expectedAsset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        const int imageByteSize = ImageByteSizes.IMAGE_1_JPG;

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);

            Asset? newSameAsset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.That(newSameAsset, Is.Null);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, "Invalid_Image_1.jpg")]
    [TestCase(FileNames.HOMER_GIF, "Invalid_Homer.gif")]
    [TestCase(FileNames.IMAGE_9_PNG, "Invalid_Image_9.png")]
    [TestCase(FileNames.IMAGE_11_HEIC, "Invalid_Image_11.heic")]
    public void CreateAsset_InvalidPicture_ReturnsNullAndDoesNotCreateAsset(string fileName, string invalidFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        string tempDirectory = Path.Combine(_dataDirectory!, Directories.INVALID_IMAGE);

        try
        {
            string imagePath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(imagePath), Is.True);

            Directory.CreateDirectory(tempDirectory);
            string invalidImagePath = Path.Combine(tempDirectory, invalidFileName);
            Assert.That(File.Exists(invalidImagePath), Is.False);

            ImageHelper.CreateInvalidImage(imagePath, invalidImagePath);

            Assert.That(File.Exists(invalidImagePath), Is.True);

            _testableAssetRepository!.AddFolder(tempDirectory); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(tempDirectory, invalidFileName);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    // [TestCase(FileNames.IMAGE_9_PNG, "Corrupted_Image_1.jpg", FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG, ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG, Rotation.Rotate0, Hashes.IMAGE_9_PNG, 2148)] // Seems to work now with Win 11
    [TestCase(FileNames.IMAGE_9_PNG, "Corrupted_Image_1.heic", FileSize.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
        PixelWidthAsset.IMAGE_9_PNG, ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
        Rotation.Rotate0, Hashes.IMAGE_9_PNG, 2107)]
    [TestCase(FileNames.IMAGE_1_JPG, "Corrupted_Image_1.heic", FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
        PixelWidthAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
        Rotation.Rotate0, Hashes.IMAGE_1_JPG, 2134)]
    public void CreateAsset_CorruptedPictureBasicHashType_ReturnsAssetWithCorruptedMessage(
        string initialFileName,
        string corruptedAssetFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        int imageByteSizeExpected)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        string tempDirectory = Path.Combine(_dataDirectory!, Directories.CORRUPTED_IMAGE);

        try
        {
            Asset expectedAsset = new()
            {
                FolderId = Guid.Empty, // Initialised later
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = corruptedAssetFileName,
                Pixel = new()
                {
                    Asset = new() { Width = pixelWidth, Height = pixelHeight },
                    Thumbnail = new() { Width = thumbnailPixelWidth, Height = thumbnailPixelHeight }
                },
                FileProperties = new()
                {
                    Size = fileSize,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = imageRotation,
                Hash = hash,
                Metadata = new()
                {
                    Corrupted = new()
                    {
                        IsTrue = true,
                        Message = _userConfigurationService!.AssetSettings.CorruptedMessage
                    },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            string imagePath = Path.Combine(_dataDirectory!, initialFileName);
            Assert.That(File.Exists(imagePath), Is.True);

            Directory.CreateDirectory(tempDirectory);
            string corruptedImagePath = Path.Combine(tempDirectory, corruptedAssetFileName);
            Assert.That(File.Exists(corruptedImagePath), Is.False);

            File.Copy(imagePath, corruptedImagePath, overwrite: true);

            Assert.That(File.Exists(corruptedImagePath), Is.True);

            Folder folder = _testableAssetRepository!.AddFolder(tempDirectory); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(tempDirectory, corruptedAssetFileName);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                corruptedImagePath,
                tempDirectory,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSizeExpected);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFolderNotAdded_ThrowsNullReferenceExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            const string assetName = FileNames.IMAGE_1_JPG;
            string imagePath = Path.Combine(_dataDirectory!, assetName);

            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() =>
                    _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, 100000, 100)] // Width > Height
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, 100, 100000)] // Height > Width
    [TestCase(FileNames.HOMER_GIF, 100, 100000)] // Width == Height
    public void
        CreateAsset_PictureAndBasicHashTypeAndThumbnailSettingsTooBig_ThrowsOverflowExceptionAndAssetIsNotCreated(
            string assetName, int thumbnailMaxWidth, int thumbnailMaxHeight)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            OverflowException? exception =
                Assert.Throws<OverflowException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_HeicPictureAndBasicHashTypeAndThumbnailSettingsTooBig_ThrowsInvalidOperationExceptionAndAssetIsNotCreated()
    {
        const string assetName = FileNames.IMAGE_11_HEIC;
        const int thumbnailMaxWidth = 150;
        const int thumbnailMaxHeight = 10000000;

        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            InvalidOperationException? exception =
                Assert.Throws<InvalidOperationException>(() =>
                    _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.That(exception?.Message,
                Is.EqualTo("Operation is not valid due to the current state of the object."));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_HeicPictureAndBasicHashTypeAndNegativeThumbnailSettings_ThrowsInvalidOperationExceptionAndAssetIsNotCreated()
    {
        const string assetName = FileNames.IMAGE_11_HEIC;
        const int thumbnailMaxWidth = -100;
        const int thumbnailMaxHeight = -100;

        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            InvalidOperationException? exception =
                Assert.Throws<InvalidOperationException>(() =>
                    _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.That(exception?.Message,
                Is.EqualTo("Operation is not valid due to the current state of the object."));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_PictureAndBasicHashTypeAndDirectoryNameIsNull_ThrowsArgumentNullExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            const string assetName = FileNames.IMAGE_1_JPG;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            ArgumentNullException? exception =
                Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(null!, assetName));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndDirectoryNameIsEmpty_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = FileNames.IMAGE_1_JPG;
            string directoryName =
                Path.Combine(_dataDirectory!, Directories.TEST_FOLDER, Directories.TEST_HIDDEN_SUB_FOLDER);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            FileNotFoundException fileNotFoundException =
                new($"The file {Path.Combine(directoryName, assetName)} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void
        CreateAsset_PictureAndBasicHashTypeAndDirectoryNamePointingToAFile_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = FileNames.IMAGE_1_JPG;
            string directoryName = Path.Combine(_dataDirectory!, assetName);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            FileNotFoundException fileNotFoundException =
                new($"The file {Path.Combine(directoryName, assetName)} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameIsNull_ThrowsArgumentNullExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string assetName = null!;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path2')"));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameDoesNotExist_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = FileNames.NON_EXISTENT_FILE_JPG;

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.False);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, assetName);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            FileNotFoundException fileNotFoundException = new($"The file {imagePath} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameIsADirectory_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = FileNames.IMAGE_1_JPG;

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, _dataDirectory!);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            FileNotFoundException fileNotFoundException = new($"The file {_dataDirectory} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, FileSize.HOMER_JPG, PixelHeightAsset.HOMER_JPG,
        PixelWidthAsset.HOMER_JPG, ThumbnailWidthAsset.HOMER_JPG, ThumbnailHeightAsset.HOMER_JPG, Rotation.Rotate0,
        Hashes.HOMER_JPG, false, null, false, null, ImageByteSizes.HOMER_JPG)]
    public void CreateAsset_VideoAndBasicHashType_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(videoPath), Is.True);

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.That(File.Exists(imagePath), Is.False);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset, Is.Null);

            Assert.That(File.Exists(imagePath), Is.True);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            asset = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.That(asset, Is.Not.Null);

            Assert.That(File.Exists(videoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            AssertAssetPropertyValidity(
                asset!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                fileSize,
                pixelWidth,
                pixelHeight,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                DateTime.Now.Date,
                imageRotation,
                hash,
                isCorrupted,
                corruptedMessage,
                isRotated,
                rotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    // TODO: Evolve this test when the rework of video handling has been made
    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, FileSize.HOMER_JPG, PixelHeightAsset.HOMER_JPG,
        PixelWidthAsset.HOMER_JPG, ThumbnailWidthAsset.HOMER_JPG, ThumbnailHeightAsset.HOMER_JPG, Rotation.Rotate0,
        Hashes.HOMER_JPG, false, null, false, null, ImageByteSizes.HOMER_JPG)]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public void CreateAsset_VideoAndBasicHashTypeAndCreateFirstFrameInSameLocationAsVideo_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY,
            _dataDirectory!); // Only needed for videos
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200");
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, "true");
        configurationRootMock.MockGetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME, "TempForVideo");

        _userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        _assetCreationService = new(_testableAssetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Directory.CreateDirectory(firstFrameVideosPath);

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(videoPath), Is.True);

            string newVideoPath = Path.Combine(firstFrameVideosPath, fileName);
            File.Copy(videoPath, newVideoPath);
            Assert.That(File.Exists(newVideoPath), Is.True);
            Assert.That(File.Exists(videoPath), Is.True);

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.That(File.Exists(imagePath), Is.False);

            // Folder folder = _testableAssetRepository!.AddFolder(firstFrameVideosPath); // Set above, not in this method
            _testableAssetRepository!.AddFolder(firstFrameVideosPath); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(firstFrameVideosPath, fileName, true);

            //Assert.That(asset, Is.Not.Null);
            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Assert.That(File.Exists(newVideoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            // AssertAssetPropertyValidity(
            //     asset!,
            //     firstFrameFileName,
            //     imagePath,
            //     firstFrameVideosPath,
            //     folder,
            //     fileSize,
            //     pixelWidth,
            //     pixelHeight,
            //     thumbnailPixelWidth,
            //     thumbnailPixelHeight,
            //     imageRotation,
            //     hash,
            //     isCorrupted,
            //     corruptedMessage,
            //     isRotated,
            //     rotatedMessage);

            // AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase(FileNames.HOMER_1_S_MP4, FileNames.HOMER_1_S_JPG)]
    public void CreateAsset_VideoTooShortAndBasicHashType_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(
        string fileName, string firstFrameFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(videoPath), Is.True);

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.That(File.Exists(imagePath), Is.False);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset, Is.Null);

            Assert.That(File.Exists(imagePath), Is.False);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, FileSize.HOMER_JPG, PixelHeightAsset.HOMER_JPG,
        PixelWidthAsset.HOMER_JPG, ThumbnailWidthAsset.HOMER_JPG, ThumbnailHeightAsset.HOMER_JPG, Rotation.Rotate0,
        Hashes.HOMER_JPG, false, null, false, null, ImageByteSizes.HOMER_JPG)]
    public void CreateAsset_VideoAndBasicHashTypeAndAddSameVideoTwice_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(videoPath), Is.True);

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.That(File.Exists(imagePath), Is.False);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset1, Is.Null);

            Assert.That(File.Exists(imagePath), Is.True);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset2 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.That(asset2, Is.Not.Null);

            Assert.That(File.Exists(videoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            AssertAssetPropertyValidity(
                asset2!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                fileSize,
                pixelWidth,
                pixelHeight,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                DateTime.Now.Date,
                imageRotation,
                hash,
                isCorrupted,
                corruptedMessage,
                isRotated,
                rotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, imageByteSize);

            Asset? asset3 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset3, Is.Null);

            Assert.That(File.Exists(videoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, imageByteSize);

            folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset4 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.That(asset4, Is.Null);

            Assert.That(File.Exists(videoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            Assert.That(asset2!.FileName, Is.EqualTo(firstFrameFileName));

            AssertCataloguedAssetAndThumbnailValidity(asset2, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG)]
    public void
        CreateAsset_VideoAndBasicHashTypeButAnotherImageAlreadyExistsInTheOutputDirectory_DoesNotCreateFirstFrame(
            string fileName, string firstFrameFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            Directory.CreateDirectory(firstFrameVideosPath);

            string sourceImagePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            File.Copy(sourceImagePath, imagePath);
            Assert.That(File.Exists(imagePath), Is.True);

            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(videoPath), Is.True);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset1, Is.Null);

            Assert.That(File.Exists(imagePath), Is.True);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset2 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.That(asset2, Is.Not.Null);

            Assert.That(File.Exists(videoPath), Is.True);
            Assert.That(File.Exists(imagePath), Is.True);

            Asset expectedAsset = new()
            {
                FolderId = Guid.Empty, // Initialised later
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = firstFrameFileName,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_1_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_1_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            const int expectedImageByteSize = ImageByteSizes.IMAGE_1_JPG;

            AssertAssetPropertyValidity(
                asset2!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                expectedAsset.FileProperties.Size,
                expectedAsset.Pixel.Asset.Width,
                expectedAsset.Pixel.Asset.Height,
                expectedAsset.Pixel.Thumbnail.Width,
                expectedAsset.Pixel.Thumbnail.Height,
                expectedAsset.FileProperties.Modification,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.Metadata.Corrupted.IsTrue,
                expectedAsset.Metadata.Corrupted.Message,
                expectedAsset.Metadata.Rotated.IsTrue,
                expectedAsset.Metadata.Rotated.Message);

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [Test]
    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, false, false)]
    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, false, true)]
    [TestCase(FileNames.HOMER_MP4, FileNames.HOMER_JPG, true, false)]
    public void CreateAsset_VideoAndIsVideoAndAnalyseVideos_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(
        string fileName, string firstFrameFileName, bool analyseVideos, bool isVideo)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(filePath), Is.True);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, isVideo);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.That(File.Exists(imagePath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, FileSize.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG,
        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, Rotation.Rotate0, Hashes.IMAGE_1_JPG, false,
        null, false, null, ImageByteSizes.IMAGE_1_JPG, false)]
    public void CreateAsset_PictureAndIsVideoIsTrueAndAnalyseVideosIsFalse_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage,
        int imageByteSize,
        bool analyseVideos)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(filePath), Is.True);

            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset, Is.Not.Null);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                filePath,
                _dataDirectory!,
                folder,
                fileSize,
                pixelWidth,
                pixelHeight,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                ModificationDate.Default,
                imageRotation,
                hash,
                isCorrupted,
                corruptedMessage,
                isRotated,
                rotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);

            string imagePath = Path.Combine(firstFrameVideosPath, fileName);
            Assert.That(File.Exists(imagePath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (analyseVideos)
            {
                Directory.Delete(firstFrameVideosPath, true);
            }
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, true)]
    public void
        CreateAsset_PictureAndIsVideoIsTrueAndAnalyseVideosIsTrue_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(
            string fileName, bool analyseVideos)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.That(File.Exists(filePath), Is.True);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            string imagePath = Path.Combine(firstFrameVideosPath, fileName);
            Assert.That(File.Exists(imagePath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (analyseVideos)
            {
                Directory.Delete(firstFrameVideosPath, true);
            }
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndDirectoryNameIsNull_ThrowsArgumentNullExceptionAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = FileNames.HOMER_MP4;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            ArgumentNullException? exception =
                Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(null!, assetName, true));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, FileNames.HOMER_JPG);
            Assert.That(File.Exists(firstFrameGeneratedPath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndDirectoryNameIsEmpty_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = FileNames.HOMER_MP4;
            string directoryName =
                Path.Combine(_dataDirectory!, Directories.TEST_FOLDER, Directories.TEST_HIDDEN_SUB_FOLDER);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName, true);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, FileNames.HOMER_JPG);
            Assert.That(File.Exists(firstFrameGeneratedPath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndDirectoryNamePointingToAFile_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = FileNames.HOMER_MP4;
            string directoryName = Path.Combine(_dataDirectory!, assetName);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName, true);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, FileNames.HOMER_JPG);
            Assert.That(File.Exists(firstFrameGeneratedPath), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndFileNameIsNull_ThrowsArgumentNullExceptionAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            string assetName = null!;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _assetCreationService!.CreateAsset(_dataDirectory!, assetName, true));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path2')"));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndFileNameDoesNotExist_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = FileNames.NON_EXISTENT_VIDEO_MP4;

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.False);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, assetName, true);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void
        CreateAsset_VideoAndBasicHashTypeAndFileNameIsADirectory_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = FileNames.HOMER_MP4;

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.That(File.Exists(imagePath), Is.True);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, _dataDirectory!, true);

            Assert.That(asset, Is.Null);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);
            Assert.That(thumbnails, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    private static void AssertAssetPropertyValidity(
        Asset asset,
        string fileName,
        string imagePath,
        string folderPath,
        Folder folder,
        long fileSize,
        int pixelWidth,
        int pixelHeight,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        DateTime fileModificationDateTime,
        Rotation imageRotation,
        string hash,
        bool isCorrupted,
        string? corruptedMessage,
        bool isRotated,
        string? rotatedMessage)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.That(asset.FileName, Is.EqualTo(fileName));
        Assert.That(asset.FolderId, Is.EqualTo(folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(fileSize));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(pixelWidth));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(pixelHeight));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(thumbnailPixelWidth));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(thumbnailPixelHeight));
        Assert.That(asset.ImageRotation, Is.EqualTo(imageRotation));
        Assert.That(asset.ThumbnailCreationDateTime.Date, Is.EqualTo(actualDate));
        Assert.That(asset.Hash, Is.EqualTo(hash));
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(isCorrupted));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(corruptedMessage));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(isRotated));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(rotatedMessage));
        Assert.That(asset.FullPath, Is.EqualTo(imagePath));
        Assert.That(asset.Folder.Path, Is.EqualTo(folderPath));
        Assert.That(asset.ImageData, Is.Null); // Set above, not in this method
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(actualDate));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(fileModificationDateTime.Date));
    }

    private void AssertCataloguedAssetAndThumbnailValidity(Asset asset, Folder folder,
        Dictionary<string, Dictionary<string, byte[]>> thumbnails, int imageByteSize)
    {
        List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
        Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

        Asset assetFromRepository = assetsFromRepository[0];

        Assert.That(assetFromRepository.FileName, Is.EqualTo(asset.FileName));
        Assert.That(assetFromRepository.FolderId, Is.EqualTo(asset.FolderId));
        Assert.That(assetFromRepository.Hash, Is.EqualTo(asset.Hash));
        Assert.That(assetFromRepository.ImageData, Is.Null);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails.ContainsKey(asset.Folder.Path), Is.True);
        Assert.That(thumbnails[folder.Path], Has.Count.EqualTo(1));
        Assert.That(thumbnails[folder.Path].ContainsKey(asset.FileName), Is.True);
        Assert.That(thumbnails[folder.Path][asset.FileName], Is.Not.Null);
        Assert.That(thumbnails[folder.Path][asset.FileName], Has.Length.EqualTo(imageByteSize));
    }
}
