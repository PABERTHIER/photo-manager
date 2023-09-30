using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetAssetsByPathTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRoot = new Mock<IConfigurationRoot>();
        _configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");
    }

    [SetUp]
    public void Setup()
    {
        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath!);
        _storageService.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
        _storageService.Setup(x => x.LoadFileInformation(It.IsAny<Asset>()));

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _testableAssetRepository = new TestableAssetRepository(_database, _storageService!.Object, userConfigurationService);

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
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
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
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
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
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssets()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder1;
            asset2!.FolderId = folder1.FolderId;
            byte[]? assetData2 = Array.Empty<byte>();

            asset3!.Folder = folder2;
            asset3!.FolderId = folder2.FolderId;
            byte[] assetData3 = new byte[] { 4, 5, 6 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1, assetData1);
            _testableAssetRepository.AddAsset(asset2, assetData2);
            _testableAssetRepository.AddAsset(asset3, assetData3);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);
            Asset[] assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2);

            Assert.AreEqual(3, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(asset2!.FileName, cataloguedAssets[1].FileName);
            Assert.AreEqual(asset3!.FileName, cataloguedAssets[2].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset3.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset2.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][asset3.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(1, assets2.Length);

            Assert.AreEqual(asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(asset2!.FileName, assets1[1].FileName);
            Assert.AreEqual(asset3!.FileName, assets2[0].FileName);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Exactly(3));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        BitmapImage? bitmapImage = null;
        Mock<IStorageService> storageService = new ();
        storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath!);
        storageService.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(bitmapImage!);
        storageService.Setup(x => x.LoadFileInformation(It.IsAny<Asset>()));

        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        TestableAssetRepository testableAssetRepository = new(_database!, storageService!.Object, userConfigurationService);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            Folder folder1 = testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            List<Asset> cataloguedAssets = testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(asset1, assetData1);

            Asset[] assets1 = testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);

            Assert.IsEmpty(assets1);

            storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButBinExists_ReturnsAssets()
    {
        try
        {
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { asset2!.FileName, new byte[] { 4, 5, 6 } }
            };

            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = new() { FolderId = folder1.FolderId, Path = folderPath2 };
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _database!.WriteBlob(blobToWrite, asset1!.Folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1, assetData1);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(2, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset2.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath1][asset2.FileName]);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath2][asset1.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][asset2.FileName]);

            Assert.AreEqual(1, assets1.Length);
            Assert.AreEqual(asset1.FileName, assets1[0].FileName);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Once);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssets()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = new () { Path = folderPath2 };
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1, assetData1);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath2][asset1.FileName]);

            Assert.AreEqual(1, assets1.Length);
            Assert.AreEqual(asset1.FileName, assets1[0].FileName);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Once);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_AssetFolderIsNull_ReturnsEmptyArray()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData1);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);
            Assert.IsEmpty(thumbnails);
            Assert.IsEmpty(assets);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_FolderDoesNotExist_ReturnsEmptyArray()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Guid folderId = Guid.NewGuid();
            Folder folder1 = new() { Path = folderPath, FolderId = folderId };
            Folder folder2 = new() { Path = folderPath, FolderId = folderId };

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1!, assetData1);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);

            Assert.IsEmpty(assets);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            _testableAssetRepository!.AddFolder(folderPath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.IsEmpty(thumbnails[folderPath]);

            Assert.IsEmpty(assets);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExistButBinExists_ReturnsEmptyArray()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { asset2!.FileName, Array.Empty<byte>() }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][asset2.FileName]);

            Assert.IsEmpty(assets);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);
            Assert.IsEmpty(thumbnails);
            Assert.IsEmpty(assets);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_DirectoryIsNull_ReturnsEmptyArray()
    {
        try
        {
            string? directory = null;
            string folderPath = Path.Combine(dataDirectory!, "NewFolder1");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1, assetData1);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(directory!);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][asset1.FileName]);

            Assert.IsEmpty(assets1);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ExceptionThrown_ReturnsAssetsWithPartialData()
    {
        Mock<IStorageService> storageService = new();
        storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath!);
        storageService.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception());
        storageService.Setup(x => x.LoadFileInformation(It.IsAny<Asset>()));

        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        TestableAssetRepository testableAssetRepository = new(_database!, storageService!.Object, userConfigurationService);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            Folder folder1 = testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder1;
            asset2!.FolderId = folder1.FolderId;
            byte[]? assetData2 = Array.Empty<byte>();

            List<Asset> cataloguedAssets = testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(asset1, assetData1);
            testableAssetRepository.AddAsset(asset2, assetData2);

            Asset[] assets1 = testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(2, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(asset2!.FileName, cataloguedAssets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset2.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset2.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(asset2.FileName, assets1[1].FileName);

            storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder1;
            asset2!.FolderId = folder1.FolderId;
            byte[]? assetData2 = Array.Empty<byte>();

            asset3!.Folder = folder2;
            asset3!.FolderId = folder2.FolderId;
            byte[] assetData3 = new byte[] { 4, 5, 6 };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(asset1, assetData1);
            _testableAssetRepository.AddAsset(asset2, assetData2);
            _testableAssetRepository.AddAsset(asset3, assetData3);

            Asset[] assets1 = Array.Empty<Asset>();
            Asset[] assets2 = Array.Empty<Asset>();

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1),
                () => assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.AreEqual(3, cataloguedAssets.Count);
            Assert.AreEqual(asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(asset2!.FileName, cataloguedAssets[1].FileName);
            Assert.AreEqual(asset3!.FileName, cataloguedAssets[2].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset3.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset2.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][asset3.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(1, assets2.Length);

            Assert.AreEqual(asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(asset2!.FileName, assets1[1].FileName);
            Assert.AreEqual(asset3!.FileName, assets2[0].FileName);

            _storageService!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
            _storageService!.Verify(x => x.LoadFileInformation(It.IsAny<Asset>()), Times.Exactly(3));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
