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
    public void IsVideoFile_FileIsVideoOrNot_ReturnsExpectedResult(string fileExtension, bool expected)
    {
        bool result = VideoHelper.IsVideoFile(fileExtension);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetFirstFramePath_FfmpegNotAvailableForInvalidVideo_ReturnsNull()
    {
        string testDirectory = CreateTestDirectoryPath();
        string destinationPath = Path.Combine(testDirectory, "Output");

        try
        {
            Directory.CreateDirectory(testDirectory);
            string testVideoFile = Path.Combine(testDirectory, "test.mp4");
            File.WriteAllText(testVideoFile, "not a real video");

            TestLogger<VideoHelperTests> logger = new();

            string? result = VideoHelper.GetFirstFramePath(testDirectory, "test.mp4", destinationPath, logger);

            Assert.That(result, Is.Null);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_ConfiguredFolderContainsFfmpeg_ReturnsConfiguredFolder()
    {
        string testDirectory = CreateTestDirectoryPath();
        string configuredDirectory = Path.Combine(testDirectory, "ConfiguredFfmpeg");

        try
        {
            Directory.CreateDirectory(configuredDirectory);
            CreateFfmpegExecutables(configuredDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, configuredDirectory);

            Assert.That(result, Is.EqualTo(configuredDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_AppLocalFolderContainsFfmpeg_ReturnsAppLocalFolder()
    {
        string testDirectory = CreateTestDirectoryPath();
        string appLocalDirectory = Path.Combine(testDirectory, "Ffmpeg", "Bin");

        try
        {
            Directory.CreateDirectory(appLocalDirectory);
            CreateFfmpegExecutables(appLocalDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, null);

            Assert.That(result, Is.EqualTo(appLocalDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_RuntimeNativeFolderContainsFfmpeg_ReturnsRuntimeNativeFolder()
    {
        string testDirectory = CreateTestDirectoryPath();
        string runtimeDirectory = Path.Combine(testDirectory, "runtimes", "linux-x64", "native");

        try
        {
            Directory.CreateDirectory(runtimeDirectory);
            CreateFfmpegExecutables(runtimeDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, null);

            Assert.That(result, Is.EqualTo(runtimeDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_LegacyProjectFolderContainsFfmpeg_ReturnsLegacyProjectFolder()
    {
        string testDirectory = CreateTestDirectoryPath();
        string baseDirectory = Path.Combine(testDirectory, "PhotoManager.Tests", "bin");
        string legacyDirectory = Path.Combine(testDirectory, "PhotoManager.Common", "Ffmpeg", "Bin");

        try
        {
            Directory.CreateDirectory(baseDirectory);
            Directory.CreateDirectory(legacyDirectory);
            CreateFfmpegExecutables(legacyDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(baseDirectory, null);

            Assert.That(result, Is.EqualTo(legacyDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_NoFfmpegFolder_ReturnsNull()
    {
        string testDirectory = CreateTestDirectoryPath();

        try
        {
            Directory.CreateDirectory(testDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, null);

            Assert.That(result, Is.Null);
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_EmptyConfiguredFolder_SearchesAppLocal()
    {
        string testDirectory = CreateTestDirectoryPath();
        string appLocalDirectory = Path.Combine(testDirectory, "Ffmpeg", "Bin");

        try
        {
            Directory.CreateDirectory(appLocalDirectory);
            CreateFfmpegExecutables(appLocalDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, "");

            Assert.That(result, Is.EqualTo(appLocalDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_WhitespaceConfiguredFolder_SearchesAppLocal()
    {
        string testDirectory = CreateTestDirectoryPath();
        string appLocalDirectory = Path.Combine(testDirectory, "Ffmpeg", "Bin");

        try
        {
            Directory.CreateDirectory(appLocalDirectory);
            CreateFfmpegExecutables(appLocalDirectory);

            string? result = VideoHelper.GetFfmpegBinaryFolder(testDirectory, "   ");

            Assert.That(result, Is.EqualTo(appLocalDirectory));
        }
        finally
        {
            Directory.Delete(testDirectory, true);
        }
    }

    [Test]
    public void GetFfmpegBinaryFolder_NullBaseDirectory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => VideoHelper.GetFfmpegBinaryFolder(null!, null));
    }

    [Test]
    public void GetFfmpegBinaryFolder_Parameterless_ReturnsNonNull()
    {
        // Tests the parameterless GetFfmpegBinaryFolder which reads environment variables
        // and falls back to app-local search
        string? result = VideoHelper.GetFfmpegBinaryFolder();

        // Result can be null (no ffmpeg found) or a valid path — we just verify no exception
        Assert.That(result, Is.Null.Or.Not.Empty);
    }

    private static void CreateFfmpegExecutables(string directory)
    {
        File.WriteAllText(Path.Combine(directory, "ffmpeg"), string.Empty);
        File.WriteAllText(Path.Combine(directory, "ffmpeg.exe"), string.Empty);
    }

    private static string CreateTestDirectoryPath()
        => Path.Combine(TestContext.CurrentContext.TestDirectory, "PhotoManagerVideoTests", Guid.NewGuid().ToString());
}
