using PhotoManager.Application;
using PhotoManager.UI.Models;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelSetDuplicatesTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
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
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
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
        _findDuplicatedAssetsViewModel = new(application);
    }

    [Test]
    public void DuplicatedAssetPosition_ThumbnailUnavailable_DoesNotRefreshDuplicateSets()
    {
        IApplication application = Substitute.For<IApplication>();
        FindDuplicatedAssetsViewModel viewModel = new(application);

        Asset firstAsset = CreateTestAsset(@"C:\Photos", "first.jpg", "same-hash", SkiaImageData.Empty());
        Asset secondAsset = CreateTestAsset(@"C:\Photos", "second.jpg", "same-hash", null);
        Asset replacementAsset = CreateTestAsset(string.Empty, string.Empty, string.Empty, null);
        application.GetDuplicatedAssets().Returns([[replacementAsset]]);

        List<string> notifyPropertyChangedEvents = [];
        viewModel.PropertyChanged += delegate (object? _, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
        };

        viewModel.SetDuplicates([[firstAsset, secondAsset]]);
        notifyPropertyChangedEvents.Clear();

        viewModel.DuplicatedAssetPosition = 1;

        application.DidNotReceive().GetDuplicatedAssets();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.DuplicatedAssetSets, Has.Count.EqualTo(1));
            Assert.That(viewModel.CurrentDuplicatedAssetSet, Has.Count.EqualTo(2));
            Assert.That(viewModel.DuplicatedAssetPosition, Is.EqualTo(1));
            Assert.That(viewModel.CurrentDuplicatedAsset!.Asset, Is.SameAs(secondAsset));
            Assert.That(viewModel.CurrentDuplicatedAsset.Asset.FullPath,
                Is.EqualTo(Path.Combine(@"C:\Photos", "second.jpg")));
            Assert.That(viewModel.CurrentDuplicatedAsset.Asset.Pixel.Asset.Width, Is.EqualTo(640));
            Assert.That(viewModel.CurrentDuplicatedAsset.Asset.FileProperties.Size, Is.EqualTo(2048));
            Assert.That(notifyPropertyChangedEvents, Is.EqualTo(
                [
                    nameof(FindDuplicatedAssetsViewModel.DuplicatedAssetPosition),
                    nameof(FindDuplicatedAssetsViewModel.CurrentDuplicatedAsset)
                ]));
        }
    }

    [Test]
    public void DuplicatedAssetSetsPosition_PositionExceedsCount_ClampsToCurrentPosition()
    {
        IApplication application = Substitute.For<IApplication>();
        FindDuplicatedAssetsViewModel viewModel = new(application);

        Asset firstAsset = CreateTestAsset(@"C:\Photos", "first.jpg", "hash1", SkiaImageData.Empty());
        Asset secondAsset = CreateTestAsset(@"C:\Photos", "second.jpg", "hash1", SkiaImageData.Empty());

        viewModel.SetDuplicates([[firstAsset, secondAsset]]);

        viewModel.DuplicatedAssetSetsPosition = 10;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.DuplicatedAssetSets, Has.Count.EqualTo(1));
            Assert.That(viewModel.DuplicatedAssetSetsPosition, Is.Zero);
            Assert.That(viewModel.CurrentDuplicatedAssetSet, Has.Count.EqualTo(2));
            Assert.That(viewModel.CurrentDuplicatedAsset!.Asset, Is.SameAs(firstAsset));
        }
    }

    [Test]
    public void DuplicatedAssetSetsPosition_NegativePosition_ClampsToCurrentPosition()
    {
        IApplication application = Substitute.For<IApplication>();
        FindDuplicatedAssetsViewModel viewModel = new(application);

        Asset firstAsset = CreateTestAsset(@"C:\Photos", "first.jpg", "hash1", SkiaImageData.Empty());
        Asset secondAsset = CreateTestAsset(@"C:\Photos", "second.jpg", "hash1", SkiaImageData.Empty());

        viewModel.SetDuplicates([[firstAsset, secondAsset]]);

        viewModel.DuplicatedAssetSetsPosition = -1;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.DuplicatedAssetSets, Has.Count.EqualTo(1));
            Assert.That(viewModel.DuplicatedAssetSetsPosition, Is.Zero);
            Assert.That(viewModel.CurrentDuplicatedAssetSet, Has.Count.EqualTo(2));
            Assert.That(viewModel.CurrentDuplicatedAsset!.Asset, Is.SameAs(firstAsset));
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHash_SetsDuplicates()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHashAndCurrentAssetIsNotVisible_SetsDuplicates()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        // Change CurrentDuplicatedAsset Visibility
        _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0].IsVisible = false;

        duplicatedAssetSet1 = [];
        duplicatedAssetSet2 = [];

        duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            IsVisible = false,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHashAndSomeAssetsAreNotVisible_SetsDuplicates()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        // Change some DuplicatedAssetViewModel Visibility
        _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1].IsVisible = false;
        _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1].IsVisible = false;

        duplicatedAssetSet1 = [];
        duplicatedAssetSet2 = [];

        duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            IsVisible = false,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            IsVisible = false,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveNotSameHash_SetsDuplicatesWithoutCheck()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash2);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void
        SetDuplicates_AssetsSetsAndCurrentAssetHaveImageDataNotLoaded_SetsDuplicatesAndLoadsThumbnailWithoutRefresh()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string folderPath = _assetsDirectory!;
        Folder folder = _testableAssetRepository!.AddFolder(folderPath);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

        string filePath1 = Path.Combine(folderPath, _asset1.FileName);
        string filePath2 = Path.Combine(folderPath, _asset2.FileName);
        string filePath3 = Path.Combine(folderPath, _asset3.FileName);
        string filePath4 = Path.Combine(folderPath, _asset4.FileName);
        string filePath5 = Path.Combine(folderPath, _asset5.FileName);

        byte[] assetData1 = File.ReadAllBytes(filePath1);
        byte[] assetData2 = File.ReadAllBytes(filePath2);
        byte[] assetData3 = File.ReadAllBytes(filePath3);
        byte[] assetData4 = File.ReadAllBytes(filePath4);
        byte[] assetData5 = File.ReadAllBytes(filePath5);

        _testableAssetRepository!.AddAsset(_asset1, assetData1);
        _testableAssetRepository!.AddAsset(_asset2, assetData2);
        _testableAssetRepository!.AddAsset(_asset3, assetData3);
        _testableAssetRepository!.AddAsset(_asset4, assetData4);
        _testableAssetRepository!.AddAsset(_asset5, assetData5);

        // Mock like the ImageData of _asset1 has become null
        _asset1.ImageData = null;

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        // SetDuplicates as load the ImageData of _asset1
        _asset1.ImageData = SkiaImageData.Empty();

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void
        SetDuplicates_AssetsSetsAndCurrentAssetHaveNullImageData_SetsDuplicatesAndKeepsDuplicateSetsWithoutRefresh()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
        _asset2 = _asset2!.WithFolder(folder).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

        _asset4 = _asset4!.WithFolder(folder).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset2, assetData);
        _testableAssetRepository!.AddAsset(_asset3, assetData);
        _testableAssetRepository!.AddAsset(_asset4, assetData);
        _testableAssetRepository!.AddAsset(_asset5, assetData);

        // Mock like the ImageData of _asset1 has become null
        _asset1.ImageData = null;

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndSomeNotCurrentAssetsHaveNullImageData_SetsDuplicatesAndDoesNotRefresh()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        _asset2.ImageData = null;
        _asset5.ImageData = null;

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet1,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsContainsSomeEmptyDuplicatedAssetsSetAndOneNotEmpty_SetsDuplicates()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        const string hash1 = Hashes.IMAGE_1_JPG;

        _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

        DuplicatedSetViewModel duplicatedAssetSet = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet
        };
        duplicatedAssetSet.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet
        };
        duplicatedAssetSet.Add(duplicatedAssetViewModel2);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet, [], []];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [], []];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet,
            duplicatedAssetViewModel1);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            duplicatedAssetSet,
            duplicatedAssetViewModel1);
    }

    [Test]
    public void SetDuplicates_AssetsSetsOnlyContainsEmptyDuplicatedAssetsSet_DoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [[], []];
        List<List<Asset>> assetsSets = [[], []];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            0,
            0,
            [],
            null);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            0,
            0,
            [],
            null);
    }

    [Test]
    public void SetDuplicates_AssetsSetsIsEmpty_DoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        List<List<Asset>> assetsSets = [];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            [],
            0,
            0,
            [],
            null);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            [],
            0,
            0,
            [],
            null);
    }

    [Test]
    public void SetDuplicates_AssetsSetsIsNull_ThrowsArgumentNullException()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        List<List<Asset>> assetsSets = null!;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'assetsSets')"));
        Assert.That(exception?.ParamName, Is.EqualTo(nameof(assetsSets)));

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            [],
            0,
            0,
            [],
            null);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);
        Assert.That(messagesInformationSent, Is.Empty);
        Assert.That(findDuplicatedAssetsViewModelInstances, Is.Empty);
    }

    // This test is only about checking if all props in FindDuplicatedAssetsViewModel are well set
    [Test]
    public void DuplicatedAssets_SetDuplicatesAndAssetsSetsAndDuplicatesHaveSameHash_SetsProperties()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        string otherDirectory = Path.Combine(_assetsDirectory!, Directories.FOLDER_1);

        Folder folder1 = _testableAssetRepository!.AddFolder(_assetsDirectory!);
        Folder folder2 = _testableAssetRepository!.AddFolder(otherDirectory);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

        _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
        _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

        _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
        _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

        DuplicatedSetViewModel duplicatedAssetSet1 = [];
        DuplicatedSetViewModel duplicatedAssetSet2 = [];

        DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
        {
            Asset = _asset1,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
        {
            Asset = _asset3,
            ParentViewModel = duplicatedAssetSet1
        };
        duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

        DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
        {
            Asset = _asset2,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

        DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
        {
            Asset = _asset4,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

        DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
        {
            Asset = _asset5,
            ParentViewModel = duplicatedAssetSet2
        };
        duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

        List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
        List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

        _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

        // Check DuplicatedAssetSets
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Has.Count.EqualTo(2));

        // Check first DuplicatedAssetSet
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0],
            Has.Count.EqualTo(2));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
            Has.Count.EqualTo(2));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition
            ][
                _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition
            ],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[0]));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1]));

        _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;
        _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 2;

        // Check second DuplicatedAssetSet
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1],
            Has.Count.EqualTo(3));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
            Has.Count.EqualTo(3));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2]));

        Assert.That(
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition
            ][
                _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition
            ],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2]));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][0],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[0]));

        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1],
            Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1]));

        CheckAfterChanges(
            _findDuplicatedAssetsViewModel!,
            expectedDuplicatedAssetsSets,
            1,
            2,
            duplicatedAssetSet2,
            duplicatedAssetViewModel5);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
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
        // DuplicatedAssetPosition update
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));

        Assert.That(messagesInformationSent, Is.Empty);

        CheckInstance(
            findDuplicatedAssetsViewModelInstances,
            expectedDuplicatedAssetsSets,
            1,
            2,
            duplicatedAssetSet2,
            duplicatedAssetViewModel5);
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

    private static Asset CreateTestAsset(string folderPath, string fileName, string hash, IImageData? imageData)
    {
        return AssetBuilder.Create()
            .WithFolderPath(folderPath, Guid.NewGuid())
            .WithFileName(fileName)
            .WithPixels(640, 480, 200, 150)
            .WithFileProperties(2048, new(2026, 1, 2, 3, 4, 5), new(2026, 1, 3, 4, 5, 6))
            .WithThumbnailCreationDateTime(new(2026, 1, 2, 3, 4, 5))
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(hash)
            .WithImageData(imageData)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }
}
