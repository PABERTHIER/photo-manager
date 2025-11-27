using PhotoManager.UI.Models;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Integration.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset1Temp;
    private Asset _asset2Temp;
    private Asset _asset3Temp;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset1Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
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
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureFindDuplicatedAssetsViewModel(
        int catalogBatchSize,
        string assetsDirectory,
        string exemptedFolderPath,
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        bool usingDHash,
        bool usingMD5Hash,
        bool usingPHash,
        bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, exemptedFolderPath);
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
        _assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (_application);
        _findDuplicatedAssetsViewModel = new (_application);
    }

    [Test]
    public async Task SetDuplicates_CataloguedAssetsAndOneDuplicatesSet_SetsDuplicates()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SetDuplicates_CataloguedAssetsAndNoDuplicates_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task SetDuplicates_NoCataloguedAssets_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task Refresh_DuplicatesAndPreviousDuplicatedAssetSets_SetsDuplicatedAssetSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet];
            List<List<Asset>> duplicatedAssetsSets = [[_asset2, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            // Refresh
            _findDuplicatedAssetsViewModel!.Refresh();

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(10));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // Refresh
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[9], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task Refresh_CataloguedAssetsAndDuplicatesAndNoPreviousDuplicatedAssetSets_SetsDuplicatedAssetSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.Refresh();

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // Refresh
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task Refresh_CataloguedAssetsAndNoDuplicates_SetsDuplicatedAssetSetsToEmpty()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            _findDuplicatedAssetsViewModel!.Refresh();

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // Refresh
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task Refresh_NoCataloguedAssets_SetsDuplicatedAssetSetsToEmpty()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            _findDuplicatedAssetsViewModel!.Refresh();

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // Refresh
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CollapseAssets_CataloguedAssetsAndCollapseAllDuplicatedAsset_CollapsesAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CollapseAssets_CataloguedAssetsAndCollapseCurrentDuplicatedAsset_CollapsesAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CollapseAssets_CataloguedAssetsAndCollapseNotCurrentDuplicatedAsset_CollapsesAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CollapseAssets_NoCataloguedAssets_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            _findDuplicatedAssetsViewModel!.CollapseAssets([]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsAndCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsAndNotCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset2);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsAndNoDuplicates_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset?.Asset!);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);
            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_NoCataloguedAssets_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset?.Asset!);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndDuplicatesAndOneMatchingDuplicatesInExemptedFolder_ReturnsOtherDuplicatedAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG);
            string imagePath1ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_9_PNG);

            File.Copy(imagePath1, imagePath1ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(3));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(duplicatedAssetsSets[0][2].FileName, Is.EqualTo(_asset2Temp.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _asset2Temp = _asset2Temp.WithFolder(exemptedFolder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2Temp,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset2);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(20));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndDuplicatesAndTwoSameMatchingDuplicatesInExemptedFolder_ReturnsOtherDuplicatedAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath1ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_1_DUPLICATE_JPG);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath1, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(3));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset1Temp.FileName));
            Assert.That(duplicatedAssetsSets[0][2].FileName, Is.EqualTo(_asset3Temp.FileName));
            Assert.That(duplicatedAssetsSets[1], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[1][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[1][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _asset1Temp = _asset1Temp.WithFolder(exemptedFolder!);
            _asset3Temp = _asset3Temp.WithFolder(exemptedFolder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1Temp,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3Temp,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(21));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndDuplicatesAndOneMatchingDuplicateInExemptedFolder_ReturnsOtherDuplicatedAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath1ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_1_JPG);

            File.Copy(imagePath1, imagePath1ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset1Temp.FileName));
            Assert.That(duplicatedAssetsSets[1], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[1][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[1][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _asset1Temp = _asset1Temp.WithFolder(exemptedFolder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1Temp,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(20));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndDuplicatesAndNoMatchingDuplicatesInExemptedFolder_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_2_JPG);
            string imagePath1ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_2_JPG);

            File.Copy(imagePath1, imagePath1ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(20));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndDuplicatesAndNoAssetsInExemptedFolder_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(1));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset3.FileName));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(19));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_CataloguedAssetsAndNoDuplicates_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(13));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);
            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task GetNotExemptedDuplicatedAssets_NoCataloguedAssets_ReturnsEmptyList()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    private
        (
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
        List<string> notifyApplicationVmPropertyChangedEvents,
        List<MessageBoxInformationSentEventArgs> messagesInformationSent,
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        )
        NotifyPropertyChangedEvents()
    {
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents = [];
        List<string> notifyApplicationVmPropertyChangedEvents = [];
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances = [];

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyFindDuplicatedAssetsVmPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        _applicationViewModel!.PropertyChanged += delegate(object? _, PropertyChangedEventArgs e)
        {
            notifyApplicationVmPropertyChangedEvents.Add(e.PropertyName!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent += delegate(object _, MessageBoxInformationSentEventArgs e)
        {
            messagesInformationSent.Add(e);
        };

        return
        (
            notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            notifyApplicationVmPropertyChangedEvents,
            messagesInformationSent,
            findDuplicatedAssetsViewModelInstances
        );
    }

    private void CheckBeforeChanges()
    {
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset, Is.Null);
    }

    private static void CheckAfterChanges(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        AssertDuplicatedAssetSets(findDuplicatedAssetsViewModelInstance, expectedDuplicatedAssetSets);

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition, Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition, Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet, expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset, expectedCurrentDuplicatedAsset);
    }

    private static void AssertDuplicatedAssetSets(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets)
    {
        if (expectedDuplicatedAssetSets.Count > 0)
        {
            for (int i = 0; i < expectedDuplicatedAssetSets.Count; i++)
            {
                AssertDuplicatedAssetsSet(
                    findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets[i],
                    expectedDuplicatedAssetSets[i]);
            }
        }
        else
        {
            Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets, Is.Empty);
        }
    }

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet, DuplicatedSetViewModel expectedDuplicatedAssetSet)
    {
        if (expectedDuplicatedAssetSet.Count > 0)
        {
            AssertDuplicatedSet(duplicatedAssetSet, expectedDuplicatedAssetSet);

            for (int i = 0; i < expectedDuplicatedAssetSet.Count; i++)
            {
                AssertDuplicatedAsset(duplicatedAssetSet[i], expectedDuplicatedAssetSet[i]);
            }
        }
        else
        {
            Assert.That(duplicatedAssetSet, Is.Empty);
        }
    }

    private static void AssertDuplicatedSet(
        DuplicatedSetViewModel duplicatedSetViewModel,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));

        Assert.That(duplicatedSetViewModel.Visible, Is.EqualTo(expectedDuplicatedSetViewModel.Visible));
    }

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset, DuplicatedAssetViewModel? expectedDuplicatedAsset)
    {
        if (expectedDuplicatedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAsset!.Asset, expectedDuplicatedAsset.Asset);

            Assert.That(duplicatedAsset.Visible, Is.EqualTo(expectedDuplicatedAsset.Visible));

            if (expectedDuplicatedAsset.ParentViewModel.Count > 0)
            {
                AssertDuplicatedSet(duplicatedAsset.ParentViewModel, expectedDuplicatedAsset.ParentViewModel);

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    Assert.That(duplicatedAsset.ParentViewModel[i].Visible, Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].Visible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset, expectedDuplicatedAsset.ParentViewModel[i].Asset);
                }
            }
            else
            {
                Assert.That(duplicatedAsset.ParentViewModel, Is.Empty);
            }
        }
        else
        {
            Assert.That(duplicatedAsset, Is.Null);
        }
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
            asset,
            expectedAsset,
            expectedAsset.FullPath,
            expectedAsset.Folder.Path,
            expectedAsset.Folder);
        // Unlike below (Application, CatalogAssetsService), it is set here for assets in the current directory
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null); 
    }

    private static void CheckInstance(
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        int findDuplicatedAssetsViewModelInstancesCount = findDuplicatedAssetsViewModelInstances.Count;

        if (findDuplicatedAssetsViewModelInstancesCount > 1)
        {
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 1],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2]));
        }

        if (findDuplicatedAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                findDuplicatedAssetsViewModelInstances[0],
                expectedDuplicatedAssetSets,
                expectedDuplicatedAssetSetsPosition,
                expectedDuplicatedAssetPosition,
                expectedCurrentDuplicatedAssetSet,
                expectedCurrentDuplicatedAsset);
        }
    }
}
