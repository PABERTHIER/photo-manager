using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteFolderTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
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
        List<Reactive.Unit> assetsUpdatedEvents = [];
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
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsWithThumbnails_RemovesThumbnailsAndFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(_asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnails_RemovesOnlyFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(_asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButCreated_RemovesFolderAndThumbnail()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.IsFalse(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsEmpty(thumbnails[_asset1!.Folder.Path]);

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(_asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButBinExists_RemovesFolderAndBinFile()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));

            _database!.WriteBlob([], _asset1!.Folder.ThumbnailsFilename);

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(_asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderDoesNotExist_RemovesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

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
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_ConcurrentAccess_ThumbnailsAndFoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1!.Folder = folder;
            _asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder)
            );

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.IsFalse(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsFalse(folders.Contains(_asset1!.Folder));
            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
