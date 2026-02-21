using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Reactive = System.Reactive;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveCatalogTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private ImageProcessingService? _imageProcessingService;
    private ImageMetadataService? _imageMetadataService;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new(_configurationRootMock!.Object);
        _imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        _imageMetadataService = new(fileOperationsService);
        _assetRepository = new(_database, _pathProviderServiceMock!.Object, _imageProcessingService,
            _imageMetadataService, _userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_JPG,
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
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

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
            // Because not stored
            Assert.That(asset?.FileProperties.Size, Is.Zero);
            Assert.That(asset?.FileProperties.Creation, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset?.FileProperties.Modification, Is.EqualTo(DateTime.MinValue));

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"), Is.True);

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string folderPath1 = _dataDirectory!;
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);
            string filePath = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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
            AssetRepository assetRepository = new(_database!, _pathProviderServiceMock!.Object,
                _imageProcessingService!, _imageMetadataService!, _userConfigurationService);

            Assert.That(assetRepository.HasChanges(), Is.False);

            assetRepository.SaveCatalog(addedFolder2);

            Assert.That(assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

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
            // Because not stored
            Assert.That(asset?.FileProperties.Size, Is.Zero);
            Assert.That(asset?.FileProperties.Creation, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset?.FileProperties.Modification, Is.EqualTo(DateTime.MinValue));

            // Loaded at init
            Asset[] assetsFromRepositoryByPath = assetRepository.GetAssetsByPath(folderPath1);
            Assert.That(assetsFromRepositoryByPath, Has.Length.EqualTo(1));
            Assert.That(assetsFromRepositoryByPath[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assetsFromRepositoryByPath[0].FolderId, Is.EqualTo(_asset1.FolderId));
            Assert.That(assetsFromRepositoryByPath[0].FileProperties.Size, Is.EqualTo(_asset1.FileProperties.Size));
            Assert.That(assetsFromRepositoryByPath[0].FileProperties.Creation.Date,
                Is.EqualTo(_asset1.FileProperties.Creation.Date));
            Assert.That(assetsFromRepositoryByPath[0].FileProperties.Modification.Date,
                Is.EqualTo(_asset1.FileProperties.Modification.Date));

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsDirectoriesDefinitions.Any(x => x.SourceDirectory == "C:\\Toto\\Screenshots"), Is.True);

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _assetRepository.DeleteFolder(addedFolder1);

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            _assetRepository.SaveCatalog(null);

            Assert.That(_assetRepository.HasChanges(), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            _assetRepository!.AddAsset(_asset1!, []);

            Assert.That(_assetRepository.HasChanges(), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

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

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder2.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

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
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
