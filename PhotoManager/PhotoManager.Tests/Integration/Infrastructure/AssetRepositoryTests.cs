using Autofac;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class AssetRepositoryTests
{
    private string? dataDirectory;
    private IAssetRepository? _assetRepository;
    private IUserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        //_assetRepository = new AssetRepository(configurationMock.Object);
        _userConfigurationService = new UserConfigurationService(configurationMock.Object);

    }

    [Test]
    public void FolderExists_DataDirectory_ReturnTrue()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
            });
        var repository = mock.Create<IAssetRepository>();
        bool folderExists = repository.FolderExists(dataDirectory!);
        Assert.IsFalse(folderExists);
        repository.AddFolder(dataDirectory!);
        folderExists = repository.FolderExists(dataDirectory!);
        Assert.IsTrue(folderExists);
    }

    [Test]
    public void HasChanges_Initial_ReturnFalse()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
            });
        var repository = mock.Create<IAssetRepository>();
        string imagePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        Assert.IsTrue(File.Exists(imagePath));
        Assert.IsFalse(repository.HasChanges());
    }

    [Test]
    public void HasChanges_AfterChange_ReturnTrue()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Create<IAssetRepository>();
        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();

        string imagePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        Assert.IsTrue(File.Exists(imagePath));
        repository.AddFolder(dataDirectory!);

        catalogAssetsService.CreateAsset(dataDirectory!, "Image 1.jpg");
        Assert.IsFalse(repository.HasChanges());
    }

    [Test]
    public void HasChanges_AfterSave_ReturnFalse()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();
        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();

        string imagePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        Assert.IsTrue(File.Exists(imagePath));

        repository.AddFolder(dataDirectory!);

        catalogAssetsService.CreateAsset(dataDirectory!, "Image 1.jpg");
        repository.SaveCatalog(null);
        Assert.IsFalse(repository.HasChanges());
    }

    [Test]
    public void IsAssetCatalogued_AssetNotInCatalog_ReturnFalse()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();
        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();

        string imagePath = Path.Combine(dataDirectory!, "Image 2.jpg");
        Assert.IsTrue(File.Exists(imagePath));

        Assert.IsFalse(repository.IsAssetCatalogued(dataDirectory!, "Image 2.jpg"));
    }

    [Test]
    public void IsAssetCatalogued_AssetInCatalog_ReturnTrue()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();
        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();

        string imagePath = Path.Combine(dataDirectory!, "Image 2.jpg");
        Assert.IsTrue(File.Exists(imagePath));

        Assert.IsFalse(repository.IsAssetCatalogued(dataDirectory!, "Image 2.jpg"));
        repository.AddFolder(dataDirectory!);
        catalogAssetsService.CreateAsset(dataDirectory!, "Image 2.jpg");
        Assert.IsTrue(repository.IsAssetCatalogued(dataDirectory!, "Image 2.jpg"));
    }

    [Test]
    public void IsAssetCatalogued_DeleteNonExistingAsset_ReturnFalse()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();

        repository.AddFolder(dataDirectory!);

        string imagePath = Path.Combine(dataDirectory!, "Non Existing Image.jpg");
        Assert.IsFalse(File.Exists(imagePath));

        repository.DeleteAsset(dataDirectory!, "Non Existing Image.jpg");
        Assert.IsFalse(repository.IsAssetCatalogued(dataDirectory!, "Non Existing Image.jpg"));

        repository.DeleteAsset(dataDirectory!, "Non Existing Image.jpg");
        Assert.IsFalse(repository.IsAssetCatalogued(dataDirectory!, "Non Existing Image.jpg"));
    }

    [Test]
    public void IsAssetCatalogued_DeleteExistingAsset_ReturnFalse()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();
        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();

        repository.AddFolder(dataDirectory!);

        string imagePath = Path.Combine(dataDirectory!, "Image 3.jpg");
        Assert.IsTrue(File.Exists(imagePath));
        Asset asset = catalogAssetsService.CreateAsset(dataDirectory!, "Image 3.jpg");

        // The asset should no longer be catalogued, but the image should still be in the filesystem.
        repository.DeleteAsset(dataDirectory!, "Image 3.jpg");
        Assert.IsFalse(repository.IsAssetCatalogued(dataDirectory!, "Image 3.jpg"));
        Assert.IsTrue(File.Exists(imagePath));
    }

    [Test]
    public void GetSyncAssetsConfiguration_ReturnArray()
    {
        using var mock = AutoMock.GetLoose(
            cfg =>
            {
                cfg.RegisterDatabaseTypes();
                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
                cfg.RegisterInstance(_userConfigurationService!).As<IUserConfigurationService>();
                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
                cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
            });
        var repository = mock.Container.Resolve<IAssetRepository>();

        SyncAssetsConfiguration syncConfiguration = new();

        syncConfiguration.Definitions.Add(
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\MyFirstGame\Screenshots",
                DestinationDirectory = @"C:\Images\MyFirstGame"
            });

        syncConfiguration.Definitions.Add(
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\MySecondGame\Screenshots",
                DestinationDirectory = @"C:\Images\MySecondGame"
            });

        repository.SaveSyncAssetsConfiguration(syncConfiguration);
        repository.SaveCatalog(null);

        syncConfiguration = repository.GetSyncAssetsConfiguration();

        Assert.AreEqual(2, syncConfiguration.Definitions.Count);
        Assert.AreEqual(@"C:\MyFirstGame\Screenshots", syncConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual(@"C:\Images\MyFirstGame", syncConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(@"C:\MySecondGame\Screenshots", syncConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual(@"C:\Images\MySecondGame", syncConfiguration.Definitions[1].DestinationDirectory);
    }
}
