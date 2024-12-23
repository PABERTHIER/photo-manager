﻿//using Autofac;
//using Autofac.Extras.Moq;
//using FluentAssertions;
//using Xunit;

//namespace PhotoManager.Tests.Integration.Application;

//public class ApplicationTests
//{
//    private readonly string dataDirectory;
//    private readonly IConfigurationRoot _configuration;

//public ApplicationTests()
//{
//    dataDirectory = Path.GetDirectoryName(typeof(ApplicationTests).Assembly.Location);
//    dataDirectory = Path.Combine(dataDirectory, "TestFiles");

//    Mock<IConfigurationRoot> configurationRootMock = new();
//    configurationRootMock.GetDefaultMockConfig();

//    _configuration = configurationRootMock.Object;
//}

//    [Test]
//    public void GetDuplicatedAssets_WithDuplicates_ReturnArray()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
//                cfg.RegisterType<FindDuplicatedAssetsService>().As<IFindDuplicatedAssetsService>().SingleInstance();
//                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
//            });
//        var database = mock.Container.Resolve<IDatabase>();
//        var repository = mock.Container.Resolve<IAssetRepository>();
//        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
//        var storageService = mock.Container.Resolve<IStorageService>();
//        var app = mock.Container.Resolve<PhotoManager.Application.Application>();

//        Folder folder = repository.AddFolder(dataDirectory);

//        string imagePath = Path.Combine(dataDirectory, "Image 2.jpg");
//        Assert.IsTrue(File.Exists(imagePath));
//        Asset asset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 1.jpg");
//        Assert.IsTrue(File.Exists(imagePath));
//        Asset anotherAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 1.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 2 duplicated.jpg");
//        Assert.IsTrue(File.Exists(imagePath));
//        Asset duplicatedAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2 duplicated.jpg");

//        Console.WriteLine("database.DataDirectory: " + database.DataDirectory);
//        Console.WriteLine("database.Separator: " + database.Separator);

//        repository.SaveCatalog(folder);

//        var duplicatedAssetSets = app.GetDuplicatedAssets();
//        duplicatedAssetSets.Should().ContainSingle();

//        var duplicatedAssets = duplicatedAssetSets[0];
//        duplicatedAssets.Should().HaveCount(2);
//        duplicatedAssets[0].FileName.Should().Be("Image 2.jpg");
//        duplicatedAssets[1].FileName.Should().Be("Image 2 duplicated.jpg");

//        repository.ContainsThumbnail(duplicatedAssets[0].Folder.Path, duplicatedAssets[0].FileName).Should().BeTrue();
//        repository.ContainsThumbnail(duplicatedAssets[1].Folder.Path, duplicatedAssets[1].FileName).Should().BeTrue();
//        repository.LoadThumbnail(duplicatedAssets[0].Folder.Path, duplicatedAssets[0].FileName, duplicatedAssets[0].ThumbnailPixelWidth, duplicatedAssets[0].ThumbnailPixelHeight).Should().NotBeNull();
//        repository.LoadThumbnail(duplicatedAssets[1].Folder.Path, duplicatedAssets[1].FileName, duplicatedAssets[1].ThumbnailPixelWidth, duplicatedAssets[1].ThumbnailPixelHeight).Should().NotBeNull();
//    }

//    [Test]
//    public void GetDuplicatedAssets_WithoutDuplicates_ReturnEmptyArray()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
//                cfg.RegisterType<FindDuplicatedAssetsService>().As<IFindDuplicatedAssetsService>().SingleInstance();
//                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
//            });
//        var repository = mock.Container.Resolve<IAssetRepository>();
//        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
//        var app = mock.Container.Resolve<PhotoManager.Application.Application>();

//        repository.AddFolder(dataDirectory);

//        string imagePath = Path.Combine(dataDirectory, "Image 2.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset asset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 1.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset anotherAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 1.jpg");

//        var duplicatedAssetSets = app.GetDuplicatedAssets();
//        duplicatedAssetSets.Should().BeEmpty();
//    }

//    /// <summary>
//    /// Tests an scenario when the user searches for duplicates before an
//    /// old entry gets deleted from the catalog.
//    /// </summary>
//    [Test]
//    public void GetDuplicatedAssets_WithInexistingDuplicatedAsset_ReturnEmptyArray()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
//                cfg.RegisterType<FindDuplicatedAssetsService>().As<IFindDuplicatedAssetsService>().SingleInstance();
//                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
//            });
//        var repository = mock.Container.Resolve<IAssetRepository>();
//        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
//        var app = mock.Container.Resolve<PhotoManager.Application.Application>();

//        repository.AddFolder(dataDirectory);

//        string imagePath = Path.Combine(dataDirectory, "Image 2.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset asset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 1.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset anotherAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 1.jpg");

//        imagePath = Path.Combine(dataDirectory, "Inexistent Image.jpg");
//        File.Exists(imagePath).Should().BeFalse();

//        Asset inexistentAsset = new()
//        {
//            FileName = "Inexistent Image.jpg",
//            Folder = asset.Folder,
//            FolderId = asset.Id,
//            Hash = asset.Hash
//        };

//        repository.AddAsset(inexistentAsset, null);
//        var duplicatedAssetSets = app.GetDuplicatedAssets();
//        duplicatedAssetSets.Should().BeEmpty();
//    }

