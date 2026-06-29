using Avalonia.Controls;
using PhotoManager.Application;
using PhotoManager.UI.Models;
using PhotoManager.UI.Windows;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.UI.Windows;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class FindDuplicatedAssetsWindowTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<FindDuplicatedAssetsWindow> _testLogger = new();

    private event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;
    private event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
    private event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssets;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;
    private Asset? _asset9;
    private Asset? _asset10;
    private Asset? _asset11;
    private Asset? _asset12;
    private Asset? _asset13;
    private Asset? _asset14;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        DateTime actualDate = DateTime.Now;

        _asset1 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_1_JPG)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_2_DUPLICATED_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_2_DUPLICATED_JPG, PixelHeightAsset.IMAGE_2_DUPLICATED_JPG,
                ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG)
            .WithFileProperties(FileSize.IMAGE_2_DUPLICATED_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_2_DUPLICATED_JPG)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileProperties(FileSize.IMAGE_2_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_2_JPG)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithPixels(PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset6 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_1_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset7 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_1_DUPLICATE_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
                ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG)
            .WithFileProperties(FileSize.IMAGE_1_DUPLICATE_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_1_DUPLICATE_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset8 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithPixels(PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset9 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_9_DUPLICATE_PNG)
            .WithPixels(PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, PixelHeightAsset.IMAGE_9_DUPLICATE_PNG,
                ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG)
            .WithFileProperties(FileSize.IMAGE_9_DUPLICATE_PNG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_9_DUPLICATE_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset10 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset11 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames._1336_JPG)
            .WithPixels(PixelWidthAsset._1336_JPG, PixelHeightAsset._1336_JPG,
                ThumbnailWidthAsset._1336_JPG, ThumbnailHeightAsset._1336_JPG)
            .WithFileProperties(FileSize._1336_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes._1336_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset12 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames._1336_ORIGINAL_JPG)
            .WithPixels(PixelWidthAsset._1336_ORIGINAL_JPG, PixelHeightAsset._1336_ORIGINAL_JPG,
                ThumbnailWidthAsset._1336_ORIGINAL_JPG, ThumbnailHeightAsset._1336_ORIGINAL_JPG)
            .WithFileProperties(FileSize._1336_ORIGINAL_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes._1336_ORIGINAL_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset13 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames._1336_4_K_ORIGINAL_JPG)
            .WithPixels(PixelWidthAsset._1336_4_K_ORIGINAL_JPG, PixelHeightAsset._1336_4_K_ORIGINAL_JPG,
                ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG)
            .WithFileProperties(FileSize._1336_4_K_ORIGINAL_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes._1336_4_K_ORIGINAL_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset14 = AssetBuilder.Create()
            .WithFolderPath("", Guid.Empty) // Initialised later
            .WithFileName(FileNames.IMAGE_1336_ORIGINAL_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG, PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG,
                ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG)
            .WithFileProperties(FileSize.IMAGE_1336_ORIGINAL_JPG, actualDate, ModificationDate.Default)
            .WithThumbnailCreationDateTime(actualDate)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(Hashes.IMAGE_1336_ORIGINAL_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
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
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
        _applicationViewModel = new(_application);
        _findDuplicatedAssetsViewModel = new(_application);
    }

    [Test]
    public async Task SelectionChanged_DuplicatedSets_SynchronizesDuplicateAssetsGrid()
    {
        IApplication application = Substitute.For<IApplication>();
        FindDuplicatedAssetsViewModel viewModel = new(application);

        Asset firstAsset = CreateWindowTestAsset(@"C:\Photos\Set1", "first.jpg", "hash-1", 640, 2048,
            SkiaImageData.Empty());
        Asset secondAsset = CreateWindowTestAsset(@"C:\Photos\Set1", "second.jpg", "hash-1", 800, 4096, null);
        Asset thirdAsset = CreateWindowTestAsset(@"C:\Photos\Set2", "third.jpg", "hash-2", 1024, 8192,
            SkiaImageData.Empty());
        Asset fourthAsset = CreateWindowTestAsset(@"C:\Photos\Set2", "fourth.jpg", "hash-2", 1280, 16384,
            SkiaImageData.Empty());
        viewModel.SetDuplicates(
            FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(
                [[firstAsset, secondAsset], [thirdAsset, fourthAsset]]));

        FindDuplicatedAssetsWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                window = new(viewModel, _testLogger);
                DataGrid setsGrid = window.FindControl<DataGrid>("DuplicatedSetsDataGrid")
                    ?? throw new InvalidOperationException("DuplicatedSetsDataGrid was not found.");
                DataGrid assetsGrid = window.FindControl<DataGrid>("DuplicatedAssetsDataGrid")
                    ?? throw new InvalidOperationException("DuplicatedAssetsDataGrid was not found.");

                setsGrid.SelectedIndex = 1;
                assetsGrid.SelectedIndex = 1;

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(viewModel.DuplicatedAssetSetsPosition, Is.EqualTo(1));
                    Assert.That(viewModel.DuplicatedAssetPosition, Is.EqualTo(1));
                    Assert.That(viewModel.CurrentDuplicatedAsset!.Asset.FullPath,
                        Is.EqualTo(@"C:\Photos\Set2\fourth.jpg"));
                    Assert.That(viewModel.CurrentDuplicatedAsset.Asset.Pixel.Asset.Width, Is.EqualTo(1280));
                    Assert.That(viewModel.CurrentDuplicatedAsset.Asset.FileProperties.Size, Is.EqualTo(16384));
                    Assert.That(viewModel.CurrentDuplicatedAsset.Asset.FileProperties.Creation,
                        Is.EqualTo(new DateTime(2026, 1, 2, 3, 4, 5)));
                }

                setsGrid.SelectedIndex = 0;

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(viewModel.DuplicatedAssetSetsPosition, Is.Zero);
                    Assert.That(viewModel.DuplicatedAssetPosition, Is.Zero);
                    Assert.That(viewModel.CurrentDuplicatedAsset!.Asset.FullPath,
                        Is.EqualTo(@"C:\Photos\Set1\first.jpg"));
                    Assert.That(viewModel.CurrentDuplicatedAssetSet[1].Asset.FullPath,
                        Is.EqualTo(@"C:\Photos\Set1\second.jpg"));
                    Assert.That(viewModel.CurrentDuplicatedAssetSet[1].Asset.Pixel.Asset.Width, Is.EqualTo(800));
                    Assert.That(viewModel.CurrentDuplicatedAssetSet[1].Asset.FileProperties.Size, Is.EqualTo(4096));
                }

                _testLogger.AssertLogExceptions([], typeof(FindDuplicatedAssetsWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public async Task
        DeleteLabel_CataloguedAssetsAndBasicHashTypeAndAllDuplicatesSets_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string rootDirectory = _assetsDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string directoryNewFolder1 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string directoryNewFolder2 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string directorySample1 = Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string directoryPart = Path.Combine(duplicatesDirectory, Directories.PART);
        string directoryResolution = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string directoryThumbnail = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, rootDirectory, exemptedFolderPath, 200, 150, false, false, false,
            true);

        string directoryOutputVideoFirstFrame = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(5));

            List<Asset> image1DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset1!.FileName));
            Assert.That(image1DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image2DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset2!.FileName));
            Assert.That(image2DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image9DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset4!.FileName));
            Assert.That(image9DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image11DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset5!.FileName));
            Assert.That(image11DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image1336DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset11!.FileName));
            Assert.That(image1336DuplicatesSet, Has.Count.EqualTo(4));

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(rootDirectory);
            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder1);
            Folder? folder3 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder2);
            Folder? folder4 = _testableAssetRepository!.GetFolderByPath(directorySample1);
            Folder? folder5 = _testableAssetRepository!.GetFolderByPath(directoryPart);
            Folder? folder6 = _testableAssetRepository!.GetFolderByPath(directoryResolution);
            Folder? folder7 = _testableAssetRepository!.GetFolderByPath(directoryThumbnail);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder4, Is.Not.Null);
            Assert.That(folder5, Is.Not.Null);
            Assert.That(folder6, Is.Not.Null);
            Assert.That(folder7, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder1!);
            _asset2 = _asset2!.WithFolder(folder1!);
            _asset3 = _asset3!.WithFolder(folder1!);
            _asset4 = _asset4!.WithFolder(folder1!);
            _asset5 = _asset5!.WithFolder(folder1!);

            _asset6 = _asset6!.WithFolder(folder2!);

            _asset7 = _asset7!.WithFolder(folder3!);
            _asset8 = _asset8!.WithFolder(folder3!);
            _asset9 = _asset9!.WithFolder(folder3!);
            _asset10 = _asset10!.WithFolder(folder3!);

            _asset11 = _asset11!.WithFolder(folder4!);
            _asset12 = _asset12!.WithFolder(folder5!);
            _asset13 = _asset13!.WithFolder(folder6!);
            _asset14 = _asset14!.WithFolder(folder7!);

            // Because _asset11 became the CurrentAsset so the ImageData has been loaded (was null because not in the current directory)
            _asset11.ImageData = SkiaImageData.Empty();

            _findDuplicatedAssetsViewModel!.SetDuplicates(
                FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(duplicatedAssetsSets));

            // First Delete
            Delete(FindDuplicatedAssetVm(_asset1));

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet4 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet5 = [];

            // Image 1
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset7,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            // Image 2
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

            // Image 9
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel7 = new()
            {
                Asset = _asset8,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel7);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel8 = new()
            {
                Asset = _asset9,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel8);

            // Image 11
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel9 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel9);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel10);

            // Image 1336
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel11);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel12);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel13);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel14);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Second Delete
            Delete(FindDuplicatedAssetVm(_asset12!));
            Delete(FindDuplicatedAssetVm(_asset14!));

            expectedDuplicatedAssetSet5 = [];

            // Image 1336
            expectedDuplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel11);

            expectedDuplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel12);

            expectedDuplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel13);

            expectedDuplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel14);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 2
                // Collapse 3

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Third Delete
            Delete(FindDuplicatedAssetVm(_asset4!));
            Delete(FindDuplicatedAssetVm(_asset8!));
            Delete(FindDuplicatedAssetVm(_asset9!));

            expectedDuplicatedAssetSet3 = [];

            // Image 9
            expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset4,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            expectedDuplicatedAssetViewModel7 = new()
            {
                Asset = _asset8,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel7);

            expectedDuplicatedAssetViewModel8 = new()
            {
                Asset = _asset9,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel8);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 4
                // Collapse 5
                // Collapse 6

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(6));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset4);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset8);
                Assert.That(deleteDuplicatedAssetsEvents[5], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[5][0], _asset9);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Fourth Delete
            Delete(FindDuplicatedAssetVm(_asset6!));

            expectedDuplicatedAssetSet1 = [];

            // Image 1
            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset6,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset7,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0],
                    Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 7

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(7));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset4);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset8);
                Assert.That(deleteDuplicatedAssetsEvents[5], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[5][0], _asset9);
                Assert.That(deleteDuplicatedAssetsEvents[6], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[6][0], _asset6);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Fifth Delete
            Delete(FindDuplicatedAssetVm(_asset3!));

            expectedDuplicatedAssetSet2 = [];

            // Image 2
            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset3,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 8

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(8));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset4);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset8);
                Assert.That(deleteDuplicatedAssetsEvents[5], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[5][0], _asset9);
                Assert.That(deleteDuplicatedAssetsEvents[6], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[6][0], _asset6);
                Assert.That(deleteDuplicatedAssetsEvents[7], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[7][0], _asset3);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Sixth Delete
            Delete(FindDuplicatedAssetVm(_asset5!));

            expectedDuplicatedAssetSet4 = [];

            // Image 11
            expectedDuplicatedAssetViewModel9 = new()
            {
                Asset = _asset5,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel9);

            expectedDuplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel10);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 9

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(9));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset4);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset8);
                Assert.That(deleteDuplicatedAssetsEvents[5], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[5][0], _asset9);
                Assert.That(deleteDuplicatedAssetsEvents[6], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[6][0], _asset6);
                Assert.That(deleteDuplicatedAssetsEvents[7], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[7][0], _asset3);
                Assert.That(deleteDuplicatedAssetsEvents[8], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[8][0], _asset5);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Seventh Delete
            Delete(FindDuplicatedAssetVm(_asset11!));

            expectedDuplicatedAssetSet5 = [];

            // Image 1336
            expectedDuplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel11);

            expectedDuplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel12);

            expectedDuplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel13);

            expectedDuplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel14);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 10

                Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(197));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[27], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[28], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[29], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[32], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[33], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[34], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[35], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[36], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[38], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[39], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[40], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[41], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[43], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[44], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[45], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[46], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[47], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[49], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[51], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[52], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[53], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[55], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[56], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[57], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[58], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[59], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[61], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[62], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[63], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[64], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[65], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[66], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[67], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[68], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[69], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[70], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[71], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[72], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[73], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[74], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[75], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[76], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[77], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[78], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[79], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[80], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[81], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[82], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[83], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[84], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[85], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[86], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[87], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[88], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[89], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[90], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[91], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[92], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[93], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[94], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[95], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[96], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[97], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[98], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[99], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[100], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[101], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[102], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[103], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[104], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[105], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[106], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[107], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[108], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[109], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[110], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[111], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[112], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[113], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[114], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[115], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[116], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[117], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[118], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[119], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[120], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[121], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[122], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[123], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[124], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[125], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[126], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[127], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[128], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[129], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[130], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[131], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[132], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[133], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[134], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[135], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[136], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[137], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[138], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[139], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[140], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[141], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[142], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[143], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[144], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[145], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[146], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[147], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[148], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[149], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[150], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[151], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[152], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[153], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[154], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[155], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[156], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[157], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[158], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[159], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[160], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[161], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[162], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[163], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[164], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[165], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[166], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[167], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[168], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[169], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[170], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[171], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[172], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[173], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[174], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[175], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[176], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[177], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[178], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[179], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[180], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[181], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[182], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[183], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[184], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[185], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[186], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[187], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[188], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[189], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[190], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[191], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[192], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[193], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[194], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[195], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[196], Is.EqualTo("StatusMessage"));

                Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
                Assert.That(messagesInformationSent[0].Message,
                    Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
                Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(10));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset12);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset4);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset8);
                Assert.That(deleteDuplicatedAssetsEvents[5], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[5][0], _asset9);
                Assert.That(deleteDuplicatedAssetsEvents[6], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[6][0], _asset6);
                Assert.That(deleteDuplicatedAssetsEvents[7], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[7][0], _asset3);
                Assert.That(deleteDuplicatedAssetsEvents[8], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[8][0], _asset5);
                Assert.That(deleteDuplicatedAssetsEvents[9], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[9][0], _asset11);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);

                CheckInstance(
                    findDuplicatedAssetsViewModelInstances,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel11);
            }
        }
        finally
        {
            Directory.Delete(directoryOutputVideoFirstFrame, true);
        }
    }

    [Test]
    public async Task DeleteLabel_NoCataloguedAssets_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset1!,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            Delete(duplicatedAssetViewModel);

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(_findDuplicatedAssetsViewModel!, [], 0, 0, [], null);

                Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);

                Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
                Assert.That(messagesInformationSent[0].Message,
                    Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
                Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1!);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);

                CheckInstance(
                    findDuplicatedAssetsViewModelInstances,
                    [],
                    0,
                    0,
                    [],
                    null);
            }
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task
        DeleteAllLabel_CataloguedAssetsAndBasicHashTypeAndAllDuplicatesSets_SendsDeleteDuplicatedAssetsEventAndCollapsesAssets()
    {
        string rootDirectory = _assetsDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string directoryNewFolder1 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string directoryNewFolder2 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string directorySample1 = Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string directoryPart = Path.Combine(duplicatesDirectory, Directories.PART);
        string directoryResolution = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string directoryThumbnail = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, rootDirectory, exemptedFolderPath, 200, 150, false, false, false,
            true);

        string directoryOutputVideoFirstFrame = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(5));

            List<Asset> image1DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset1!.FileName));
            Assert.That(image1DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image2DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset2!.FileName));
            Assert.That(image2DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image9DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset4!.FileName));
            Assert.That(image9DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image11DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset5!.FileName));
            Assert.That(image11DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image1336DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset11!.FileName));
            Assert.That(image1336DuplicatesSet, Has.Count.EqualTo(4));

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(rootDirectory);
            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder1);
            Folder? folder3 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder2);
            Folder? folder4 = _testableAssetRepository!.GetFolderByPath(directorySample1);
            Folder? folder5 = _testableAssetRepository!.GetFolderByPath(directoryPart);
            Folder? folder6 = _testableAssetRepository!.GetFolderByPath(directoryResolution);
            Folder? folder7 = _testableAssetRepository!.GetFolderByPath(directoryThumbnail);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder4, Is.Not.Null);
            Assert.That(folder5, Is.Not.Null);
            Assert.That(folder6, Is.Not.Null);
            Assert.That(folder7, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder1!);
            _asset2 = _asset2!.WithFolder(folder1!);
            _asset3 = _asset3!.WithFolder(folder1!);
            _asset4 = _asset4!.WithFolder(folder1!);
            _asset5 = _asset5!.WithFolder(folder1!);

            _asset6 = _asset6!.WithFolder(folder2!);

            _asset7 = _asset7!.WithFolder(folder3!);
            _asset8 = _asset8!.WithFolder(folder3!);
            _asset9 = _asset9!.WithFolder(folder3!);
            _asset10 = _asset10!.WithFolder(folder3!);

            _asset11 = _asset11!.WithFolder(folder4!);
            _asset12 = _asset12!.WithFolder(folder5!);
            _asset13 = _asset13!.WithFolder(folder6!);
            _asset14 = _asset14!.WithFolder(folder7!);

            // Because _asset11 became the CurrentAsset so the ImageData has been loaded (was null because not in the current directory)
            _asset11.ImageData = SkiaImageData.Empty();

            _findDuplicatedAssetsViewModel!.SetDuplicates(
                FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(duplicatedAssetsSets));

            // First DeleteAll - 1336 set (current position [0])
            DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet4 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet5 = [];

            // Image 1336
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel11);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel13);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel12);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel14);

            // Image 1
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel7 = new()
            {
                Asset = _asset7,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel7);

            // Image 2
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel3);

            // Image 9
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel8 = new()
            {
                Asset = _asset8,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel8);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel9 = new()
            {
                Asset = _asset9,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel9);

            // Image 11
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel10);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    1,
                    0,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetViewModel1);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(9));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(3));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset13);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset12);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset14);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Second DeleteAll - Image1 set (current position [1])
            DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0]);

            expectedDuplicatedAssetSet2 = [];

            // Image 1
            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

            expectedDuplicatedAssetViewModel7 = new()
            {
                Asset = _asset7,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel7);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    2,
                    0,
                    expectedDuplicatedAssetSet3,
                    expectedDuplicatedAssetViewModel2);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(13));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 2
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[9],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[10],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[11],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[12],
                    Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(3));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset13);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset12);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset6);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset7);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Third DeleteAll - Image2 set (current position [2])
            DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[2][0]);

            expectedDuplicatedAssetSet3 = [];

            // Image 2
            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    3,
                    0,
                    expectedDuplicatedAssetSet4,
                    expectedDuplicatedAssetViewModel4);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(17));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 2
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[9],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[10],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[11],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[12],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 3
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[13],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[14],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[15],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[16],
                    Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(3));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset13);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset12);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset6);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset7);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Fourth DeleteAll - Image9 set (current position [3])
            DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[3][0]);

            expectedDuplicatedAssetSet4 = [];

            // Image 9
            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel8 = new()
            {
                Asset = _asset8,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel8);

            expectedDuplicatedAssetViewModel9 = new()
            {
                Asset = _asset9,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel9);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    4,
                    0,
                    expectedDuplicatedAssetSet5,
                    expectedDuplicatedAssetViewModel5);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(21));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 2
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[9],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[10],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[11],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[12],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 3
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[13],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[14],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[15],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[16],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 4
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[17],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[18],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[19],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[20],
                    Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(messagesInformationSent, Is.Empty);
                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(4));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(3));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset13);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset12);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset6);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset7);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset8);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][1], _asset9);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);
            }

            // Fifth DeleteAll - Image11 set (current position [4])
            DeleteAll(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[4][0]);

            expectedDuplicatedAssetSet5 = [];

            // Image 11
            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel10);

            expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(25));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 1
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[5],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[6],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[7],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 2
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[9],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[10],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[11],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[12],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 3
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[13],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[14],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[15],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[16],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 4
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[17],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[18],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[19],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[20],
                    Is.EqualTo("CurrentDuplicatedAsset"));
                // Collapse 5
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[21],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[22],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[23],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[24],
                    Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(197));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[27], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[28], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[29], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[32], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[33], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[34], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[35], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[36], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[38], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[39], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[40], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[41], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[43], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[44], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[45], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[46], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[47], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[49], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[51], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[52], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[53], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[55], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[56], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[57], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[58], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[59], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[61], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[62], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[63], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[64], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[65], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[66], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[67], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[68], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[69], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[70], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[71], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[72], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[73], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[74], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[75], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[76], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[77], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[78], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[79], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[80], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[81], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[82], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[83], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[84], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[85], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[86], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[87], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[88], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[89], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[90], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[91], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[92], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[93], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[94], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[95], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[96], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[97], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[98], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[99], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[100], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[101], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[102], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[103], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[104], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[105], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[106], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[107], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[108], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[109], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[110], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[111], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[112], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[113], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[114], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[115], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[116], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[117], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[118], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[119], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[120], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[121], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[122], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[123], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[124], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[125], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[126], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[127], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[128], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[129], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[130], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[131], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[132], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[133], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[134], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[135], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[136], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[137], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[138], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[139], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[140], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[141], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[142], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[143], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[144], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[145], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[146], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[147], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[148], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[149], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[150], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[151], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[152], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[153], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[154], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[155], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[156], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[157], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[158], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[159], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[160], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[161], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[162], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[163], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[164], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[165], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[166], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[167], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[168], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[169], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[170], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[171], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[172], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[173], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[174], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[175], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[176], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[177], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[178], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[179], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[180], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[181], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[182], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[183], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[184], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[185], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[186], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[187], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[188], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[189], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[190], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[191], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[192], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[193], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[194], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[195], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[196], Is.EqualTo("StatusMessage"));

                Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
                Assert.That(messagesInformationSent[0].Message,
                    Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
                Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

                Assert.That(getExemptedFolderPathEvents, Is.Empty);

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(5));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(3));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset13);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset12);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset14);
                Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset6);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][1], _asset7);
                Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);
                Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(2));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset8);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][1], _asset9);
                Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset10);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);

                CheckInstance(
                    findDuplicatedAssetsViewModelInstances,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetViewModel11);
            }
        }
        finally
        {
            Directory.Delete(directoryOutputVideoFirstFrame, true);
        }
    }

    [Test]
    public async Task DeleteAllLabel_NoCataloguedAssets_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset1!,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            DeleteAll(duplicatedAssetViewModel);

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(_findDuplicatedAssetsViewModel!, [], 0, 0, [], null);

                Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Is.Empty);

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
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task
        DeleteAllNotExemptedLabel_CataloguedAssetsAndBasicHashTypeAndAllDuplicatesSets_SendsDeleteDuplicatedAssetsEventAndCollapsesAssets()
    {
        string rootDirectory = _assetsDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string directoryNewFolder1 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string directoryNewFolder2 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string directorySample1 = Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string directoryPart = Path.Combine(duplicatesDirectory, Directories.PART);
        string directoryResolution = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string directoryThumbnail = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);
        string exemptedFolderPath = Path.Combine(directoryNewFolder2);

        ConfigureFindDuplicatedAssetsViewModel(100, rootDirectory, exemptedFolderPath, 200, 150, false, false, false,
            true);

        string directoryOutputVideoFirstFrame = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(5));

            List<Asset> image1DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset1!.FileName));
            Assert.That(image1DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image2DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset2!.FileName));
            Assert.That(image2DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image9DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset4!.FileName));
            Assert.That(image9DuplicatesSet, Has.Count.EqualTo(3));

            List<Asset> image11DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset5!.FileName));
            Assert.That(image11DuplicatesSet, Has.Count.EqualTo(2));

            List<Asset> image1336DuplicatesSet =
                duplicatedAssetsSets.First(s => s.Any(a => a.FileName == _asset11!.FileName));
            Assert.That(image1336DuplicatesSet, Has.Count.EqualTo(4));

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(rootDirectory);
            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder1);
            Folder? folder3 = _testableAssetRepository!.GetFolderByPath(directoryNewFolder2);
            Folder? folder4 = _testableAssetRepository!.GetFolderByPath(directorySample1);
            Folder? folder5 = _testableAssetRepository!.GetFolderByPath(directoryPart);
            Folder? folder6 = _testableAssetRepository!.GetFolderByPath(directoryResolution);
            Folder? folder7 = _testableAssetRepository!.GetFolderByPath(directoryThumbnail);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder4, Is.Not.Null);
            Assert.That(folder5, Is.Not.Null);
            Assert.That(folder6, Is.Not.Null);
            Assert.That(folder7, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder1!);
            _asset2 = _asset2!.WithFolder(folder1!);
            _asset3 = _asset3!.WithFolder(folder1!);
            _asset4 = _asset4!.WithFolder(folder1!);
            _asset5 = _asset5!.WithFolder(folder1!);

            _asset6 = _asset6!.WithFolder(folder2!);

            _asset7 = _asset7!.WithFolder(folder3!);
            _asset8 = _asset8!.WithFolder(folder3!);
            _asset9 = _asset9!.WithFolder(folder3!);
            _asset10 = _asset10!.WithFolder(folder3!);

            _asset11 = _asset11!.WithFolder(folder4!);
            _asset12 = _asset12!.WithFolder(folder5!);
            _asset13 = _asset13!.WithFolder(folder6!);
            _asset14 = _asset14!.WithFolder(folder7!);

            // Because _asset11 became the CurrentAsset so the ImageData has been loaded (was null because not in the current directory)
            _asset11.ImageData = SkiaImageData.Empty();

            _findDuplicatedAssetsViewModel!.SetDuplicates(
                FindDuplicatedAssetsViewModel.CreateDuplicatedAssetSets(duplicatedAssetsSets));

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet4 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet5 = [];

            // Image 1336
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel11);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel13);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel12);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel14);

            // Image 1
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel7 = new()
            {
                Asset = _asset7,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel7);

            // Image 2
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel3);

            // Image 9
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel8 = new()
            {
                Asset = _asset8,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel8);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel9 = new()
            {
                Asset = _asset9,
                ParentViewModel = expectedDuplicatedAssetSet4
            };
            expectedDuplicatedAssetSet4.Add(expectedDuplicatedAssetViewModel9);

            // Image 11
            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                IsVisible = false,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                ParentViewModel = expectedDuplicatedAssetSet5
            };
            expectedDuplicatedAssetSet5.Add(expectedDuplicatedAssetViewModel10);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetSet4,
                expectedDuplicatedAssetSet5
            ];

            using (Assert.EnterMultipleScope())
            {
                CheckAfterChanges(
                    _findDuplicatedAssetsViewModel!,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetViewModel11);

                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
                // SetDuplicates
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1],
                    Is.EqualTo("DuplicatedAssetSetsPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2],
                    Is.EqualTo("CurrentDuplicatedAssetSet"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3],
                    Is.EqualTo("DuplicatedAssetPosition"));
                Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

                Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(197));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[27], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[28], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[29], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[32], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[33], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[34], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[35], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[36], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[38], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[39], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[40], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[41], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[43], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[44], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[45], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[46], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[47], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[49], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[51], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[52], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[53], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[55], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[56], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[57], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[58], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[59], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[61], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[62], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[63], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[64], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[65], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[66], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[67], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[68], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[69], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[70], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[71], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[72], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[73], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[74], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[75], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[76], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[77], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[78], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[79], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[80], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[81], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[82], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[83], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[84], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[85], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[86], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[87], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[88], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[89], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[90], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[91], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[92], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[93], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[94], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[95], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[96], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[97], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[98], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[99], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[100], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[101], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[102], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[103], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[104], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[105], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[106], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[107], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[108], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[109], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[110], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[111], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[112], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[113], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[114], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[115], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[116], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[117], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[118], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[119], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[120], Is.EqualTo("AppTitle"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[121], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[122], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[123], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[124], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[125], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[126], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[127], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[128], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[129], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[130], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[131], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[132], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[133], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[134], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[135], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[136], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[137], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[138], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[139], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[140], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[141], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[142], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[143], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[144], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[145], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[146], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[147], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[148], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[149], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[150], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[151], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[152], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[153], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[154], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[155], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[156], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[157], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[158], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[159], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[160], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[161], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[162], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[163], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[164], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[165], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[166], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[167], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[168], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[169], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[170], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[171], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[172], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[173], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[174], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[175], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[176], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[177], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[178], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[179], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[180], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[181], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[182], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[183], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[184], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[185], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[186], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[187], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[188], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[189], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[190], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[191], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[192], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[193], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[194], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[195], Is.EqualTo("StatusMessage"));
                Assert.That(notifyApplicationVmPropertyChangedEvents[196], Is.EqualTo("StatusMessage"));

                Assert.That(messagesInformationSent, Is.Empty);

                Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
                Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

                Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
                Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(4));
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset4);
                AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][3], _asset5);

                Assert.That(refreshAssetsCounterEvents, Is.Empty);

                CheckInstance(
                    findDuplicatedAssetsViewModelInstances,
                    expectedDuplicatedAssetsSets,
                    0,
                    0,
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetViewModel11);
            }
        }
        finally
        {
            Directory.Delete(directoryOutputVideoFirstFrame, true);
        }
    }

    [Test]
    public async Task DeleteAllNotExemptedLabel_NoCataloguedAssets_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false,
            false);

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents =
            NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Is.Empty);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset1!,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            DeleteNotExemptedDuplicatedAssets();

            using (Assert.EnterMultipleScope())
            {
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

                Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
                Assert.That(messagesInformationSent[0].Message,
                    Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
                Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

                Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
                Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

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
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public void FindDuplicatedAssetsWindowClosing_RefreshAssetsCounter_InvokesRefreshAssetsCounterEvent()
    {
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        RefreshAssetsCounter?.Invoke(this);

        Assert.That(refreshAssetsCounterEvents, Has.Count.EqualTo(1));
        Assert.That(refreshAssetsCounterEvents[0], Is.EqualTo(string.Empty));
    }

    private DuplicatedAssetViewModel FindDuplicatedAssetVm(Asset asset)
    {
        return _findDuplicatedAssetsViewModel!.DuplicatedAssetSets
            .SelectMany(s => s.AsEnumerable())
            .First(a => a.Asset.FullPath == asset.FullPath);
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

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyFindDuplicatedAssetsVmPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        _applicationViewModel!.PropertyChanged += delegate (object? _, PropertyChangedEventArgs e)
        {
            notifyApplicationVmPropertyChangedEvents.Add(e.PropertyName!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent +=
            delegate (object _, MessageBoxInformationSentEventArgs e)
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
            Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets,
                Has.Count.EqualTo(expectedDuplicatedAssetSets.Count));

            for (int i = 0; i < expectedDuplicatedAssetSets.Count; i++)
            {
                DuplicatedSetViewModel actualSet =
                    findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets
                        .First(x => x[0].Asset.Hash == expectedDuplicatedAssetSets[i][0].Asset.Hash);

                AssertDuplicatedAssetsSet(actualSet, expectedDuplicatedAssetSets[i]);
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

            Assert.That(duplicatedAssetSet, Has.Count.EqualTo(expectedDuplicatedAssetSet.Count));

            for (int i = 0; i < expectedDuplicatedAssetSet.Count; i++)
            {
                DuplicatedAssetViewModel actualAsset =
                    duplicatedAssetSet.First(x => x.Asset.FullPath == expectedDuplicatedAssetSet[i].Asset.FullPath);

                AssertDuplicatedAsset(actualAsset, expectedDuplicatedAssetSet[i]);
            }
        }
        else
        {
            Assert.That(duplicatedAssetSet, Is.Empty);
        }
    }

    private static void AssertDuplicatedSet(DuplicatedSetViewModel duplicatedSetViewModel,
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

                Assert.That(duplicatedAsset.ParentViewModel,
                    Has.Count.EqualTo(expectedDuplicatedAsset.ParentViewModel.Count));

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    DuplicatedAssetViewModel actualSibling =
                        duplicatedAsset.ParentViewModel.First(x =>
                            x.Asset.FullPath == expectedDuplicatedAsset.ParentViewModel[i].Asset.FullPath);

                    Assert.That(actualSibling.IsVisible,
                        Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].IsVisible));

                    AssertAssetPropertyValidity(actualSibling.Asset, expectedDuplicatedAsset.ParentViewModel[i].Asset);
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

    private void Delete(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        DeleteDuplicatedAssets?.Invoke(this, [duplicatedAssetViewModel.Asset]);

        _findDuplicatedAssetsViewModel!.CollapseAssets([duplicatedAssetViewModel]);
    }

    private void DeleteAll(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        List<DuplicatedAssetViewModel> assetsToDelete =
            _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel!.CollapseAssets(assetsToDelete);
    }

    private void DeleteNotExemptedDuplicatedAssets()
    {
        string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;

        List<DuplicatedAssetViewModel> assetsToDelete =
            _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel!.CollapseAssets(assetsToDelete);
    }

    private static Asset CreateWindowTestAsset(string folderPath, string fileName, string hash, int pixelWidth,
        long fileSize, IImageData? imageData)
    {
        return AssetBuilder.Create()
            .WithFolderPath(folderPath, Guid.NewGuid())
            .WithFileName(fileName)
            .WithPixels(pixelWidth, 480, 200, 150)
            .WithFileProperties(fileSize, new(2026, 1, 2, 3, 4, 5), new(2026, 1, 3, 4, 5, 6))
            .WithThumbnailCreationDateTime(new(2026, 1, 2, 3, 4, 5))
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(hash)
            .WithImageData(imageData)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }
}
