using PhotoManager.UI;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetAboutInformationTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(string assetsDirectory, string projectName, string projectOwner)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.PROJECT_NAME, projectName);
        configurationRootMock.MockGetValue(UserConfigurationKeys.PROJECT_OWNER, projectOwner);

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        AssetRepository assetRepository = new(database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new(userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(assetRepository, storageService, userConfigurationService);
        _application = new(assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
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
        ConfigureApplication(_dataDirectory!, projectName, projectOwner);

        try
        {
            AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(App).Assembly);

            Assert.That(aboutInformation.Product, Is.EqualTo(expectedProjectName));
            Assert.That(aboutInformation.Author, Is.EqualTo(expectedProjectOwner));
            Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
            Assert.That(aboutInformation.Version, Does.StartWith("v"));
            Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetAboutInformation_WithDifferentAssembly_ReturnsDifferentProduct()
    {
        ConfigureApplication(_dataDirectory!, "PhotoManager", "Toto");

        try
        {
            AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(int).Assembly);

            Assert.That(aboutInformation.Product, Is.Not.EqualTo("PhotoManager"));
            Assert.That(aboutInformation.Product, Is.EqualTo("Microsoft® .NET"));
            Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
            Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Manager Photo", "Toto")]
    [TestCase("Photo Toto", "Tutu")]
    public void GetAboutInformation_WithAssemblyWithoutProductAttribute_ReturnsDefaultProduct(string expectedProjectName, string expectedProjectOwner)
    {
        ConfigureApplication(_dataDirectory!, expectedProjectName, expectedProjectOwner);

        AssemblyName assemblyName = new("TestAssemblyWithoutProductAttribute");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        try
        {
            AboutInformation aboutInformation = _application!.GetAboutInformation(assemblyBuilder);

            Assert.That(aboutInformation.Product, Is.EqualTo(expectedProjectName));
            Assert.That(aboutInformation.Author, Is.EqualTo(expectedProjectOwner));
            Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
