using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.UI.ViewModels.SyncAssetsVM;

[TestFixture]
public class SyncAssetsViewModelRunProcessAsyncTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private SyncAssetsViewModel? _syncAssetsViewModel;
    private MoveAssetsService? _moveAssetsService;
    private FileOperationsService? _fileOperationsService;
    private AssetRepository? _assetRepository;

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

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        _fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(_fileOperationsService);
        _assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, _fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, _fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        _moveAssetsService = new(_assetRepository, _fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, _fileOperationsService, assetsComparator, _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, _fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService,
            catalogAssetsService, _moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            _fileOperationsService, imageProcessingService);
        _syncAssetsViewModel = new(application);
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreEmpty_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_SYNC);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 0,
                    Message = $"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsNotEmptyAndDestinationIsEmpty_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 4,
                    Message = $"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsEmptyAndDestinationIsNotEmpty_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceToCopyPath = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceDirectory = Path.Combine(sourceToCopyPath, Directories.SOURCE);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetSourceToCopyPath1 = Path.Combine(sourceToCopyPath, assetName1);
            string assetSourceToCopyPath2 = Path.Combine(sourceToCopyPath, assetName2);
            string assetSourceToCopyPath3 = Path.Combine(sourceToCopyPath, assetName3);
            string assetSourceToCopyPath4 = Path.Combine(sourceToCopyPath, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopyPath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopyPath2, assetDestinationPath2);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Is.Empty);

            string[] fileNamesInSourceToCopy = _fileOperationsService!.GetFileNames(sourceToCopyPath);
            Assert.That(fileNamesInSourceToCopy, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Is.Empty);

            fileNamesInSourceToCopy = _fileOperationsService!.GetFileNames(sourceToCopyPath);
            Assert.That(fileNamesInSourceToCopy, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourceToCopyPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 0,
                    Message = $"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptyMultipleNewImages_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationPath3);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(2));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 2,
                    Message = $"2 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptyOneNewImage_NotifiesAndStatusChangesAndImageAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationPath3);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(3));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptyMultipleNewImagesAndTwoDefinitions_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string firstSourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string secondSourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}");
        string firstDestinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC_1);
        string secondDestinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(firstDestinationDirectory);
            Directory.CreateDirectory(secondDestinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            const string assetName5 = FileNames.IMAGE_1336_MINI_JPG;
            const string assetName6 = FileNames.IMAGE_1336_ORIGINAL_JPG;
            const string assetName7 = FileNames.IMAGE_1336_SHIT_QUALITY_JPG;
            const string assetName8 = FileNames.IMAGE_1336_SMALL_JPG;

            string assetSourcePath1 = Path.Combine(firstSourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(firstSourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(firstSourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(firstSourceDirectory, assetName4);

            string assetSourcePath5 = Path.Combine(secondSourceDirectory, assetName5);
            string assetSourcePath6 = Path.Combine(secondSourceDirectory, assetName6);
            string assetSourcePath7 = Path.Combine(secondSourceDirectory, assetName7);
            string assetSourcePath8 = Path.Combine(secondSourceDirectory, assetName8);

            string assetDestinationPath1 = Path.Combine(firstDestinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(firstDestinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(firstDestinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(firstDestinationDirectory, assetName4);

            string assetDestinationPath5 = Path.Combine(secondDestinationDirectory, assetName5);
            string assetDestinationPath6 = Path.Combine(secondDestinationDirectory, assetName6);
            string assetDestinationPath7 = Path.Combine(secondDestinationDirectory, assetName7);
            string assetDestinationPath8 = Path.Combine(secondDestinationDirectory, assetName8);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationPath3);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.That(assetHasBeenCopied, Is.True);

            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath6, assetDestinationPath6);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath8, assetDestinationPath8);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource1 = _fileOperationsService!.GetFileNames(firstSourceDirectory);
            Assert.That(fileNamesInSource1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSource2 = _fileOperationsService!.GetFileNames(secondSourceDirectory);
            Assert.That(fileNamesInSource2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName8), Is.True);

            string[] fileNamesInDestination1 = _fileOperationsService!.GetFileNames(firstDestinationDirectory);
            Assert.That(fileNamesInDestination1, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination2 = _fileOperationsService!.GetFileNames(secondDestinationDirectory);
            Assert.That(fileNamesInDestination2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName8), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath5), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath6), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath7), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath8), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath7), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath8), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = firstSourceDirectory, DestinationDirectory = firstDestinationDirectory });
            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = secondSourceDirectory, DestinationDirectory = secondDestinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[1].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[1].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[1].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[1].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[1].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[1].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource1 = _fileOperationsService!.GetFileNames(firstSourceDirectory);
            Assert.That(fileNamesInSource1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName4), Is.True);

            fileNamesInSource2 = _fileOperationsService!.GetFileNames(secondSourceDirectory);
            Assert.That(fileNamesInSource2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName8), Is.True);

            fileNamesInDestination1 = _fileOperationsService!.GetFileNames(firstDestinationDirectory);
            Assert.That(fileNamesInDestination1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName4), Is.True);

            fileNamesInDestination2 = _fileOperationsService!.GetFileNames(secondDestinationDirectory);
            Assert.That(fileNamesInDestination2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName8), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath5), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath6), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath7), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath8), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath7), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath7), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath5}' => '{assetDestinationPath5}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath7}' => '{assetDestinationPath7}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = firstSourceDirectory,
                    DestinationDirectory = firstDestinationDirectory,
                    SyncedImages = 2,
                    Message = $"2 images synced from '{firstSourceDirectory}' to '{firstDestinationDirectory}'."
                },
                new()
                {
                    SourceDirectory = secondSourceDirectory,
                    DestinationDirectory = secondDestinationDirectory,
                    SyncedImages = 2,
                    Message = $"2 images synced from '{secondSourceDirectory}' to '{secondDestinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(firstDestinationDirectory, true);
            Directory.Delete(secondDestinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreSingleLevel_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string assetDestinationSubDirectory1Path1 = Path.Combine(destinationSubDirectory1, assetName1);
            string assetDestinationSubDirectory1Path2 = Path.Combine(destinationSubDirectory1, assetName2);
            string assetDestinationSubDirectory1Path3 = Path.Combine(destinationSubDirectory1, assetName3);
            string assetDestinationSubDirectory1Path4 = Path.Combine(destinationSubDirectory1, assetName4);

            string assetDestinationSubDirectory2Path1 = Path.Combine(destinationSubDirectory2, assetName1);
            string assetDestinationSubDirectory2Path2 = Path.Combine(destinationSubDirectory2, assetName2);
            string assetDestinationSubDirectory2Path3 = Path.Combine(destinationSubDirectory2, assetName3);
            string assetDestinationSubDirectory2Path4 = Path.Combine(destinationSubDirectory2, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptyAllImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreSingleLevel_NotifiesAndStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string assetDestinationSubDirectory1Path1 = Path.Combine(destinationSubDirectory1, assetName1);
            string assetDestinationSubDirectory1Path2 = Path.Combine(destinationSubDirectory1, assetName2);
            string assetDestinationSubDirectory1Path3 = Path.Combine(destinationSubDirectory1, assetName3);
            string assetDestinationSubDirectory1Path4 = Path.Combine(destinationSubDirectory1, assetName4);

            string assetDestinationSubDirectory2Path1 = Path.Combine(destinationSubDirectory2, assetName1);
            string assetDestinationSubDirectory2Path2 = Path.Combine(destinationSubDirectory2, assetName2);
            string assetDestinationSubDirectory2Path3 = Path.Combine(destinationSubDirectory2, assetName3);
            string assetDestinationSubDirectory2Path4 = Path.Combine(destinationSubDirectory2, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationSubDirectory2Path4);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.True);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 0,
                    Message = $"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsFalse_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

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

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
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

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

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

            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory2), Is.False);

            Assert.That(statusChanges, Has.Count.EqualTo(3));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 3,
                    Message = $"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

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
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
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

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

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

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
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

            Assert.That(statusChanges, Has.Count.EqualTo(5));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));

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

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceAndDestinationSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

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
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationSubDirectory1Path);
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

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Is.Empty);

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
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

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

            fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
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

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));

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
                    SyncedImages = 0,
                    Message = $"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."
                },
                new()
                {
                    SourceDirectory = sourceSubDirectory2,
                    DestinationDirectory = destinationSubDirectory2,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreMultipleLevel_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, $"{Directories.SUB_DIRECTORY_2}\\{Directories.SUB_DIRECTORY_2}");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, $"{Directories.SUB_DIRECTORY_3}\\{Directories.SUB_DIRECTORY_4}\\{Directories.SUB_DIRECTORY_5}");

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string assetDestinationSubDirectory1Path1 = Path.Combine(destinationSubDirectory1, assetName1);
            string assetDestinationSubDirectory1Path2 = Path.Combine(destinationSubDirectory1, assetName2);
            string assetDestinationSubDirectory1Path3 = Path.Combine(destinationSubDirectory1, assetName3);
            string assetDestinationSubDirectory1Path4 = Path.Combine(destinationSubDirectory1, assetName4);

            string assetDestinationSubDirectory2Path1 = Path.Combine(destinationSubDirectory2, assetName1);
            string assetDestinationSubDirectory2Path2 = Path.Combine(destinationSubDirectory2, assetName2);
            string assetDestinationSubDirectory2Path3 = Path.Combine(destinationSubDirectory2, assetName3);
            string assetDestinationSubDirectory2Path4 = Path.Combine(destinationSubDirectory2, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreMultipleLevelAndIncludeSubFoldersIsTrue_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceSubDirectory1, Directories.SUB_DIRECTORY_2);
        string sourceSubDirectory3 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_3);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationSubDirectory1, Directories.SUB_DIRECTORY_2);
        string destinationSubDirectory3 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_3);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);
            Directory.CreateDirectory(sourceSubDirectory3);

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

            string assetSourceToCopySubDirectory2Path = Path.Combine(sourceToCopy, assetName5);
            string assetSourceToCopySubDirectory3Path = Path.Combine(sourceToCopy, assetName6);
            string assetDestinationToCopySubDirectory2Path = Path.Combine(sourceSubDirectory2, assetName5);
            string assetDestinationToCopySubDirectory3Path = Path.Combine(sourceSubDirectory3, assetName6);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);
            string assetDestinationPath5 = Path.Combine(destinationDirectory, assetName5);
            string assetDestinationPath6 = Path.Combine(destinationDirectory, assetName6);

            string assetDestinationSubDirectory2Path = Path.Combine(destinationSubDirectory2, assetName5);
            string assetDestinationSubDirectory3Path = Path.Combine(destinationSubDirectory3, assetName6);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory3Path, assetDestinationToCopySubDirectory3Path);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Is.Empty);

            string[] fileNamesInSourceSubDirectory2 = _fileOperationsService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory3 = _fileOperationsService!.GetFileNames(sourceSubDirectory3);
            Assert.That(fileNamesInSourceSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory3.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory3Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory2), Is.False);
            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory3), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Is.Empty);

            fileNamesInSourceSubDirectory2 = _fileOperationsService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory3 = _fileOperationsService!.GetFileNames(sourceSubDirectory3);
            Assert.That(fileNamesInSourceSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory3.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Is.Empty);

            string[] fileNamesInDestinationSubDirectory2 = _fileOperationsService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory3 = _fileOperationsService!.GetFileNames(destinationSubDirectory3);
            Assert.That(fileNamesInDestinationSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory3.Any(x => x == assetName6), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory3Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory2Path), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory3Path), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(5));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory3Path}' => '{assetDestinationSubDirectory3Path}'"));

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
                    SyncedImages = 0,
                    Message = $"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."
                },
                new()
                {
                    SourceDirectory = sourceSubDirectory2,
                    DestinationDirectory = destinationSubDirectory2,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."
                },
                new()
                {
                    SourceDirectory = sourceSubDirectory3,
                    DestinationDirectory = destinationSubDirectory3,
                    SyncedImages = 1,
                    Message = $"1 image synced from '{sourceSubDirectory3}' to '{destinationSubDirectory3}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory3, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndDuplicatedAssetInSourceAndIncludeSubFoldersIsTrue_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetSourceToCopySubDirectory1Path = Path.Combine(sourceDirectory, assetName2);
            string assetDestinationToCopySubDirectory1Path = Path.Combine(sourceSubDirectory1, assetName2);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string assetDestinationSubDirectory1Path = Path.Combine(destinationSubDirectory1, assetName2);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_fileOperationsService.FolderExists(destinationSubDirectory1), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _fileOperationsService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _fileOperationsService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationSubDirectory1Path), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'"));

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
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceAndDestinationAreNotEmptyBothContainDifferentImagesAndDeleteAssetsNotInSourceIsTrue_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}");

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            const string assetName5 = FileNames.IMAGE_1336_MINI_JPG;
            const string assetName6 = FileNames.IMAGE_1336_ORIGINAL_JPG;
            const string assetName7 = FileNames.IMAGE_1336_SHIT_QUALITY_JPG;
            const string assetName8 = FileNames.IMAGE_1336_SMALL_JPG;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);
            string assetDestinationPath5 = Path.Combine(destinationDirectory, assetName5);
            string assetDestinationPath6 = Path.Combine(destinationDirectory, assetName6);
            string assetDestinationPath7 = Path.Combine(destinationDirectory, assetName7);
            string assetDestinationPath8 = Path.Combine(destinationDirectory, assetName8);

            string assetDestinationToCopyPath1 = Path.Combine(destinationToCopy, assetName5);
            string assetDestinationToCopyPath2 = Path.Combine(destinationToCopy, assetName6);
            string assetDestinationToCopyPath3 = Path.Combine(destinationToCopy, assetName7);
            string assetDestinationToCopyPath4 = Path.Combine(destinationToCopy, assetName8);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath1, assetDestinationPath5);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath2, assetDestinationPath6);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath3, assetDestinationPath7);
            Assert.That(assetHasBeenCopied, Is.True);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath4, assetDestinationPath8);
            Assert.That(assetHasBeenCopied, Is.True);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName8), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath7), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath8), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    DeleteAssetsNotInSource = true,
                });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath6), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath7), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath8), Is.False);

            Assert.That(statusChanges, Has.Count.EqualTo(8));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath5}'"));
            Assert.That(statusChanges[5].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath6}'"));
            Assert.That(statusChanges[6].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath7}'"));
            Assert.That(statusChanges[7].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath8}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 8,
                    Message = $"8 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefinitionIsEmpty_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Is.Empty);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            CheckAfterChanges(_syncAssetsViewModel, [], []);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], []);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void RunProcessAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefinitionIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();
            SyncAssetsDirectoriesDefinition? definition = null;

            syncAssetsConfiguration.Definitions.Add(definition!);

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref in Validate and Normalize methods
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(1));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0], Is.Null);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add));
            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(_syncAssetsViewModel, [], []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, [], []);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationNotSaved_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Is.Empty);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            CheckAfterChanges(_syncAssetsViewModel, [], []);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], []);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void RunProcessAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration? syncAssetsConfiguration = null;

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration!);
            // _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref for Validate and Normalize methods
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository, Is.Null);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add));
            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(_syncAssetsViewModel, [], []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, [], []);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceDirectoryDoesNotExist_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_SYNC);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 0,
                    Message = $"Source directory '{sourceDirectory}' not found."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceDirectoryIsNull_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string? sourceDirectory = null;
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory!, DestinationDirectory = destinationDirectory });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref during regex verification
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory!,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 0,
                    Message = $"Source directory '{sourceDirectory}' not found."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsNotEmptyAndDestinationDirectoryDoesNotExist_NotifiesAndStatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FolderExists(destinationDirectory), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _fileOperationsService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    SyncedImages = 4,
                    Message = $"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'."
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task RunProcessAsync_SourceIsNotEmptyAndDestinationDirectoryIsNull_NotifiesAndNoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string? destinationDirectory = null;

        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string[] fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_fileOperationsService.FolderExists(destinationDirectory!), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory! });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _syncAssetsViewModel!.SetProcessConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref during regex verification
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _syncAssetsViewModel!.GetProcessConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            await _syncAssetsViewModel!.RunProcessAsync(statusChanges.Add);

            fileNamesInSource = _fileOperationsService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_fileOperationsService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_fileOperationsService.FileExists(assetSourcePath4), Is.True);

            Assert.That(statusChanges, Is.Empty);

            List<SyncAssetsResult> expectedResult =
            [
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory!,
                    SyncedImages = 0,
                    Message = "Value cannot be null. (Parameter 'path')"
                }
            ];

            CheckAfterChanges(_syncAssetsViewModel, [], expectedResult);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Results"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanViewResults"));

            CheckInstance(syncAssetsViewModelInstances, [], expectedResult);
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
        List<SyncAssetsDirectoriesDefinition> definitions,
        List<SyncAssetsResult> expectedResult)
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

        Assert.That(syncAssetsViewModelInstance.Results, Has.Count.EqualTo(expectedResult.Count));

        for (int i = 0; i < expectedResult.Count; i++)
        {
            Assert.That(syncAssetsViewModelInstance.Results[i].SourceDirectory, Is.EqualTo(expectedResult[i].SourceDirectory));
            Assert.That(syncAssetsViewModelInstance.Results[i].DestinationDirectory, Is.EqualTo(expectedResult[i].DestinationDirectory));
            Assert.That(syncAssetsViewModelInstance.Results[i].SyncedImages, Is.EqualTo(expectedResult[i].SyncedImages));
            Assert.That(syncAssetsViewModelInstance.Results[i].Message, Is.EqualTo(expectedResult[i].Message));
        }

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
        List<SyncAssetsDirectoriesDefinition> definitions,
        List<SyncAssetsResult> expectedResult)
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
            CheckAfterChanges(syncAssetsViewModelInstances[0], definitions, expectedResult);
        }
    }
}
