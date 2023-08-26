namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BackupStorageTests
{
    private string? dataDirectory;
    private BackupStorage? _backupStorage;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        string directoryName = Path.GetDirectoryName(typeof(BackupStorageTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");

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
        string backupDirectory = Path.Combine(dataDirectory!, "EmptyFolder");

        Assert.Throws<DirectoryNotFoundException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory!));
    }

    [Test]
    public void GetBackupFilesPaths_PathIsNull_ThrowsArgumentNullException()
    {
        string? backupDirectory = null;

        Assert.Throws<ArgumentNullException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory!));
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
            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                _backupStorage!.WriteFolderToZipFile(invalidSourceDirectoryName, destinationArchiveFileName);
            });
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

        Assert.Throws<ArgumentNullException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName!);
        });
    }

    [Test]
    public void WriteFolderToZipFile_DestinationArchiveFileNameDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string sourceDirectoryName = Path.Combine(dataDirectory!, "TestFolder");
        string destinationArchiveFileName = Path.Combine(dataDirectory!, "nonexistent", "backup.zip");

        BackupStorage backupStorage = new();

        Assert.Throws<DirectoryNotFoundException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);
        });
    }

    [Test]
    public void DeleteBackupFile_FileExists_FileDeleted()
    {
        string tempFilePath = Path.Combine(dataDirectory!, "fileToDelete.txt");
        File.WriteAllText(tempFilePath, "Test content");

        _backupStorage!.DeleteBackupFile(tempFilePath);

        Assert.IsFalse(File.Exists(tempFilePath));
    }

    [Test]
    public void DeleteBackupFile_FileDoesNotExist_NoExceptionThrown()
    {
        string tempFilePath = Path.Combine(dataDirectory!, "nonExistentFileToDelete.txt");

        Assert.DoesNotThrow(() => _backupStorage!.DeleteBackupFile(tempFilePath));
    }

    [Test]
    public void DeleteBackupFile_PathIsNull_ThrowsArgumentNullException()
    {
        string? tempFilePath = null;

        Assert.Throws<ArgumentNullException>(() => _backupStorage!.DeleteBackupFile(tempFilePath!));
    }
}
