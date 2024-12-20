using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveCatalogTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

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
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    [Test]
    public void SaveCatalog_HasChanges_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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

            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave);
            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.IsFalse(_assetRepository.HasChanges());

            _assetRepository.SaveCatalog(addedFolder2);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == _asset1.FolderId)?.Name == _asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.IsTrue(asset?.FileName == _asset1.FileName && asset.FolderId == _asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _storageServiceMock!.Verify(x => x.UpdateAssetFileProperties(It.IsAny<Asset>()), Times.Never);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_HasChangesAndInitializeAgain_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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

            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave);
            // Just to fill the syncassetsdirectoriesdefinitions.db and the recenttargetpaths.db files

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.IsFalse(_assetRepository.HasChanges());

            // New AssetRepository to test Initialize method with content in DB
            AssetRepository assetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);

            Assert.IsFalse(assetRepository.HasChanges());

            assetRepository.SaveCatalog(addedFolder2);

            Assert.IsFalse(assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == _asset1.FolderId)?.Name == _asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.AreEqual(_asset1.FileName, asset?.FileName);
            Assert.AreEqual(_asset1.FolderId, asset?.FolderId);
            // TODO: When ResolveDataDirectory has been removed from StorageService
            // // Before and after ?
            // Assert.AreEqual(0, asset?.FileProperties.Size);
            // Assert.AreEqual(DateTime.MinValue, asset?.FileProperties.Creation);
            // Assert.AreEqual(DateTime.MinValue, asset?.FileProperties.Modification);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _storageServiceMock!.Verify(x => x.UpdateAssetFileProperties(It.IsAny<Asset>()), Times.Exactly(1));
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasChanges_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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
    public void SaveCatalog_FolderIsNullAndHasChangesAndEverythingEmpty_SaveOperationsPerformed()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _assetRepository.DeleteFolder(addedFolder1);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_FolderIsNullAndHasNoChanges_DoesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            _assetRepository.SaveCatalog(null);

            Assert.IsFalse(_assetRepository.HasChanges());

            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveCatalog_ConcurrentAccess_EverythingIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.IsTrue(_assetRepository.HasChanges());
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

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

            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

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

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)));
            Assert.IsFalse(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)));

            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")));

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.AreEqual(2, folders.Count);
            Assert.IsTrue(folders.FirstOrDefault(x => x.FolderId == _asset1.FolderId)?.Name == _asset1.Folder.Name);

            Assert.AreEqual(1, assets.Count);
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.IsTrue(asset?.FileName == _asset1.FileName && asset.FolderId == _asset1.FolderId);

            Assert.AreEqual(2, syncAssetsDirectoriesDefinitions.Count);
            Assert.IsTrue(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"));

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
