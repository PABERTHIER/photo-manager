using System.IO.Abstractions;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class VideoHelperTests
{
    private string? dataDirectory;

    private IUserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new UserConfigurationService(configurationRootMock.Object);
    }

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
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetFirstFramePath_ExistingFile_ExtractsFirstFrame()
    {
        string fileName = "Homer.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        string expectedFileName = "Homer.jpg";
        string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

        // Create a mock of IProcessExecutor
        Mock<IProcessExecutor> mockProcessExecutor = new();

        // Set the static ProcessExecutor to the mock instance
        VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

        // Create a virtual file system using System.IO.Abstractions
        FileSystem fileSystem = new();
        fileSystem.Directory.CreateDirectory(destinationPath);
        fileSystem.File.WriteAllText(Path.Combine(destinationPath, expectedFileName), "fake video content");

        // Set up a StringWriter to capture console output
        StringWriter stringWriter = new();
        Console.SetOut(stringWriter);

        try
        {
            string firstFrameVideoPath = VideoHelper.GetFirstFramePath(dataDirectory!, fileName, destinationPath, _userConfigurationService!.PathSettings.FfmpegPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsTrue(fileSystem.File.Exists(firstFrameVideoPath));
            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsTrue(consoleOutput.Contains("First frame extracted successfully for:"));

            // Verify that ExecuteFFmpegCommand was called with the correct arguments
            string expectedArguments = $"-i \"{Path.Combine(dataDirectory!, fileName)}\" -ss 00:00:01 -vframes 1 \"{firstFrameVideoPath}\"";
            mockProcessExecutor.Verify(x => x.ExecuteFFmpegCommand(_userConfigurationService!.PathSettings.FfmpegPath, expectedArguments), Times.Once);
        }
        finally
        {
            // Clean up: delete the first frame file
            File.Delete(expectedFirstFrameVideoPath);
            Directory.Delete(destinationPath);
        }
    }

    [Test]
    public void GetFirstFramePath_NonExistingFile_ReturnsPathButNoFileCreated()
    {
        string fileName = "non_existing_video.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        try
        {
            // Create a mock of IProcessExecutor
            Mock<IProcessExecutor> mockProcessExecutor = new();

            // Set the static ProcessExecutor to the mock instance
            VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

            // Set up a StringWriter to capture console output
            StringWriter stringWriter = new();
            Console.SetOut(stringWriter);

            string firstFrameVideoPath = VideoHelper.GetFirstFramePath(dataDirectory!, fileName, destinationPath, _userConfigurationService!.PathSettings.FfmpegPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsFalse(File.Exists(firstFrameVideoPath));
            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsTrue(consoleOutput.Contains("First frame extracted successfully for:"));
        }
        finally
        {
            // Clean up: delete the first frame file
            Directory.Delete(destinationPath);
        }
    }

    [Test]
    public void GetFirstFramePath_ExecuteFFmpegCommandThrowsException_PrintsErrorMessage()
    {
        string fileName = "Homer.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        try
        {
            // Create a mock of IProcessExecutor that throws an exception when ExecuteFFmpegCommand is called
            Mock<IProcessExecutor> mockProcessExecutor = new();
            mockProcessExecutor
                .Setup(x => x.ExecuteFFmpegCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Error executing FFmpeg command"));

            // Replace the actual ProcessExecutor with the mock
            VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

            // Set up a StringWriter to capture console output
            StringWriter stringWriter = new();
            Console.SetOut(stringWriter);

            string firstFrameVideoPath = VideoHelper.GetFirstFramePath(dataDirectory!, fileName, destinationPath, _userConfigurationService!.PathSettings.FfmpegPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsFalse(File.Exists(firstFrameVideoPath));

            // Get the captured console output
            string consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsTrue(consoleOutput.Contains("Failed to extract the first frame for:"));
        }
        finally
        {
            // Clean up: delete the first frame file
            Directory.Delete(destinationPath);
        }
    }
}
