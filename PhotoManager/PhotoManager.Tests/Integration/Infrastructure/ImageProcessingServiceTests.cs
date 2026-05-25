using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using ImageByteSizes = PhotoManager.Tests.Integration.Constants.ImageByteSizes;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class ImageProcessingServiceTests
{
    private string? _assetsDirectory;

    private ImageProcessingService? _imageProcessingService;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<ImageProcessingService>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock);

        _testLogger = new();
        _imageProcessingService = new(_testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        using (SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
                   _testLogger!))
        {
            byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(image);

            Assert.That(imageBuffer, Is.Not.Null);
            Assert.That(imageBuffer, Is.Not.Empty);

            string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

            try
            {
                Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
                Directory.CreateDirectory(destinationNewFileDirectory);
                string destinationNewFilePath =
                    Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
                File.WriteAllBytes(destinationNewFilePath, imageBuffer);
                Assert.That(IsValidImage(destinationNewFilePath), Is.True);

                _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
            }
            finally
            {
                Directory.Delete(destinationNewFileDirectory, true);
            }
        }
    }

    [Test]
    public void GetJpegBitmapImage_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        using IImageData image =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService!.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        using SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.GetJpegBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        using (SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
                   _testLogger!))
        {
            byte[] imageBuffer = _imageProcessingService!.GetPngBitmapImage(image);

            Assert.That(imageBuffer, Is.Not.Null);
            Assert.That(imageBuffer, Is.Not.Empty);

            string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

            try
            {
                Assert.That(_imageProcessingService!.IsValidGdiPlusImage(imageBuffer), Is.True);
                Directory.CreateDirectory(destinationNewFileDirectory);
                string destinationNewFilePath =
                    Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
                File.WriteAllBytes(destinationNewFilePath, imageBuffer);
                Assert.That(IsValidImage(destinationNewFilePath), Is.True);

                _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
            }
            finally
            {
                Directory.Delete(destinationNewFileDirectory, true);
            }
        }
    }

    [Test]
    public void GetPngBitmapImage_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        using IImageData image =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService!.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        using SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetPngBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        using (SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
                   _testLogger!))
        {
            byte[] imageBuffer = _imageProcessingService!.GetGifBitmapImage(image);

            Assert.That(imageBuffer, Is.Not.Null);
            Assert.That(imageBuffer, Is.Not.Empty);

            string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

            try
            {
                Assert.That(_imageProcessingService!.IsValidGdiPlusImage(imageBuffer), Is.True);
                Directory.CreateDirectory(destinationNewFileDirectory);
                string destinationNewFilePath =
                    Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
                File.WriteAllBytes(destinationNewFilePath, imageBuffer);
                Assert.That(IsValidImage(destinationNewFilePath), Is.True);

                _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
            }
            finally
            {
                Directory.Delete(destinationNewFileDirectory, true);
            }
        }
    }

    [Test]
    public void GetGifBitmapImage_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        using IImageData image =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService!.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        using SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetGifBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    public void IsValidGdiPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidGdiPlusImage(validImageData);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidGdiPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = _imageProcessingService!.IsValidGdiPlusImage(emptyHeicData);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions(
            [new Exception("No imaging component suitable to complete this operation was found.")],
            typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidHeic(validHeicData);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = _imageProcessingService!.IsValidHeic(invalidHeicData);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _imageProcessingService!.IsValidHeic(emptyHeicData));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, ImageRotation.Rotate90, ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, ImageRotation.Rotate180, ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, ImageRotation.Rotate270, ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, ImageRotation.Rotate0,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG,
        "")]
    [TestCase(FileNames.HOMER_GIF, ImageRotation.Rotate0, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames.IMAGE_1_DUPLICATE_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1_DUPLICATE_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}")]
    [TestCase(FileNames.IMAGE_9_DUPLICATE_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_9_DUPLICATE_PNG,
        $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_THUMBNAIL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_MINI_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_SMALL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, ImageRotation.Rotate0, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, ImageRotation.Rotate0, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, ImageRotation.Rotate0, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, ImageRotation.Rotate0, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, ImageRotation.Rotate0, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, ImageRotation.Rotate270, ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void LoadBitmapThumbnailImage_ValidImage_ReturnsValidBitmapImage(
        string fileName, ImageRotation rotation, int imageByteSize, string additionalPath)
    {
        string folderPath = string.IsNullOrEmpty(additionalPath)
            ? _assetsDirectory!
            : Path.Combine(_assetsDirectory!, additionalPath);

        string filePath = Path.Combine(folderPath, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        int thumbnailWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;
        int thumbnailHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        using IImageData thumbnailImage =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, thumbnailWidth, thumbnailHeight);

        Assert.That(thumbnailImage, Is.Not.Null);
        Assert.That(thumbnailImage.Width, Is.EqualTo(thumbnailWidth));
        Assert.That(thumbnailImage.Height, Is.EqualTo(thumbnailHeight));

        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());
        byte[] imageBuffer;

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            imageBuffer = _imageProcessingService!.GetPngBitmapImage(thumbnailImage);
        }
        else if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            imageBuffer = _imageProcessingService!.GetGifBitmapImage(thumbnailImage);
        }
        else
        {
            imageBuffer = _imageProcessingService!.GetJpegBitmapImage(thumbnailImage);
        }

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Has.Length.EqualTo(imageByteSize));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, ImageRotation.Rotate0, ImageByteSizes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, ImageByteSizes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, ImageByteSizes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, ImageByteSizes.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapThumbnailImage_HeicValidImage_ReturnsValidBitmapImage(
        string fileName, ImageRotation rotation, int imageByteSize)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        int thumbnailWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;
        int thumbnailHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        // With preserve-orientation, rotation is applied manually via SkiaSharp.
        // 90°/270° rotations swap dimensions → landscape (4032×3024) → fits 200×150
        // 0°/180° keep portrait (3024×4032) → fits 113×150
        int expectedThumbnailWidth =
            rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270 ? thumbnailWidth : 113;

        using IImageData thumbnailImage =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, thumbnailWidth,
                thumbnailHeight);

        Assert.That(thumbnailImage, Is.Not.Null);
        Assert.That(thumbnailImage.Width, Is.EqualTo(expectedThumbnailWidth));
        Assert.That(thumbnailImage.Height, Is.EqualTo(thumbnailHeight));

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(thumbnailImage);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Has.Length.EqualTo(imageByteSize));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1_JPG, "")]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, ImageRotation.Rotate90, ImageByteSizes.IMAGE_1_90_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, ImageRotation.Rotate180, ImageByteSizes.IMAGE_1_180_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, ImageRotation.Rotate270, ImageByteSizes.IMAGE_1_270_DEG_JPG, "")]
    [TestCase(FileNames.IMAGE_2_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_2_JPG, "")]
    [TestCase(FileNames.IMAGE_2_DUPLICATED_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_2_DUPLICATED_JPG, "")]
    [TestCase(FileNames.IMAGE_3_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_3_JPG, "")]
    [TestCase(FileNames.IMAGE_4_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_4_JPG, "")]
    [TestCase(FileNames.IMAGE_5_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_5_JPG, "")]
    [TestCase(FileNames.IMAGE_6_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_6_JPG, "")]
    [TestCase(FileNames.IMAGE_7_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_7_JPG, "")]
    [TestCase(FileNames.IMAGE_8_JPEG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_8_JPEG, "")]
    [TestCase(FileNames.IMAGE_9_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_9_PNG, "")]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_10_PORTRAIT_PNG, "")]
    [TestCase(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG, ImageRotation.Rotate0,
        ImageByteSizes.IMAGE_WITH_UPPERCASE_NAME_JPG, "")]
    [TestCase(FileNames.HOMER_GIF, ImageRotation.Rotate0, ImageByteSizes.HOMER_GIF, "")]
    [TestCase(FileNames.IMAGE_1_DUPLICATE_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1_DUPLICATE_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}")]
    [TestCase(FileNames.IMAGE_9_DUPLICATE_PNG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_9_DUPLICATE_PNG,
        $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}")]
    [TestCase(FileNames._1336_BOTTOM_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_BOTTOM_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_BOTTOM_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_LEFT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_LEFT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_TOP_RIGHT_PART_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_TOP_RIGHT_PART_JPG,
        $"{Directories.DUPLICATES}\\{Directories.PART}")]
    [TestCase(FileNames._1336_1_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_1_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_2_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_2_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_3_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_3_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_4_K_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_4_K_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_8_K_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_8_K_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames._1336_THUMBNAIL_JPG, ImageRotation.Rotate0, ImageByteSizes._1336_THUMBNAIL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.RESOLUTION}")]
    [TestCase(FileNames.IMAGE_1336_MINI_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_MINI_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_ORIGINAL_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_ORIGINAL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SHIT_QUALITY_JPG, ImageRotation.Rotate0,
        ImageByteSizes.IMAGE_1336_SHIT_QUALITY_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames.IMAGE_1336_SMALL_JPG, ImageRotation.Rotate0, ImageByteSizes.IMAGE_1336_SMALL_JPG,
        $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}")]
    [TestCase(FileNames._1337_JPG, ImageRotation.Rotate0, ImageByteSizes._1337_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}")]
    [TestCase(FileNames._1349_JPG, ImageRotation.Rotate0, ImageByteSizes._1349_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1350_JPG, ImageRotation.Rotate0, ImageByteSizes._1350_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}")]
    [TestCase(FileNames._1413_JPG, ImageRotation.Rotate0, ImageByteSizes._1413_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1414_JPG, ImageRotation.Rotate0, ImageByteSizes._1414_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    [TestCase(FileNames._1415_JPG, ImageRotation.Rotate270, ImageByteSizes._1415_JPG,
        $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}")]
    public void LoadBitmapThumbnailImage_StoredThumbnailWithRotation_ReturnsValidBitmapImage(
        string fileName, ImageRotation rotation, int imageByteSize, string additionalPath)
    {
        string folderPath = string.IsNullOrEmpty(additionalPath)
            ? _assetsDirectory!
            : Path.Combine(_assetsDirectory!, additionalPath);

        string filePath = Path.Combine(folderPath, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        int thumbnailWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;
        int thumbnailHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        using IImageData thumbnailImage =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, thumbnailWidth, thumbnailHeight);

        Assert.That(thumbnailImage, Is.Not.Null);
        Assert.That(thumbnailImage.Width, Is.EqualTo(thumbnailWidth));
        Assert.That(thumbnailImage.Height, Is.EqualTo(thumbnailHeight));

        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());
        byte[] imageBuffer;

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            imageBuffer = _imageProcessingService!.GetPngBitmapImage(thumbnailImage);
        }
        else if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            imageBuffer = _imageProcessingService!.GetGifBitmapImage(thumbnailImage);
        }
        else
        {
            imageBuffer = _imageProcessingService!.GetJpegBitmapImage(thumbnailImage);
        }

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Has.Length.EqualTo(imageByteSize));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    // HEIC thumbnail byte sizes vary across machines because the Windows HEIF Image Extensions codec
    // (installed via Microsoft Store) differs between environments. Different codec versions produce
    // slightly different decoded pixel data, leading to different JPEG re-encoding sizes (~1% variance).
    // A 2% tolerance accommodates codec differences while still catching genuine encoding regressions.
    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, ImageRotation.Rotate0, ImageByteSizes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, ImageByteSizes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, ImageByteSizes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, ImageByteSizes.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapThumbnailImage_StoredHeicThumbnailWithRotation_ReturnsValidBitmapImage(
        string fileName, ImageRotation rotation, int expectedByteSize)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        int thumbnailWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;
        int thumbnailHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        int expectedThumbnailWidth =
            rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270 ? thumbnailWidth : 113;

        using IImageData thumbnailImage =
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, thumbnailWidth, thumbnailHeight);

        Assert.That(thumbnailImage, Is.Not.Null);
        Assert.That(thumbnailImage.Width, Is.EqualTo(expectedThumbnailWidth));
        Assert.That(thumbnailImage.Height, Is.EqualTo(thumbnailHeight));

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(thumbnailImage);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Has.Length.EqualTo(expectedByteSize).Within(2).Percent);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    private static bool IsValidImage(string filePath)
    {
        try
        {
            using (Image.FromFile(filePath))
            {
                // The image is successfully loaded; consider it valid
                return true;
            }
        }
        catch (Exception)
        {
            // An exception occurred while loading the image; consider it invalid
            return false;
        }
    }
}
