using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class TotoTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";
    private string? _databaseBackupPath;
    private const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset2Temp;
    private Asset? _asset3Temp;

    private const int ASSET2_TEMP_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET3_TEMP_IMAGE_BYTE_SIZE = 8594;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
        _databaseBackupPath = Path.Combine(_databaseDirectory, DATABASE_BACKUP_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _blobStorage = new();
        _database = new (new ObjectListStorage(), _blobStorage, new BackupStorage());
    }

    [SetUp]
    public void Setup()
    {
        _asset2Temp = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset3Temp = new()
        {
            FileName = "Homer.gif",
            FileSize = 64123,
            PixelHeight = 320,
            PixelWidth = 320,
            ThumbnailPixelWidth = 150,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
    }

    private void ConfigureApplicationViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        _application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (_application);
    }

    // UPDATE SECTION (Start) -------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task TotoTests_AssetsAndRootCatalogFolderExistsAndOneCorruptedImageIsUpdated_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"TempAssetsDirectory_{analyseVideos}"); // Issues in the CI to corrupt image if same directory for each case
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, $"Image 1_Temp_{analyseVideos}.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPaths = [imagePath2ToCopy, imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp!.Folder = folder!;
            _asset2Temp!.FolderId = folder!.FolderId;
            _asset3Temp!.Folder = folder;
            _asset3Temp!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder][..(i + 1)],
                    folderToAssetsMapping[folder][i],
                    folder,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder],
                folderToAssetsMapping[folder][0],
                folder,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder],
                folderToAssetsMapping[folder][0],
                folder,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            _asset2Temp.ThumbnailCreationDateTime = DateTime.Now; // Because recreated with CreateInvalidImage()
            File.SetLastWriteTime(imagePath1ToCopy, DateTime.Now.AddDays(10));

            // Corrupt image
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelper.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(imagePath1ToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset2Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET2_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new() { { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(13, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder!].Count,
                folderToAssetsMappingUpdated[folder!],
                _asset2Temp,
                folder!,
                true,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath1ToCopyTemp))
            {
                File.Delete(imagePath1ToCopyTemp);
            }
        }
    }
    // UPDATE SECTION (End) -------------------------------------------------------------------------------------------

    private
        (List<string> notifyPropertyChangedEvents,
        List<ApplicationViewModel> applicationViewModelInstances,
        List<Folder> folderAddedEvents,
        List<Folder> folderRemovedEvents)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = new();
        List<ApplicationViewModel> applicationViewModelInstances = new();

        _applicationViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            applicationViewModelInstances.Add((ApplicationViewModel)sender!);
        };

        List<Folder> folderAddedEvents = new();

        _applicationViewModel.FolderAdded += delegate(object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = new();

        _applicationViewModel.FolderRemoved += delegate(object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private void CheckBeforeNotifyCatalogChanges(string expectedRootDirectory)
    {
        Assert.IsTrue(_applicationViewModel!.SortAscending);
        Assert.IsNull(_applicationViewModel!.Product);
        Assert.IsNull(_applicationViewModel!.Version);
        Assert.IsFalse(_applicationViewModel!.IsRefreshingFolders);
        Assert.AreEqual(AppMode.Thumbnails, _applicationViewModel!.AppMode);
        Assert.AreEqual(SortCriteria.FileName, _applicationViewModel!.SortCriteria);
        Assert.AreEqual(Visibility.Visible, _applicationViewModel!.ThumbnailsVisible);
        Assert.AreEqual(Visibility.Hidden, _applicationViewModel!.ViewerVisible);
        Assert.AreEqual(0, _applicationViewModel!.ViewerPosition);
        Assert.IsEmpty(_applicationViewModel!.SelectedAssets);
        Assert.AreEqual(expectedRootDirectory, _applicationViewModel!.CurrentFolder);
        Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        Assert.IsNull(_applicationViewModel!.GlobaleAssetsCounter);
        Assert.IsNull(_applicationViewModel!.ExecutionTime);
        Assert.IsNull(_applicationViewModel!.TotalFilesNumber);
        Assert.AreEqual($"  - {expectedRootDirectory} - image 1 of 0 - sorted by file name ascending", _applicationViewModel!.AppTitle);
        Assert.IsNull(_applicationViewModel!.StatusMessage);
        Assert.IsNull(_applicationViewModel!.CurrentAsset);
        Assert.IsNull(_applicationViewModel!.LastSelectedFolder); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.IsFalse(_applicationViewModel!.CanGoToPreviousAsset);
        Assert.IsFalse(_applicationViewModel!.CanGoToNextAsset);
    }

    private static void CheckAfterNotifyCatalogChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        IReadOnlyCollection<Asset> expectedObservableAssets,
        Asset expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.IsTrue(applicationViewModelInstance.SortAscending);
        Assert.IsNull(applicationViewModelInstance.Product);
        Assert.IsNull(applicationViewModelInstance.Version);
        Assert.IsFalse(applicationViewModelInstance.IsRefreshingFolders);
        Assert.AreEqual(AppMode.Thumbnails, applicationViewModelInstance.AppMode);
        Assert.AreEqual(SortCriteria.FileName, applicationViewModelInstance.SortCriteria);
        Assert.AreEqual(Visibility.Visible, applicationViewModelInstance.ThumbnailsVisible);
        Assert.AreEqual(Visibility.Hidden, applicationViewModelInstance.ViewerVisible);
        Assert.AreEqual(0, applicationViewModelInstance.ViewerPosition);
        Assert.IsEmpty(applicationViewModelInstance.SelectedAssets);
        Assert.AreEqual(expectedLastDirectoryInspected, applicationViewModelInstance.CurrentFolder);
        Assert.AreEqual(expectedObservableAssets.Count, applicationViewModelInstance.ObservableAssets.Count);
        Assert.IsNull(applicationViewModelInstance.GlobaleAssetsCounter);
        Assert.IsNull(applicationViewModelInstance.ExecutionTime);
        Assert.IsNull(applicationViewModelInstance.TotalFilesNumber);
        Assert.AreEqual($"  - {expectedLastDirectoryInspected} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", applicationViewModelInstance.AppTitle);
        Assert.AreEqual("The catalog process has ended.", applicationViewModelInstance.StatusMessage);

        if (applicationViewModelInstance.CurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }

        Assert.IsNull(applicationViewModelInstance.LastSelectedFolder); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.IsFalse(applicationViewModelInstance.CanGoToPreviousAsset);
        Assert.AreEqual(expectedCanGoToNextAsset, applicationViewModelInstance.CanGoToNextAsset);
    }

    private void NotifyCatalogChangeFolderInspectionInProgress(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderInspectionInProgress, catalogChange.Reason);
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeFolderInspectionCompleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderInspectionCompleted, catalogChange.Reason);
        Assert.AreEqual($"Folder inspection for {assetsDirectory}, subfolders included, has been completed.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Folder inspection for {assetsDirectory}, subfolders included, has been completed.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeAssetCreated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        int expectedAppTitleAssetsCount,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNotNull(catalogChange.Asset);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageData(expectedAsset, currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.AreEqual(CatalogChangeReason.AssetCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
        if (string.Equals(expectedAsset.FullPath, catalogChange.Asset!.FullPath))
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
        }
        else
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
        }

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset.Folder.Path, currentDirectory))
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList());
        }

        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeAssetDeleted(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        int expectedAppTitleAssetsCount,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        bool isCorrupted,
        ref int increment)
    {
        string expectedStatusMessage = isCorrupted ? $"Image {expectedAsset.FullPath} deleted from catalog (corrupted)." : $"Image {expectedAsset.FullPath} deleted from catalog.";

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNotNull(catalogChange.Asset);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageDataAssetDeleted(currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.AreEqual(CatalogChangeReason.AssetDeleted, catalogChange.Reason);
        Assert.AreEqual(expectedStatusMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset!.Folder.Path, currentDirectory))
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList());
        }

        Assert.AreEqual(expectedStatusMessage, _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeReason catalogChangeReason = string.Equals(expectedMessage, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE) ? CatalogChangeReason.BackupCreationStarted : CatalogChangeReason.BackupUpdateStarted;

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(catalogChangeReason, catalogChange.Reason);
        Assert.AreEqual(expectedMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual(expectedMessage, _applicationViewModel!.StatusMessage);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.BackupCompleted, catalogChange.Reason);
        Assert.AreEqual("Backup completed successfully.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("Backup completed successfully.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.CatalogProcessEnded, catalogChange.Reason);
        Assert.AreEqual("The catalog process has ended.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("The catalog process has ended.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private static void AssertCataloguedAssetsByPathPropertyValidity(IReadOnlyList<Asset> expectedAssets, CatalogChangeCallbackEventArgs catalogChange, int cataloguedAssetsByPathCount)
    {
        for (int i = 0; i < cataloguedAssetsByPathCount; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
                catalogChange.CataloguedAssetsByPath[i],
                currentExpectedAsset,
                currentExpectedAsset.FullPath,
                currentExpectedAsset.Folder.Path,
                currentExpectedAsset.Folder);
        }
    }

    private static void AssertCataloguedAssetsByPathImageData(
        Asset expectedNewAsset,
        string currentDirectory,
        CatalogChangeCallbackEventArgs catalogChange,
        int cataloguedAssetsByPathCount)
    {
        if (cataloguedAssetsByPathCount > 0)
        {
            // The ImageData of the last current has not been loaded yet
            for (int i = 0; i < cataloguedAssetsByPathCount - 1; i++)
            {
                Asset currentCataloguedAssetsByPath = catalogChange.CataloguedAssetsByPath[i];

                // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
                if (string.Equals(currentCataloguedAssetsByPath.FullPath, expectedNewAsset.FullPath))
                {
                    Assert.IsNull(currentCataloguedAssetsByPath.ImageData);
                }
                else if (!string.Equals(currentDirectory, currentCataloguedAssetsByPath.Folder.Path)) // All assets in other directories have ImageData null
                {
                    Assert.IsNull(currentCataloguedAssetsByPath.ImageData);
                }
                else
                {
                    Assert.IsNotNull(currentCataloguedAssetsByPath.ImageData);
                }
            }

            Assert.IsNull(catalogChange.CataloguedAssetsByPath[^1].ImageData);
        }
    }

    private static void AssertCataloguedAssetsByPathImageDataAssetDeleted(
        string currentDirectory,
        CatalogChangeCallbackEventArgs catalogChange,
        int cataloguedAssetsByPathCount)
    {
        if (cataloguedAssetsByPathCount > 0 && string.Equals(currentDirectory, catalogChange.CataloguedAssetsByPath[0].Folder.Path))
        {
            Assert.IsTrue(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData != null));
        }
        else
        {
            Assert.IsTrue(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData == null)); 
        }
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.IsNotNull(asset.ImageData); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void AssertObservableAssets(string currentDirectory, IReadOnlyList<Asset> expectedAssets, IReadOnlyList<Asset> observableAssets)
    {
        Assert.AreEqual(expectedAssets.Count, observableAssets.Count);

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAssets = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAssets, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            if (string.Equals(currentObservableAssets.Folder.Path, currentDirectory))
            {
                Assert.IsNotNull(currentObservableAssets.ImageData);
            }
            else
            {
                Assert.IsNull(currentObservableAssets.ImageData);
            }
        }
    }

    private static void CheckInstance(
        IReadOnlyList<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        IReadOnlyCollection<Asset> expectedObservableAssets,
        Asset expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;
        Assert.AreEqual(applicationViewModelInstances[0], applicationViewModelInstances[applicationViewModelInstancesCount - 2]);
        // No need to go deeper same instance because ref updated each time
        Assert.AreEqual(applicationViewModelInstances[applicationViewModelInstancesCount - 2], applicationViewModelInstances[applicationViewModelInstancesCount - 1]);

        CheckAfterNotifyCatalogChanges(
            applicationViewModelInstances[0],
            expectedLastDirectoryInspected,
            expectedAppTitleAssetsCount,
            expectedObservableAssets,
            expectedCurrentAsset,
            expectedFolder,
            expectedCanGoToNextAsset);
    }
}
