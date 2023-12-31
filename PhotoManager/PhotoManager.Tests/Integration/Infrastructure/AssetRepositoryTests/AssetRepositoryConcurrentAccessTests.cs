namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryConcurrentAccessTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
        _storageServiceMock.Setup(x => x.LoadFileInformation(It.IsAny<Asset>()));
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _testableAssetRepository = new TestableAssetRepository(_database, _storageServiceMock!.Object, userConfigurationService);

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
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
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
    public void AssetRepository_ConcurrentAccess_WorksCorrectly()
    {
        Asset asset4 = new()
        {
            FolderId = new Guid("cf9293ad-d835-4126-8a76-17c7bbfe626e"),
            FileName = "Image 2.jpg",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "b92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
        Asset asset5 = new()
        {
            FolderId = new Guid("1e3c2274-bc11-482a-a7b4-e86e5aeb17f1"),
            FileName = "Image 3.jpg",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 8, 27, 6, 49, 20),
            Hash = "c92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");
            string folderPath3 = Path.Combine(dataDirectory!, "NewFolder3");
            string folderPath4 = Path.Combine(dataDirectory!, "NewFolder4");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            Folder folder3 = new() { FolderId = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { FolderId = Guid.NewGuid(), Path = folderPath4 };

            asset1!.Folder = folder1;
            asset1!.FolderId = folder1.FolderId;
            byte[] assetData1 = new byte[] { 1, 2, 3 };

            asset2!.Folder = folder1;
            asset2!.FolderId = folder1.FolderId;
            byte[]? assetData2 = Array.Empty<byte>();

            asset3!.Folder = folder2;
            asset3!.FolderId = folder2.FolderId;
            byte[] assetData3 = new byte[] { 4, 5, 6 };

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            List<string> recentTargetPathsToSave = new()
            {
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            };

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(cataloguedAssets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            // DeleteAsset
            string folderPath5 = Path.Combine(dataDirectory!, "NewFolder5");
            string folderPath6 = Path.Combine(dataDirectory!, "NewFolder6");
            Folder folder5 = _testableAssetRepository!.AddFolder(folderPath5);
            Folder folder6 = _testableAssetRepository!.AddFolder(folderPath6);
            asset4!.Folder = folder5;
            asset4!.FolderId = folder5.FolderId;
            byte[] assetData4 = new byte[] { 1, 2, 3 };

            asset5!.Folder = folder6;
            asset5!.FolderId = folder6.FolderId;
            byte[]? assetData5 = Array.Empty<byte>();

            _testableAssetRepository.AddAsset(asset4, assetData4);
            _testableAssetRepository.AddAsset(asset5, assetData5);
            // DeleteAsset

            bool folderExists1 = false;
            bool folderExists2 = false;

            Folder? folderByPath1 = null; 
            Folder? folderByPath2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository.GetAssetsByPath(folderPath1),
                () => _testableAssetRepository.SaveCatalog(folder1),
                () => _testableAssetRepository.DeleteFolder(folder4),
                () => _testableAssetRepository.HasChanges(),
                () => _testableAssetRepository.ContainsThumbnail(folderPath2, asset2.FileName),
                () => _testableAssetRepository.GetCataloguedAssets(),
                () => _testableAssetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _testableAssetRepository.GetSubFolders(folder1, false),
                () => _testableAssetRepository.BackupExists(),
                () => _testableAssetRepository.GetSyncAssetsConfiguration(),
                () => _testableAssetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _testableAssetRepository.DeleteAsset(folderPath5, asset4.FileName),
                () => _testableAssetRepository.LoadThumbnail(folderPath1, asset2.FileName, 150, 150),
                () => _testableAssetRepository.GetAssetsCounter(),
                () => _testableAssetRepository.HasChanges(),
                () => _testableAssetRepository.AddAsset(asset1, assetData1),
                () => _testableAssetRepository.SaveCatalog(folder1),
                () => _testableAssetRepository.GetSyncAssetsConfiguration(),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.ContainsThumbnail(folderPath1, asset1.FileName),
                () => _testableAssetRepository.GetCataloguedAssetsByPath(folderPath1),
                () => _testableAssetRepository.GetFolders(),
                () => _testableAssetRepository.DeleteFolder(folder3),
                () => _testableAssetRepository.DeleteAsset(folderPath6, asset5.FileName),
                () => _testableAssetRepository.GetRecentTargetPaths(),
                () => _testableAssetRepository.LoadThumbnail(folderPath2, asset3.FileName, 150, 150),
                () => _testableAssetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _testableAssetRepository.AddFolder(folderPath3),
                () => _testableAssetRepository.FolderHasThumbnails(folder2),
                () => _testableAssetRepository.SaveCatalog(null),
                () => _testableAssetRepository.AddAsset(asset2, assetData2),
                () => _testableAssetRepository.IsAssetCatalogued(folderPath1, asset1.FileName),
                () => folderExists1 = _testableAssetRepository.FolderExists(folderPath1),
                () => _testableAssetRepository.GetFolders(),
                () => folderByPath1 = _testableAssetRepository.GetFolderByPath(folderPath1),
                () => _testableAssetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _testableAssetRepository.GetCataloguedAssets(),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.AddAsset(asset3, assetData3),
                () => _testableAssetRepository.GetSubFolders(folder2, false),
                () => _testableAssetRepository.AddFolder(folderPath4),
                () => _testableAssetRepository.GetRecentTargetPaths(),
                () => folderByPath2 = _testableAssetRepository.GetFolderByPath(folderPath2),
                () => _testableAssetRepository.IsAssetCatalogued(folderPath2, asset2.FileName),
                () => _testableAssetRepository.FolderHasThumbnails(folder1),
                () => _testableAssetRepository.GetCataloguedAssetsByPath(folderPath2),
                () => _testableAssetRepository.BackupExists(),
                () => folderExists2 = _testableAssetRepository.FolderExists(folderPath2),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.GetAssetsCounter(),
                () => _testableAssetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.AreEqual(3, cataloguedAssets.Count);
            Assert.IsTrue(cataloguedAssets.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssets.Any(x => x.FileName == asset2.FileName));
            Assert.IsTrue(cataloguedAssets.Any(x => x.FileName == asset3.FileName));

            Assert.AreEqual(4, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(folderPath1));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath2));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath5));
            Assert.IsTrue(thumbnails.ContainsKey(folderPath6));

            Assert.AreEqual(2, thumbnails[folderPath1].Count);
            Assert.AreEqual(1, thumbnails[folderPath2].Count);
            Assert.IsEmpty(thumbnails[folderPath5]);
            Assert.IsEmpty(thumbnails[folderPath6]);

            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath1].ContainsKey(asset2.FileName));
            Assert.IsTrue(thumbnails[folderPath2].ContainsKey(asset3.FileName));

            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath1][asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath1][asset2.FileName]);
            Assert.AreEqual(new byte[] { 4, 5, 6 }, thumbnails[folderPath2][asset3.FileName]);

            Folder[] folders = _testableAssetRepository!.GetFolders();
            Assert.GreaterOrEqual(folders.Length, 4);
            Assert.IsTrue(folders.Any(x => x.Path == folderPath1));
            Assert.IsTrue(folders.Any(x => x.Path == folderPath2));
            Assert.IsTrue(folders.Any(x => x.Path == folderPath5));
            Assert.IsTrue(folders.Any(x => x.Path == folderPath6));

            Assert.IsTrue(folderExists1);
            Assert.IsTrue(folderExists2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
