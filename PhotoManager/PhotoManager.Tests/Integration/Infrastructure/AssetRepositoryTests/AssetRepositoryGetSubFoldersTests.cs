using Reactive = System.Reactive;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetSubFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

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
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            string childFolderPath1 = Path.Combine(parentFolderPath1, Directories.TEST_SUB_FOLDER_1);
            string childFolderPath2 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);
            string childFolderPath3 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _assetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _assetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _assetRepository!.AddFolder(childFolderPath3);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Folder[] childFolders1 = _assetRepository!.GetSubFolders(childFolder1);
            Folder[] childFolders2 = _assetRepository!.GetSubFolders(childFolder2);
            Folder[] childFolders3 = _assetRepository!.GetSubFolders(childFolder3);

            Assert.That(parentFolders1, Is.Not.Empty);
            Assert.That(parentFolders1, Has.Length.EqualTo(1));
            Assert.That(parentFolders1[0].Path, Is.EqualTo(childFolderPath1));

            Assert.That(parentFolders2, Is.Not.Empty);
            Assert.That(parentFolders2, Has.Length.EqualTo(2));
            Assert.That(parentFolders2[0].Path, Is.EqualTo(childFolderPath2));
            Assert.That(parentFolders2[1].Path, Is.EqualTo(childFolderPath3));

            Assert.That(childFolders1, Is.Empty);
            Assert.That(childFolders2, Is.Empty);
            Assert.That(childFolders3, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string parentFolderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderPath1 };
            Folder parentFolder2 = new() { Id = Guid.NewGuid(), Path = parentFolderPath2 };

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2);

            Assert.That(parentFolders1, Is.Empty);
            Assert.That(parentFolders2, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            string parentFolderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            _assetRepository!.AddFolder(parentFolderPath2); // At least one folder to trigger the Where on folders

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _assetRepository!.GetSubFolders(parentFolder1!));

            Assert.That(exception?.Message, Is.EqualTo("Delegate to an instance method cannot have null 'this'."));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
