namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class VideoHelperTests
{
    [Test]
    [TestCase(".3g2", true)]
    [TestCase(".3G2", true)]
    [TestCase(".3gp", true)]
    [TestCase(".3GP", true)]
    [TestCase(".asf", true)]
    [TestCase(".ASF", true)]
    [TestCase(".av1", true)]
    [TestCase(".AV1", true)]
    [TestCase(".avi", true)]
    [TestCase(".AVI", true)]
    [TestCase(".flv", true)]
    [TestCase(".FLV", true)]
    [TestCase(".m4v", true)]
    [TestCase(".M4V", true)]
    [TestCase(".mkv", true)]
    [TestCase(".MKV", true)]
    [TestCase(".mov", true)]
    [TestCase(".MOV", true)]
    [TestCase(".MoV", true)]
    [TestCase(".mOv", true)]
    [TestCase(".mp4", true)]
    [TestCase(".MP4", true)]
    [TestCase(".mpeg", true)]
    [TestCase(".MPEG", true)]
    [TestCase(".mpg", true)]
    [TestCase(".MPG", true)]
    [TestCase(".ogv", true)]
    [TestCase(".OGV", true)]
    [TestCase(".webm", true)]
    [TestCase(".WEBM", true)]
    [TestCase(".wmv", true)]
    [TestCase(".WMV", true)]
    [TestCase("mov", false)]
    [TestCase(".toto", false)]
    [TestCase(".", false)]
    public void Should_Detect_When_AssetIsVideo(string fileExtension, bool expected)
    {
        bool result = VideoHelper.IsVideoFile(fileExtension);
        Assert.That(result, Is.EqualTo(expected));
    }
}
