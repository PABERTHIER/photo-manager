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
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            _assetRepository.SaveCatalog(addedFolder2);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.That(folders, Has.Count.EqualTo(2));
            Assert.That(folders.FirstOrDefault(x => x.Id == _asset1.FolderId)?.Name == _asset1.Folder.Name, Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.That(asset?.FileName == _asset1.FileName && asset.FolderId == _asset1.FolderId, Is.True);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"), Is.True);

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

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

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.SaveCatalog(addedFolder1);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            // New AssetRepository to test Initialize method with content in DB
            AssetRepository assetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);

            Assert.That(assetRepository.HasChanges(), Is.False);

            assetRepository.SaveCatalog(addedFolder2);

            Assert.That(assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.That(folders, Has.Count.EqualTo(2));
            Assert.That(folders.FirstOrDefault(x => x.Id == _asset1.FolderId)?.Name == _asset1.Folder.Name, Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.That(asset?.FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(asset?.FolderId, Is.EqualTo(_asset1.FolderId));
            // TODO: When ResolveDataDirectory has been removed from StorageService
            // // Before and after ?
            //Assert.That(asset?.FileProperties.Size, Is.EqualTo(0));
            //Assert.That(asset?.FileProperties.Creation, Is.EqualTo(DateTime.MinValue));
            //Assert.That(asset?.FileProperties.Modification, Is.EqualTo(DateTime.MinValue));

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"), Is.True);

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

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

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
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

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
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
            Assert.That(_assetRepository!.HasChanges(), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
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

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository.SaveCatalog(addedFolder1),
                () => _assetRepository.SaveCatalog(addedFolder2)
            );

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            List<Folder> folders = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
            List<Asset> assets = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
                _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
            List<string> recentTargetPaths = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

            Assert.That(folders, Has.Count.EqualTo(2));
            Assert.That(folders.FirstOrDefault(x => x.Id == _asset1.FolderId)?.Name == _asset1.Folder.Name, Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Asset? asset = assets.FirstOrDefault(x => x.Hash == _asset1.Hash);
            Assert.That(asset?.FileName == _asset1.FileName && asset.FolderId == _asset1.FolderId, Is.True);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"), Is.True);

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
