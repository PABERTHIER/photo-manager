using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFolderByPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
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
    public void GetFolderByPath_DifferentPaths_ReturnsCorrectFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_SamePath_ReturnsFirstFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath2.Id, Is.Not.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_PathNotRegistered_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.That(folderByPath1, Is.Null);

            Assert.That(assetsUpdatedEvents, Is.Empty);

        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_PathIsNull_ReturnsNull()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? folderPath1 = null;

            _assetRepository!.AddFolder(Path.Combine(_dataDirectory!, "TestFolder1"));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1!);

            Assert.That(folderByPath1, Is.Null);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolderByPath_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
            Folder? folderByPath2 = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1),
                () => folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2)
            );

            Assert.That(folderByPath1, Is.Not.Null);
            Assert.That(folderByPath2, Is.Not.Null);

            Assert.That(folderByPath1!.Path, Is.EqualTo(folderPath1));
            Assert.That(folderByPath1.Id, Is.EqualTo(addedFolder1.Id));

            Assert.That(folderByPath2!.Path, Is.EqualTo(folderPath2));
            Assert.That(folderByPath2.Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