//    /// <summary>
//    /// Tests an scenario when the user searches for duplicates before an
//    /// old entry gets deleted from the catalog.
//    /// </summary>
//    [Test]
//    public void GetDuplicatedAssets_WithInexistingNotDuplicatedAsset_ReturnEmptyArray()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
//                cfg.RegisterType<FindDuplicatedAssetsService>().As<IFindDuplicatedAssetsService>().SingleInstance();
//                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
//            });
//        var repository = mock.Container.Resolve<IAssetRepository>();
//        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
//        var app = mock.Container.Resolve<PhotoManager.Application.Application>();

//        Folder folder = repository.AddFolder(dataDirectory);

//        string imagePath = Path.Combine(dataDirectory, "Inexistent Image.jpg");
//        File.Exists(imagePath).Should().BeFalse();

//        repository.AddAsset(new Asset
//        {
//            FileName = "Inexistent Image.jpg",
//            Folder = folder,
//            FolderId = folder.Id,
//            Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//        }, null);

//        imagePath = Path.Combine(dataDirectory, "Image 2.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset asset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 1.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset anotherAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 1.jpg");

//        var duplicatedAssetSets = app.GetDuplicatedAssets();
//        duplicatedAssetSets.Should().BeEmpty();
//    }

//    [Test]
//    public void AddAssets_ToNonExistingFolder_AddFolderToCatalog()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
//            });
//        var repository = mock.Container.Resolve<IAssetRepository>();

//        Folder folder = new() { FolderId = "1", Path = "C:\\Inexistent Folder" };

//        string imagePath = Path.Combine(dataDirectory, "Inexistent Image.jpg");
//        File.Exists(imagePath).Should().BeFalse();

//        repository.AddAsset(new Asset
//        {
//            FileName = "Inexistent Image.jpg",
//            Folder = folder,
//            FolderId = folder.Id,
//            Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078"
//        }, null);

//        folder = repository.GetFolderByPath("C:\\Inexistent Folder");
//        folder.Should().NotBeNull();
//        folder.Path.Should().Be("C:\\Inexistent Folder");
//        folder.Name.Should().Be("Inexistent Folder");
//    }

//    [Test]
//    public void GetAssets_WithThumbnailNotFound_ReturnArrayIncludingAssetWithNoThumbnail()
//    {
//        IUserConfigurationService userConfigurationService = new UserConfigurationService(_configuration);

//        using var mock = AutoMock.GetLoose(
//            cfg =>
//            {
//                cfg.RegisterDatabaseTypes();
//                cfg.RegisterInstance(userConfigurationService).As<IUserConfigurationService>();
//                cfg.RegisterType<StorageService>().As<IStorageService>().SingleInstance();
//                cfg.RegisterType<AssetsComparator>().As<IAssetsComparator>().SingleInstance();
//                cfg.RegisterType<UnencapsulatedAssetRepository>().As<IAssetRepository>().SingleInstance();
//                cfg.RegisterType<FindDuplicatedAssetsService>().As<IFindDuplicatedAssetsService>().SingleInstance();
//                cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
//            });
//        UnencapsulatedAssetRepository repository = (UnencapsulatedAssetRepository)mock.Container.Resolve<IAssetRepository>();
//        var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
//        var app = mock.Container.Resolve<PhotoManager.Application.Application>();

//        Folder folder = repository.AddFolder(dataDirectory);
//        Mock<IAssetHashCalculatorService> hashCalculator = new();

//        string imagePath = Path.Combine(dataDirectory, "Image 2.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset asset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 1.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset anotherAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 1.jpg");

//        imagePath = Path.Combine(dataDirectory, "Image 2 duplicated.jpg");
//        File.Exists(imagePath).Should().BeTrue();
//        Asset duplicatedAsset = catalogAssetsService.CreateAsset(dataDirectory, "Image 2 duplicated.jpg");

//        repository.SaveCatalog(folder);
//        repository.RemoveThumbnail(folder.Path, "Image 2 duplicated.jpg");
//        repository.SaveCatalog(folder);

//        Asset[] assets = app.GetAssets(dataDirectory);
//        assets.Should().NotBeEmpty();

//        repository.GetAssets(dataDirectory).Should().Contain(a => a.FileName == "Image 2.jpg");
//        repository.GetAssets(dataDirectory).Should().NotContain(a => a.FileName == "Image 2 duplicated.jpg");
//        repository.ContainsThumbnail(dataDirectory, "Image 2.jpg").Should().BeTrue();
//        repository.ContainsThumbnail(dataDirectory, "Image 2 duplicated.jpg").Should().BeFalse();
//        repository.LoadThumbnail(dataDirectory, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight).Should().NotBeNull();
//        repository.LoadThumbnail(dataDirectory, duplicatedAsset.FileName, duplicatedAsset.ThumbnailPixelWidth, duplicatedAsset.ThumbnailPixelHeight).Should().BeNull();
//    }
//}

//class UnencapsulatedAssetRepository : AssetRepository
//{
//    public UnencapsulatedAssetRepository(IDatabase database, IStorageService storageService, IUserConfigurationService userConfigurationService)
//        : base(database, storageService, userConfigurationService)
//    {

//    }

//    internal void RemoveThumbnail(string directoryName, string fileName)
//    {
//        if (Thumbnails.ContainsKey(directoryName) && Thumbnails[directoryName].ContainsKey(fileName))
//        {
//            Thumbnails[directoryName].Remove(fileName);
//        }
//    }
//}
