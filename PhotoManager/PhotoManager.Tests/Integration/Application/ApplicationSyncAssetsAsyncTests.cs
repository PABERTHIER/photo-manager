using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationSyncAssetsAsyncTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private MoveAssetsService? _moveAssetsService;
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

        _userConfigurationService = new(configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(_database, _storageServiceMock!.Object, _userConfigurationService);
        _storageService = new(_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, _storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, _storageService, assetCreationService, _userConfigurationService, assetsComparator);
        _moveAssetsService = new(_assetRepository, _storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(_assetRepository, _storageService, assetsComparator, _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_assetRepository, _storageService, _userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, _moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, _storageService);
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_SYNC);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsNotEmptyAndDestinationIsEmpty_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(4));
            Assert.That(result[0].Message, Is.EqualTo($"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsEmptyAndDestinationIsNotEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceToCopyPath = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceDirectory = Path.Combine(sourceToCopyPath, Directories.SOURCE);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Is.Empty);

            string[] fileNamesInSourceToCopy = _storageService!.GetFileNames(sourceToCopyPath);
            Assert.That(fileNamesInSourceToCopy, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.False);

            Assert.That(_storageService.FileExists(assetSourceToCopyPath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Is.Empty);

            fileNamesInSourceToCopy = _storageService!.GetFileNames(sourceToCopyPath);
            Assert.That(fileNamesInSourceToCopy, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSourceToCopy.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.False);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.False);

            Assert.That(_storageService.FileExists(assetSourceToCopyPath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourceToCopyPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptyMultipleNewImages_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(2));
            Assert.That(result[0].Message, Is.EqualTo($"2 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(2));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptyOneNewImage_StatusChangesAndImageAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(3));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(1));
            Assert.That(result[0].Message, Is.EqualTo($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptyMultipleNewImagesAndTwoDefinitions_StatusChangesAndImagesAreAdded()
    {
        string firstSourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string secondSourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}");
        string firstDestinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC_1);
        string secondDestinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource1 = _storageService!.GetFileNames(firstSourceDirectory);
            Assert.That(fileNamesInSource1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSource2 = _storageService!.GetFileNames(secondSourceDirectory);
            Assert.That(fileNamesInSource2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName8), Is.True);

            string[] fileNamesInDestination1 = _storageService!.GetFileNames(firstDestinationDirectory);
            Assert.That(fileNamesInDestination1, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination2 = _storageService!.GetFileNames(secondDestinationDirectory);
            Assert.That(fileNamesInDestination2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName8), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath5), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath6), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath7), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath8), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath7), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath8), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = firstSourceDirectory, DestinationDirectory = firstDestinationDirectory });
            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = secondSourceDirectory, DestinationDirectory = secondDestinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

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

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource1 = _storageService!.GetFileNames(firstSourceDirectory);
            Assert.That(fileNamesInSource1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource1.Any(x => x == assetName4), Is.True);

            fileNamesInSource2 = _storageService!.GetFileNames(secondSourceDirectory);
            Assert.That(fileNamesInSource2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInSource2.Any(x => x == assetName8), Is.True);

            fileNamesInDestination1 = _storageService!.GetFileNames(firstDestinationDirectory);
            Assert.That(fileNamesInDestination1, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination1.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination1.Any(x => x == assetName4), Is.True);

            fileNamesInDestination2 = _storageService!.GetFileNames(secondDestinationDirectory);
            Assert.That(fileNamesInDestination2, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination2.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInDestination2.Any(x => x == assetName8), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath5), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath6), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath7), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath8), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath7), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath7), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(2));

            Assert.That(result[0].SourceDirectory, Is.EqualTo(firstSourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(firstDestinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(2));
            Assert.That(result[0].Message, Is.EqualTo($"2 images synced from '{firstSourceDirectory}' to '{firstDestinationDirectory}'."));

            Assert.That(result[1].SourceDirectory, Is.EqualTo(secondSourceDirectory));
            Assert.That(result[1].DestinationDirectory, Is.EqualTo(secondDestinationDirectory));
            Assert.That(result[1].SyncedImages, Is.EqualTo(2));
            Assert.That(result[1].Message, Is.EqualTo($"2 images synced from '{secondSourceDirectory}' to '{secondDestinationDirectory}'."));

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath5}' => '{assetDestinationPath5}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath7}' => '{assetDestinationPath7}'"));
        }
        finally
        {
            Directory.Delete(firstDestinationDirectory, true);
            Directory.Delete(secondDestinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreSingleLevel_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(1));
            Assert.That(result[0].Message, Is.EqualTo($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptyAllImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreSingleLevel_StatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsFalse_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_storageService.FolderExists(destinationSubDirectory2), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_storageService.FolderExists(destinationSubDirectory2), Is.False);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(3));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
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
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_storageService.FolderExists(destinationSubDirectory2), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName6), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));

            Assert.That(result[1].SourceDirectory, Is.EqualTo(sourceSubDirectory1));
            Assert.That(result[1].DestinationDirectory, Is.EqualTo(destinationSubDirectory1));
            Assert.That(result[1].SyncedImages, Is.EqualTo(1));
            Assert.That(result[1].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."));

            Assert.That(result[2].SourceDirectory, Is.EqualTo(sourceSubDirectory2));
            Assert.That(result[2].DestinationDirectory, Is.EqualTo(destinationSubDirectory2));
            Assert.That(result[2].SyncedImages, Is.EqualTo(1));
            Assert.That(result[2].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."));

            Assert.That(statusChanges, Has.Count.EqualTo(5));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));
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
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceAndDestinationSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_2);
        string sourceToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_2);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5), Is.True);

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName6), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));

            Assert.That(result[1].SourceDirectory, Is.EqualTo(sourceSubDirectory1));
            Assert.That(result[1].DestinationDirectory, Is.EqualTo(destinationSubDirectory1));
            Assert.That(result[1].SyncedImages, Is.Zero);
            Assert.That(result[1].Message, Is.EqualTo($"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."));

            Assert.That(result[2].SourceDirectory, Is.EqualTo(sourceSubDirectory2));
            Assert.That(result[2].DestinationDirectory, Is.EqualTo(destinationSubDirectory2));
            Assert.That(result[2].SyncedImages, Is.EqualTo(1));
            Assert.That(result[2].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."));

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));
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
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriesAndDestinationSubDirectoriesAreMultipleLevel_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, $"{Directories.SUB_DIRECTORY_2}\\{Directories.SUB_DIRECTORY_2}");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, $"{Directories.SUB_DIRECTORY_3}\\{Directories.SUB_DIRECTORY_4}\\{Directories.SUB_DIRECTORY_5}");

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(2));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path4), Is.False);

            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path4), Is.False);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(1));
            Assert.That(result[0].Message, Is.EqualTo($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(1));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreMultipleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
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

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Is.Empty);

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName5), Is.True);

            string[] fileNamesInSourceSubDirectory3 = _storageService!.GetFileNames(sourceSubDirectory3);
            Assert.That(fileNamesInSourceSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory3.Any(x => x == assetName6), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory3Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.False);
            Assert.That(_storageService.FolderExists(destinationSubDirectory2), Is.False);
            Assert.That(_storageService.FolderExists(destinationSubDirectory3), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Is.Empty);

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.That(fileNamesInSourceSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory2.Any(x => x == assetName5), Is.True);

            fileNamesInSourceSubDirectory3 = _storageService!.GetFileNames(sourceSubDirectory3);
            Assert.That(fileNamesInSourceSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory3.Any(x => x == assetName6), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Is.Empty);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.That(fileNamesInDestinationSubDirectory2, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory2.Any(x => x == assetName5), Is.True);

            string[] fileNamesInDestinationSubDirectory3 = _storageService!.GetFileNames(destinationSubDirectory3);
            Assert.That(fileNamesInDestinationSubDirectory3, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory3.Any(x => x == assetName6), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory2Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory3Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory2Path), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory3Path), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));

            Assert.That(result[1].SourceDirectory, Is.EqualTo(sourceSubDirectory1));
            Assert.That(result[1].DestinationDirectory, Is.EqualTo(destinationSubDirectory1));
            Assert.That(result[1].SyncedImages, Is.Zero);
            Assert.That(result[1].Message, Is.EqualTo($"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."));

            Assert.That(result[2].SourceDirectory, Is.EqualTo(sourceSubDirectory2));
            Assert.That(result[2].DestinationDirectory, Is.EqualTo(destinationSubDirectory2));
            Assert.That(result[2].SyncedImages, Is.EqualTo(1));
            Assert.That(result[2].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'."));

            Assert.That(result[3].SourceDirectory, Is.EqualTo(sourceSubDirectory3));
            Assert.That(result[3].DestinationDirectory, Is.EqualTo(destinationSubDirectory3));
            Assert.That(result[3].SyncedImages, Is.EqualTo(1));
            Assert.That(result[3].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory3}' to '{destinationSubDirectory3}'."));

            Assert.That(statusChanges, Has.Count.EqualTo(5));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory3Path}' => '{assetDestinationSubDirectory3Path}'"));
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
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndDuplicatedAssetInSourceAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, Directories.SUB_DIRECTORY_1);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, Directories.SUB_DIRECTORY_1);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName2), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(_storageService.FolderExists(destinationSubDirectory1), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.That(fileNamesInSourceSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInSourceSubDirectory1.Any(x => x == assetName2), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.That(fileNamesInDestinationSubDirectory1, Has.Length.EqualTo(1));
            Assert.That(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationToCopySubDirectory1Path), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationSubDirectory1Path), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));

            Assert.That(result[1].SourceDirectory, Is.EqualTo(sourceSubDirectory1));
            Assert.That(result[1].DestinationDirectory, Is.EqualTo(destinationSubDirectory1));
            Assert.That(result[1].SyncedImages, Is.EqualTo(1));
            Assert.That(result[1].Message, Is.EqualTo($"1 image synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'."));

            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceAndDestinationAreNotEmptyBothContainDifferentImagesAndDeleteAssetsNotInSourceIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);
        string destinationToCopy = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.THUMBNAIL}");

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName5), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName6), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName7), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName8), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath7), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath8), Is.True);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new()
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    DeleteAssetsNotInSource = true,
                });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath5), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath6), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath7), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath8), Is.False);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(8));
            Assert.That(result[0].Message, Is.EqualTo($"8 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(8));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
            Assert.That(statusChanges[4].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath5}'"));
            Assert.That(statusChanges[5].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath6}'"));
            Assert.That(statusChanges[6].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath7}'"));
            Assert.That(statusChanges[7].NewStatus, Is.EqualTo($"Deleted '{assetDestinationPath8}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefinitionIsEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Is.Empty);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(result, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SyncAssetsAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefinitionIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();
            SyncAssetsDirectoriesDefinition? definition = null;

            syncAssetsConfiguration.Definitions.Add(definition!);

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref in Validate and Normalize methods
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(1));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0], Is.Null);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _application!.SyncAssetsAsync(statusChanges.Add));
            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationNotSaved_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Is.Empty);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            Assert.That(result, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SyncAssetsAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Is.Empty);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.False);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.False);

            SyncAssetsConfiguration? syncAssetsConfiguration = null;

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration!);
            // _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration!); // Using instead _assetRepository to prevent null ref for Validate and Normalize methods
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();
            Assert.That(syncAssetsConfigurationFromRepository, Is.Null);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _application!.SyncAssetsAsync(statusChanges.Add));
            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceDirectoryDoesNotExist_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_SYNC);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo($"Source directory '{sourceDirectory}' not found."));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceDirectoryIsNull_NoStatusChangesAndNoImagesAreAdded()
    {
        string? sourceDirectory = null;
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory!, DestinationDirectory = destinationDirectory });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref during regex verification
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo($"Source directory '{sourceDirectory}' not found."));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsNotEmptyAndDestinationDirectoryDoesNotExist_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_SYNC);

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FolderExists(destinationDirectory), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

            _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.That(fileNamesInDestination, Has.Length.EqualTo(4));
            Assert.That(fileNamesInDestination.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInDestination.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FileExists(assetDestinationPath1), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath2), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath3), Is.True);
            Assert.That(_storageService.FileExists(assetDestinationPath4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.EqualTo(4));
            Assert.That(result[0].Message, Is.EqualTo($"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'."));
            Assert.That(statusChanges, Has.Count.EqualTo(4));
            Assert.That(statusChanges[0].NewStatus, Is.EqualTo($"'{assetSourcePath1}' => '{assetDestinationPath1}'"));
            Assert.That(statusChanges[1].NewStatus, Is.EqualTo($"'{assetSourcePath2}' => '{assetDestinationPath2}'"));
            Assert.That(statusChanges[2].NewStatus, Is.EqualTo($"'{assetSourcePath3}' => '{assetDestinationPath3}'"));
            Assert.That(statusChanges[3].NewStatus, Is.EqualTo($"'{assetSourcePath4}' => '{assetDestinationPath4}'"));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SyncAssetsAsync_SourceIsNotEmptyAndDestinationDirectoryIsNull_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string? destinationDirectory = null;

        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            const string assetName1 = FileNames.IMAGE_1_DUPLICATE_JPG;
            const string assetName2 = FileNames.IMAGE_9_PNG;
            const string assetName3 = FileNames.IMAGE_9_DUPLICATE_PNG;
            const string assetName4 = FileNames.IMAGE_11_HEIC;

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(_storageService.FolderExists(destinationDirectory!), Is.False);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory! });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            // _application!.SetSyncAssetsConfiguration(syncAssetsConfiguration); // Using instead _assetRepository to prevent null ref during regex verification
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _application!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfigurationFromRepository.Definitions, Has.Count.EqualTo(syncAssetsConfiguration.Definitions.Count));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].SourceDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DestinationDirectory));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsConfiguration.Definitions[0].IncludeSubFolders));
            Assert.That(syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource));

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _application!.SyncAssetsAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.That(fileNamesInSource, Has.Length.EqualTo(4));
            Assert.That(fileNamesInSource.Any(x => x == assetName1), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName2), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName3), Is.True);
            Assert.That(fileNamesInSource.Any(x => x == assetName4), Is.True);

            Assert.That(_storageService.FileExists(assetSourcePath1), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath2), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath3), Is.True);
            Assert.That(_storageService.FileExists(assetSourcePath4), Is.True);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
            Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
            Assert.That(result[0].SyncedImages, Is.Zero);
            Assert.That(result[0].Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
            Assert.That(statusChanges, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
