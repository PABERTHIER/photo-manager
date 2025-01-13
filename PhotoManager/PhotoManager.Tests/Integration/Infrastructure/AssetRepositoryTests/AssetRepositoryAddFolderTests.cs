using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddFolderTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly Guid _defaultGuid = Guid.Empty;
    private const string DATABASE_END_PATH = "v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
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

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));
            Assert.That(addedFolder2.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.Not.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(addedFolder1.Id));

            Assert.That(_assetRepository!.HasChanges(), Is.True);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(2));
            Assert.That(folders[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders[0].Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folders[1].Path, Is.EqualTo(folderPath2));
            Assert.That(folders[1].Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder2.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(addedFolder1.Id));

            Assert.That(_assetRepository!.HasChanges(), Is.True);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath2.Id, Is.Not.EqualTo(addedFolder2.Id));

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(2));
            Assert.That(folders[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders[0].Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folders[1].Path, Is.EqualTo(folderPath1));
            Assert.That(folders[1].Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder1.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));
            Assert.That(addedFolder2.Id == _defaultGuid, Is.False);

            Assert.That(addedFolder2.Path, Is.Not.EqualTo(addedFolder1.Path));
            Assert.That(addedFolder2.Id, Is.Not.EqualTo(addedFolder1.Id));

            Assert.That(_assetRepository!.HasChanges(), Is.True);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Has.Length.EqualTo(2));
            Folder? folder1 = folders.FirstOrDefault(x => x.Path == folderPath1);
            Folder? folder2 = folders.FirstOrDefault(x => x.Path == folderPath2);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);

            Assert.That(folder1!.Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folder2!.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
