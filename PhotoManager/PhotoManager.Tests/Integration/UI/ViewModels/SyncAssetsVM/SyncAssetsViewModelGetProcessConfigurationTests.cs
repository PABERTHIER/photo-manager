using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.SyncAssetsVM;

[TestFixture]
public class SyncAssetsViewModelGetProcessConfigurationTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private SyncAssetsViewModel? _syncAssetsViewModel;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        AssetRepository assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (assetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new (assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _syncAssetsViewModel = new (application);
    }

    [Test]
    public void GetProcessConfiguration_SyncAssetsConfiguration_ReturnsSyncAssetsConfiguration()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetProcessConfiguration_NoSyncAssetsConfiguration_ReturnsEmptyList()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

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
    public void GetProcessConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<SyncAssetsViewModel> syncAssetsViewModelInstances = [];

        _syncAssetsViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
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
