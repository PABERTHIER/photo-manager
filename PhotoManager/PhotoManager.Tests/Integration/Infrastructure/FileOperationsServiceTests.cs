using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class FileOperationsServiceTests
{
    private string? _assetsDirectory;

    private FileOperationsService? _fileOperationsService;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<FileOperationsService>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _assetsDirectory);

        _userConfigurationService = configurationRootMock.CreateUserConfigurationService();
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _fileOperationsService = new(_userConfigurationService!, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void GetSubDirectories_ValidDirectory_ReturnsArrayOfSubDirectories()
    {
        string directoryPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        DirectoryInfo[] subDirectories = _fileOperationsService!.GetSubDirectories(directoryPath);

        Assert.That(subDirectories, Is.Not.Null);
        Assert.That(subDirectories, Has.Exactly(3).Items);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(subDirectories[0].Name, Is.EqualTo(Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(subDirectories[1].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_1));
        Assert.That(subDirectories[2].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_2));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_assetsDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception =
            Assert.Throws<DirectoryNotFoundException>(() => _fileOperationsService!.GetSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsArrayOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        DirectoryInfo[] recursiveSubDirectories = _fileOperationsService!.GetRecursiveSubDirectories(directoryPath);

        Assert.That(recursiveSubDirectories, Has.Length.EqualTo(4));
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_3), Is.True);
        Assert.That(recursiveSubDirectories[0].FullName,
            Does.EndWith(Path.DirectorySeparatorChar + Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(recursiveSubDirectories[1].FullName,
            Does.EndWith(Path.DirectorySeparatorChar + Directories.TEST_SUB_FOLDER_1));
        Assert.That(recursiveSubDirectories[2].FullName,
            Does.EndWith(Path.DirectorySeparatorChar + Directories.TEST_SUB_FOLDER_2));
        Assert.That(recursiveSubDirectories[3].FullName,
            Does.EndWith(Path.DirectorySeparatorChar +
                         Path.Combine(Directories.TEST_SUB_FOLDER_2, Directories.TEST_SUB_FOLDER_3)));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetRecursiveSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_assetsDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception =
            Assert.Throws<DirectoryNotFoundException>(() =>
                _fileOperationsService!.GetRecursiveSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(_assetsDirectory!, Directories.TEST_DIRECTORY);

        _fileOperationsService!.CreateDirectory(testDirectory);

        Assert.That(Directory.Exists(testDirectory), Is.True);

        Directory.Delete(testDirectory);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void DeleteFile_FileExists_DeletesFile()
    {
        const string fileName = FileNames.IMAGE_1_JPG;
        const string newFileName = FileNames.IMAGE_TO_DELETE_JPG;
        string sourceFilePath = Path.Combine(_assetsDirectory!, fileName);
        string destinationFilePath = Path.Combine(_assetsDirectory!, newFileName);

        File.Copy(sourceFilePath, destinationFilePath);

        Assert.That(File.Exists(destinationFilePath), Is.True);

        _fileOperationsService!.DeleteFile(_assetsDirectory!, newFileName);

        Assert.That(File.Exists(destinationFilePath), Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void DeleteFile_FileDoesNotExist_NoActionTaken()
    {
        const string testFileName = FileNames.NON_EXISTENT_IMAGE_JPG;
        string testFilePath = Path.Combine(_assetsDirectory!, testFileName);

        Assert.That(File.Exists(testFilePath), Is.False);

        Assert.DoesNotThrow(() => _fileOperationsService!.DeleteFile(_assetsDirectory!, testFileName));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    [Platform("Win", Reason = "Only Windows blocks deleting a file that another stream holds open")]
    public void DeleteFile_FileIsLocked_LogsItAndDoesNotThrowAndNoActionTaken()
    {
        const string testFileName = FileNames.IMAGE_TO_DELETE_JPG;
        string tempDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string sourceFilePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);

        try
        {
            Directory.CreateDirectory(tempDirectory);
            string testFilePath = Path.Combine(tempDirectory, testFileName);
            File.Copy(sourceFilePath, testFilePath);

            Assert.That(File.Exists(testFilePath), Is.True);

            // Open a file stream to lock the file
            using (new FileStream(testFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                _fileOperationsService!.DeleteFile(tempDirectory, testFileName);

                Assert.That(File.Exists(testFilePath), Is.True);

                _testLogger!.AssertLogErrors(
                    [
                        $"Failed to delete file '{testFilePath}'. The process cannot access the file '{testFilePath}' because it is being used by another process."
                    ],
                    typeof(FileOperationsService));
            }
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _fileOperationsService!.GetFileNames(_assetsDirectory!);

        Assert.That(fileNames, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_2_JPG));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_1_JPG));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetFileInfos_ReturnsFileInfos()
    {
        FileInfo[] fileInfos = _fileOperationsService!.GetFileInfos(_assetsDirectory!);

        Assert.That(fileInfos, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(fileInfos.Select(static fileInfo => fileInfo.Name), Does.Contain(FileNames.IMAGE_2_JPG));
        Assert.That(fileInfos.Select(static fileInfo => fileInfo.Name), Does.Contain(FileNames.IMAGE_1_JPG));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetFileBytes_FileExists_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);

        byte[] actualBytes = _fileOperationsService!.GetFileBytes(testFilePath);

        Assert.That(actualBytes, Is.Not.Null);
        Assert.That(actualBytes, Is.Not.Empty);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetFileBytes_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_FILE_JPG);

        FileNotFoundException? exception =
            Assert.Throws<FileNotFoundException>(() => _fileOperationsService!.GetFileBytes(nonExistentFilePath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find file '{nonExistentFilePath}'."));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetFileBytes_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception =
            Assert.Throws<UnauthorizedAccessException>(() => _fileOperationsService!.GetFileBytes(_assetsDirectory!));
        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_assetsDirectory!}' is denied."));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void WriteFileBytes_ValidFilePath_WritesFileBytes()
    {
        string testDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_FOLDER);
        string testFilePath = Path.Combine(testDirectory, "WrittenBytes.bin");
        byte[] bytes = [0x01, 0x02, 0x03];

        try
        {
            Directory.CreateDirectory(testDirectory);

            _fileOperationsService!.WriteFileBytes(testFilePath, bytes);

            Assert.That(File.ReadAllBytes(testFilePath), Is.EqualTo(bytes));

            _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
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
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_9_PNG)]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    [TestCase(FileNames.HOMER_GIF)]
    public void WriteFileBytes_RealImageBytes_WritesByteIdenticalFile(string fileName)
    {
        byte[] sourceBytes = File.ReadAllBytes(Path.Combine(_assetsDirectory!, fileName));

        string testDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_FOLDER);
        string destinationPath = Path.Combine(testDirectory, fileName);

        try
        {
            Directory.CreateDirectory(testDirectory);

            _fileOperationsService!.WriteFileBytes(destinationPath, sourceBytes);

            byte[] writtenBytes = File.ReadAllBytes(destinationPath);

            Assert.That(writtenBytes, Has.Length.EqualTo(sourceBytes.Length));
            Assert.That(writtenBytes, Is.EqualTo(sourceBytes));

            _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
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
    public void WriteFileBytes_DestinationFileAlreadyExists_OverwritesAndTruncatesToNewContent()
    {
        byte[] largeImageBytes = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC));
        byte[] smallImageBytes = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG));

        string testDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_FOLDER);
        string destinationPath = Path.Combine(testDirectory, FileNames.IMAGE_CONVERTED_JPEG);

        try
        {
            Directory.CreateDirectory(testDirectory);

            _fileOperationsService!.WriteFileBytes(destinationPath, largeImageBytes);
            _fileOperationsService!.WriteFileBytes(destinationPath, smallImageBytes);

            byte[] writtenBytes = File.ReadAllBytes(destinationPath);

            Assert.That(writtenBytes, Has.Length.EqualTo(smallImageBytes.Length));
            Assert.That(writtenBytes, Is.EqualTo(smallImageBytes));

            _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
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
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _assetsDirectory! };
        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(default)
            .WithHash(string.Empty)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        bool exists = _fileOperationsService!.FileExists(folder, asset);

        Assert.That(exists, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _assetsDirectory! };
        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(FileNames.NON_EXISTENT_IMAGE_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, PixelHeightAsset.NON_EXISTENT_IMAGE_JPG,
                ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(default)
            .WithHash(string.Empty)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        bool exists = _fileOperationsService!.FileExists(folder, asset);

        Assert.That(exists, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    [TestCase("toto", null, "path2")]
    [TestCase(null, FileNames.IMAGE_1_JPG, "path1")]
    public void FileExists_NullFileNameOrNullPath_ThrowsArgumentNullException(string? path, string? fileName,
        string exceptionParameter)
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = path! };
        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName!)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(default)
            .WithHash(string.Empty)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _fileOperationsService!.FileExists(folder, asset));

        Assert.That(exception?.Message, Is.EqualTo($"Value cannot be null. (Parameter '{exceptionParameter}')"));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);

        bool exists = _fileOperationsService!.FileExists(fullPath);

        Assert.That(exists, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FileExistsFullPath_FileDoesNotExist_ReturnsFalse()
    {
        string fullPath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);

        bool exists = _fileOperationsService!.FileExists(fullPath);

        Assert.That(exists, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FileExistsFullPath_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _fileOperationsService!.FileExists(fullPath!);

        Assert.That(exists, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FolderExists_ExistingFolder_ReturnsTrue()
    {
        bool exists = _fileOperationsService!.FolderExists(_assetsDirectory!);

        Assert.That(exists, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FolderExists_NonExistentFolder_ReturnsFalse()
    {
        bool exists = _fileOperationsService!.FolderExists("toto");

        Assert.That(exists, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void FolderExists_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _fileOperationsService!.FolderExists(fullPath!);

        Assert.That(exists, Is.False);

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetTotalFilesCount_FilesInDirectory_ReturnsTotalFilesCount()
    {
        int totalFilesCount = _fileOperationsService!.GetTotalFilesCount();
        Assert.That(totalFilesCount, Is.EqualTo(61));

        _testLogger!.AssertLogExceptions([], typeof(FileOperationsService));
    }

    [Test]
    public void GetTotalFilesCount_EmptyDirectory_ReturnsTotalFilesCount()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

            UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();
            TestLogger<FileOperationsService> logger = new();
            FileOperationsService fileOperationsService = new(userConfigurationService, logger);

            int totalFilesCount = fileOperationsService.GetTotalFilesCount();
            Assert.That(totalFilesCount, Is.Zero);

            logger.AssertLogExceptions([], typeof(FileOperationsService));
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }
}
