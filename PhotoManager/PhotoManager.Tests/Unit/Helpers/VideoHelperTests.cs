using Moq;
using NUnit.Framework;
using PhotoManager.Common;
using PhotoManager.Constants;
using System.IO;
using System.IO.Abstractions;

namespace PhotoManager.Tests.Unit.Helpers;

[TestFixture]
public class VideoHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var directoryName = Path.GetDirectoryName(typeof(ApplicationTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [Test]
    [TestCase(".mov", true)]
    [TestCase(".MOV", true)]
    [TestCase(".MoV", true)]
    [TestCase(".mOv", true)]
    [TestCase(".avi", true)]
    [TestCase(".AVI", true)]
    [TestCase(".av1", true)]
    [TestCase(".AV1", true)]
    [TestCase(".mp4", true)]
    [TestCase(".MP4", true)]
    [TestCase(".3g2", true)]
    [TestCase(".3G2", true)]
    [TestCase(".3gp", true)]
    [TestCase(".3GP", true)]
    [TestCase(".asf", true)]
    [TestCase(".ASF", true)]
    [TestCase(".flv", true)]
    [TestCase(".FLV", true)]
    [TestCase(".m4v", true)]
    [TestCase(".M4V", true)]
    [TestCase(".mkv", true)]
    [TestCase(".MKV", true)]
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
        var result = VideoHelper.IsVideoFile(fileExtension);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetFirstFrame_ExistingFile_ExtractsFirstFrame()
    {
        var fileName = "Homer.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        var expectedFileName = "Homer.jpg";
        string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

        // Create a mock of IProcessExecutor
        var mockProcessExecutor = new Mock<IProcessExecutor>();

        // Set the static ProcessExecutor to the mock instance
        VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

        // Create a virtual file system using System.IO.Abstractions
        var fileSystem = new FileSystem();
        fileSystem.Directory.CreateDirectory(destinationPath);
        fileSystem.File.WriteAllText(Path.Combine(destinationPath, expectedFileName), "fake video content");

        // Set up a StringWriter to capture console output
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            var firstFrameVideoPath = VideoHelper.GetFirstFrame(dataDirectory!, fileName, destinationPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
            Assert.IsTrue(fileSystem.File.Exists(firstFrameVideoPath));
            // Get the captured console output
            var consoleOutput = stringWriter.ToString();

            // Assert that the expected error message was printed to the console
            Assert.IsTrue(consoleOutput.Contains("Frist frame extracted successfully for:"));

            // Verify that ExecuteFFmpegCommand was called with the correct arguments
            string ffmpegPath = PathConstants.FfmpegPath;
            string expectedArguments = $"-i \"{Path.Combine(dataDirectory!, fileName)}\" -ss 00:00:01 -vframes 1 \"{firstFrameVideoPath}\"";
            mockProcessExecutor.Verify(x => x.ExecuteFFmpegCommand(ffmpegPath, expectedArguments), Times.Once);
        }
        finally
        {
            // Clean up: delete the first frame file
            File.Delete(expectedFirstFrameVideoPath);
        }
    }

    [Test]
    public void GetFirstFrame_NonExistingFile_ReturnsPathButNoFileCreated()
    {
        var fileName = "non_existing_video.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        // Create a mock of IProcessExecutor
        var mockProcessExecutor = new Mock<IProcessExecutor>();

        // Set the static ProcessExecutor to the mock instance
        VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

        // Set up a StringWriter to capture console output
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var firstFrameVideoPath = VideoHelper.GetFirstFrame(dataDirectory!, fileName, destinationPath);

        Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
        Assert.IsFalse(File.Exists(firstFrameVideoPath));
        // Get the captured console output
        var consoleOutput = stringWriter.ToString();

        // Assert that the expected error message was printed to the console
        Assert.IsTrue(consoleOutput.Contains("Frist frame extracted successfully for:"));
    }

    [Test]
    public void GetFirstFrame_ExecuteFFmpegCommandThrowsException_PrintsErrorMessage()
    {
        var fileName = "Homer.mp4";
        string destinationPath = Path.Combine(dataDirectory!, "OutputVideoFirstFrame");

        // Create a mock of IProcessExecutor that throws an exception when ExecuteFFmpegCommand is called
        var mockProcessExecutor = new Mock<IProcessExecutor>();
        mockProcessExecutor
            .Setup(x => x.ExecuteFFmpegCommand(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Error executing FFmpeg command"));

        // Replace the actual ProcessExecutor with the mock
        VideoHelper.ProcessExecutor = mockProcessExecutor.Object;

        // Set up a StringWriter to capture console output
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        var firstFrameVideoPath = VideoHelper.GetFirstFrame(dataDirectory!, fileName, destinationPath);

        Assert.IsFalse(string.IsNullOrEmpty(firstFrameVideoPath));
        Assert.IsFalse(File.Exists(firstFrameVideoPath));

        // Get the captured console output
        var consoleOutput = stringWriter.ToString();

        // Assert that the expected error message was printed to the console
        Assert.IsTrue(consoleOutput.Contains("Failed to extract the first frame for:"));
    }
}
