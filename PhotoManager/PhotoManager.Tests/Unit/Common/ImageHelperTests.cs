namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ImageHelperTests
{
    [Test]
    [TestCase(".bmp", true)]
    [TestCase(".BMP", true)]
    [TestCase(".dng", true)]
    [TestCase(".DNG", true)]
    [TestCase(".gif", true)]
    [TestCase(".GIF", true)]
    [TestCase(".heic", true)]
    [TestCase(".HEIC", true)]
    [TestCase(".ico", true)]
    [TestCase(".ICO", true)]
    [TestCase(".jfif", true)]
    [TestCase(".JFIF", true)]
    [TestCase(".jpeg", true)]
    [TestCase(".JPEG", true)]
    [TestCase(".jpg", true)]
    [TestCase(".JPG", true)]
    [TestCase(".png", true)]
    [TestCase(".PNG", true)]
    [TestCase(".PnG", true)]
    [TestCase(".tiff", true)]
    [TestCase(".TIFF", true)]
    [TestCase(".tif", true)]
    [TestCase(".TIF", true)]
    [TestCase(".webp", true)]
    [TestCase(".WEBP", true)]
    [TestCase("png", false)]
    [TestCase(".toto", false)]
    [TestCase(".", false)]
    public void Should_Detect_When_AssetIsImage(string fileExtension, bool expected)
    {
        bool result = PhotoManager.Common.ImageHelper.IsImageFile(fileExtension);
        Assert.That(result, Is.EqualTo(expected));
    }
}
