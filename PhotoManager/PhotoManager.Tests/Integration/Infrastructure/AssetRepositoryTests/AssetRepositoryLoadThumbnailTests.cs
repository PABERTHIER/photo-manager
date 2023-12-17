using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryLoadThumbnailTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    private Asset? asset1;

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

        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
        _storageService!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
    }

    [SetUp]
    public void Setup()
    {
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
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_ReturnsBitmapImage()
    {
        try
        {
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);
            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, new byte[] { 1, 2, 3 });

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);
            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExists_ReturnsBitmapImage()
    {
        try
        {
            string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { fileName, new byte[] { 4, 5, 6 } }
            };

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);
            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(dataDirectory!));
            Assert.AreEqual(2, thumbnails[dataDirectory!].Count);
            Assert.IsTrue(thumbnails[dataDirectory!].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[dataDirectory!][asset1.FileName]);

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExistsAndRemoveOldThumbnailsDictionaryEntriesIs0_ReturnsNull()
    {
        Mock<IConfigurationRoot> configurationRoot = new();
        configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "0");

        UserConfigurationService userConfigurationService = new(configurationRoot!.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, _storageService!.Object, userConfigurationService);

        try
        {
            string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { fileName, new byte[] { 4, 5, 6 } }
            };

            Folder addedFolder1 = testableAssetRepository!.AddFolder(dataDirectory!);

            _database!.WriteBlob(blobToWrite, addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);
            Assert.IsNull(bitmapImage);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.IsEmpty(thumbnails);

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinNotContainingTheAssetExists_ReturnsNullAndWritesBlobAndDbFiles()
    {
        try
        {
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);

            _database!.WriteBlob(new Dictionary<string, byte[]>(), addedFolder1.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);

            Assert.IsNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(dataDirectory!));
            Assert.IsEmpty(thumbnails[dataDirectory!]);

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, addedFolder1.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_ReturnsBitmapImage()
    {
        try
        {
            Guid folderId = Guid.NewGuid();
            asset1!.Folder = new Folder() { FolderId = folderId, Path = dataDirectory! };
            asset1!.FolderId = folderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(dataDirectory!));
            Assert.AreEqual(1, thumbnails[dataDirectory!].Count);
            Assert.IsTrue(thumbnails[dataDirectory!].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[dataDirectory!][asset1.FileName]);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);

            Assert.IsNotNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(dataDirectory!));
            Assert.AreEqual(1, thumbnails[dataDirectory!].Count);
            Assert.IsTrue(thumbnails[dataDirectory!].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[dataDirectory!][asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_AssetAndFolderDoNotExist_ReturnsNull()
    {
        try
        {
            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            BitmapImage? bitmapImage = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);

            Assert.IsNull(bitmapImage);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assets.Count);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(dataDirectory!));
            Assert.IsEmpty(thumbnails[dataDirectory!]);

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_DirectoryNameIsNull_ThrowsArgumentNullException()
    {
        try
        {
            string? directoryName = null;
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);
            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _testableAssetRepository!.LoadThumbnail(directoryName!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight);
            });

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_FileNameIsNull_ThrowsArgumentNullException()
    {
        try
        {
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);
            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            string? fileName = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.LoadThumbnail(dataDirectory!, fileName!, 0, 0));

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_ThumbnailsIsHandledSafely()
    {
        try
        {
            Folder addedFolder1 = _testableAssetRepository!.AddFolder(dataDirectory!);
            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, new byte[] { 1, 2, 3 });

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            BitmapImage? bitmapImage1 = null;
            BitmapImage? bitmapImage2 = null;
            BitmapImage? bitmapImage3 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => bitmapImage1 = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight),
                () => bitmapImage2 = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight),
                () => bitmapImage3 = _testableAssetRepository!.LoadThumbnail(dataDirectory!, asset1!.FileName, asset1.ThumbnailPixelWidth, asset1.ThumbnailPixelHeight)
            );

            Assert.IsNotNull(bitmapImage1);
            Assert.IsNotNull(bitmapImage2);
            Assert.IsNotNull(bitmapImage3);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1!.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1!.Folder.Path][asset1.FileName]);

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
