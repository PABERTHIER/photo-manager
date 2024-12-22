using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddFolderTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly Guid _defaultGuid = Guid.Empty;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

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
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void AddFolder_DifferentPaths_AddsFoldersToList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.Id == _defaultGuid);

            Assert.AreEqual(folderPath2, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.Id == _defaultGuid);

            Assert.AreNotEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.Id, addedFolder2.Id);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.Id, folderByPath1.Id);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.Id, folderByPath2.Id);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Assert.AreEqual(folderPath1, folders[0].Path);
            Assert.AreEqual(addedFolder1.Id, folders[0].Id);
            Assert.AreEqual(folderPath2, folders[1].Path);
            Assert.AreEqual(addedFolder2.Id, folders[1].Id);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddFolder_SamePath_AddsFoldersToList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.Id == _defaultGuid);

            Assert.AreEqual(folderPath1, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.Id == _defaultGuid);

            Assert.AreEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.Id, addedFolder2.Id);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.Id, folderByPath1.Id);

            Assert.AreEqual(folderPath1, folderByPath2!.Path);
            Assert.AreNotEqual(addedFolder2.Id, folderByPath2.Id);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Assert.AreEqual(folderPath1, folders[0].Path);
            Assert.AreEqual(addedFolder1.Id, folders[0].Id);
            Assert.AreEqual(folderPath1, folders[1].Path);
            Assert.AreEqual(addedFolder2.Id, folders[1].Id);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddFolder_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
            Folder addedFolder2 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => addedFolder1 = _assetRepository!.AddFolder(folderPath1),
                () => addedFolder2 = _assetRepository!.AddFolder(folderPath2)
            );

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.Id == _defaultGuid);

            Assert.AreEqual(folderPath2, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.Id == _defaultGuid);

            Assert.AreNotEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.Id, addedFolder2.Id);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.Id, folderByPath1.Id);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.Id, folderByPath2.Id);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Folder? folder1 = folders.FirstOrDefault(x => x.Path == folderPath1);
            Folder? folder2 = folders.FirstOrDefault(x => x.Path == folderPath2);

            Assert.IsNotNull(folder1);
            Assert.IsNotNull(folder2);

            Assert.AreEqual(addedFolder1.Id, folder1!.Id);
            Assert.AreEqual(addedFolder2.Id, folder2!.Id);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
