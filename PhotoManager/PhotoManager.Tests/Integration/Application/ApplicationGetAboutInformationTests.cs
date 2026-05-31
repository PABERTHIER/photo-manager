using PhotoManager.UI;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetAboutInformationTests
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

    private void ConfigureApplication(string assetsDirectory, string projectName, string projectOwner)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.PROJECT_NAME, projectName);
        configurationRootMock.MockGetValue(UserConfigurationKeys.PROJECT_OWNER, projectOwner);

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService,
            new ImageMagickThumbnailGenerator(imageProcessingService),
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator,
            new CatalogFolderPipeline(fileOperationsService, assetCreationService,
                _testableAssetRepository),
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
    [TestCase("PhotoManager", "Toto", "PhotoManager", "Toto")]
    [TestCase("Photo Toto", "Tutu", "PhotoManager", "Tutu")]
    public void GetAboutInformation_WithValidAssembly_ReturnsAboutInformation(
        string projectName,
        string projectOwner,
        string expectedProjectName,
        string expectedProjectOwner)
    {
        ConfigureApplication(_assetsDirectory!, projectName, projectOwner);

        AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(App).Assembly);

        Assert.That(aboutInformation.Product, Is.EqualTo(expectedProjectName));
        Assert.That(aboutInformation.Author, Is.EqualTo(expectedProjectOwner));
        Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
        Assert.That(aboutInformation.Version, Does.StartWith("v"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void GetAboutInformation_WithDifferentAssembly_ReturnsDifferentProduct()
    {
        ConfigureApplication(_assetsDirectory!, "PhotoManager", "Toto");

        AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(int).Assembly);

        Assert.That(aboutInformation.Product, Is.Not.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Product, Is.EqualTo("Microsoft® .NET"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    [TestCase("Manager Photo", "Toto")]
    [TestCase("Photo Toto", "Tutu")]
    public void GetAboutInformation_WithAssemblyWithoutProductAttribute_ReturnsDefaultProduct(
        string expectedProjectName, string expectedProjectOwner)
    {
        ConfigureApplication(_assetsDirectory!, expectedProjectName, expectedProjectOwner);

        AssemblyName assemblyName = new("TestAssemblyWithoutProductAttribute");
        AssemblyBuilder assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        AboutInformation aboutInformation = _application!.GetAboutInformation(assemblyBuilder);

        Assert.That(aboutInformation.Product, Is.EqualTo(expectedProjectName));
        Assert.That(aboutInformation.Author, Is.EqualTo(expectedProjectOwner));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }
}
