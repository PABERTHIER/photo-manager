using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetSubFoldersTests
{
    private string? _dataDirectory;
    private string? _backupPath;
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
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            string childFolderPath1 = Path.Combine(parentFolderPath1, "TestSubFolder1");
            string childFolderPath2 = Path.Combine(parentFolderPath2, "TestSubFolder2");
            string childFolderPath3 = Path.Combine(parentFolderPath2, "TestSubFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _assetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _assetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _assetRepository!.AddFolder(childFolderPath3);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Folder[] childFolders1 = _assetRepository!.GetSubFolders(childFolder1, includeHidden: false);
            Folder[] childFolders2 = _assetRepository!.GetSubFolders(childFolder2, includeHidden: false);
            Folder[] childFolders3 = _assetRepository!.GetSubFolders(childFolder3, includeHidden: false);

            Assert.IsNotEmpty(parentFolders1);
            Assert.AreEqual(1, parentFolders1.Length);
            Assert.AreEqual(childFolderPath1, parentFolders1[0].Path);

            Assert.IsNotEmpty(parentFolders2);
            Assert.AreEqual(2, parentFolders2.Length);
            Assert.AreEqual(childFolderPath2, parentFolders2[0].Path);
            Assert.AreEqual(childFolderPath3, parentFolders2[1].Path);

            Assert.IsEmpty(childFolders1);
            Assert.IsEmpty(childFolders2);
            Assert.IsEmpty(childFolders3);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Assert.IsEmpty(parentFolders1);
            Assert.IsEmpty(parentFolders2);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderPath1 };
            Folder parentFolder2 = new() { Id = Guid.NewGuid(), Path = parentFolderPath2 };

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Assert.IsEmpty(parentFolders1);
            Assert.IsEmpty(parentFolders2);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsArgumentException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? parentFolder1 = null;

            string parentFolderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            _assetRepository!.AddFolder(parentFolderPath2); // At least one folder to trigger the Where on folders

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _assetRepository!.GetSubFolders(parentFolder1!, includeHidden: false));

            Assert.AreEqual("Delegate to an instance method cannot have null 'this'.", exception?.Message);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
