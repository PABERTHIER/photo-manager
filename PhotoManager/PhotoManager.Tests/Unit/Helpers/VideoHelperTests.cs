using FluentAssertions;
using PhotoManager.Common;
using Xunit;

namespace PhotoManager.Tests.Unit.Helpers;

public class VideoHelperTests
{
    [Theory]
    [InlineData(".mov", true)]
    [InlineData(".MOV", true)]
    [InlineData(".MoV", true)]
    [InlineData(".mOv", true)]
    [InlineData(".avi", true)]
    [InlineData(".AVI", true)]
    [InlineData(".av1", true)]
    [InlineData(".AV1", true)]
    [InlineData(".mp4", true)]
    [InlineData(".MP4", true)]
    [InlineData(".3g2", true)]
    [InlineData(".3G2", true)]
    [InlineData(".3gp", true)]
    [InlineData(".3GP", true)]
    [InlineData(".asf", true)]
    [InlineData(".ASF", true)]
    [InlineData(".flv", true)]
    [InlineData(".FLV", true)]
    [InlineData(".m4v", true)]
    [InlineData(".M4V", true)]
    [InlineData(".mkv", true)]
    [InlineData(".MKV", true)]
    [InlineData(".mpeg", true)]
    [InlineData(".MPEG", true)]
    [InlineData(".mpg", true)]
    [InlineData(".MPG", true)]
    [InlineData(".ogv", true)]
    [InlineData(".OGV", true)]
    [InlineData(".webm", true)]
    [InlineData(".WEBM", true)]
    [InlineData(".wmv", true)]
    [InlineData(".WMV", true)]
    [InlineData("mov", false)]
    [InlineData(".toto", false)]
    [InlineData(".", false)]
    public void Should_Detect_When_AssetIsVideo(string fileExtension, bool expected)
    {
        var result = VideoHelper.IsVideoFile(fileExtension);
        result.Should().Be(expected);
    }
}
