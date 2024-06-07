namespace PhotoManager.Tests.Integration.Common;

[TestFixture]
public class VideoHelperTests
{
    private string? _dataDirectory;

    private const string FFMPEG_PATH = "E:\\ffmpeg\\bin\\ffmpeg.exe"; // TODO: Will be removed when the dll of Ffmpeg would have been generated and stored in the project

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    public void GetFirstFramePath_ExistingFile_ReturnsPathAndExtractsFirstFrame()
    {
        const string fileName = "Homer.mp4";
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");
        
        string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

        // Set up a StringWriter to capture console output
        StringWriter stringWriter = new();
        Console.SetOut(stringWriter);

        try
        {
            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath, FFMPEG_PATH);

            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsTrue(consoleOutput.Contains($"First frame extracted successfully for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsTrue(consoleOutput.Contains($"First frame saved at: {firstFrameVideoPath}"));
            Assert.IsFalse(consoleOutput.Contains($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsFalse(consoleOutput.Contains("Error: FFmpeg failed to generate the first frame."));

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.AreEqual(expectedFirstFrameVideoPath, firstFrameVideoPath);

            Assert.IsTrue(File.Exists(expectedFirstFrameVideoPath));
        }
        finally
        {
            File.Delete(expectedFirstFrameVideoPath);
            Directory.Delete(destinationPath);
        }
    }

    [Test]
    public void GetFirstFramePath_ExistingFileButLessThanOneSecond_ReturnsNullAndDoesNotExtractFirstFrame()
    {
        const string fileName = "Homer1s.mp4"; // Video that has less than 1 second
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        // Set up a StringWriter to capture console output
        StringWriter stringWriter = new();
        Console.SetOut(stringWriter);

        try
        {
            string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath, FFMPEG_PATH);

            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsFalse(consoleOutput.Contains($"First frame extracted successfully for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsFalse(consoleOutput.Contains($"First frame saved at: {firstFrameVideoPath}"));
            Assert.IsTrue(consoleOutput.Contains($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsTrue(consoleOutput.Contains("Error: FFmpeg failed to generate the first frame."));

            Assert.IsTrue(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsFalse(File.Exists(expectedFirstFrameVideoPath));
        }
        finally
        {
            Directory.Delete(destinationPath);
        }
    }

    [Test]
    public void GetFirstFramePath_NonExistingFile_ReturnsNullAndDoesNotExtractFirstFrame()
    {
        const string fileName = "toto.mp4";
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        // Set up a StringWriter to capture console output
        StringWriter stringWriter = new();
        Console.SetOut(stringWriter);

        try
        {
            string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath, FFMPEG_PATH);

            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsFalse(consoleOutput.Contains($"First frame extracted successfully for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsFalse(consoleOutput.Contains($"First frame saved at: {firstFrameVideoPath}"));
            Assert.IsTrue(consoleOutput.Contains($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, fileName)}"));
            Assert.IsTrue(consoleOutput.Contains("Error: FFmpeg failed to generate the first frame."));

            Assert.IsTrue(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsFalse(File.Exists(expectedFirstFrameVideoPath));
        }
        finally
        {
            Directory.Delete(destinationPath);
        }
    }
}
