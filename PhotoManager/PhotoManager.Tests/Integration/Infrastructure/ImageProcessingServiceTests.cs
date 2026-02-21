using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class ImageProcessingServiceTests
{
    private string? _dataDirectory;

    private ImageProcessingService? _imageProcessingService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _imageProcessingService = new();
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBitmapImage_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _imageProcessingService!.GetJpegBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.GetJpegBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _imageProcessingService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBitmapImage_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _imageProcessingService!.GetPngBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetPngBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _imageProcessingService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBitmapImage_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _imageProcessingService!.GetGifBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetGifBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidGDIPlusImage(validImageData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = _imageProcessingService!.IsValidGDIPlusImage(emptyHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidHeic(validHeicData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = _imageProcessingService!.IsValidHeic(invalidHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _imageProcessingService!.IsValidHeic(emptyHeicData));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
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
