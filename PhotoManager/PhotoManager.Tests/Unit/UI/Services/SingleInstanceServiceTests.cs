using PhotoManager.UI.Services;

namespace PhotoManager.Tests.Unit.UI.Services;

[TestFixture]
public class SingleInstanceServiceTests
{
    [Test]
    public void Constructor_EmptyLockFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new SingleInstanceService(string.Empty));
    }

    [Test]
    public void TryAcquire_LockIsAvailable_ReturnsTrueAndCreatesLockFile()
    {
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "PhotoManager.lock");

        try
        {
            using (SingleInstanceService service = new(lockFilePath))
            {
                bool result = service.TryAcquire();

                Assert.That(result, Is.True);
                Assert.That(File.Exists(lockFilePath), Is.True);
            }
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void TryAcquire_LockIsAlreadyAcquired_ReturnsTrue()
    {
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "PhotoManager.lock");

        try
        {
            using (SingleInstanceService service = new(lockFilePath))
            {
                bool firstResult = service.TryAcquire();
                bool secondResult = service.TryAcquire();

                Assert.That(firstResult, Is.True);
                Assert.That(secondResult, Is.True);
            }
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void TryAcquire_LockIsHeldByAnotherInstance_ReturnsFalse()
    {
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "PhotoManager.lock");

        try
        {
            using (SingleInstanceService firstService = new(lockFilePath))
            {

                using (SingleInstanceService secondService = new(lockFilePath))
                {
                    bool firstResult = firstService.TryAcquire();
                    bool secondResult = secondService.TryAcquire();

                    Assert.That(firstResult, Is.True);
                    Assert.That(secondResult, Is.False);
                }
            }
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void Dispose_LockIsAcquired_ReleasesLock()
    {
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "PhotoManager.lock");

        try
        {
            using (SingleInstanceService firstService = new(lockFilePath))
            {
                Assert.That(firstService.TryAcquire(), Is.True);

                // ReSharper disable once DisposeOnUsingVariable
                firstService.Dispose();

                using (SingleInstanceService secondService = new(lockFilePath))
                {
                    Assert.That(secondService.TryAcquire(), Is.True);
                }
            }
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void Constructor_DefaultParameterless_UsesDefaultLockFilePath()
    {
        using (SingleInstanceService service = new())
        {
            bool result = service.TryAcquire();

            Assert.That(result, Is.True);

            // Cleanup: dispose releases lock, then remove the lock file
            // ReSharper disable once DisposeOnUsingVariable
            service.Dispose();

            string lockFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PhotoManager", "PhotoManager.lock");

            if (File.Exists(lockFilePath))
            {
                File.Delete(lockFilePath);
            }
        }
    }

    [Test]
    public void TryAcquire_LockDirectoryCannotBeCreated_ThrowsIOException()
    {
        // The lock directory cannot be created below an existing file, on any OS
        string blockingFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "BlockingFile.tmp");
        File.WriteAllText(blockingFilePath, string.Empty);

        string lockFilePath = Path.Combine(blockingFilePath, "SubDir", "PhotoManager.lock");

        try
        {
            // Windows throws IOException; Unix throws DirectoryNotFoundException, which derives from it
            Assert.Catch<IOException>(() =>
            {
                using (SingleInstanceService service = new(lockFilePath))
                {
                    service.TryAcquire();
                }
            });
        }
        finally
        {
            File.Delete(blockingFilePath);
        }
    }

    [Test]
    public void IsLockUnavailable_SharingViolationHResult_ReturnsTrue()
    {
        IOException exception = new() { HResult = unchecked((int)0x80070020) };

        bool result = SingleInstanceService.IsLockUnavailable(exception);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLockUnavailable_LockViolationHResult_ReturnsTrue()
    {
        IOException exception = new() { HResult = unchecked((int)0x80070021) };

        bool result = SingleInstanceService.IsLockUnavailable(exception);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLockUnavailable_OtherHResult_ReturnsFalse()
    {
        IOException exception = new() { HResult = unchecked((int)0x80070005) };

        bool result = SingleInstanceService.IsLockUnavailable(exception);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryAcquire_LockFileIsExistingDirectory_ThrowsUnauthorizedAccessException()
    {
        // When the lock file path points to a directory, FileStream throws
        // UnauthorizedAccessException which is NOT caught (not IOException)
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "PhotoManager.lock");

        try
        {
            Directory.CreateDirectory(lockFilePath);

            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                using (SingleInstanceService service = new(lockFilePath))
                {
                    service.TryAcquire();
                }
            });
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void TryAcquire_LockFileInNonExistentDirectoryPath_CreatesDirectoryAndAcquires()
    {
        string testDirectory = GetTestDirectoryPath();
        string lockFilePath = Path.Combine(testDirectory, "sub", "PhotoManager.lock");

        try
        {
            using (SingleInstanceService service = new(lockFilePath))
            {
                bool result = service.TryAcquire();

                Assert.That(result, Is.True);
                Assert.That(File.Exists(lockFilePath), Is.True);
            }
        }
        finally
        {
            DeleteTestDirectory(testDirectory);
        }
    }

    [Test]
    public void TryAcquire_LockFilePathWithoutDirectory_AcquiresLock()
    {
        string lockFileName = $"PhotoManagerTest_{Guid.NewGuid()}.lock";

        try
        {
            using (SingleInstanceService service = new(lockFileName))
            {
                bool result = service.TryAcquire();

                Assert.That(result, Is.True);
            }
        }
        finally
        {
            if (File.Exists(lockFileName))
            {
                File.Delete(lockFileName);
            }
        }
    }

    private static string GetTestDirectoryPath()
        => Path.Combine(TestContext.CurrentContext.TestDirectory, "PhotoManagerLockTests", Guid.NewGuid().ToString());

    private static void DeleteTestDirectory(string testDirectory)
    {
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, true);
        }
    }
}
