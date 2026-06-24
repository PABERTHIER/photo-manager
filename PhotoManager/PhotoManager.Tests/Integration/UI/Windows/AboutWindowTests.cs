using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.UI;
using PhotoManager.UI.Windows;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.Windows;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class AboutWindowTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
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
        ImageMagickThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
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
    [TestCase("PhotoManager", "Toto", "PhotoManager", "Toto")]
    [TestCase("Photo Toto", "Tutu", "PhotoManager", "Tutu")]
    public async Task Constructor_WithValidAssembly_SetsTitle(
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

        string title = await GetAboutWindowTitle(aboutInformation);
        string expectedTitle = $"About {expectedProjectName} {Constants.VERSION}";

        Assert.That(title, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task Constructor_WithDifferentAssembly_SetsTitle()
    {
        ConfigureApplication(_assetsDirectory!, "PhotoManager", "Toto");

        AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(int).Assembly);

        Assert.That(aboutInformation.Product, Is.Not.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Product, Is.EqualTo("Microsoft® .NET"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));

        string title = await GetAboutWindowTitle(aboutInformation);
        const string expectedTitle = $"About Microsoft® .NET {Constants.VERSION}";

        Assert.That(title, Is.EqualTo(expectedTitle));
    }

    [Test]
    [TestCase("Manager Photo", "Toto")]
    [TestCase("Photo Toto", "Tutu")]
    public async Task Constructor_WithAssemblyWithoutProductAttribute_SetsTitle(string expectedProjectName,
        string expectedProjectOwner)
    {
        ConfigureApplication(_assetsDirectory!, expectedProjectName, expectedProjectOwner);

        AssemblyName assemblyName = new("TestAssemblyWithoutProductAttribute");
        AssemblyBuilder assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        AboutInformation aboutInformation = _application!.GetAboutInformation(assemblyBuilder);

        Assert.That(aboutInformation.Product, Is.EqualTo(expectedProjectName));
        Assert.That(aboutInformation.Author, Is.EqualTo(expectedProjectOwner));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));

        string title = await GetAboutWindowTitle(aboutInformation);
        string expectedTitle = $"About {expectedProjectName} {Constants.VERSION}";

        Assert.That(title, Is.EqualTo(expectedTitle));
    }

    private static Task<string> GetAboutWindowTitle(AboutInformation aboutInformation)
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            AboutWindow? window = null;

            try
            {
                window = new(aboutInformation, NullLogger<AboutWindow>.Instance);

                return window.Title ?? string.Empty;
            }
            finally
            {
                window?.Close();
            }
        });
    }
}
