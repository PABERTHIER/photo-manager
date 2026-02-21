using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.ViewModels.SyncAssetsVM;

[TestFixture]
public class SyncAssetsViewModelSetProcessConfigurationTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private string? _syncAssetsDirectoriesDefinitionsTableName;

    private SyncAssetsViewModel? _syncAssetsViewModel;
    private Database? _database;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureSyncAssetsViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        _syncAssetsDirectoriesDefinitionsTableName = userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName;

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        AssetRepository assetRepository = new(_database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(assetRepository, fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(assetRepository, syncAssetsService, catalogAssetsService,
            moveAssetsService, findDuplicatedAssetsService, userConfigurationService, fileOperationsService,
            imageProcessingService);
        _syncAssetsViewModel = new(application);
    }

    [Test]
    public void SetProcessConfiguration_ValidDefinitions_SavesConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            // Different source and destination
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });
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

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(16));

            AssertValidConfiguration(syncAssetsConfiguration.Definitions);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(16));

            AssertValidConfiguration(syncAssetsDirectoriesDefinitions);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("C:\\Valid1\\Path", "C:\\Valid2\\Path", "C:\\Valid1\\Path", "C:\\Valid2\\Path")]
    [TestCase("\\Server\\Valid1\\Path", "\\Server\\Valid2\\Path", "\\Server\\Valid1\\Path", "\\Server\\Valid2\\Path")]
    [TestCase("C:\\Valid1\\Path", "\\Server\\Valid2\\Path", "C:\\Valid1\\Path", "\\Server\\Valid2\\Path")]
    [TestCase("\\Server\\Valid1\\Path", "C:\\Valid2\\Path", "\\Server\\Valid1\\Path", "C:\\Valid2\\Path")]
    [TestCase(@"C:\Some\\Extra\Backslashes1", @"\\Remote\With\\\\Extra\Backslashes2", "C:\\Some\\Extra\\Backslashes1", "\\Remote\\With\\Extra\\Backslashes2")]
    [TestCase(@"\\Remote\With\\\\Extra\Backslashes1", @"C:\Some\\Extra\Backslashes2", "\\Remote\\With\\Extra\\Backslashes1", "C:\\Some\\Extra\\Backslashes2")]
    public void SetProcessConfiguration_SameValidDefinitions_SavesConfigurationWithDuplicateDefinitions(
        string sourceDirectory,
        string destinationDirectory,
        string expectedSourceDirectory,
        string expectedDestinationDirectory)
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

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

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(5));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[2].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[2].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource, Is.True);

            Assert.That(syncAssetsConfiguration.Definitions[3].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[3].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[3].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource, Is.True);

            Assert.That(syncAssetsConfiguration.Definitions[4].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[4].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsConfiguration.Definitions[4].IncludeSubFolders, Is.True);
            Assert.That(syncAssetsConfiguration.Definitions[4].DeleteAssetsNotInSource, Is.False);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(5));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[2].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[2].IncludeSubFolders, Is.True);
            Assert.That(syncAssetsDirectoriesDefinitions[2].DeleteAssetsNotInSource, Is.True);

            Assert.That(syncAssetsDirectoriesDefinitions[3].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[3].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[3].DeleteAssetsNotInSource, Is.True);

            Assert.That(syncAssetsDirectoriesDefinitions[4].SourceDirectory, Is.EqualTo(expectedSourceDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[4].DestinationDirectory, Is.EqualTo(expectedDestinationDirectory));
            Assert.That(syncAssetsDirectoriesDefinitions[4].IncludeSubFolders, Is.True);
            Assert.That(syncAssetsDirectoriesDefinitions[4].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_SomeDefinitionsAreValidAndSomeInvalid_SavesSomeConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "Invalid2\\Path" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = @"C:\Some\\Extra\Backslashes1", DestinationDirectory = "C:\\Valid2\\Path" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes" });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(4));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[2].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsConfiguration.Definitions[2].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[2].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[3].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsConfiguration.Definitions[3].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
            Assert.That(syncAssetsConfiguration.Definitions[3].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource, Is.False);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(4));
            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[2].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
            Assert.That(syncAssetsDirectoriesDefinitions[2].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[2].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[2].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[3].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
            Assert.That(syncAssetsDirectoriesDefinitions[3].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
            Assert.That(syncAssetsDirectoriesDefinitions[3].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[3].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_SyncAssetsConfigurationWithDifferentConfigurations_SavesConfigurationAndEraseThePreviousOne()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Tutu\\Screenshots", DestinationDirectory = "C:\\Images\\Tutu" });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.False);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.False);

            syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });

            // New save
            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.False);

            syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(1));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.False);

            // New empty save
            _syncAssetsViewModel!.SetProcessConfiguration(new());

            syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_SyncAssetsConfigurationHasNoDefinition_SavesNoConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            _syncAssetsViewModel!.SetProcessConfiguration(new());

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_DefinitionsAreInvalid_SavesNoConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

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

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_DefinitionsWithNullPathsForSourceOrDestination_ThrowsArgumentNullExceptionAndDoesNotSaveConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

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

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'input')"));

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetProcessConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();

            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Toto\\Screenshots", DestinationDirectory = "C:\\Images\\Toto" });
            syncAssetsConfigurationToSave.Definitions.Add(new() { SourceDirectory = "C:\\Tutu\\Screenshots", DestinationDirectory = "C:\\Images\\Tutu" });

            // Simulate concurrent access
            Parallel.Invoke(
                () => _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave),
                () => _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave),
                () => _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave)
            );

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.False);

            List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions = _database!.ReadObjectList(
                _syncAssetsDirectoriesDefinitionsTableName!,
                SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);

            Assert.That(syncAssetsDirectoriesDefinitions, Has.Count.EqualTo(2));

            Assert.That(syncAssetsDirectoriesDefinitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinitions[0].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(syncAssetsDirectoriesDefinitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
            Assert.That(syncAssetsDirectoriesDefinitions[1].IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinitions[1].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private static void AssertValidConfiguration(List<SyncAssetsDirectoriesDefinition> definitions)
    {
        Assert.That(definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[0].IncludeSubFolders, Is.False);
        Assert.That(definitions[0].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(definitions[1].IncludeSubFolders, Is.False);
        Assert.That(definitions[1].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[2].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[2].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(definitions[2].IncludeSubFolders, Is.False);
        Assert.That(definitions[2].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[3].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[3].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[3].IncludeSubFolders, Is.True);
        Assert.That(definitions[3].DeleteAssetsNotInSource, Is.True);

        Assert.That(definitions[4].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[4].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[4].IncludeSubFolders, Is.False);
        Assert.That(definitions[4].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[5].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[5].DestinationDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(definitions[5].IncludeSubFolders, Is.False);
        Assert.That(definitions[5].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[6].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[6].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[6].IncludeSubFolders, Is.False);
        Assert.That(definitions[6].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[7].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[7].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[7].IncludeSubFolders, Is.False);
        Assert.That(definitions[7].DeleteAssetsNotInSource, Is.True);

        Assert.That(definitions[8].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[8].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(definitions[8].IncludeSubFolders, Is.False);
        Assert.That(definitions[8].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[9].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[9].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[9].IncludeSubFolders, Is.False);
        Assert.That(definitions[9].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[10].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[10].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[10].IncludeSubFolders, Is.True);
        Assert.That(definitions[10].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[11].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[11].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[11].IncludeSubFolders, Is.False);
        Assert.That(definitions[11].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[12].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(definitions[12].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(definitions[12].IncludeSubFolders, Is.False);
        Assert.That(definitions[12].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[13].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes1"));
        Assert.That(definitions[13].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
        Assert.That(definitions[13].IncludeSubFolders, Is.False);
        Assert.That(definitions[13].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[14].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes1"));
        Assert.That(definitions[14].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes2"));
        Assert.That(definitions[14].IncludeSubFolders, Is.False);
        Assert.That(definitions[14].DeleteAssetsNotInSource, Is.False);

        Assert.That(definitions[15].SourceDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes1"));
        Assert.That(definitions[15].DestinationDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes2"));
        Assert.That(definitions[15].IncludeSubFolders, Is.False);
        Assert.That(definitions[15].DeleteAssetsNotInSource, Is.False);
    }

    private (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<SyncAssetsViewModel> syncAssetsViewModelInstances = [];

        _syncAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            syncAssetsViewModelInstances.Add((SyncAssetsViewModel)sender!);
        };

        return (notifyPropertyChangedEvents, syncAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        // From SyncAssetsViewModel
        Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

        // From BaseProcessViewModel
        Assert.That(_syncAssetsViewModel!.ProcessStatusMessages, Is.Empty);
        Assert.That(_syncAssetsViewModel!.Results, Is.Empty);
        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.ViewDescription));
        Assert.That(_syncAssetsViewModel!.DescriptionVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_syncAssetsViewModel!.ConfigurationVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.RunVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.ResultsVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.CanViewDescription, Is.True);
        Assert.That(_syncAssetsViewModel!.CanConfigure, Is.False);
        Assert.That(_syncAssetsViewModel!.CanViewResults, Is.False);
    }

    private static void CheckAfterChanges(
        SyncAssetsViewModel syncAssetsViewModelInstance,
        List<SyncAssetsDirectoriesDefinition> definitions)
    {
        // From SyncAssetsViewModel
        Assert.That(syncAssetsViewModelInstance.Definitions, Has.Count.EqualTo(definitions.Count));

        for (int i = 0; i < definitions.Count; i++)
        {
            Assert.That(syncAssetsViewModelInstance.Definitions[i].SourceDirectory, Is.EqualTo(definitions[i].SourceDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DestinationDirectory, Is.EqualTo(definitions[i].DestinationDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].IncludeSubFolders, Is.EqualTo(definitions[i].IncludeSubFolders));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DeleteAssetsNotInSource, Is.EqualTo(definitions[i].DeleteAssetsNotInSource));
        }

        // From BaseProcessViewModel
        Assert.That(syncAssetsViewModelInstance.ProcessStatusMessages, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Results, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Step, Is.EqualTo(ProcessStep.ViewDescription));
        Assert.That(syncAssetsViewModelInstance.DescriptionVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(syncAssetsViewModelInstance.ConfigurationVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.RunVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.ResultsVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.CanViewDescription, Is.True);
        Assert.That(syncAssetsViewModelInstance.CanConfigure, Is.False);
        Assert.That(syncAssetsViewModelInstance.CanViewResults, Is.False);
    }

    private static void CheckInstance(
        List<SyncAssetsViewModel> syncAssetsViewModelInstances,
        List<SyncAssetsDirectoriesDefinition> definitions)
    {
        int syncAssetsViewModelInstancesCount = syncAssetsViewModelInstances.Count;

        if (syncAssetsViewModelInstancesCount > 1)
        {
            Assert.That(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 2],
                Is.EqualTo(syncAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 1],
                Is.EqualTo(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 2]));
        }

        if (syncAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(syncAssetsViewModelInstances[0], definitions);
        }
    }
}
