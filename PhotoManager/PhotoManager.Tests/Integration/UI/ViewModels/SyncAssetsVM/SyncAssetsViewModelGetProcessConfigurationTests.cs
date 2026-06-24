using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.ViewModels.SyncAssetsVM;

[TestFixture]
public class SyncAssetsViewModelGetProcessConfigurationTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private SyncAssetsViewModel? _syncAssetsViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureSyncAssetsViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ImageMagickThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);
        _syncAssetsViewModel = new(application);
    }

    [Test]
    public void GetProcessConfiguration_SyncAssetsConfiguration_ReturnsSyncAssetsConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
        syncAssetsConfigurationToSave.Definitions.Add(
            new()
            {
                SourceDirectory = "C:\\Toto\\Screenshots",
                DestinationDirectory = "C:\\Images\\Toto",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            });
        syncAssetsConfigurationToSave.Definitions.Add(
            new()
            {
                SourceDirectory = "C:\\Tutu\\Screenshots",
                DestinationDirectory = "C:\\Images\\Tutu",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            });

        _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

        SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

        Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        CheckAfterChanges(_syncAssetsViewModel, []);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(syncAssetsViewModelInstances, []);
    }

    [Test]
    public void GetProcessConfiguration_NoSyncAssetsConfiguration_ReturnsEmptyList()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

        Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

        CheckAfterChanges(_syncAssetsViewModel, []);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(syncAssetsViewModelInstances, []);
    }

    [Test]
    public void GetProcessConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
        syncAssetsConfigurationToSave.Definitions.Add(
            new()
            {
                SourceDirectory = "C:\\Toto\\Screenshots",
                DestinationDirectory = "C:\\Images\\Toto",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            });
        syncAssetsConfigurationToSave.Definitions.Add(
            new()
            {
                SourceDirectory = "C:\\Tutu\\Screenshots",
                DestinationDirectory = "C:\\Images\\Tutu",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            });

        _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfigurationToSave);

        SyncAssetsConfiguration syncAssetsConfiguration1 = new();
        SyncAssetsConfiguration syncAssetsConfiguration2 = new();
        SyncAssetsConfiguration syncAssetsConfiguration3 = new();

        // Simulate concurrent access
        Parallel.Invoke(
            () => syncAssetsConfiguration1 = _syncAssetsViewModel!.GetProcessConfiguration(),
            () => syncAssetsConfiguration2 = _syncAssetsViewModel!.GetProcessConfiguration(),
            () => syncAssetsConfiguration3 = _syncAssetsViewModel!.GetProcessConfiguration()
        );

        Assert.That(syncAssetsConfiguration1.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration1.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration1.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration1.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration1.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        Assert.That(syncAssetsConfiguration2.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration2.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration2.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration2.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration2.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        Assert.That(syncAssetsConfiguration3.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration3.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration3.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration3.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration3.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        CheckAfterChanges(_syncAssetsViewModel, []);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(syncAssetsViewModelInstances, []);
    }

    private (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances)
        NotifyPropertyChangedEvents()
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
        Assert.That(_syncAssetsViewModel!.IsDescriptionVisible, Is.True);
        Assert.That(_syncAssetsViewModel!.IsConfigurationVisible, Is.False);
        Assert.That(_syncAssetsViewModel!.IsRunVisible, Is.False);
        Assert.That(_syncAssetsViewModel!.IsResultsVisible, Is.False);
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
            Assert.That(syncAssetsViewModelInstance.Definitions[i].SourceDirectory,
                Is.EqualTo(definitions[i].SourceDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DestinationDirectory,
                Is.EqualTo(definitions[i].DestinationDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].IncludeSubFolders,
                Is.EqualTo(definitions[i].IncludeSubFolders));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DeleteAssetsNotInSource,
                Is.EqualTo(definitions[i].DeleteAssetsNotInSource));
        }

        // From BaseProcessViewModel
        Assert.That(syncAssetsViewModelInstance.ProcessStatusMessages, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Results, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Step, Is.EqualTo(ProcessStep.ViewDescription));
        Assert.That(syncAssetsViewModelInstance.IsDescriptionVisible, Is.True);
        Assert.That(syncAssetsViewModelInstance.IsConfigurationVisible, Is.False);
        Assert.That(syncAssetsViewModelInstance.IsRunVisible, Is.False);
        Assert.That(syncAssetsViewModelInstance.IsResultsVisible, Is.False);
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
