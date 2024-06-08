using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveCatalogTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private AssetRepository? _assetRepository;
    PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

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
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
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
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.IsFalse(_assetRepository.HasChanges());

            _assetRepository.SaveCatalog(addedFolder2);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == asset1.FolderId)?.Name == asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == asset1.Hash);
            Assert.IsTrue(asset?.FileName == asset1.FileName && asset.FolderId == asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasChanges_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            asset1!.Folder = addedFolder1;
            asset1!.FolderId = addedFolder1.FolderId;
            _assetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasChangesAndEverythingEmpty_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _assetRepository.DeleteFolder(addedFolder1);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasNoChanges_DoesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_ConcurrentAccess_EverythingIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository.SaveCatalog(addedFolder1),
                () => _assetRepository.SaveCatalog(addedFolder2)
            );

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == asset1.FolderId)?.Name == asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == asset1.Hash);
            Assert.IsTrue(asset?.FileName == asset1.FileName && asset.FolderId == asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
