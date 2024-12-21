using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetAssetsByPathTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
    }

    [SetUp]
    public void SetUp()
    {
        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath!);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 363888,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = "Image 9.png",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 6000, Height = 6120 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4602393,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        _asset3 = new()
        {
            Folder = new() { Path = "" },
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
            FileName = "Image_11.heic",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2247285,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            _asset3 = _asset3!.WithFolder(folder2);
            byte[] assetData3 = [4, 5, 6];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);
            Asset[] assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2);

            Assert.AreEqual(3, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(_asset2!.FileName, cataloguedAssets[1].FileName);
            Assert.AreEqual(_asset3!.FileName, cataloguedAssets[2].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(_asset3.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset2.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][_asset3.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(1, assets2.Length);

            Assert.AreEqual(_asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(_asset2!.FileName, assets1[1].FileName);
            Assert.AreEqual(_asset3!.FileName, assets2[0].FileName);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        BitmapImage? bitmapImage = null;
        Mock<IStorageService> storageService = new();
        storageService.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath!);
        storageService.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(bitmapImage!);

        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, storageService.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            Folder folder1 = testableAssetRepository.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset[] assets1 = testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][_asset1.FileName]);

            Assert.IsEmpty(assets1);

            storageService.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButBinExists_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { _asset2!.FileName, [4, 5, 6]}
            };

            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder1.Id, Path = folderPath2 });
            byte[] assetData1 = [1, 2, 3];

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(2, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(_asset2.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][_asset1.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath1][_asset2.FileName]);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath2][_asset1.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][_asset2.FileName]);

            Assert.AreEqual(1, assets1.Length);
            Assert.AreEqual(_asset1.FileName, assets1[0].FileName);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssets()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder1.Id, Path = folderPath2 });
            byte[] assetData1 = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath2][_asset1.FileName]);

            Assert.AreEqual(1, assets1.Length);
            Assert.AreEqual(_asset1.FileName, assets1[0].FileName);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_AssetFolderIsNull_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            byte[] assetData1 = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData1);

            Assert.IsEmpty(assetsUpdatedEvents);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);
            Assert.IsEmpty(thumbnails);
            Assert.IsEmpty(assets);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderDoesNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Guid folderId = Guid.NewGuid();
            Folder folder1 = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData1);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);

            Assert.IsEmpty(assets);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExistButBinExists_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { _asset2!.FileName, [] }
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
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][_asset2.FileName]);

            Assert.IsEmpty(assets);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset[] assets = _testableAssetRepository.GetAssetsByPath(folderPath);

            Assert.IsEmpty(cataloguedAssets);
            Assert.IsEmpty(thumbnails);
            Assert.IsEmpty(assets);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_DirectoryIsNull_ReturnsEmptyArray()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? directory = null;
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder1");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData1 = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            Asset[] assets1 = _testableAssetRepository.GetAssetsByPath(directory!);

            Assert.AreEqual(1, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);

            Assert.IsEmpty(assets1);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ExceptionThrown_ReturnsAssetsWithPartialData()
    {
        Mock<IStorageService> storageService = new();
        storageService.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath!);
        storageService.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception());

        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, storageService.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            Folder folder1 = testableAssetRepository.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(_asset1, assetData1);
            testableAssetRepository.AddAsset(_asset2, assetData2);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);

            Asset[] assets1 = testableAssetRepository.GetAssetsByPath(folderPath1);

            Assert.AreEqual(2, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(_asset2!.FileName, cataloguedAssets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset2.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset2.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(_asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(_asset2.FileName, assets1[1].FileName);

            storageService.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            _asset3 = _asset3!.WithFolder(folder2);
            byte[] assetData3 = [4, 5, 6];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);

            Asset[] assets1 = [];
            Asset[] assets2 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _testableAssetRepository.GetAssetsByPath(folderPath1),
                () => assets2 = _testableAssetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.AreEqual(3, cataloguedAssets.Count);
            Assert.AreEqual(_asset1.FileName, cataloguedAssets[0].FileName);
            Assert.AreEqual(_asset2!.FileName, cataloguedAssets[1].FileName);
            Assert.AreEqual(_asset3!.FileName, cataloguedAssets[2].FileName);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(_asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(_asset3.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][_asset2.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][_asset3.FileName]);

            Assert.AreEqual(2, assets1.Length);
            Assert.AreEqual(1, assets2.Length);

            Assert.AreEqual(_asset1.FileName, assets1[0].FileName);
            Assert.AreEqual(_asset2!.FileName, assets1[1].FileName);
            Assert.AreEqual(_asset3!.FileName, assets2[0].FileName);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));

            Assert.AreEqual(3, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[2]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
