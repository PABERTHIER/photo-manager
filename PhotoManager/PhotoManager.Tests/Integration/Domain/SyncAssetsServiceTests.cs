namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class SyncAssetsServiceTests
{
    private string? dataDirectory;
    private string? databasePath;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private SyncAssetsService? _syncAssetsService;
    private UserConfigurationService? _userConfigurationService;
    private AssetRepository? _assetRepository;
    private Database? _database;
    private StorageService? _storageService;
    private MoveAssetsService? _moveAssetsService;
    private CatalogAssetsService? _catalogAssetsService;
    private DirectoryComparer? _directoryComparer;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        databasePath = Path.Combine(dataDirectory, "DatabaseTests");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
    }

    [SetUp]
    public void Setup()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        _storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        DirectoryComparer directoryComparer = new(_storageService);
        _catalogAssetsService = new (_assetRepository, assetHashCalculatorService, _storageService, _userConfigurationService, directoryComparer);
        _moveAssetsService = new (_assetRepository, _storageService, _catalogAssetsService);
        _directoryComparer = new (_storageService);
        _syncAssetsService = new (_assetRepository, _storageService, _directoryComparer, _moveAssetsService);
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "SourceToSync");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.IsEmpty(statusChanges);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsNotEmptyAndDestinationIsEmpty_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(4, result[0].SyncedImages);
            Assert.AreEqual($"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath1}' => '{assetDestinationPath1}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[3].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsEmptyAndDestinationIsNotEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceToCopyPath = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceDirectory = Path.Combine(sourceToCopyPath, "Source");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopyPath2, assetDestinationPath2);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.IsEmpty(fileNamesInSource);

            string[] fileNamesInSourceToCopy = _storageService!.GetFileNames(sourceToCopyPath);
            Assert.AreEqual(4, fileNamesInSourceToCopy.Length);
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));

            Assert.IsFalse(_storageService.FileExists(assetSourcePath1));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath2));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath3));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath1));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath2));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath3));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.IsEmpty(fileNamesInSource);

            fileNamesInSourceToCopy = _storageService!.GetFileNames(sourceToCopyPath);
            Assert.AreEqual(4, fileNamesInSourceToCopy.Length);
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSourceToCopy.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));

            Assert.IsFalse(_storageService.FileExists(assetSourcePath1));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath2));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath3));
            Assert.IsFalse(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath1));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath2));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath3));
            Assert.IsTrue(_storageService.FileExists(assetSourceToCopyPath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(0, statusChanges.Count);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptyMultipleNewImages_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationPath3);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(2, result[0].SyncedImages);
            Assert.AreEqual($"2 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(2, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath1}' => '{assetDestinationPath1}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[1].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptyOneNewImage_StatusChangesAndImageAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            bool assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath1, assetDestinationPath1);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationPath3);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(3, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(1, result[0].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(1, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptyMultipleNewImagesAndTwoDefinitions_StatusChangesAndImagesAreAdded()
    {
        string firstSourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string secondSourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");
        string firstDestinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync1");
        string secondDestinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync2");

        try
        {
            Directory.CreateDirectory(firstDestinationDirectory);
            Directory.CreateDirectory(secondDestinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetName5 = "Image_1336_Mini.JPG";
            string assetName6 = "Image_1336_Original.JPG";
            string assetName7 = "Image_1336_ShitQuality.JPG";
            string assetName8 = "Image_1336_Small.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationPath4);
            Assert.IsTrue(assetHasBeenCopied);

            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath6, assetDestinationPath6);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath8, assetDestinationPath8);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource1 = _storageService!.GetFileNames(firstSourceDirectory);
            Assert.AreEqual(4, fileNamesInSource1.Length);
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName4));

            string[] fileNamesInSource2 = _storageService!.GetFileNames(secondSourceDirectory);
            Assert.AreEqual(4, fileNamesInSource2.Length);
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName5));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName6));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName7));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName8));

            string[] fileNamesInDestination1 = _storageService!.GetFileNames(firstDestinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination1.Length);
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName4));

            string[] fileNamesInDestination2 = _storageService!.GetFileNames(secondDestinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination2.Length);
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName6));
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName8));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath5));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath6));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath7));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath8));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath6));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath7));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath8));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = firstSourceDirectory,
                    DestinationDirectory = firstDestinationDirectory
                });

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = secondSourceDirectory,
                    DestinationDirectory = secondDestinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[1].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[1].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[1].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[1].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[1].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[1].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[1].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource1 = _storageService!.GetFileNames(firstSourceDirectory);
            Assert.AreEqual(4, fileNamesInSource1.Length);
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource1.Any(x => x == assetName4));

            fileNamesInSource2 = _storageService!.GetFileNames(secondSourceDirectory);
            Assert.AreEqual(4, fileNamesInSource2.Length);
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName5));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName6));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName7));
            Assert.IsTrue(fileNamesInSource2.Any(x => x == assetName8));

            fileNamesInDestination1 = _storageService!.GetFileNames(firstDestinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination1.Length);
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination1.Any(x => x == assetName4));

            fileNamesInDestination2 = _storageService!.GetFileNames(secondDestinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination2.Length);
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName5));
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName6));
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName7));
            Assert.IsTrue(fileNamesInDestination2.Any(x => x == assetName8));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath5));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath6));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath7));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath8));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath5));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath6));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath7));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath7));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(firstSourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(firstDestinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(2, result[0].SyncedImages);
            Assert.AreEqual($"2 images synced from '{firstSourceDirectory}' to '{firstDestinationDirectory}'.", result[0].Message);
            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath1}' => '{assetDestinationPath1}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[1].NewStatus);

            Assert.AreEqual(secondSourceDirectory, result[1].SourceDirectory);
            Assert.AreEqual(secondDestinationDirectory, result[1].DestinationDirectory);
            Assert.AreEqual(2, result[1].SyncedImages);
            Assert.AreEqual($"2 images synced from '{secondSourceDirectory}' to '{secondDestinationDirectory}'.", result[1].Message);
            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath5}' => '{assetDestinationPath5}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetSourcePath7}' => '{assetDestinationPath7}'", statusChanges[3].NewStatus);
        }
        finally
        {
            Directory.Delete(firstDestinationDirectory, true);
            Directory.Delete(secondDestinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriresAndDestinationSubDirectoriesAreSingleLevel_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory2");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(1, result[0].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(1, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[0].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptyAllImagesAreInDestinationOrSubDirectoriresAndDestinationSubDirectoriesAreSingleLevel_StatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory2");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath4, assetDestinationSubDirectory2Path4);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(2, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(2, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual($"No images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.IsEmpty(statusChanges);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsFalse_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, "SubDirectory1");
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, "SubDirectory2");
        string sourceToCopy = Path.Combine(dataDirectory!, "Duplicates\\Part");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory2");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";
            string assetName5 = "1336_BottomLeftPart.JPG";
            string assetName6 = "1336_TopRightPart.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory1));
            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory2));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory1));
            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory2));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(3, result[0].SyncedImages);
            Assert.AreEqual($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(3, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[2].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, "SubDirectory1");
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, "SubDirectory2");
        string sourceToCopy = Path.Combine(dataDirectory!, "Duplicates\\Part");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory2");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";
            string assetName5 = "1336_BottomLeftPart.JPG";
            string assetName6 = "1336_TopRightPart.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory1));
            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory2));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5));

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName6));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(3, result[0].SyncedImages);
            Assert.AreEqual($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);

            Assert.AreEqual(sourceSubDirectory1, result[1].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory1, result[1].DestinationDirectory);
            Assert.AreEqual(1, result[1].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'.", result[1].Message);

            Assert.AreEqual(sourceSubDirectory2, result[2].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory2, result[2].DestinationDirectory);
            Assert.AreEqual(1, result[2].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'.", result[2].Message);

            Assert.AreEqual(5, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'", statusChanges[3].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'", statusChanges[4].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceAndDestinationSubDirectoriesAreSingleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, "SubDirectory1");
        string sourceSubDirectory2 = Path.Combine(sourceDirectory, "SubDirectory2");
        string sourceToCopy = Path.Combine(dataDirectory!, "Duplicates\\Part");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory2");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";
            string assetName5 = "1336_BottomLeftPart.JPG";
            string assetName6 = "1336_TopRightPart.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationSubDirectory1Path);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5));

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.IsEmpty(fileNamesInDestinationSubDirectory2);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName5));

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName6));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName5));

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName6));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(3, result[0].SyncedImages);
            Assert.AreEqual($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);

            Assert.AreEqual(sourceSubDirectory1, result[1].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory1, result[1].DestinationDirectory);
            Assert.AreEqual(0, result[1].SyncedImages);
            Assert.AreEqual($"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'.", result[1].Message);

            Assert.AreEqual(sourceSubDirectory2, result[2].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory2, result[2].DestinationDirectory);
            Assert.AreEqual(1, result[2].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'.", result[2].Message);

            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'", statusChanges[3].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory2, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationOrSubDirectoriresAndDestinationSubDirectoriesAreMultipleLevel_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory2//SubDirectory2");
        string destinationSubDirectory2 = Path.Combine(destinationDirectory, "SubDirectory3//SubDirectory4//SubDirectory5");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(destinationSubDirectory1);
            Directory.CreateDirectory(destinationSubDirectory2);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath2, assetDestinationSubDirectory1Path2);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourcePath3, assetDestinationSubDirectory2Path3);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(2, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName3));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory1Path4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationSubDirectory2Path4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(1, result[0].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(1, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[0].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndSourceSubDirectoriesAreMultipleLevelAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, "SubDirectory1");
        string sourceSubDirectory2 = Path.Combine(sourceSubDirectory1, "SubDirectory2");
        string sourceSubDirectory3 = Path.Combine(sourceDirectory, "SubDirectory3");
        string sourceToCopy = Path.Combine(dataDirectory!, "Duplicates\\Part");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");
        string destinationSubDirectory2 = Path.Combine(destinationSubDirectory1, "SubDirectory2");
        string destinationSubDirectory3 = Path.Combine(destinationDirectory, "SubDirectory3");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);
            Directory.CreateDirectory(sourceSubDirectory2);
            Directory.CreateDirectory(sourceSubDirectory3);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";
            string assetName5 = "1336_BottomLeftPart.JPG";
            string assetName6 = "1336_TopRightPart.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory2Path, assetDestinationToCopySubDirectory2Path);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory3Path, assetDestinationToCopySubDirectory3Path);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.IsEmpty(fileNamesInSourceSubDirectory1);

            string[] fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName5));

            string[] fileNamesInSourceSubDirectory3 = _storageService!.GetFileNames(sourceSubDirectory3);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory3.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory3.Any(x => x == assetName6));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory3Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory1));
            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory2));
            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory3));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.IsEmpty(fileNamesInSourceSubDirectory1);

            fileNamesInSourceSubDirectory2 = _storageService!.GetFileNames(sourceSubDirectory2);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory2.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory2.Any(x => x == assetName5));

            fileNamesInSourceSubDirectory3 = _storageService!.GetFileNames(sourceSubDirectory3);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory3.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory3.Any(x => x == assetName6));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            string [] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.IsEmpty(fileNamesInDestinationSubDirectory1);

            string[] fileNamesInDestinationSubDirectory2 = _storageService!.GetFileNames(destinationSubDirectory2);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory2.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory2.Any(x => x == assetName5));

            string[] fileNamesInDestinationSubDirectory3 = _storageService!.GetFileNames(destinationSubDirectory3);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory3.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory3.Any(x => x == assetName6));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory2Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory3Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));

            Assert.IsTrue(_storageService.FolderExists(destinationSubDirectory1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory2Path));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory3Path));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(3, result[0].SyncedImages);
            Assert.AreEqual($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);

            Assert.AreEqual(sourceSubDirectory1, result[1].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory1, result[1].DestinationDirectory);
            Assert.AreEqual(0, result[1].SyncedImages);
            Assert.AreEqual($"No images synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'.", result[1].Message);

            Assert.AreEqual(sourceSubDirectory2, result[2].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory2, result[2].DestinationDirectory);
            Assert.AreEqual(1, result[2].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory2}' to '{destinationSubDirectory2}'.", result[2].Message);

            Assert.AreEqual(sourceSubDirectory3, result[3].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory3, result[3].DestinationDirectory);
            Assert.AreEqual(1, result[3].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory3}' to '{destinationSubDirectory3}'.", result[3].Message);

            Assert.AreEqual(5, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory2Path}' => '{assetDestinationSubDirectory2Path}'", statusChanges[3].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory3Path}' => '{assetDestinationSubDirectory3Path}'", statusChanges[4].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(sourceSubDirectory3, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptySomeImagesAreInDestinationAndDuplicatedAssetInSourceAndIncludeSubFoldersIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string sourceSubDirectory1 = Path.Combine(sourceDirectory, "SubDirectory1");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationSubDirectory1 = Path.Combine(destinationDirectory, "SubDirectory1");

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(sourceSubDirectory1);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetSourceToCopySubDirectory1Path, assetDestinationToCopySubDirectory1Path);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName2));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(1, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsFalse(_storageService.FolderExists(destinationSubDirectory1));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    IncludeSubFolders = true
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInSourceSubDirectory1 = _storageService!.GetFileNames(sourceSubDirectory1);
            Assert.AreEqual(1, fileNamesInSourceSubDirectory1.Length);
            Assert.IsTrue(fileNamesInSourceSubDirectory1.Any(x => x == assetName2));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            string[] fileNamesInDestinationSubDirectory1 = _storageService!.GetFileNames(destinationSubDirectory1);
            Assert.AreEqual(1, fileNamesInDestinationSubDirectory1.Length);
            Assert.IsTrue(fileNamesInDestinationSubDirectory1.Any(x => x == assetName2));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationToCopySubDirectory1Path));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationSubDirectory1Path));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(3, result[0].SyncedImages);
            Assert.AreEqual($"3 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);

            Assert.AreEqual(sourceSubDirectory1, result[1].SourceDirectory);
            Assert.AreEqual(destinationSubDirectory1, result[1].DestinationDirectory);
            Assert.AreEqual(1, result[1].SyncedImages);
            Assert.AreEqual($"1 image synced from '{sourceSubDirectory1}' to '{destinationSubDirectory1}'.", result[1].Message);

            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetDestinationToCopySubDirectory1Path}' => '{assetDestinationSubDirectory1Path}'", statusChanges[3].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(sourceSubDirectory1, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceAndDestinationAreNotEmptyBothContainDifferentImagesAndDeleteAssetsNotInSourceIsTrue_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");
        string destinationToCopy = Path.Combine(dataDirectory!, "Duplicates\\Thumbnail");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetName5 = "Image_1336_Mini.JPG";
            string assetName6 = "Image_1336_Original.JPG";
            string assetName7 = "Image_1336_ShitQuality.JPG";
            string assetName8 = "Image_1336_Small.JPG";

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
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath2, assetDestinationPath6);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath3, assetDestinationPath7);
            Assert.IsTrue(assetHasBeenCopied);
            assetHasBeenCopied = _moveAssetsService!.CopyAsset(assetDestinationToCopyPath4, assetDestinationPath8);
            Assert.IsTrue(assetHasBeenCopied);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName5));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName6));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName7));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName8));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath5));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath6));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath7));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath8));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory,
                    DeleteAssetsNotInSource = true,
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath5));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath6));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath7));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath8));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(8, result[0].SyncedImages);
            Assert.AreEqual($"8 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(8, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath1}' => '{assetDestinationPath1}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[3].NewStatus);
            Assert.AreEqual($"Deleted '{assetDestinationPath5}'", statusChanges[4].NewStatus);
            Assert.AreEqual($"Deleted '{assetDestinationPath6}'", statusChanges[5].NewStatus);
            Assert.AreEqual($"Deleted '{assetDestinationPath7}'", statusChanges[6].NewStatus);
            Assert.AreEqual($"Deleted '{assetDestinationPath8}'", statusChanges[7].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefintionIsEmpty_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.IsEmpty(syncAssetsConfigurationFromRepository.Definitions);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsEmpty(result);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public void ExecuteAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationDefintionIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfiguration = new();
            SyncAssetsDirectoriesDefinition? definition = null;

            syncAssetsConfiguration.Definitions.Add(definition!);

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(1, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(null, syncAssetsConfigurationFromRepository.Definitions[0]);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _syncAssetsService!.ExecuteAsync(statusChanges.Add));
            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationNotSaved_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository!.GetSyncAssetsConfiguration();
            Assert.IsEmpty(syncAssetsConfigurationFromRepository.Definitions);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            Assert.IsEmpty(result);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public void ExecuteAsync_SourceIsNotEmptyAndDestinationIsEmptyAndSyncAssetsConfigurationIsNull_ThrowsNullReferenceException()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.IsEmpty(fileNamesInDestination);

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FileExists(assetDestinationPath1));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath2));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath3));
            Assert.IsFalse(_storageService.FileExists(assetDestinationPath4));

            SyncAssetsConfiguration? syncAssetsConfiguration = null;

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration!);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.IsNull(syncAssetsConfigurationFromRepository);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _syncAssetsService!.ExecuteAsync(statusChanges.Add));
            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceDirectoryDoesNotExist_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "SourceToSync");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual($"Source directory '{sourceDirectory}' not found.", result[0].Message);
            Assert.IsEmpty(statusChanges);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceDirectoryIsNull_NoStatusChangesAndNoImagesAreAdded()
    {
        string? sourceDirectory = null;
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory!,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual($"Source directory '{sourceDirectory}' not found.", result[0].Message);
            Assert.IsEmpty(statusChanges);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsNotEmptyAndDestinationDirectoryDoesNotExist_StatusChangesAndImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(dataDirectory!, "DestinationToSync");

        try
        {
            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string assetDestinationPath1 = Path.Combine(destinationDirectory, assetName1);
            string assetDestinationPath2 = Path.Combine(destinationDirectory, assetName2);
            string assetDestinationPath3 = Path.Combine(destinationDirectory, assetName3);
            string assetDestinationPath4 = Path.Combine(destinationDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FolderExists(destinationDirectory));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            string[] fileNamesInDestination = _storageService!.GetFileNames(destinationDirectory);
            Assert.AreEqual(4, fileNamesInDestination.Length);
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInDestination.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsTrue(_storageService.FileExists(assetDestinationPath1));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath2));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath3));
            Assert.IsTrue(_storageService.FileExists(assetDestinationPath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(4, result[0].SyncedImages);
            Assert.AreEqual($"4 images synced from '{sourceDirectory}' to '{destinationDirectory}'.", result[0].Message);
            Assert.AreEqual(4, statusChanges.Count);
            Assert.AreEqual($"'{assetSourcePath1}' => '{assetDestinationPath1}'", statusChanges[0].NewStatus);
            Assert.AreEqual($"'{assetSourcePath2}' => '{assetDestinationPath2}'", statusChanges[1].NewStatus);
            Assert.AreEqual($"'{assetSourcePath3}' => '{assetDestinationPath3}'", statusChanges[2].NewStatus);
            Assert.AreEqual($"'{assetSourcePath4}' => '{assetDestinationPath4}'", statusChanges[3].NewStatus);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(databasePath!, true);
        }
    }

    [Test]
    public async Task ExecuteAsync_SourceIsNotEmptyAndDestinationDirectoryIsNull_NoStatusChangesAndNoImagesAreAdded()
    {
        string sourceDirectory = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");
        string? destinationDirectory = null;

        try
        {
            string assetName1 = "Image 1_duplicate.jpg";
            string assetName2 = "Image 9.png";
            string assetName3 = "Image 9_duplicate.png";
            string assetName4 = "Image_11.heic";

            string assetSourcePath1 = Path.Combine(sourceDirectory, assetName1);
            string assetSourcePath2 = Path.Combine(sourceDirectory, assetName2);
            string assetSourcePath3 = Path.Combine(sourceDirectory, assetName3);
            string assetSourcePath4 = Path.Combine(sourceDirectory, assetName4);

            string[] fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsFalse(_storageService.FolderExists(destinationDirectory!));

            SyncAssetsConfiguration syncAssetsConfiguration = new();

            syncAssetsConfiguration.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = sourceDirectory,
                    DestinationDirectory = destinationDirectory!
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
            SyncAssetsConfiguration syncAssetsConfigurationFromRepository = _assetRepository.GetSyncAssetsConfiguration();
            Assert.AreEqual(syncAssetsConfiguration.Definitions.Count, syncAssetsConfigurationFromRepository.Definitions.Count);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].SourceDirectory, syncAssetsConfigurationFromRepository.Definitions[0].SourceDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DestinationDirectory, syncAssetsConfigurationFromRepository.Definitions[0].DestinationDirectory);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].IncludeSubFolders, syncAssetsConfigurationFromRepository.Definitions[0].IncludeSubFolders);
            Assert.AreEqual(syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, syncAssetsConfigurationFromRepository.Definitions[0].DeleteAssetsNotInSource);

            List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

            List<SyncAssetsResult> result = await _syncAssetsService!.ExecuteAsync(statusChanges.Add);

            fileNamesInSource = _storageService!.GetFileNames(sourceDirectory);
            Assert.AreEqual(4, fileNamesInSource.Length);
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName1));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName2));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName3));
            Assert.IsTrue(fileNamesInSource.Any(x => x == assetName4));

            Assert.IsTrue(_storageService.FileExists(assetSourcePath1));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath2));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath3));
            Assert.IsTrue(_storageService.FileExists(assetSourcePath4));

            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sourceDirectory, result[0].SourceDirectory);
            Assert.AreEqual(destinationDirectory, result[0].DestinationDirectory);
            Assert.AreEqual(0, result[0].SyncedImages);
            Assert.AreEqual("Value cannot be null. (Parameter 'path')", result[0].Message);
            Assert.IsEmpty(statusChanges);
        }
        finally
        {
            Directory.Delete(databasePath!, true);
        }
    }
}
