using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetRootCatalogFoldersTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplication(string assetsDirectory)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
    }

    [Test]
    public async Task GetRootCatalogFolders_CataloguedAssets_ReturnsRootCatalogFolders()
    {
        const string folderName = Directories.NEW_FOLDER_2;
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, folderName);

        ConfigureApplication(assetsDirectory);

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

    [Test]
    public void GetRootCatalogFolders_FolderIsAdded_ReturnsRootCatalogFolders()
    {
        ConfigureApplication(_assetsDirectory!);

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        Assert.That(folder.Path, Is.EqualTo(_assetsDirectory!));
        Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

        Folder[] folders = _application!.GetRootCatalogFolders();

        Assert.That(folders, Has.Length.EqualTo(1));
        Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
        Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
        Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

    }

    [Test]
    public void GetRootCatalogFolders_FolderIsNotAdded_AddsFolderAndReturnsRootCatalogFolders()
    {
        ConfigureApplication(_assetsDirectory!);


        Folder[] folders = _application!.GetRootCatalogFolders();

        Folder? folder = _testableAssetRepository!.GetFolderByPath(_assetsDirectory!);
        Assert.That(folder, Is.Not.Null);
        Assert.That(folder.Path, Is.EqualTo(_assetsDirectory!));
        Assert.That(folder.Name, Is.EqualTo(Directories.TEST_FILES));

        Assert.That(folders, Has.Length.EqualTo(1));
        Assert.That(folders[0].Id, Is.EqualTo(folder.Id));
        Assert.That(folders[0].Path, Is.EqualTo(folder.Path));
        Assert.That(folders[0].Name, Is.EqualTo(folder.Name));

    }
}
