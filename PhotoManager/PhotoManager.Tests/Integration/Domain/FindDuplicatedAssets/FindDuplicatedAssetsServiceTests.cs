﻿using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IFindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private IAssetRepository? _assetRepository;
    private IStorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;
    private Asset? asset4;
    private Asset? asset5;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new StorageService(userConfigurationService);
        _findDuplicatedAssetsService = new FindDuplicatedAssetsService(_assetRepository, _storageService);

        asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 09),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 10),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1_duplicate.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 09),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset4 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 9_duplicate.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 10),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image_11.heic",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset2!.Folder = folder2;
            asset3!.Folder = folder2;
            asset4!.Folder = folder2;
            asset5!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);
            _assetRepository.AddAsset(asset3!, assetData1);
            _assetRepository.AddAsset(asset4!, assetData2);
            _assetRepository.AddAsset(asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsNotEmpty(duplicatedAssets);
            Assert.AreEqual(2, duplicatedAssets.Count);

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];

            Assert.AreEqual(2, firstDuplicatedAssetsSet.Count);
            Assert.AreEqual(2, secondDuplicatedAssetsSet.Count);

            Asset? duplicatedAsset1 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset1.FileName);
            Asset? duplicatedAsset3 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset3.FileName);

            DateTime actualDate = DateTime.Now.Date;
            DateTime fileModificationDate = new DateTime(2023, 1, 7).Date;

            Assert.IsNotNull(duplicatedAsset1);
            Assert.IsNotNull(duplicatedAsset3);
            Assert.AreEqual(asset1.FileName, duplicatedAsset1!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset1!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset1!.FileModificationDateTime.Date);
            Assert.AreEqual(asset3.FileName, duplicatedAsset3!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset3!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset3!.FileModificationDateTime.Date);

            Asset? duplicatedAsset2 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset2.FileName);
            Asset? duplicatedAsset4 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset4.FileName);

            Assert.IsNotNull(duplicatedAsset2);
            Assert.IsNotNull(duplicatedAsset4);
            Assert.AreEqual(asset2.FileName, duplicatedAsset2!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset2!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset2!.FileModificationDateTime.Date);
            Assert.AreEqual(asset4.FileName, duplicatedAsset4!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset4!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset4!.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        try
        {
            string hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960";
            string folderPath = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder = new() { Path = folderPath };

            asset2!.Folder = folder;
            asset2.Hash = hash;
            asset3!.Folder = folder;
            asset3.Hash = hash;
            asset4!.Folder = folder;
            asset4.Hash = hash;
            asset5!.Folder = folder;
            asset5.Hash = hash;

            byte[] assetData = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset2!, assetData);
            _assetRepository.AddAsset(asset3!, assetData);
            _assetRepository.AddAsset(asset4!, assetData);
            _assetRepository.AddAsset(asset5!, assetData);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsNotEmpty(duplicatedAssets);
            Assert.AreEqual(1, duplicatedAssets.Count);

            List<Asset> duplicatedAssetsSet = duplicatedAssets[0];

            Assert.AreEqual(4, duplicatedAssetsSet.Count);

            Asset? duplicatedAsset2 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset2.FileName);
            Asset? duplicatedAsset3 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset3.FileName);
            Asset? duplicatedAsset4 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset4.FileName);
            Asset? duplicatedAsset5 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset5.FileName);

            DateTime actualDate = DateTime.Now.Date;
            DateTime fileModificationDate = new DateTime(2023, 1, 7).Date;

            Assert.IsNotNull(duplicatedAsset2);
            Assert.IsNotNull(duplicatedAsset3);
            Assert.IsNotNull(duplicatedAsset4);
            Assert.IsNotNull(duplicatedAsset5);

            Assert.AreEqual(asset2.FileName, duplicatedAsset2!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset2!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset2!.FileModificationDateTime.Date);

            Assert.AreEqual(asset3.FileName, duplicatedAsset3!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset3!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset3!.FileModificationDateTime.Date);

            Assert.AreEqual(asset4.FileName, duplicatedAsset4!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset4!.FileCreationDateTime.Date);
            Assert.AreEqual(fileModificationDate, duplicatedAsset4!.FileModificationDateTime.Date);

            Assert.AreEqual(asset5.FileName, duplicatedAsset5!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset5!.FileCreationDateTime.Date);
            Assert.AreEqual(new DateTime(2023, 8, 9).Date, duplicatedAsset5!.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset3!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset3!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset2!.Folder = folder2;
            asset3!.Folder = folder2;
            asset4!.Folder = folder2;
            asset5!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);
            _assetRepository.AddAsset(asset3!, assetData1);
            _assetRepository.AddAsset(asset4!, assetData2);
            _assetRepository.AddAsset(asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        try
        {
            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");

            Folder folder = new() { Path = folderPath };

            asset1!.Folder = folder;
            asset2!.Folder = folder;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}