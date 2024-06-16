using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteFolderTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

        asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    [Test]
    public void DeleteFolder_FolderIsNull_ThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? folder = null;

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _testableAssetRepository!.DeleteFolder(folder!));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsWithThumbnails_RemovesThumbnailsAndFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[asset1!.Folder.Path][asset1.FileName]);

            _testableAssetRepository!.DeleteFolder(asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnails_RemovesOnlyFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            _testableAssetRepository.SaveCatalog(asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));

            _testableAssetRepository!.DeleteFolder(asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButCreated_RemovesFolderAndThumbnail()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsFalse(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsEmpty(thumbnails[asset1!.Folder.Path]);

            _testableAssetRepository!.DeleteFolder(asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButBinExists_RemovesFolderAndBinFile()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            _testableAssetRepository.SaveCatalog(asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));

            _database!.WriteBlob(new Dictionary<string, byte[]>(), asset1!.Folder.ThumbnailsFilename);

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));

            _testableAssetRepository!.DeleteFolder(asset1!.Folder);

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderDoesNotExist_RemovesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);

            Folder folder = new()
            {
                Path = folderPath,
                FolderId = Guid.NewGuid(),
            };

            _testableAssetRepository!.DeleteFolder(folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(folders.Contains(folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_ConcurrentAccess_ThumbnailsAndFoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[asset1!.Folder.Path][asset1.FileName]);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository!.DeleteFolder(asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(asset1!.Folder)
            );

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
