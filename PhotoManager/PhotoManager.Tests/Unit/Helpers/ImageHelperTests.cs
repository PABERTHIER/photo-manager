using FluentAssertions;
using PhotoManager.Common;
using Xunit;

namespace PhotoManager.Tests.Unit.Helpers;

public class ImageHelperTests
{
    [Theory]
    [InlineData(".jpg", true)]
    [InlineData(".JPG", true)]
    [InlineData(".jpeg", true)]
    [InlineData(".JPEG", true)]
    [InlineData(".jfif", true)]
    [InlineData(".JFIF", true)]
    [InlineData(".png", true)]
    [InlineData(".PNG", true)]
    [InlineData(".PnG", true)]
    [InlineData(".gif", true)]
    [InlineData(".GIF", true)]
    [InlineData(".heic", true)]
    [InlineData(".HEIC", true)]
    [InlineData(".dng", true)]
    [InlineData(".DNG", true)]
    [InlineData(".bmp", true)]
    [InlineData(".BMP", true)]
    [InlineData(".tiff", true)]
    [InlineData(".TIFF", true)]
    [InlineData(".tif", true)]
    [InlineData(".TIF", true)]
    [InlineData("png", false)]
    [InlineData(".toto", false)]
    [InlineData(".", false)]
    public void Should_Detect_When_AssetIsImage(string fileExtension, bool expected)
    {
        var result = ImageHelper.IsImageFile(fileExtension);
        result.Should().Be(expected);
    }
}
