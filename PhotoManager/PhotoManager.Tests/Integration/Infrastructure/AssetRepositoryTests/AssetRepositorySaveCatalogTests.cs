using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveCatalogTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    PhotoManager.Infrastructure.Database.Database? _database;

    private IAssetRepository? _assetRepository;
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
        _assetRepository = new AssetRepository(_database, _storageService!.Object, userConfigurationService);

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
    public void SaveCatalog_HasChanges_SaveOperationsPerformed()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files
            SyncAssetsConfiguration syncAssetsConfiguration = new();
            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfiguration.Definitions.Add(
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

            _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave);
            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.IsFalse(_assetRepository.HasChanges());

            _assetRepository.SaveCatalog(addedFolder2);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(AssetConstants.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(AssetConstants.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(AssetConstants.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(AssetConstants.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == asset1.FolderId)?.Name == asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == asset1.Hash);
            Assert.IsTrue(asset?.FileName == asset1.FileName && asset?.FolderId == asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasChanges_SaveOperationsPerformed()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
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
    public void SaveCatalog_FolderIsNullAndHasChangesAndEverythingEmpty_SaveOperationsPerformed()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _assetRepository.DeleteFolder(addedFolder1);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

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
    public void SaveCatalog_FolderIsNullAndHasNoChanges_DoesNothing()
    {
        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

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
    public void SaveCatalog_ConcurrentAccess_EverythingIsHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files
            SyncAssetsConfiguration syncAssetsConfiguration = new();
            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfiguration.Definitions.Add(
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

            _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave);
            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository.SaveCatalog(addedFolder1),
                () => _assetRepository.SaveCatalog(addedFolder2)
            );

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, AssetConstants.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, AssetConstants.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(AssetConstants.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(AssetConstants.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(AssetConstants.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(AssetConstants.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == asset1.FolderId)?.Name == asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == asset1.Hash);
            Assert.IsTrue(asset?.FileName == asset1.FileName && asset?.FolderId == asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
