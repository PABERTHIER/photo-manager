using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteAssetTests
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
    public void DeleteAsset_FolderAndAssetExist_ThumbnailsAndAssetAreDeleted()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsFalse(thumbnails.ContainsKey(folderPath2));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset1.FileName]);

            _testableAssetRepository!.DeleteAsset(folderPath1, asset1.FileName);
            _testableAssetRepository!.DeleteAsset(folderPath2, "toto.jpg");

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.IsEmpty(thumbnails[folderPath2]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsFalse(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            Assert.IsTrue(_testableAssetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_AssetDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_DoesNothing()
    {
        Mock<IConfigurationRoot> configurationRoot = new();
        configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "0");
        UserConfigurationService userConfigurationService = new(configurationRoot!.Object);
        TestableAssetRepository testableAssetRepository = new(_database!, _storageService!.Object, userConfigurationService);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "TestFolder2");

            testableAssetRepository!.AddFolder(folderPath);

            List<Asset> assets = testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository!.DeleteAsset(folderPath, "toto.jpg");

            Assert.IsEmpty(thumbnails);

            assets = testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsTrue(testableAssetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_FolderIsNull_DoesNothing()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset1.FileName]);

            _testableAssetRepository!.DeleteAsset("non_existent_path", asset1.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_ThumbnailsDoNotExistInRepositoryButBinExists_ThumbnailsAndAssetAreDeleted()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "TestFolder2");
            string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { fileName, new byte[] { 4, 5, 6 } }
            };

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _database!.WriteBlob(blobToWrite, asset1!.Folder.ThumbnailsFilename);

            _testableAssetRepository!.DeleteAsset(folderPath, asset1!.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath));
            Assert.AreEqual(1, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(fileName));
            Assert.AreEqual(blobToWrite[fileName], thumbnails[folderPath][fileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_DirectoryIsNull_DoesNothing()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string? folderPath2 = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset1.FileName]);

            _testableAssetRepository!.DeleteAsset(folderPath2!, asset1.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_FileNameIsNull_ThrowsArgumentNullException()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string? assetFileName = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset1.FileName]);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.DeleteAsset(folderPath1!, assetFileName!));

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte[]>(), thumbnails[folderPath1][asset1.FileName]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsNotEmpty(assets);
            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_FolderAndAssetDoNotExist_HasChangesIsFalse()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            _testableAssetRepository!.DeleteAsset(folderPath1, "toto.jpg");

            Assert.IsFalse(_testableAssetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void DeleteAsset_ConcurrentAccess_AssetsAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(asset1.FileName, assets.FirstOrDefault()?.FileName);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsFalse(thumbnails.ContainsKey(folderPath2));
            Assert.AreEqual(1, thumbnails[folderPath1].Count);
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset1.FileName]);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository!.DeleteAsset(folderPath1, asset1.FileName),
                () => _testableAssetRepository!.DeleteAsset(folderPath2, "toto.jpg")
            );

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsEmpty(thumbnails[folderPath1]);
            Assert.IsEmpty(thumbnails[folderPath2]);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);
            Assert.IsFalse(_testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName));
            Assert.IsTrue(_testableAssetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
