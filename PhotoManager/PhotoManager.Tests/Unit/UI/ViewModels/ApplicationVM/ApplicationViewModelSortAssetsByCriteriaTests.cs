using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelSortAssetsByCriteriaTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        Guid folderId = Guid.NewGuid();

        _asset1 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _assetsDirectory },
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty,
            ImageData = SkiaImageData.Empty(),
            FileProperties = new()
            {
                Size = 2020,
                Creation = new(2010, 1, 1, 20, 20, 20, 20, 20),
                Modification = new(2011, 1, 1, 20, 20, 20, 20, 20)
            },
            ThumbnailCreationDateTime = new(2010, 1, 1, 20, 20, 20, 20, 20),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _assetsDirectory },
            FileName = FileNames.IMAGE_2_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
            },
            Hash = string.Empty,
            ImageData = SkiaImageData.Empty(),
            FileProperties = new()
            {
                Size = 2048,
                Creation = new(2020, 6, 1),
                Modification = new(2020, 7, 1)
            },
            ThumbnailCreationDateTime = new(2020, 6, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _assetsDirectory },
            FileName = FileNames.IMAGE_3_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_3_JPG, Height = PixelHeightAsset.IMAGE_3_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_3_JPG, Height = ThumbnailHeightAsset.IMAGE_3_JPG }
            },
            Hash = string.Empty,
            ImageData = SkiaImageData.Empty(),
            FileProperties = new()
            {
                Size = 2000,
                Creation = new(2010, 1, 1),
                Modification = new(2011, 1, 1)
            },
            ThumbnailCreationDateTime = new(2010, 1, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _assetsDirectory },
            FileName = FileNames.IMAGE_4_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_4_JPG, Height = PixelHeightAsset.IMAGE_4_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_4_JPG, Height = ThumbnailHeightAsset.IMAGE_4_JPG }
            },
            Hash = string.Empty,
            ImageData = SkiaImageData.Empty(),
            FileProperties = new()
            {
                Size = 2030,
                Creation = new(2010, 8, 1),
                Modification = new(2011, 9, 1)
            },
            ThumbnailCreationDateTime = new(2010, 8, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _assetsDirectory },
            FileName = FileNames.IMAGE_5_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_5_JPG, Height = PixelHeightAsset.IMAGE_5_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_5_JPG, Height = ThumbnailHeightAsset.IMAGE_5_JPG }
            },
            Hash = string.Empty,
            ImageData = SkiaImageData.Empty(),
            FileProperties = new()
            {
                Size = 2048,
                Creation = new(2020, 6, 1),
                Modification = new(2020, 7, 1)
            },
            ThumbnailCreationDateTime = new(2020, 6, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplicationViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
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

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);
        _applicationViewModel = new(application);
    }

    [Test]
    [TestCase(SortCriteria.FileName, false)]
    [TestCase(SortCriteria.FileSize, true)]
    [TestCase(SortCriteria.FileCreationDateTime, true)]
    [TestCase(SortCriteria.FileModificationDateTime, true)]
    [TestCase(SortCriteria.ThumbnailCreationDateTime, true)]
    public void SortAssetsByCriteria_OneAsset_SortsByCriteria(SortCriteria sortCriteria, bool expectedSortAscending)
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset1!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        string expectedAppTitle = sortCriteria switch
        {
            SortCriteria.FileName =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by file name descending",
            SortCriteria.FileSize =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by file size ascending",
            SortCriteria.FileCreationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by file creation ascending",
            SortCriteria.FileModificationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by file modification ascending",
            SortCriteria.ThumbnailCreationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by thumbnail creation ascending",
            _ => $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by  ascending"
        };

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            expectedSortAscending,
            sortCriteria,
            expectedAppTitle,
            assets,
            assets[0],
            assets[0].Folder,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            expectedSortAscending,
            sortCriteria,
            expectedAppTitle,
            assets,
            assets[0],
            assets[0].Folder,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileName_SortsByFileName()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileName;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file name ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Asset[] expectedAssetsUpdated = [_asset5!, _asset4!, _asset3!, _asset2!, _asset1!];
        string expectedAppTitleUpdated =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file name descending";

        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedAppTitleUpdated,
            expectedAssetsUpdated,
            expectedAssetsUpdated[0],
            expectedAssetsUpdated[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedAppTitleUpdated,
            expectedAssetsUpdated,
            expectedAssetsUpdated[0],
            expectedAssetsUpdated[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileName_SortsByFileNameAscendingOrDescending()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileName;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAscendingAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];
        string expectedAscendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file name ascending";

        Asset[] expectedDescendingAssets = [_asset5!, _asset4!, _asset3!, _asset2!, _asset1!];
        string expectedDescendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file name descending";

        for (int i = 0; i < 10; i++)
        {
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            if (i % 2 == 0) // Descending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    false,
                    sortCriteria,
                    expectedDescendingAppTitle,
                    expectedDescendingAssets,
                    expectedDescendingAssets[0],
                    expectedDescendingAssets[0].Folder,
                    true);
            }
            else // Ascending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    true,
                    sortCriteria,
                    expectedAscendingAppTitle,
                    expectedAscendingAssets,
                    expectedAscendingAssets[0],
                    expectedAscendingAssets[0].Folder,
                    true);
            }
        }

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAscendingAppTitle,
            expectedAscendingAssets,
            expectedAscendingAssets[0],
            expectedAscendingAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileSize_SortsByFileSizeAndThenByFileName()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileSize;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        Asset[] expectedAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file size ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileSize_SortsByFileSizeAscendingOrDescending()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileSize;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAscendingAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAscendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file size ascending";

        Asset[] expectedDescendingAssets = [_asset5!, _asset2!, _asset4!, _asset1!, _asset3!];
        string expectedDescendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file size descending";

        for (int i = 0; i < 10; i++)
        {
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            if (i % 2 == 0) // Ascending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    true,
                    sortCriteria,
                    expectedAscendingAppTitle,
                    expectedAscendingAssets,
                    expectedAscendingAssets[0],
                    expectedAscendingAssets[0].Folder,
                    true);
            }
            else // Descending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    false,
                    sortCriteria,
                    expectedDescendingAppTitle,
                    expectedDescendingAssets,
                    expectedDescendingAssets[0],
                    expectedDescendingAssets[0].Folder,
                    true);
            }
        }

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedDescendingAppTitle,
            expectedDescendingAssets,
            expectedDescendingAssets[0],
            expectedDescendingAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndSortByFileCreationDateTime_SortsByFileCreationDateTimeAndThenByFileName()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileCreationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        Asset[] expectedAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file creation ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileCreationDateTime_SortsByFileCreationDateTimeAscendingOrDescending()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileCreationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAscendingAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAscendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file creation ascending";

        Asset[] expectedDescendingAssets = [_asset5!, _asset2!, _asset4!, _asset1!, _asset3!];
        string expectedDescendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file creation descending";

        for (int i = 0; i < 10; i++)
        {
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            if (i % 2 == 0) // Ascending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    true,
                    sortCriteria,
                    expectedAscendingAppTitle,
                    expectedAscendingAssets,
                    expectedAscendingAssets[0],
                    expectedAscendingAssets[0].Folder,
                    true);
            }
            else // Descending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    false,
                    sortCriteria,
                    expectedDescendingAppTitle,
                    expectedDescendingAssets,
                    expectedDescendingAssets[0],
                    expectedDescendingAssets[0].Folder,
                    true);
            }
        }

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedDescendingAppTitle,
            expectedDescendingAssets,
            expectedDescendingAssets[0],
            expectedDescendingAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndSortByFileModificationDateTime_SortsByFileModificationDateTimeAndThenByFileName()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileModificationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        Asset[] expectedAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file modification ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileModificationDateTime_SortsByFileModificationDateTimeAscendingOrDescending()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.FileModificationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAscendingAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAscendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file modification ascending";

        Asset[] expectedDescendingAssets = [_asset5!, _asset2!, _asset4!, _asset1!, _asset3!];
        string expectedDescendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by file modification descending";

        for (int i = 0; i < 10; i++)
        {
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            if (i % 2 == 0) // Ascending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    true,
                    sortCriteria,
                    expectedAscendingAppTitle,
                    expectedAscendingAssets,
                    expectedAscendingAssets[0],
                    expectedAscendingAssets[0].Folder,
                    true);
            }
            else // Descending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    false,
                    sortCriteria,
                    expectedDescendingAppTitle,
                    expectedDescendingAssets,
                    expectedDescendingAssets[0],
                    expectedDescendingAssets[0].Folder,
                    true);
            }
        }

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedDescendingAppTitle,
            expectedDescendingAssets,
            expectedDescendingAssets[0],
            expectedDescendingAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndSortByThumbnailCreationDateTime_SortsByThumbnailCreationDateTimeAndThenByFileName()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.ThumbnailCreationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        Asset[] expectedAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by thumbnail creation ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            sortCriteria,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByThumbnailCreationDateTime_SortsByThumbnailCreationDateTimeAscendingOrDescending()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset5!, _asset2!, _asset1!, _asset3!, _asset4!];

        const SortCriteria sortCriteria = SortCriteria.ThumbnailCreationDateTime;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        Asset[] expectedAscendingAssets = [_asset3!, _asset1!, _asset4!, _asset2!, _asset5!];
        string expectedAscendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by thumbnail creation ascending";

        Asset[] expectedDescendingAssets = [_asset5!, _asset2!, _asset4!, _asset1!, _asset3!];
        string expectedDescendingAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 5 - sorted by thumbnail creation descending";

        for (int i = 0; i < 10; i++)
        {
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            if (i % 2 == 0) // Ascending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    true,
                    sortCriteria,
                    expectedAscendingAppTitle,
                    expectedAscendingAssets,
                    expectedAscendingAssets[0],
                    expectedAscendingAssets[0].Folder,
                    true);
            }
            else // Descending
            {
                CheckAfterChanges(
                    _applicationViewModel!,
                    _assetsDirectory!,
                    false,
                    sortCriteria,
                    expectedDescendingAppTitle,
                    expectedDescendingAssets,
                    expectedDescendingAssets[0],
                    expectedDescendingAssets[0].Folder,
                    true);
            }
        }

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            false,
            sortCriteria,
            expectedDescendingAppTitle,
            expectedDescendingAssets,
            expectedDescendingAssets[0],
            expectedDescendingAssets[0].Folder,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    [TestCase(SortCriteria.FileName, false)]
    [TestCase(SortCriteria.FileSize, true)]
    [TestCase(SortCriteria.FileCreationDateTime, true)]
    [TestCase(SortCriteria.FileModificationDateTime, true)]
    [TestCase(SortCriteria.ThumbnailCreationDateTime, true)]
    public void SortAssetsByCriteria_NoCataloguedAssets_DoesNothing(SortCriteria sortCriteria,
        bool expectedSortAscending)
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

        string expectedAppTitle = sortCriteria switch
        {
            SortCriteria.FileName =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by file name descending",
            SortCriteria.FileSize =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by file size ascending",
            SortCriteria.FileCreationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by file creation ascending",
            SortCriteria.FileModificationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by file modification ascending",
            SortCriteria.ThumbnailCreationDateTime =>
                $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by thumbnail creation ascending",
            _ => $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 0 of 0 - sorted by  ascending"
        };

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            expectedSortAscending,
            sortCriteria,
            expectedAppTitle,
            [],
            null,
            null!,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        // SortAssetsByCriteria
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("SortCriteria"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            expectedSortAscending,
            sortCriteria,
            expectedAppTitle,
            [],
            null,
            null!,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void SortAssetsByCriteria_InvalidSortCriteria_ThrowsArgumentOutOfRangeException()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Asset[] assets = [_asset1!];
        const SortCriteria invalidSortCriteria = (SortCriteria)999;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        ArgumentOutOfRangeException? exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _applicationViewModel!.SortAssetsByCriteria(invalidSortCriteria));

        Assert.That(exception?.Message, Is.EqualTo($"Unknown sort criteria (Parameter '{nameof(SortCriteria)}')"));

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory} - image 1 of 1 - sorted by file name ascending";

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            true,
            invalidSortCriteria,
            expectedAppTitle,
            assets,
            assets[0],
            assets[0].Folder,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            true,
            invalidSortCriteria,
            expectedAppTitle,
            assets,
            assets[0],
            assets[0].Folder,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    private
        (List<string> notifyPropertyChangedEvents,
        List<ApplicationViewModel> applicationViewModelInstances,
        List<Folder> folderAddedEvents,
        List<Folder> folderRemovedEvents)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<ApplicationViewModel> applicationViewModelInstances = [];

        _applicationViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            applicationViewModelInstances.Add((ApplicationViewModel)sender!);
        };

        List<Folder> folderAddedEvents = [];

        _applicationViewModel.FolderAdded += delegate (object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = [];

        _applicationViewModel.FolderRemoved += delegate (object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private void CheckBeforeChanges(string expectedRootDirectory)
    {
        Assert.That(_applicationViewModel!.SortAscending, Is.True);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.IsThumbnailsVisible, Is.True);
        Assert.That(_applicationViewModel!.IsViewerVisible, Is.False);
        Assert.That(_applicationViewModel!.ViewerPosition, Is.Zero);
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.AppTitle,
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        bool expectedSortAscending,
        SortCriteria expectedSortCriteria,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.EqualTo(expectedSortAscending));
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(expectedSortCriteria));
        Assert.That(applicationViewModelInstance.IsThumbnailsVisible, Is.True);
        Assert.That(applicationViewModelInstance.IsViewerVisible, Is.False);
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.Zero);
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets,
            applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(string.Empty));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset,
                expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
    {
        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(expectedAsset.ThumbnailCreationDateTime));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(assetPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(folderPath));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(expectedAsset.FileProperties.Creation.Date));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath,
                currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            if (string.Equals(currentObservableAsset.Folder.Path, currentDirectory))
            {
                Assert.That(currentObservableAsset.ImageData, Is.Not.Null);
            }
            else
            {
                Assert.That(currentObservableAsset.ImageData, Is.Null);
            }
        }
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        bool expectedSortAscending,
        SortCriteria expectedSortCriteria,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2],
                Is.EqualTo(applicationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1],
                Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));
        }

        if (applicationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                applicationViewModelInstances[0],
                expectedLastDirectoryInspected,
                expectedSortAscending,
                expectedSortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToNextAsset);
        }
    }
}
