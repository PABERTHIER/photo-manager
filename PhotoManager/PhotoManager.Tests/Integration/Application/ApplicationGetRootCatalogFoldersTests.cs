namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetRootCatalogFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    private void ConfigureApplication(string assetsDirectory)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
    }

    [Test]
    public async Task GetRootCatalogFolders_CataloguedAssets_ReturnsRootCatalogFolders()
    {
        const string folderName = "NewFolder2";
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", folderName);

        ConfigureApplication(assetsDirectory);

        try
        {
            await _application!.CatalogAssetsAsync(_ => {});

            Assert.That(_assetRepository!.HasChanges(), Is.False);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(assetsDirectory));
            Assert.That(folder.Name, Is.EqualTo(folderName));

            Folder[] folders = _application!.GetRootCatalogFolders();
            Assert.That(folders, Has.Length.EqualTo(1));

            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

            Assert.That(_assetRepository!.HasChanges(), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetRootCatalogFolders_FolderIsAdded_ReturnsRootCatalogFolders()
    {
        ConfigureApplication(_dataDirectory!);

        try
        {
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Assert.That(folder.Path, Is.EqualTo(_dataDirectory!));
            Assert.That(folder.Name, Is.EqualTo("TestFiles"));

            Assert.That(_assetRepository!.HasChanges(), Is.True);

            _assetRepository!.SaveCatalog(folder);

            Assert.That(_assetRepository!.HasChanges(), Is.False);

            Folder[] folders = _application!.GetRootCatalogFolders();

            Assert.That(folders, Has.Length.EqualTo(1));
            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

            Assert.That(_assetRepository!.HasChanges(), Is.False);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetRootCatalogFolders_FolderIsNotAdded_AddsFolderAndReturnsRootCatalogFolders()
    {
        ConfigureApplication(_dataDirectory!);

        try
        {
            Assert.That(_assetRepository!.HasChanges(), Is.False);

            Folder[] folders = _application!.GetRootCatalogFolders();

            Folder? folder = _assetRepository!.GetFolderByPath(_dataDirectory!);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(_dataDirectory!));
            Assert.That(folder.Name, Is.EqualTo("TestFiles"));

            Assert.That(folders, Has.Length.EqualTo(1));
            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

            Assert.That(_assetRepository!.HasChanges(), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
