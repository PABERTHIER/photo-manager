namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BackupStorageTests
{
    private string? _dataDirectory;

    private BackupStorage? _backupStorage;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        _backupStorage = new BackupStorage();
    }

    [Test]
    public void GetBackupFilesPaths_PathWithFiles_ReturnsCorrectFiles()
    {
        string[] partialExpectedFiles =
        [
            Path.Combine(_dataDirectory!, "Homer.gif"),
            Path.Combine(_dataDirectory!, "Homer.mp4"),
            Path.Combine(_dataDirectory!, "Image 8.jpeg"),
            Path.Combine(_dataDirectory!, "Image 9.png"),
            Path.Combine(_dataDirectory!, "Image_11.heic")
        ];

        string[] actualFiles = _backupStorage!.GetBackupFilesPaths(_dataDirectory!);

        CollectionAssert.IsSubsetOf(partialExpectedFiles, actualFiles);
    }

    [Test]
    public void GetBackupFilesPaths_PathWithNoFiles_ReturnsEmptyArray()
    {
        string backupDirectory = Path.Combine(_dataDirectory!, "EmptyFolder");

        try
        {
            Directory.CreateDirectory(backupDirectory);

            string[] actualFiles = _backupStorage!.GetBackupFilesPaths(backupDirectory);

            Assert.IsNotNull(actualFiles);
            Assert.AreEqual(0, actualFiles.Length);
            Assert.IsEmpty(actualFiles);
        }
        finally
        {
            Directory.Delete(backupDirectory);
        }
        
    }

    [Test]
    public void GetBackupFilesPaths_PathDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string backupDirectory = Path.Combine(_dataDirectory!, "NonExistentFolder");

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory));

        Assert.AreEqual($"Could not find a part of the path '{backupDirectory}'.", exception?.Message);
    }

    [Test]
    public void GetBackupFilesPaths_PathIsNull_ThrowsArgumentNullException()
    {
        string? backupDirectory = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory!));

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }

    [Test]
    public void WriteFolderToZipFile_ValidSourceDirectoryName_SuccessfullyCreatesZipFile()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "TestFolder.zip");

        try
        {
            _backupStorage!.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);

            Assert.IsTrue(File.Exists(destinationArchiveFileName));
        }
        finally
        {
            File.Delete(destinationArchiveFileName);
        }
    }

    [Test]
    public void WriteFolderToZipFile_InvalidSourceDirectoryName_ThrowsDirectoryNotFoundException()
    {
        const string invalidSourceDirectoryName = "InvalidDirectory";
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "backup.zip");

        try
        {
            DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() =>
            {
                _backupStorage!.WriteFolderToZipFile(invalidSourceDirectoryName, destinationArchiveFileName);
            });

            Assert.AreEqual($"Could not find a part of the path '{Path.Combine(TestContext.CurrentContext.TestDirectory, invalidSourceDirectoryName)}'.", exception?.Message);
            Assert.IsTrue(File.Exists(destinationArchiveFileName));
        }
        finally
        {
            File.Delete(destinationArchiveFileName);
        }
    }

    [Test]
    public void WriteFolderToZipFile_DestinationArchiveFileNameIsNull_ThrowsArgumentNullException()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, "TestFolder");
        string? destinationArchiveFileName = null;

        BackupStorage backupStorage = new();

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName!);
        });

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }

    [Test]
    public void WriteFolderToZipFile_DestinationArchiveFileNameDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "nonexistent", "backup.zip");

        BackupStorage backupStorage = new();

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);
        });

        Assert.AreEqual($"Could not find a part of the path '{destinationArchiveFileName}'.", exception?.Message);
    }

    [Test]
    public void DeleteBackupFile_FileExists_FileDeleted()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "TestFolder.zip");

        _backupStorage!.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);

        Assert.IsTrue(File.Exists(destinationArchiveFileName));

        _backupStorage!.DeleteBackupFile(destinationArchiveFileName);

        Assert.IsFalse(File.Exists(destinationArchiveFileName));
    }

    [Test]
    public void DeleteBackupFile_FileDoesNotExist_NoExceptionThrown()
    {
        string tempFilePath = Path.Combine(_dataDirectory!, "nonExistentFileToDelete.zip");

        Assert.DoesNotThrow(() => _backupStorage!.DeleteBackupFile(tempFilePath));
    }

    [Test]
    public void DeleteBackupFile_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() => _backupStorage!.DeleteBackupFile(_dataDirectory!));

        Assert.AreEqual($"Access to the path '{_dataDirectory!}' is denied.", exception?.Message);
    }

    [Test]
    public void DeleteBackupFile_PathIsNull_ThrowsArgumentNullException()
    {
        string? tempFilePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _backupStorage!.DeleteBackupFile(tempFilePath!));

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }
}
