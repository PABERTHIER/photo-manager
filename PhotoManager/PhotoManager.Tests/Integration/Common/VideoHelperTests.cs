﻿namespace PhotoManager.Tests.Integration.Common;

[TestFixture]
public class VideoHelperTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    public void GetFirstFramePath_ExistingFile_ReturnsPathAndLogsItAndExtractsFirstFrame()
    {
        const string fileName = "Homer.mp4";
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");
        
        string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.False);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath);

            string[] messages = [
                $"First frame extracted successfully for: {Path.Combine(_dataDirectory!, fileName)}",
                $"First frame saved at: {firstFrameVideoPath}"
            ];
            Type typeOfService = typeof(VideoHelper);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);

            Assert.That(string.IsNullOrEmpty(firstFrameVideoPath), Is.False);
            Assert.That(firstFrameVideoPath, Is.EqualTo(expectedFirstFrameVideoPath));

            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.True);
        }
        finally
        {
            File.Delete(expectedFirstFrameVideoPath);
            Directory.Delete(destinationPath);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetFirstFramePath_ExistingFileButButAnotherImageAlreadyExistsInTheOutputDirectory_ReturnsNullAndLogsItAndDoesNotExtractFirstFrame()
    {
        const string videoFileName = "Homer.mp4";
        const string expectedFirstFrameVideoName = "Homer.jpg";

        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationPath);

            string sourceImagePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);
            File.Copy(sourceImagePath, expectedFirstFrameVideoPath);
            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.True);

            string videoPath = Path.Combine(_dataDirectory!, videoFileName);
            Assert.That(File.Exists(videoPath), Is.True);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, videoFileName, destinationPath);

            Exception exception = new ($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, videoFileName)}, Message: Output file already exists and overwrite is disabled");
            Exception[] expectedExceptions = [exception];
            Type typeOfService = typeof(VideoHelper);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            Assert.That(string.IsNullOrEmpty(firstFrameVideoPath), Is.True);
            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetFirstFramePath_ExistingFileButLessThanOneSecond_ReturnsNullAndLogsItAndDoesNotExtractFirstFrame()
    {
        const string fileName = "Homer1s.mp4"; // Video that has less than 1 second
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath);

            Exception exception = new ($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, fileName)}, Message: FFmpeg failed to generate the first frame file due to its format or content.");
            Exception[] expectedExceptions = [exception];
            Type typeOfService = typeof(VideoHelper);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            Assert.That(string.IsNullOrEmpty(firstFrameVideoPath), Is.True);
            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.False);
        }
        finally
        {
            Directory.Delete(destinationPath);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetFirstFramePath_NonExistingFile_ReturnsNullAndLogsItAndDoesNotExtractFirstFrame()
    {
        const string fileName = "toto.mp4";
        string destinationPath = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string expectedFirstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            string expectedFirstFrameVideoPath = Path.Combine(destinationPath, expectedFirstFrameVideoName);

            string? firstFrameVideoPath = VideoHelper.GetFirstFramePath(_dataDirectory!, fileName, destinationPath);

            Exception exception = new ($"Failed to extract the first frame for: {Path.Combine(_dataDirectory!, fileName)}, Message: Input file not found");
            Exception[] expectedExceptions = [exception];
            Type typeOfService = typeof(VideoHelper);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            Assert.That(string.IsNullOrEmpty(firstFrameVideoPath), Is.True);
            Assert.That(File.Exists(expectedFirstFrameVideoPath), Is.False);
        }
        finally
        {
            Directory.Delete(destinationPath);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }
}
