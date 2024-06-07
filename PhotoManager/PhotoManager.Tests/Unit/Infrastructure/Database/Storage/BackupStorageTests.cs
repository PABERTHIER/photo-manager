namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BackupStorageTests
{
    private string? dataDirectory;
    private BackupStorage? _backupStorage;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        _backupStorage = new BackupStorage();
    }

    [Test]
    public void GetBackupFilesPaths_PathWithFiles_ReturnsCorrectFiles()
    {
        string[] partialExpectedFiles = new string[]
        {
            Path.Combine(dataDirectory!, "Homer.gif"),
            Path.Combine(dataDirectory!, "Homer.mp4"),
            Path.Combine(dataDirectory!, "Image 8.jpeg"),
            Path.Combine(dataDirectory!, "Image 9.png"),
            Path.Combine(dataDirectory!, "Image_11.heic")
        };

        string[] actualFiles = _backupStorage!.GetBackupFilesPaths(dataDirectory!);

        CollectionAssert.IsSubsetOf(partialExpectedFiles, actualFiles);
    }

    [Test]
    public void GetBackupFilesPaths_PathWithNoFiles_ReturnsEmptyArray()
    {
        string backupDirectory = Path.Combine(dataDirectory!, "EmptyFolder");

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
        string backupDirectory = Path.Combine(dataDirectory!, "NonExistentFolder");

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory!));

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
        string sourceDirectoryName = Path.Combine(dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(dataDirectory!, "TestFolder.zip");

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
        string invalidSourceDirectoryName = "InvalidDirectory";
        string destinationArchiveFileName = Path.Combine(dataDirectory!, "backup.zip");

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
        string sourceDirectoryName = Path.Combine(dataDirectory!, "TestFolder");
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
        string sourceDirectoryName = Path.Combine(dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(dataDirectory!, "nonexistent", "backup.zip");

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
        string sourceDirectoryName = Path.Combine(dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(dataDirectory!, "TestFolder.zip");

        _backupStorage!.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);

        Assert.IsTrue(File.Exists(destinationArchiveFileName));

        _backupStorage!.DeleteBackupFile(destinationArchiveFileName);

        Assert.IsFalse(File.Exists(destinationArchiveFileName));
    }

    [Test]
    public void DeleteBackupFile_FileDoesNotExist_NoExceptionThrown()
    {
        string tempFilePath = Path.Combine(dataDirectory!, "nonExistentFileToDelete.zip");

        Assert.DoesNotThrow(() => _backupStorage!.DeleteBackupFile(tempFilePath));
    }

    [Test]
    public void DeleteBackupFile_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() => _backupStorage!.DeleteBackupFile(dataDirectory!));

        Assert.AreEqual($"Access to the path '{dataDirectory!}' is denied.", exception?.Message);
    }

    [Test]
    public void DeleteBackupFile_PathIsNull_ThrowsArgumentNullException()
    {
        string? tempFilePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _backupStorage!.DeleteBackupFile(tempFilePath!));

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }
}
