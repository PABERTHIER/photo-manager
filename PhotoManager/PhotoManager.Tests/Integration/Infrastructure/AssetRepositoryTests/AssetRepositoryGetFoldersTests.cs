using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
    }

    [Test]
    public void GetFolders_Folders_ReturnsCorrectFolders()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

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
    public void GetFolders_NoFolders_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder[] folders = _assetRepository!.GetFolders();

            Assert.That(folders, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFolders_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders1 = [];
            Folder[] folders2 = [];
            Folder[] folders3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => folders1 = _assetRepository!.GetFolders(),
                () => folders2 = _assetRepository!.GetFolders(),
                () => folders3 = _assetRepository!.GetFolders()
            );

            Assert.That(folders1, Has.Length.EqualTo(2));
            Assert.That(folders1[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders1[0].Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folders1[1].Path, Is.EqualTo(folderPath2));
            Assert.That(folders1[1].Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(folders2, Has.Length.EqualTo(2));
            Assert.That(folders2[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders2[0].Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folders2[1].Path, Is.EqualTo(folderPath2));
            Assert.That(folders2[1].Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(folders3, Has.Length.EqualTo(2));
            Assert.That(folders3[0].Path, Is.EqualTo(folderPath1));
            Assert.That(folders3[0].Id, Is.EqualTo(addedFolder1.Id));
            Assert.That(folders3[1].Path, Is.EqualTo(folderPath2));
            Assert.That(folders3[1].Id, Is.EqualTo(addedFolder2.Id));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
