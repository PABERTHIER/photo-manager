using SkiaSharp;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ExifHelperTests
{
    private string? _assetsDirectory;

    private UserConfigurationService? _userConfigurationService;

    private TestLogger<ExifHelperTests>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, 1)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, 6)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, 3)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, 8)]
    [TestCase(FileNames.IMAGE_8_JPEG, 1)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF)] // GIF has no EXIF orientation data, returns defaultExifOrientation
    public void GetExifOrientation_FormatWithoutExifData_ReturnsDefaultOrientation(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.DefaultExifOrientation));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC)]
    public void GetExifOrientation_UnsupportedFormat_ReturnsCorruptedOrientationValue(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));

        _testLogger!.AssertLogExceptions([new Exception("The image is corrupted")], typeof(ExifHelperTests));
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = ExifHelper.GetExifOrientation(
            invalidImageBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));

        _testLogger!.AssertLogExceptions([new Exception("The image is corrupted")], typeof(ExifHelperTests));
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = ExifHelper.GetExifOrientation(
            nullBuffer!,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));

        _testLogger!.AssertLogExceptions(
            [new Exception("Value cannot be null. (Parameter 'buffer')")],
            typeof(ExifHelperTests));
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = [];

        ushort orientation = ExifHelper.GetExifOrientation(
            emptyBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));

        _testLogger!.AssertLogExceptions([new Exception("The image is corrupted")], typeof(ExifHelperTests));
    }

    [Test]
    public void GetExifOrientation_FormatWithoutExifMetadata_ReturnsDefaultOrientationValue()
    {
        using (SKBitmap bitmap = new(10, 10))
        {
            using (SKImage image = SKImage.FromBitmap(bitmap))
            {
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    byte[] buffer = data.ToArray();

                    ushort orientation = ExifHelper.GetExifOrientation(
                        buffer,
                        _userConfigurationService!.AssetSettings.DefaultExifOrientation,
                        _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

                    Assert.That(orientation,
                        Is.EqualTo(_userConfigurationService!.AssetSettings.DefaultExifOrientation));

                    _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
                }
            }
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, 1)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, 6)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, 3)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, 8)]
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName,
        int expectedOrientation)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetHeicExifOrientation(buffer,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = ExifHelper.GetHeicExifOrientation(invalidHeicBuffer,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));

        _testLogger!.AssertLogExceptions(
            [new Exception("The image is not valid or in an unsupported format")],
            typeof(ExifHelperTests));
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            ExifHelper.GetHeicExifOrientation(nullBuffer!,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            ExifHelper.GetHeicExifOrientation(emptyBuffer,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation, _testLogger!);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    [TestCase((ushort)0, ImageRotation.Rotate0)]
    [TestCase((ushort)1, ImageRotation.Rotate0)]
    [TestCase((ushort)2, ImageRotation.Rotate0)]
    [TestCase((ushort)3, ImageRotation.Rotate180)]
    [TestCase((ushort)4, ImageRotation.Rotate180)]
    [TestCase((ushort)5, ImageRotation.Rotate90)]
    [TestCase((ushort)6, ImageRotation.Rotate90)]
    [TestCase((ushort)7, ImageRotation.Rotate270)]
    [TestCase((ushort)8, ImageRotation.Rotate270)]
    [TestCase((ushort)9, ImageRotation.Rotate0)]
    [TestCase((ushort)10, ImageRotation.Rotate0)]
    [TestCase((ushort)10000, ImageRotation.Rotate0)]
    [TestCase(ushort.MinValue, ImageRotation.Rotate0)]
    [TestCase(ushort.MaxValue, ImageRotation.Rotate0)]
    public void GetImageRotation_ValidExifOrientation_ReturnsCorrectRotationValue(ushort exifOrientation,
        ImageRotation expectedRotation)
    {
        ImageRotation rotation = ExifHelper.GetImageRotation(exifOrientation);

        Assert.That(rotation, Is.EqualTo(expectedRotation));
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        ImageRotation rotation = ExifHelper.GetImageRotation((ushort)(short)exifOrientation);

        Assert.That(rotation, Is.EqualTo(ImageRotation.Rotate0));
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => ExifHelper.GetImageRotation((ushort)exifOrientation!));

        Assert.That(exception?.Message, Is.EqualTo("Nullable object must have a value."));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    public void IsValidImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidImage(validImageData, _testLogger!);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    public void IsValidImage_UnsupportedFormat_ReturnsFalse(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidImage(validImageData, _testLogger!);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidImage_NullBuffer_ReturnsFalse()
    {
        bool result = ExifHelper.IsValidImage(null!, _testLogger!);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([new Exception("Value cannot be null. (Parameter 'buffer')")],
            typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidImage_InvalidBuffer_ReturnsFalse()
    {
        byte[] buffer = "not an image"u8.ToArray();

        bool result = ExifHelper.IsValidImage(buffer, _testLogger!);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidImage_EmptyBuffer_ReturnsFalse()
    {
        byte[] emptyBuffer = [];

        bool result = ExifHelper.IsValidImage(emptyBuffer, _testLogger!);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidHeic(validHeicData, _testLogger!);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = ExifHelper.IsValidHeic(invalidHeicData, _testLogger!);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions(
            [new Exception("The image is not valid or in an unsupported format")],
            typeof(ExifHelperTests));
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            ExifHelper.IsValidHeic(emptyHeicData, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ExifHelperTests));
    }
}
