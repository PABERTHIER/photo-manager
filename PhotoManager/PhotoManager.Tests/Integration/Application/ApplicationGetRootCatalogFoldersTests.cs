using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetRootCatalogFoldersTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(string assetsDirectory)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
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


            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(assetsDirectory));
            Assert.That(folder.Name, Is.EqualTo(folderName));

            Folder[] folders = _application!.GetRootCatalogFolders();
            Assert.That(folders, Has.Length.EqualTo(1));

            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

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
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Assert.That(folder.Path, Is.EqualTo(_dataDirectory!));
            Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

            Folder[] folders = _application!.GetRootCatalogFolders();

            Assert.That(folders, Has.Length.EqualTo(1));
            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

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

            Folder[] folders = _application!.GetRootCatalogFolders();

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_dataDirectory!);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(_dataDirectory!));
            Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

            Assert.That(folders, Has.Length.EqualTo(1));
            Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
            Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
            Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
