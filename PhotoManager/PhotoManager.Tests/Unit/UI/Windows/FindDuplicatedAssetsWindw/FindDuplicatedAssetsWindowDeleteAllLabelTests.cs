using PhotoManager.UI.Models;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.Windows.FindDuplicatedAssetsWindw;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class FindDuplicatedAssetsWindowDeleteAllLabelTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;

#pragma warning disable CS0067 // Event is never used
    private event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;
    private event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
#pragma warning restore CS0067 // Event is never used
    private event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssets;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void Setup()
    {
        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(2020, new(2010, 1, 1, 20, 20, 20, 20, 20), new(2011, 1, 1, 20, 20, 20, 20, 20))
            .WithThumbnailCreationDateTime(new(2010, 1, 1, 20, 20, 20, 20, 20))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileProperties(2048, new(2020, 6, 1), new(2020, 7, 1))
            .WithThumbnailCreationDateTime(new(2020, 6, 1))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_3_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG,
                ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG)
            .WithFileProperties(2000, new(2010, 1, 1), new(2011, 1, 1))
            .WithThumbnailCreationDateTime(new(2010, 1, 1))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_4_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG,
                ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG)
            .WithFileProperties(2030, new(2010, 8, 1), new(2011, 9, 1))
            .WithThumbnailCreationDateTime(new(2010, 8, 1))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_5_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG,
                ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG)
            .WithFileProperties(2048, new(2020, 6, 1), new(2020, 7, 1))
            .WithThumbnailCreationDateTime(new(2020, 6, 1))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset6 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Set in each tests
            .WithFileName(FileNames.IMAGE_5_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG,
                ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG)
            .WithFileProperties(2048, new(2020, 6, 1), new(2020, 7, 1))
            .WithThumbnailCreationDateTime(new(2020, 6, 1))
            .WithHash(string.Empty) // Set in each tests
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
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
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
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

        _userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, _userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            _userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            _userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);
        _findDuplicatedAssetsViewModel = new(application);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndThreeSetsAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssetsInTheSet()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
        const string hash3 = Hashes.IMAGE_5_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

        _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
        _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

        List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        // First DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            1,
            0,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetViewModel3);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Second DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            2,
            0,
            expectedDuplicatedAssetSet3,
            expectedDuplicatedAssetViewModel5);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset3);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Third DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 3
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset3);
        Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndThreeSetsAndCurrentDuplicatedAssetFromSecondSet_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssetsInTheSet()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
        const string hash3 = Hashes.IMAGE_5_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

        _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
        _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

        List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

        // First DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            2,
            0,
            expectedDuplicatedAssetSet3,
            expectedDuplicatedAssetViewModel5);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetSetsPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Second DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetSetsPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Third DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetSetsPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 3

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset5);
        Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset2);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndThreeSetsAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssetsInTheSet()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
        const string hash3 = Hashes.IMAGE_5_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

        _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
        _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

        List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        // First DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Second DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            2,
            0,
            expectedDuplicatedAssetSet3,
            expectedDuplicatedAssetViewModel5);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset6);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset1);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Third DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];
        expectedDuplicatedAssetSet3 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet3
        };
        expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets =
        [
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetSet3
        ];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 3
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset6);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset1);
        Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset4);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndTwoSetsAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssetsInTheSet()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);
        _asset6 = _asset6!.WithFolder(folder2).WithHash(hash1);

        _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset6], [_asset3, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        // First DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            1,
            0,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetViewModel4);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Second DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndTwoSetsAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssetsInTheSet()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);
        _asset6 = _asset6!.WithFolder(folder2).WithHash(hash1);

        _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset6], [_asset3, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        // First DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
        DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            1,
            0,
            expectedDuplicatedAssetSet2,
            expectedDuplicatedAssetViewModel4);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        // Second DeleteAll
        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        expectedDuplicatedAssetSet1 = [];
        expectedDuplicatedAssetSet2 = [];

        expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

        expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

        expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

        expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

        expectedDuplicatedAssetViewModel6 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

        expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets 2
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);
        Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset3);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithMultipleAssetsAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(4));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset3);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset4);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][3], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithMultipleAssetsAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string folder1Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);
        string folder2Directory = Path.Combine(_assetsDirectory!, Directories.FOLDER_2);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(folder1Directory);
        Folder folder3 = _testableAssetRepository!.AddFolder(folder2Directory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset2 = _asset2!.WithFolder(folder3).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(4));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset3);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset4);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][3], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndFirstAssetIsTheCurrent_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset5);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndFirstAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset5);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndSecondAssetIsTheCurrent_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndSecondAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndThirdAssetIsTheCurrent_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 2;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            2,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel3);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            2,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel3);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndThirdAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset5,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset6,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset5);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithTwoAssetsAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
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

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithTwoAssetsAndCurrentDuplicatedAssetAndNewAssetPosition_SendsDeleteDuplicatedAssetsEventAndCollapsesNotCurrentDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
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
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithTwoAssetsAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesCurrentDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
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

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetWithTwoAssetsAndNotCurrentDuplicatedAssetAndNewAssetPosition_SendsDeleteDuplicatedAssetsEventAndCollapsesCurrentDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            1,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel2);
    }

    // This case cannot happen (having same file in same folder) and shows that those assets cannot be picked up
    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetAndCurrentDuplicatedAssetAndTwoAssetsWithSameNameInSameFolder_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset5 = _asset5!.WithFolder(folder).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset6,
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

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    // This case cannot happen (having same file in same folder) and shows that those assets cannot be picked up
    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetAndNotCurrentDuplicatedAssetAndTwoAssetsWithSameNameInSameFolder_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset5 = _asset5!.WithFolder(folder).WithHash(hash);
        _asset6 = _asset6!.WithFolder(folder).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset5, _asset6]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset6,
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

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetNotVisibleAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].IsVisible = false;
        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].IsVisible = false;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetNotVisibleAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].IsVisible = false;
        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].IsVisible = false;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetContainsOneAssetNotVisibleAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndAndCollapsesVisibleDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].IsVisible = false;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndOneSetContainsOneAssetNotVisibleAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesVisibleDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].IsVisible = false;

        DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
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
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
        // CollapseAssets

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
        AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void
        DeleteAllLabel_DuplicatesAndDuplicatedAssetViewModelIsUnknown_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        const string hash = Hashes.IMAGE_1_JPG;

        _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

        List<List<Asset>> assetsSets = [[_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

        DuplicatedAssetViewModel unknownDuplicatedAssetViewModel = new()
        {
            Asset = _asset1!,
            ParentViewModel = unknownDuplicatedAssetSet
        };
        unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel);

        DeleteAll(unknownDuplicatedAssetViewModel);

        DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
        {
            Asset = _asset2,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
        {
            Asset = _asset4,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet
        };
        expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void DeleteAllLabel_NoDuplicatesAndDuplicatedAssetViewModel_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        DuplicatedSetViewModel duplicatedAssetSet = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel = new()
        {
            Asset = _asset1!,
            ParentViewModel = duplicatedAssetSet
        };
        duplicatedAssetSet.Add(duplicatedAssetViewModel);

        DeleteAll(duplicatedAssetViewModel);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            [],
            0,
            0,
            [],
            null);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
        Assert.That(messagesInformationSent[0].Message,
            Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
        Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

        Assert.That(getExemptedFolderPathEvents, Is.Empty);

        Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
        Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            [],
            0,
            0,
            [],
            null);
    }

    [Test]
    public void DeleteAllLabel_DuplicatesAndDuplicatedAssetViewModelIsNull_ThrowNullReferenceException()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(assetsSets));

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => DeleteAll(null!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

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
            Asset = _asset3,
            ParentViewModel = expectedDuplicatedAssetSet1
        };
        expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

        DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
        {
            Asset = _asset4,
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
            Asset = _asset5,
            ParentViewModel = expectedDuplicatedAssetSet2
        };
        expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetDuplicates
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);
        Assert.That(deleteDuplicatedAssetsEvents, Is.Empty);
        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            expectedDuplicatedAssetSet1,
            expectedDuplicatedAssetViewModel1);
    }

    [Test]
    public void DeleteAllLabel_NoDuplicatesAndDuplicatedAssetViewModelIsNull_ThrowNullReferenceException()
    {
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        CheckBeforeChanges();

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => DeleteAll(null!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            [],
            0,
            0,
            [],
            null);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);
        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(getExemptedFolderPathEvents, Is.Empty);
        Assert.That(deleteDuplicatedAssetsEvents, Is.Empty);
        Assert.That(refreshAssetsCounterEvents, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            [],
            0,
            0,
            [],
            null);
    }

    private
        (
        List<string> notifyPropertyChangedEvents,
        List<MessageBoxInformationSentEventArgs> messagesInformationSent,
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        )
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances = [];

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent +=
            delegate (object _, MessageBoxInformationSentEventArgs e)
            {
                messagesInformationSent.Add(e);
            };

        return (notifyPropertyChangedEvents, messagesInformationSent, findDuplicatedAssetsViewModelInstances);
    }

    private List<string> NotifyGetExemptedFolderPath(string exemptedFolderPathToReturn)
    {
        List<string> getExemptedFolderPathEvents = [];

        GetExemptedFolderPath += delegate
        {
            getExemptedFolderPathEvents.Add(exemptedFolderPathToReturn);

            return exemptedFolderPathToReturn;
        };

        return getExemptedFolderPathEvents;
    }

    private List<Asset[]> NotifyDeleteDuplicatedAssets()
    {
        List<Asset[]> deleteDuplicatedAssetsEvents = [];

        DeleteDuplicatedAssets += delegate (object _, Asset[] asset)
        {
            deleteDuplicatedAssetsEvents.Add(asset);
        };

        return deleteDuplicatedAssetsEvents;
    }

    private List<string> NotifyRefreshAssetsCounter()
    {
        List<string> refreshAssetsCounterEvents = [];

        RefreshAssetsCounter += delegate
        {
            refreshAssetsCounterEvents.Add(string.Empty);
        };

        return refreshAssetsCounterEvents;
    }

    private void CheckBeforeChanges()
    {
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition, Is.Zero);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetPosition, Is.Zero);
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

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition,
            Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition,
            Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet,
            expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset,
            expectedCurrentDuplicatedAsset);
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

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet,
        DuplicatedSetViewModel expectedDuplicatedAssetSet)
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

        Assert.That(duplicatedSetViewModel.IsVisible, Is.EqualTo(expectedDuplicatedSetViewModel.IsVisible));
    }

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset,
        DuplicatedAssetViewModel? expectedDuplicatedAsset)
    {
        if (expectedDuplicatedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAsset!.Asset, expectedDuplicatedAsset.Asset);

            Assert.That(duplicatedAsset.IsVisible, Is.EqualTo(expectedDuplicatedAsset.IsVisible));

            if (expectedDuplicatedAsset.ParentViewModel.Count > 0)
            {
                AssertDuplicatedSet(duplicatedAsset.ParentViewModel, expectedDuplicatedAsset.ParentViewModel);

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    Assert.That(duplicatedAsset.ParentViewModel[i].IsVisible,
                        Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].IsVisible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset,
                        expectedDuplicatedAsset.ParentViewModel[i].Asset);
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
        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(expectedAsset.Folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(expectedAsset.Folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(expectedAsset.ThumbnailCreationDateTime));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null);
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(expectedAsset.FullPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(expectedAsset.Folder.Path));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(expectedAsset.FileProperties.Creation.Date));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
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

    private void DeleteAll(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        List<DuplicatedAssetViewModel> assetsToDelete =
            _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel!.CollapseAssets(assetsToDelete);
    }
}
