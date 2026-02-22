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
    private string? _dataDirectory;

    private FileOperationsService? _fileOperationsService;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new(configurationRootMock.Object);
        _fileOperationsService = new(_userConfigurationService);
    }

    [Test]
    public void GetSubDirectories_ValidDirectory_ReturnsArrayOfSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        DirectoryInfo[] subDirectories = _fileOperationsService!.GetSubDirectories(directoryPath);

        Assert.That(subDirectories, Is.Not.Null);
        Assert.That(subDirectories, Has.Exactly(3).Items);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(subDirectories[0].Name, Is.EqualTo(Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(subDirectories[1].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_1));
        Assert.That(subDirectories[2].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_2));
    }

    [Test]
    public void GetSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception =
            Assert.Throws<DirectoryNotFoundException>(() => _fileOperationsService!.GetSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsArrayOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        DirectoryInfo[] recursiveSubDirectories = _fileOperationsService!.GetRecursiveSubDirectories(directoryPath);

        Assert.That(recursiveSubDirectories, Has.Length.EqualTo(4));
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_3), Is.True);
        Assert.That(recursiveSubDirectories[0].FullName, Does.EndWith("\\" + Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(recursiveSubDirectories[1].FullName, Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_1));
        Assert.That(recursiveSubDirectories[2].FullName, Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_2));
        Assert.That(recursiveSubDirectories[3].FullName,
            Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_2 + "\\" + Directories.TEST_SUB_FOLDER_3));
    }

    [Test]
    public void GetRecursiveSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception =
            Assert.Throws<DirectoryNotFoundException>(() =>
                _fileOperationsService!.GetRecursiveSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(_dataDirectory!, Directories.TEST_DIRECTORY);

        _fileOperationsService!.CreateDirectory(testDirectory);

        Assert.That(Directory.Exists(testDirectory), Is.True);

        Directory.Delete(testDirectory);
    }

    [Test]
    public void DeleteFile_FileExists_DeletesFile()
    {
        const string fileName = FileNames.IMAGE_1_JPG;
        const string newFileName = FileNames.IMAGE_TO_DELETE_JPG;
        string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
        string destinationFilePath = Path.Combine(_dataDirectory!, newFileName);

        File.Copy(sourceFilePath, destinationFilePath);

        Assert.That(File.Exists(destinationFilePath), Is.True);

        _fileOperationsService!.DeleteFile(_dataDirectory!, newFileName);

        Assert.That(File.Exists(destinationFilePath), Is.False);
    }

    [Test]
    public void DeleteFile_FileDoesNotExist_NoActionTaken()
    {
        const string testFileName = FileNames.NON_EXISTENT_IMAGE_JPG;
        string testFilePath = Path.Combine(_dataDirectory!, testFileName);

        Assert.That(File.Exists(testFilePath), Is.False);

        Assert.DoesNotThrow(() => _fileOperationsService!.DeleteFile(_dataDirectory!, testFileName));
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _fileOperationsService!.GetFileNames(_dataDirectory!);

        Assert.That(fileNames, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_2_JPG));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_1_JPG));
    }

    [Test]
    public void GetFileBytes_FileExists_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        byte[] actualBytes = _fileOperationsService!.GetFileBytes(testFilePath);

        Assert.That(actualBytes, Is.Not.Null);
        Assert.That(actualBytes, Is.Not.Empty);
    }

    [Test]
    public void GetFileBytes_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_FILE_JPG);

        FileNotFoundException? exception =
            Assert.Throws<FileNotFoundException>(() => _fileOperationsService!.GetFileBytes(nonExistentFilePath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find file '{nonExistentFilePath}'."));
    }

    [Test]
    public void GetFileBytes_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception =
            Assert.Throws<UnauthorizedAccessException>(() => _fileOperationsService!.GetFileBytes(_dataDirectory!));
        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_dataDirectory!}' is denied."));
    }

    [Test]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty
        };

        bool exists = _fileOperationsService!.FileExists(folder, asset);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = FileNames.NON_EXISTENT_IMAGE_JPG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG,
                    Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG,
                    Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG
                }
            },
            Hash = string.Empty
        };

        bool exists = _fileOperationsService!.FileExists(folder, asset);

        Assert.That(exists, Is.False);
    }

    [Test]
    [TestCase("toto", null!, "path2")]
    [TestCase(null!, FileNames.IMAGE_1_JPG, "path1")]
    public void FileExists_NullFileNameOrNullPath_ThrowsArgumentNullException(string path, string fileName,
        string exceptionParameter)
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = path };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = fileName,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty
        };

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _fileOperationsService!.FileExists(folder, asset));

        Assert.That(exception?.Message, Is.EqualTo($"Value cannot be null. (Parameter '{exceptionParameter}')"));
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        bool exists = _fileOperationsService!.FileExists(fullPath);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FileExistsFullPath_FileDoesNotExist_ReturnsFalse()
    {
        string fullPath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);

        bool exists = _fileOperationsService!.FileExists(fullPath);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FileExistsFullPath_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _fileOperationsService!.FileExists(fullPath!);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FolderExists_ExistingFolder_ReturnsTrue()
    {
        bool exists = _fileOperationsService!.FolderExists(_dataDirectory!);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FolderExists_NonExistentFolder_ReturnsFalse()
    {
        bool exists = _fileOperationsService!.FolderExists("toto");

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FolderExists_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _fileOperationsService!.FolderExists(fullPath!);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void GetTotalFilesCount_FilesInDirectory_ReturnsTotalFilesCount()
    {
        int totalFilesCount = _fileOperationsService!.GetTotalFilesCount();
        Assert.That(totalFilesCount, Is.EqualTo(67));
    }

    [Test]
    public void GetTotalFilesCount_EmptyDirectory_ReturnsTotalFilesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            Mock<IConfigurationRoot> configurationRootMock = new();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

            UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
            FileOperationsService fileOperationsService = new(userConfigurationService);

            int totalFilesCount = fileOperationsService.GetTotalFilesCount();
            Assert.That(totalFilesCount, Is.Zero);
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }
}
