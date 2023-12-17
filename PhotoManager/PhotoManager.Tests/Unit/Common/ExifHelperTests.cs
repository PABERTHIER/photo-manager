namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ExifHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    [TestCase("Image 1.jpg", 1)]
    [TestCase("Image 1_90_deg.jpg", 6)]
    [TestCase("Image 1_180_deg.jpg", 3)]
    [TestCase("Image 1_270_deg.jpg", 8)]
    [TestCase("Image 8.jpeg", 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    [TestCase("Image 10 portrait.png", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Homer.gif", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Image_11.heic", AssetConstants.OrientationCorruptedImage)] // Error on BitmapFrame.Create(stream)
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsOrientationCorruptedImage(string fileName, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        ushort orientation = ExifHelper.GetExifOrientation(invalidImageBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = ExifHelper.GetExifOrientation(nullBuffer!);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = Array.Empty<byte>();

        ushort orientation = ExifHelper.GetExifOrientation(emptyBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    [TestCase("Image_11.heic", 0, 1)]
    //[TestCase("Image_11_90.heic", 90, 8)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    //[TestCase("Image_11_180.heic", 180, 3)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    //[TestCase("Image_11_270.heic", 270, 6)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int degrees, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);
        //byte[] bufferRotated = GetHeicRotatedBuffer(buffer, degrees);

        ushort orientation = ExifHelper.GetHeicExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        ushort orientation = ExifHelper.GetHeicExifOrientation(invalidHeicBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => ExifHelper.GetHeicExifOrientation(nullBuffer!));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = Array.Empty<byte>();

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => ExifHelper.GetHeicExifOrientation(emptyBuffer));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
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

        Assert.AreEqual(expectedRotation, rotation);
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        Rotation rotation = ExifHelper.GetImageRotation((ushort)exifOrientation);

        Assert.AreEqual(Rotation.Rotate0, rotation);
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => ExifHelper.GetImageRotation((ushort)exifOrientation!));

        Assert.AreEqual("Nullable object must have a value.", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg")]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 10 portrait.png")]
    [TestCase("Homer.gif")]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(validImageData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(invalidImageData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = Array.Empty<byte>();

        bool result = ExifHelper.IsValidGDIPlusImage(emptyHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidHeic(validHeicData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        bool result = ExifHelper.IsValidHeic(invalidHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = Array.Empty<byte>();

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => ExifHelper.IsValidHeic(emptyHeicData));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    //private static byte[] GetHeicRotatedBuffer(byte[] buffer, int degrees)
    //{
    //    using (MemoryStream stream = new(buffer))
    //    {
    //        using (MagickImage image = new(stream))
    //        {
    //            image.Rotate(degrees);

    //            // Convert the rotated image back to a byte array
    //            //byte[] rotatedImageData = image.ToByteArray(MagickFormat.Heic);
    //            //
    //            //MemoryStream newStream = stream;
    //            //stream.CopyTo(newStream);

    //            //MagickReadSettings readSettings = new() { Format = MagickFormat.Heic };
    //            //newStream.Seek(0, SeekOrigin.Begin); //THIS IS NEEDED!!!
    //            //MagickImage newImage = new (newStream, readSettings);
    //            //newImage.Rotate(degrees);
    //            byte[] rotatedImageData = image.ToByteArray(MagickFormat.Heic);

    //            // Return the rotated image data
    //            return rotatedImageData;
    //        }
    //    }
    //}
}
