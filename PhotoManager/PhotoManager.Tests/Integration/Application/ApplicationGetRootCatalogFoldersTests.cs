using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetRootCatalogFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(string assetsDirectory)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetRootCatalogFolders_CataloguedAssets_ReturnsRootCatalogFolders()
    {
        const string folderName = Directories.NEW_FOLDER_2;
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, folderName);

        ConfigureApplication(assetsDirectory);

        try
        {
            await _application!.CatalogAssetsAsync(_ => { });

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
            Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

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
            Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

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
