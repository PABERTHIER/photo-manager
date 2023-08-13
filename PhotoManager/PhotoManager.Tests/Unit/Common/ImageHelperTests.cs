using NUnit.Framework;
using PhotoManager.Common;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class ImageHelperTests
{
    [Test]
    [TestCase(".jpg", true)]
    [TestCase(".JPG", true)]
    [TestCase(".jpeg", true)]
    [TestCase(".JPEG", true)]
    [TestCase(".jfif", true)]
    [TestCase(".JFIF", true)]
    [TestCase(".png", true)]
    [TestCase(".PNG", true)]
    [TestCase(".PnG", true)]
    [TestCase(".gif", true)]
    [TestCase(".GIF", true)]
    [TestCase(".heic", true)]
    [TestCase(".HEIC", true)]
    [TestCase(".dng", true)]
    [TestCase(".DNG", true)]
    [TestCase(".bmp", true)]
    [TestCase(".BMP", true)]
    [TestCase(".tiff", true)]
    [TestCase(".TIFF", true)]
    [TestCase(".tif", true)]
    [TestCase(".TIF", true)]
    [TestCase("png", false)]
    [TestCase(".toto", false)]
    [TestCase(".", false)]
    public void Should_Detect_When_AssetIsImage(string fileExtension, bool expected)
    {
        var result = ImageHelper.IsImageFile(fileExtension);
        Assert.That(result, Is.EqualTo(expected));
    }
}
