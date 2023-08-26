namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ExifHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var directoryName = Path.GetDirectoryName(typeof(ExifHelperTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [Test]
    [TestCase("Image 1.jpg", 1)]
    [TestCase("Image 1_90_deg.jpg", 6)]
    [TestCase("Image 1_180_deg.jpg", 3)]
    [TestCase("Image 1_270_deg.jpg", 8)]
    [TestCase("Image 8.jpeg", 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOriention)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.That(orientation, Is.EqualTo(expectedOriention));
    }

    [Test]
    [TestCase("Image 10 portrait.png", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Homer.gif", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Image_11.heic", AssetConstants.OrientationCorruptedImage)] // Error on BitmapFrame.Create(stream)
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsOrientationCorruptedImage(string fileName, int expectedOriention)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = ExifHelper.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.That(orientation, Is.EqualTo(expectedOriention));
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
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int degrees, int orientationExpected)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);
        //byte[] bufferRotated = GetHeicRotatedBuffer(buffer, degrees);

        ushort orientation = ExifHelper.GetHeicExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.That(orientation, Is.EqualTo(orientationExpected));
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

        Assert.Throws<ArgumentNullException>(() => ExifHelper.GetHeicExifOrientation(nullBuffer!));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = Array.Empty<byte>();

        Assert.Throws<ArgumentException>(() => ExifHelper.GetHeicExifOrientation(emptyBuffer));
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
        var exifOrientation = -10;
        Rotation rotation = ExifHelper.GetImageRotation((ushort)exifOrientation);

        Assert.AreEqual(Rotation.Rotate0, rotation);
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        Assert.Throws<InvalidOperationException>(() => ExifHelper.GetImageRotation((ushort)exifOrientation!));
    }

    [Test]
    [TestCase("Image 1.jpg")]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 10 portrait.png")]
    [TestCase("Homer.gif")]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(validImageData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = ExifHelper.IsValidGDIPlusImage(invalidImageData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = Array.Empty<byte>();

        var result = ExifHelper.IsValidGDIPlusImage(emptyHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
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

        Assert.Throws<ArgumentException>(() => ExifHelper.IsValidHeic(emptyHeicData));
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

    //            //var readSettings = new MagickReadSettings() { Format = MagickFormat.Heic };
    //            //newStream.Seek(0, SeekOrigin.Begin); //THIS IS NEEDED!!!
    //            //var newImage = new MagickImage(newStream, readSettings);
    //            //newImage.Rotate(degrees);
    //            byte[] rotatedImageData = image.ToByteArray(MagickFormat.Heic);

    //            // Return the rotated image data
    //            return rotatedImageData;
    //        }
    //    }
    //}
}
