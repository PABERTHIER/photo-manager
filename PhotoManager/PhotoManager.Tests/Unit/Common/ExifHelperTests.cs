using System.Drawing.Imaging;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ExifHelperTests
{
    private string? _dataDirectory;

    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, 1)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, 6)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, 3)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, 8)]
    [TestCase(FileNames.IMAGE_8_JPEG, 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase(FileNames.HOMER_GIF)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase(FileNames.IMAGE_11_HEIC)] // Error on BitmapFrame.Create(stream)
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsCorruptedImageOrientation(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = ExifHelper.GetExifOrientation(
            invalidImageBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = ExifHelper.GetExifOrientation(
            nullBuffer!,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = [];

        ushort orientation = ExifHelper.GetExifOrientation(
            emptyBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidFormat_ReturnsCorruptedOrientationValue()
    {
        Bitmap image = new (10, 10);

        using (MemoryStream ms = new())
        {
            image.Save(ms, ImageFormat.Bmp); // Save as BMP to create an invalid format for JPEG
            byte[] buffer = ms.ToArray(); // Buffer with invalid Exif Metadata (Metadata null)

            ushort orientation = ExifHelper.GetExifOrientation(
                buffer,
                _userConfigurationService!.AssetSettings.DefaultExifOrientation,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

            Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, 1)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, 6)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, 3)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, 8)]
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetHeicExifOrientation(buffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = ExifHelper.GetHeicExifOrientation(invalidHeicBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            ExifHelper.GetHeicExifOrientation(nullBuffer!, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            ExifHelper.GetHeicExifOrientation(emptyBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [TestCase((ushort)0, Rotation.Rotate0)]
    [TestCase((ushort)1, Rotation.Rotate0)]
    [TestCase((ushort)2, Rotation.Rotate0)]
    [TestCase((ushort)3, Rotation.Rotate180)]
    [TestCase((ushort)4, Rotation.Rotate180)]
    [TestCase((ushort)5, Rotation.Rotate90)]
    [TestCase((ushort)6, Rotation.Rotate90)]
    [TestCase((ushort)7, Rotation.Rotate270)]
    [TestCase((ushort)8, Rotation.Rotate270)]
    [TestCase((ushort)9, Rotation.Rotate0)]
    [TestCase((ushort)10, Rotation.Rotate0)]
    [TestCase((ushort)10000, Rotation.Rotate0)]
    [TestCase(ushort.MinValue, Rotation.Rotate0)]
    [TestCase(ushort.MaxValue, Rotation.Rotate0)]
    public void GetImageRotation_ValidExifOrientation_ReturnsCorrectRotationValue(ushort exifOrientation, Rotation expectedRotation)
    {
        Rotation rotation = ExifHelper.GetImageRotation(exifOrientation);

        Assert.That(rotation, Is.EqualTo(expectedRotation));
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        Rotation rotation = ExifHelper.GetImageRotation((ushort)(short)exifOrientation);

        Assert.That(rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => ExifHelper.GetImageRotation((ushort)exifOrientation!));

        Assert.That(exception?.Message, Is.EqualTo("Nullable object must have a value."));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(validImageData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(invalidImageData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = ExifHelper.IsValidGDIPlusImage(emptyHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidHeic(validHeicData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = ExifHelper.IsValidHeic(invalidHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => ExifHelper.IsValidHeic(emptyHeicData));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }
}
