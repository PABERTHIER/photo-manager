namespace PhotoManager.Tests.Integration.Common;

[TestFixture]
public class VideoHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    public void GetFirstFramePath_IntegrationTest()
    {
        string fileName = "Homer.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");
        
        string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

        try
        {
            string firstFrameVideoPath = VideoHelper.GetFirstFramePath(dataDirectory!, fileName, destinationPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.AreEqual(expectedFirstFrameVideoPath, firstFrameVideoPath);

            // Verify that the first frame file is created in the output directory
            Assert.IsTrue(File.Exists(expectedFirstFrameVideoPath));
        }
        finally
        {
            // Clean up: delete the first frame file
            File.Delete(expectedFirstFrameVideoPath);
            Directory.Delete(destinationPath);
        }
    }

    [Test]
    public void NotGetFirstFramePath_IntegrationTest()
    {
        string fileName = "Homer1s.mp4"; // Video that has less than 1 second
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        try
        {
            string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

            string firstFrameVideoPath = VideoHelper.GetFirstFramePath(dataDirectory!, fileName, destinationPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.AreEqual(expectedFirstFrameVideoPath, firstFrameVideoPath);

            // Verify that the first frame file is created in the output directory
            Assert.IsFalse(File.Exists(expectedFirstFrameVideoPath));
        }
        finally
        {
            Directory.Delete(destinationPath);
        }
    }
}
