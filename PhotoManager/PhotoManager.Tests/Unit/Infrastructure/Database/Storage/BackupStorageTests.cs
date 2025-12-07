using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BackupStorageTests
{
    private string? _dataDirectory;

    private BackupStorage? _backupStorage;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        _backupStorage = new BackupStorage();
    }

    [Test]
    public void GetBackupFilesPaths_PathWithFiles_ReturnsCorrectFiles()
    {
        string[] partialExpectedFiles =
        [
            Path.Combine(_dataDirectory!, FileNames.HOMER_GIF),
            Path.Combine(_dataDirectory!, FileNames.HOMER_MP4),
            Path.Combine(_dataDirectory!, FileNames.IMAGE_8_JPEG),
            Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG),
            Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC)
        ];

        string[] actualFiles = _backupStorage!.GetBackupFilesPaths(_dataDirectory!);

        Assert.That(partialExpectedFiles, Is.SubsetOf(actualFiles));
    }

    [Test]
    public void GetBackupFilesPaths_PathWithNoFiles_ReturnsEmptyArray()
    {
        string backupDirectory = Path.Combine(_dataDirectory!, "EmptyFolder");

        try
        {
            Directory.CreateDirectory(backupDirectory);

            string[] actualFiles = _backupStorage!.GetBackupFilesPaths(backupDirectory);

            Assert.That(actualFiles, Is.Not.Null);
            Assert.That(actualFiles, Is.Empty);
            Assert.That(actualFiles, Is.Empty);
        }
        finally
        {
            Directory.Delete(backupDirectory);
        }
        
    }

    [Test]
    public void GetBackupFilesPaths_PathDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string backupDirectory = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{backupDirectory}'."));
    }

    [Test]
    public void GetBackupFilesPaths_PathIsNull_ThrowsArgumentNullException()
    {
        string? backupDirectory = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _backupStorage!.GetBackupFilesPaths(backupDirectory!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
    }

    [Test]
    public void WriteFolderToZipFile_ValidSourceDirectoryName_SuccessfullyCreatesZipFile()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "TestFolder.zip");

        try
        {
            _backupStorage!.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);

            Assert.That(File.Exists(destinationArchiveFileName), Is.True);
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

            Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{Path.Combine(TestContext.CurrentContext.TestDirectory, invalidSourceDirectoryName)}'."));
            Assert.That(File.Exists(destinationArchiveFileName), Is.True);
        }
        finally
        {
            File.Delete(destinationArchiveFileName);
        }
    }

    [Test]
    public void WriteFolderToZipFile_DestinationArchiveFileNameIsNull_ThrowsArgumentNullException()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);
        string? destinationArchiveFileName = null;

        BackupStorage backupStorage = new();

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName!);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
    }

    [Test]
    public void WriteFolderToZipFile_DestinationArchiveFileNameDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER, "backup.zip");

        BackupStorage backupStorage = new();

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() =>
        {
            backupStorage.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);
        });

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{destinationArchiveFileName}'."));
    }

    [Test]
    public void DeleteBackupFile_FileExists_FileDeleted()
    {
        string sourceDirectoryName = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);
        string destinationArchiveFileName = Path.Combine(_dataDirectory!, "TestFolder.zip");

        _backupStorage!.WriteFolderToZipFile(sourceDirectoryName, destinationArchiveFileName);

        Assert.That(File.Exists(destinationArchiveFileName), Is.True);

        _backupStorage!.DeleteBackupFile(destinationArchiveFileName);

        Assert.That(File.Exists(destinationArchiveFileName), Is.False);
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

        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_dataDirectory!}' is denied."));
    }

    [Test]
    public void DeleteBackupFile_PathIsNull_ThrowsArgumentNullException()
    {
        string? tempFilePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _backupStorage!.DeleteBackupFile(tempFilePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
    }
}
