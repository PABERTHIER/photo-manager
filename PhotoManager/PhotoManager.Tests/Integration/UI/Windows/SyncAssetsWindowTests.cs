using Avalonia.Controls;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.UI.ViewModels.Enums;
using PhotoManager.UI.Windows;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.UI.Windows;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class SyncAssetsWindowTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private SyncAssetsViewModel? _syncAssetsViewModel;
    private MoveAssetsService? _moveAssetsService;
    private FileOperationsService? _fileOperationsService;
    private TestableAssetRepository? _testableAssetRepository;
    private SyncAssetsWindow? _window;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        if (_window != null)
        {
            AvaloniaTestSetup.RunOnUiThreadAsync(() => _window.Close()).GetAwaiter().GetResult();
        }

        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);

        _window = null;
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
        _fileOperationsService = new(userConfigurationService, new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(_fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, _fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(_fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, _fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        _moveAssetsService = new(_testableAssetRepository, _fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, _fileOperationsService, assetsComparator,
            _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, _fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(_fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, _moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            _fileOperationsService, imageProcessingService, assetConversionService);
        _syncAssetsViewModel = new(application);
    }

    [Test]
    public void Initialize_HasConfiguration_NotifiesAndSetsDefinitions()
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

        _syncAssetsViewModel!.Definitions = [.. syncAssetsConfiguration.Definitions];

        CheckAfterChanges(
            _syncAssetsViewModel!,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.ViewDescription,
            true,
            false,
            false,
            false,
            true,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

        CheckInstance(
            syncAssetsViewModelInstances,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.ViewDescription,
            true,
            false,
            false,
            false,
            true,
            false,
            false);
    }

    [Test]
    public void Initialize_HasNoConfiguration_NotifiesAndSetsEmptyDefinitions()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

        Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);

        _syncAssetsViewModel!.Definitions = [.. syncAssetsConfiguration.Definitions];

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.ViewDescription,
            true,
            false,
            false,
            false,
            true,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.ViewDescription,
            true,
            false,
            false,
            false,
            true,
            false,
            false);
    }

    [Test]
    public void ContinueButton_FromDescription_NotifiesAndSetsStepToConfigure()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Configure));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);
    }

    [Test]
    public void SaveButton_FromConfigure_NotifiesAndSavesDefinitions()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Configure));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

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

        _syncAssetsViewModel!.Definitions = [.. syncAssetsConfigurationToSave.Definitions];

        SyncAssetsConfiguration configuration = new();
        configuration.Definitions.AddRange(_syncAssetsViewModel!.Definitions);
        _syncAssetsViewModel!.SetProcessConfiguration(configuration);

        SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

        Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));

        CheckInstance(
            syncAssetsViewModelInstances,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);
    }

    [Test]
    public void ContinueButton_FromConfigure_NotifiesAndSetsToRun()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Configure));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

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

        _syncAssetsViewModel!.Definitions = [.. syncAssetsConfigurationToSave.Definitions];

        SyncAssetsConfiguration configuration = new();
        configuration.Definitions.AddRange(_syncAssetsViewModel!.Definitions);
        _syncAssetsViewModel!.SetProcessConfiguration(configuration);

        SyncAssetsConfiguration syncAssetsConfiguration = _syncAssetsViewModel!.GetProcessConfiguration();

        Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
        Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));

        CheckInstance(
            syncAssetsViewModelInstances,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Run));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Run,
            false,
            false,
            true,
            false,
            false,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            syncAssetsConfiguration.Definitions,
            [],
            [],
            ProcessStep.Run,
            false,
            false,
            true,
            false,
            false,
            false,
            false);
    }

    [Test]
    public async Task RunButton_FromRun_NotifiesAndSetsStepToViewResults()
    {
        string sourceDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.PART);
        string destinationDirectory = Path.Combine(_assetsDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;
            const string assetName5 = FileNames._1336_BOTTOM_LEFT_PART_JPG;
            const string assetName6 = FileNames._1336_TOP_RIGHT_PART_JPG;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetSourceToCopySubDirectory1Path = Path.Combine(sourceToCopy, assetName5);
            string assetSourceToCopySubDirectory2Path = Path.Combine(sourceToCopy, assetName6);
            string assetDestinationToCopySubDirectory1Path = Path.Combine(sourceSubDirectory1, assetName5);
            string assetDestinationToCopySubDirectory2Path = Path.Combine(sourceSubDirectory2, assetName6);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);
            string assetDestinationPath5 = Path.Combine(destinationDirectory, assetName5);
            string assetDestinationPath6 = Path.Combine(destinationDirectory, assetName6);

            string assetDestinationSubDirectory1Path = Path.Combine(destinationSubDirectory1, assetName5);
            string assetDestinationSubDirectory2Path = Path.Combine(destinationSubDirectory2, assetName6);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path,
                assetDestinationToCopySubDirectory1Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path,
                assetDestinationToCopySubDirectory2Path);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory2 = _fileOperationsService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory2), Is.False);

            _syncAssetsViewModel!.AdvanceStep();

            Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Configure));

            CheckAfterChanges(
                _syncAssetsViewModel!,
                [],
                [],
                [],
                ProcessStep.Configure,
                false,
                true,
                false,
                false,
                false,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));

            CheckInstance(
                syncAssetsViewModelInstances,
                [],
                [],
                [],
                ProcessStep.Configure,
                false,
                true,
                false,
                false,
                false,
                true,
                false);

            SyncAssetsConfiguration syncAssetsConfiguration = new();
            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _syncAssetsViewModel!.Definitions = [.. syncAssetsConfiguration.Definitions];

            SyncAssetsConfiguration configuration = new();
            configuration.Definitions.AddRange(_syncAssetsViewModel!.Definitions);
            _syncAssetsViewModel!.SetProcessConfiguration(configuration);

            SyncAssetsConfiguration syncAssetsConfigurationFromRepository =
                _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions,
                Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory,
                Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory,
                Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders,
                Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource,
                Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            CheckAfterChanges(
                _syncAssetsViewModel!,
                syncAssetsConfigurationFromRepository.Definitions,
                [],
                [],
                ProcessStep.Configure,
                false,
                true,
                false,
                false,
                false,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));

            CheckInstance(
                syncAssetsViewModelInstances,
                syncAssetsConfigurationFromRepository.Definitions,
                [],
                [],
                ProcessStep.Configure,
                false,
                true,
                false,
                false,
                false,
                true,
                false);

            _syncAssetsViewModel!.AdvanceStep();

            Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Run));

            CheckAfterChanges(
                _syncAssetsViewModel!,
                syncAssetsConfigurationFromRepository.Definitions,
                [],
                [],
                ProcessStep.Run,
                false,
                false,
                true,
                false,
                false,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanConfigure"));

            CheckInstance(
                syncAssetsViewModelInstances,
                syncAssetsConfigurationFromRepository.Definitions,
                [],
                [],
                ProcessStep.Run,
                false,
                false,
                true,
                false,
                false,
                false,
                false);

            configuration = new();
            configuration.Definitions.AddRange(_syncAssetsViewModel!.Definitions);
            _syncAssetsViewModel!.SetProcessConfiguration(configuration);

            await _syncAssetsViewModel!.RunProcessAsync(_syncAssetsViewModel.NotifyProcessStatusChanged);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory2 = _fileOperationsService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 =
                _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory2 =
                _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName6), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path), Is.True);

            List<string> expectedProcessStatusMessages =
            [
                $"'{assetSourcePath2}' => '{assetDestinationPath2}'",
                $"'{assetSourcePath3}' => '{assetDestinationPath3}'",
                $"'{assetSourcePath4}' => '{assetDestinationPath4}'",
                $"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'",
                $"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"
            ];

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 3,
                    Message = $"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                },
                new()
                {
                    SourceDirectory = sourceSubDirectory1,
                    DestinationDirectory = destinationSubDirectory1,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."
                },
                new()
                {
                    SourceDirectory = sourceSubDirectory2,
                    DestinationDirectory = destinationSubDirectory2,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."
                }
            ];

            _syncAssetsViewModel!.AdvanceStep();

            Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.ViewResults));

            CheckAfterChanges(
                _syncAssetsViewModel!,
                syncAssetsConfigurationFromRepository.Definitions,
                expectedProcessStatusMessages,
                expectedResult,
                ProcessStep.ViewResults,
                false,
                false,
                false,
                true,
                false,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanConfigure"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CanViewResults"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("Step"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("IsDescriptionVisible"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("IsConfigurationVisible"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("IsRunVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("IsResultsVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CanConfigure"));

            CheckInstance(
                syncAssetsViewModelInstances,
                syncAssetsConfigurationFromRepository.Definitions,
                expectedProcessStatusMessages,
                expectedResult,
                ProcessStep.ViewResults,
                false,
                false,
                false,
                true,
                false,
                false,
                true);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
        }
    }

    [Test]
    public void AdvanceStep_AllStepsWithoutDefinitions_NotifiesAndSetsRightStep()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) =
            NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Configure));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.Configure,
            false,
            true,
            false,
            false,
            false,
            true,
            false);

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.Run));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.Run,
            false,
            false,
            true,
            false,
            false,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.Run,
            false,
            false,
            true,
            false,
            false,
            false,
            false);

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.ViewResults));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.ViewResults,
            false,
            false,
            false,
            true,
            false,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.ViewResults,
            false,
            false,
            false,
            true,
            false,
            false,
            false);

        _syncAssetsViewModel!.AdvanceStep();

        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.ViewResults));

        CheckAfterChanges(
            _syncAssetsViewModel!,
            [],
            [],
            [],
            ProcessStep.ViewResults,
            false,
            false,
            false,
            true,
            false,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(24));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CanConfigure"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("Step"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("IsDescriptionVisible"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("IsConfigurationVisible"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("IsRunVisible"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("IsResultsVisible"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CanConfigure"));

        CheckInstance(
            syncAssetsViewModelInstances,
            [],
            [],
            [],
            ProcessStep.ViewResults,
            false,
            false,
            false,
            true,
            false,
            false,
            false);
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

    [Test]
    public Task Constructor_SyncAssetsViewModel_SetsMaximizedWindowAndDataContext()
    {
        ConfigureSyncAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            _window = new(_syncAssetsViewModel!, NullLogger<SyncAssetsWindow>.Instance);

            Assert.That(_window.Title, Is.EqualTo("Sync Assets"));
            Assert.That(_window.Width, Is.EqualTo(900));
            Assert.That(_window.Height, Is.EqualTo(600));
            Assert.That(_window.WindowState, Is.EqualTo(WindowState.Maximized));
            Assert.That(_window.DataContext, Is.SameAs(_syncAssetsViewModel));
        });
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
        List<SyncAssetsDirectoriesDefinition> definitions,
        List<string> expectedProcessStatusMessages,
        List<SyncAssetsResult> expectedResult,
        ProcessStep expectedStep,
        bool expectedDescriptionVisible,
        bool expectedConfigurationVisible,
        bool expectedRunVisible,
        bool expectedResultsVisible,
        bool expectedCanViewDescription,
        bool expectedCanConfigure,
        bool expectedCanViewResults)
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
        Assert.That(syncAssetsViewModelInstance.ProcessStatusMessages,
            Has.Count.EqualTo(expectedProcessStatusMessages.Count));

        for (int i = 0; i < expectedProcessStatusMessages.Count; i++)
        {
            Assert.That(syncAssetsViewModelInstance.ProcessStatusMessages[i],
                Is.EqualTo(expectedProcessStatusMessages[i]));
        }

        Assert.That(syncAssetsViewModelInstance.Results, Has.Count.EqualTo(expectedResult.Count));

        for (int i = 0; i < expectedResult.Count; i++)
        {
            Assert.That(syncAssetsViewModelInstance.Results[i].SourceDirectory,
                Is.EqualTo(expectedResult[i].SourceDirectory));
            Assert.That(syncAssetsViewModelInstance.Results[i].DestinationDirectory,
                Is.EqualTo(expectedResult[i].DestinationDirectory));
            Assert.That(syncAssetsViewModelInstance.Results[i].SyncedImages,
                Is.EqualTo(expectedResult[i].SyncedImages));
            Assert.That(syncAssetsViewModelInstance.Results[i].Message, Is.EqualTo(expectedResult[i].Message));
        }

        Assert.That(syncAssetsViewModelInstance.Step, Is.EqualTo(expectedStep));
        Assert.That(syncAssetsViewModelInstance.IsDescriptionVisible, Is.EqualTo(expectedDescriptionVisible));
        Assert.That(syncAssetsViewModelInstance.IsConfigurationVisible, Is.EqualTo(expectedConfigurationVisible));
        Assert.That(syncAssetsViewModelInstance.IsRunVisible, Is.EqualTo(expectedRunVisible));
        Assert.That(syncAssetsViewModelInstance.IsResultsVisible, Is.EqualTo(expectedResultsVisible));
        Assert.That(syncAssetsViewModelInstance.CanViewDescription, Is.EqualTo(expectedCanViewDescription));
        Assert.That(syncAssetsViewModelInstance.CanConfigure, Is.EqualTo(expectedCanConfigure));
        Assert.That(syncAssetsViewModelInstance.CanViewResults, Is.EqualTo(expectedCanViewResults));
    }

    private static void CheckInstance(
        List<SyncAssetsViewModel> syncAssetsViewModelInstances,
        List<SyncAssetsDirectoriesDefinition> definitions,
        List<string> expectedProcessStatusMessages,
        List<SyncAssetsResult> expectedResult,
        ProcessStep expectedStep,
        bool expectedDescriptionVisible,
        bool expectedConfigurationVisible,
        bool expectedRunVisible,
        bool expectedResultsVisible,
        bool expectedCanViewDescription,
        bool expectedCanConfigure,
        bool expectedCanViewResults)
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
            CheckAfterChanges(
                syncAssetsViewModelInstances[0],
                definitions,
                expectedProcessStatusMessages,
                expectedResult,
                expectedStep,
                expectedDescriptionVisible,
                expectedConfigurationVisible,
                expectedRunVisible,
                expectedResultsVisible,
                expectedCanViewDescription,
                expectedCanConfigure,
                expectedCanViewResults);
        }
    }
}
