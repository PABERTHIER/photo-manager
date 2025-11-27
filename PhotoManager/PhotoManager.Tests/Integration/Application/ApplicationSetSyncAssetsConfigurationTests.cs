using Reactive = System.Reactive;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationSetSyncAssetsConfigurationTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        _application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
    }

    [Test]
    public async Task SetSyncAssetsConfiguration_CataloguedAssetsAndValidDefinitions_SavesConfiguration()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string asset1FileName = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string asset2FileName = FileNames.IMAGE_9_PNG;
            const string asset3FileName = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string asset4FileName = FileNames.IMAGE_11_HEIC;

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = @"C:\Some\\Extra\Backslashes1",
                DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes2",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = true
            });

            await _application!.CatalogAssetsAsync(_ => {});

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset1FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset1FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset2FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset2FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset3FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset3FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset4FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset4FileName], Is.Not.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(true));

            // True because the catalog has been saved during the CatalogAssetsAsync
            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Has.Length.EqualTo(1));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.EqualTo(true));

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(asset1FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(asset3FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(asset4FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset1FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset1FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset2FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset2FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset3FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset3FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(asset4FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][asset4FileName], Is.Not.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_ValidDefinitions_SavesConfiguration()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            // Different source and destination
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path"});
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = "\\Server\\Valid1\\Path",
                DestinationDirectory = "C:\\Valid2\\Path",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = true
            });

            // Same source and destination
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes1", DestinationDirectory = @"C:\Some\\Extra\Backslashes1" });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes",
                DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes",
                DeleteAssetsNotInSource = true
            });

            // Different source and destination not normalized
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes1", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = @"C:\Some\\Extra\Backslashes2" });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = @"C:\Some\\Extra\Backslashes1",
                DestinationDirectory = @"C:\Some\\Extra\Backslashes2",
                IncludeSubFolders = true
            });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes", DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes1", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes2" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes1", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes2" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes1", DestinationDirectory = @"C:\Some\\Extra\Backslashes2" });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(16));

            AssertValidConfiguration(syncAssetsConfiguration.Definitions);

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(16));

            AssertValidConfiguration(syncAssetsDirectoriesDefinitions);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    [TestCase("C:\\Valid1\\Path", "C:\\Valid2\\Path", "C:\\Valid1\\Path", "C:\\Valid2\\Path")]
    [TestCase("\\Server\\Valid1\\Path", "\\Server\\Valid2\\Path", "\\Server\\Valid1\\Path", "\\Server\\Valid2\\Path")]
    [TestCase("C:\\Valid1\\Path", "\\Server\\Valid2\\Path", "C:\\Valid1\\Path", "\\Server\\Valid2\\Path")]
    [TestCase("\\Server\\Valid1\\Path", "C:\\Valid2\\Path", "\\Server\\Valid1\\Path", "C:\\Valid2\\Path")]
    [TestCase(@"C:\Some\\Extra\Backslashes1", @"\\Remote\With\\\\Extra\Backslashes2", "C:\\Some\\Extra\\Backslashes1", "\\Remote\\With\\Extra\\Backslashes2")]
    [TestCase(@"\\Remote\With\\\\Extra\Backslashes1", @"C:\Some\\Extra\Backslashes2", "\\Remote\\With\\Extra\\Backslashes1", "C:\\Some\\Extra\\Backslashes2")]
    public void SetSyncAssetsConfiguration_SameValidDefinitions_SavesConfigurationWithDuplicateDefinitions(
        string sourceDirectory,
        string destinationDirectory,
        string expectedSourceDirectory,
        string expectedDestinationDirectory)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = sourceDirectory,
                DestinationDirectory = destinationDirectory,
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = true
            });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = sourceDirectory,
                DestinationDirectory = destinationDirectory,
                DeleteAssetsNotInSource = true
            });
            syncAssetsConfigurationToSave.Definitions.Add(new()
            {
                SourceDirectory = sourceDirectory,
                DestinationDirectory = destinationDirectory,
                IncludeSubFolders = true
            });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(5));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[2].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[2].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[2].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource, Is.EqualTo(true));

            Assert.That(syncAssetsConfiguration.Definitions[3].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[3].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[3].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource, Is.EqualTo(true));

            Assert.That(syncAssetsConfiguration.Definitions[4].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[4].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[4].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsConfiguration.Definitions[4].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(5));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[2].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[2].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DeleteAssetsNotInSource, Is.EqualTo(true));

            Assert.That(syncAssetsDirectoriesDefinitions[3].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[3].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DeleteAssetsNotInSource, Is.EqualTo(true));

            Assert.That(syncAssetsDirectoriesDefinitions[4].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[4].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[4].IncludeSubFolders, Is.EqualTo(true));
            Assert.That(syncAssetsDirectoriesDefinitions[4].DeleteAssetsNotInSource, Is.EqualTo(false));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_SomeDefinitionsAreValidAndSomeInvalid_SavesSomeConfiguration()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes1", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes" });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(4));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[2].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsConfiguration.Definitions[2].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[2].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[3].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[3].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
            Assert.That(syncAssetsConfiguration.Definitions[3].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(4));
            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[2].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[2].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[3].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
            Assert.That(syncAssetsDirectoriesDefinitions[3].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DeleteAssetsNotInSource, Is.EqualTo(false));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_SyncAssetsConfigurationWithDifferentConfigurations_SavesConfigurationAndEraseThePreviousOne()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Tutu\\Screenshots", DestinationDirectory = "C:\\Images\\Tutu" });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });

            // New save
            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(1));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            // New empty save
            _application!.SetSyncAssetsConfiguration(new());

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_SyncAssetsConfigurationHasNoDefinition_SavesNoConfiguration()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(new());

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_DefinitionsAreInvalid_SavesNoConfiguration()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "Invalid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "https://www.some-site.com", DestinationDirectory = "ftp://some-location.com" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid@Value.com", DestinationDirectory = "Invalid@Value.com" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = " " });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = " ", DestinationDirectory = "" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = null! });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = @"C:\Some\\Extra\Backslashes" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes", DestinationDirectory = "Invalid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes", DestinationDirectory = "Invalid1\\Path" });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_DefinitionsWithNullPathsForSourceOrDestination_ThrowsArgumentNullExceptionAndDoesNotSaveConfiguration()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = null!, DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = null! });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = null!, DestinationDirectory = "\\Server\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = null! });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = null!, DestinationDirectory = @"C:\Some\\Extra\Backslashes" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes", DestinationDirectory = null! });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = null!, DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"\\Remote\With\\\\Extra\Backslashes", DestinationDirectory = null! });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = null!, DestinationDirectory = null! });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'input')"));

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SetSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Tutu\\Screenshots", DestinationDirectory = "C:\\Images\\Tutu" });

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.False);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _application!.SetSyncAssetsConfiguration(syncAssetsConfigurationToSave)
            );

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(Directory.GetFiles(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)), Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.EqualTo(false));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    private static void AssertValidConfiguration(List<SyncAssetsDirectoriesDefinition> definitions)
    {
        Assert.That(definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[2].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[2].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(definitions[2].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[2].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[3].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[3].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[3].IncludeSubFolders, Is.EqualTo(true));
        Assert.That(definitions[3].DeleteAssetsNotInSource, Is.EqualTo(true));

        Assert.That(definitions[4].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[4].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[4].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[4].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[5].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[5].DestinationDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[5].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[5].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[6].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[6].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[6].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[6].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[7].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[7].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[7].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[7].DeleteAssetsNotInSource, Is.EqualTo(true));

        Assert.That(definitions[8].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[8].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[8].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[8].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[9].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[9].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[9].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[9].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[10].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[10].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[10].IncludeSubFolders, Is.EqualTo(true));
        Assert.That(definitions[10].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[11].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[11].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[11].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[11].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[12].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[12].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[12].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[12].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[13].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes1"));
        Assert.That(definitions[13].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
        Assert.That(definitions[13].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[13].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[14].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[14].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
        Assert.That(definitions[14].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[14].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(definitions[15].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes1"));
        Assert.That(definitions[15].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[15].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(definitions[15].DeleteAssetsNotInSource, Is.EqualTo(false));
    }
}
