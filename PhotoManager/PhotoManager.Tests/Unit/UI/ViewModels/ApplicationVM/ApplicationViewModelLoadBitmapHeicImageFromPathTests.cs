using Avalonia.Media.Imaging;
using PhotoManager.Application;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using FileSize = PhotoManager.Tests.Unit.Constants.FileSize;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Unit.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelLoadBitmapHeicImageFromPathTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;

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
        _applicationViewModel = new(application);
    }

    [Test]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapImageFromPath_HeicValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const string fileName = FileNames.IMAGE_11_HEIC;
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithRotation(rotation)
            .WithPixels(expectedWidth, expectedHeight,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC,
                FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default), ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        byte[] assetData = File.ReadAllBytes(filePath);

        _testableAssetRepository.AddAsset(asset, assetData);

        CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
        {
            Asset = asset,
            CataloguedAssetsByPath = [asset],
            Reason = CatalogChangeReason.AssetCreated,
            Message = $"Image {filePath} added to catalog."
        };

        _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

        Bitmap image = _applicationViewModel!.LoadBitmapImageFromPath()!;

        Assert.That(image, Is.Not.Null);
        Assert.That(image.PixelSize.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelSize.Height, Is.EqualTo(expectedHeight));
        image.Dispose();

        string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset, false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC,
        ThumbnailHeightAsset.IMAGE_11_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, Hashes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
        ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, Hashes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC,
        ThumbnailHeightAsset.IMAGE_11_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, Hashes.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapImageFromPath_HeicValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(
        string fileName,
        ImageRotation rotation,
        int expectedWidth,
        int expectedHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        long expectedFileSize,
        string expectedHash)
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string filePath = Path.Combine(_assetsDirectory!, fileName);

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithRotation(rotation)
            .WithPixels(expectedWidth, expectedHeight,
                expectedThumbnailPixelWidth, expectedThumbnailPixelHeight)
            .WithFileProperties(expectedFileSize,
                FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default), ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(expectedHash)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        byte[] assetData = File.ReadAllBytes(filePath);

        _testableAssetRepository.AddAsset(asset, assetData);

        CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
        {
            Asset = asset,
            CataloguedAssetsByPath = [asset],
            Reason = CatalogChangeReason.AssetCreated,
            Message = $"Image {filePath} added to catalog."
        };

        _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

        Bitmap image = _applicationViewModel!.LoadBitmapImageFromPath()!;

        Assert.That(image, Is.Not.Null);
        Assert.That(image.PixelSize.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelSize.Height, Is.EqualTo(expectedHeight));
        image.Dispose();

        string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset, false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const string fileName = FileNames.NON_EXISTENT_IMAGE_HEIC;
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        const ImageRotation rotation = ImageRotation.Rotate90;

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithRotation(rotation)
            .WithPixels(PixelWidthAsset.NON_EXISTENT_IMAGE_HEIC, PixelHeightAsset.NON_EXISTENT_IMAGE_HEIC,
                ThumbnailWidthAsset.NON_EXISTENT_IMAGE_HEIC, ThumbnailHeightAsset.NON_EXISTENT_IMAGE_HEIC)
            .WithFileProperties(FileSize.NON_EXISTENT_IMAGE_HEIC,
                FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default), ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.NON_EXISTENT_IMAGE_HEIC)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        byte[] assetData = File.ReadAllBytes(Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC));

        _testableAssetRepository.AddAsset(asset, assetData);

        CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
        {
            Asset = asset,
            CataloguedAssetsByPath = [asset],
            Reason = CatalogChangeReason.AssetCreated,
            Message = $"Image {filePath} added to catalog."
        };

        _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

        Bitmap image = _applicationViewModel!.LoadBitmapImageFromPath()!;

        Assert.That(image, Is.Not.Null);
        Assert.That(image.PixelSize.Width, Is.EqualTo(1));
        Assert.That(image.PixelSize.Height, Is.EqualTo(1));
        image.Dispose();

        string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset, false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicInvalidRotation_ReturnBitmapImageWithRotate0()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const string fileName = FileNames.IMAGE_11_HEIC;
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        const ImageRotation rotation = (ImageRotation)999;

        Folder folder = _testableAssetRepository!.AddFolder(_assetsDirectory!);

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithRotation(rotation)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC,
                FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default), ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        byte[] assetData = File.ReadAllBytes(filePath);

        _testableAssetRepository.AddAsset(asset, assetData);

        CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
        {
            Asset = asset,
            CataloguedAssetsByPath = [asset],
            Reason = CatalogChangeReason.AssetCreated,
            Message = $"Image {filePath} added to catalog."
        };

        _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

        Bitmap image = _applicationViewModel!.LoadBitmapImageFromPath()!;

        Assert.That(image, Is.Not.Null);
        Assert.That(image.PixelSize.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.PixelSize.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
        image.Dispose();

        string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset, false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, 1, [asset], expectedStatusMessage, asset,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicImageDataEncodesToEmptyBytes_ReturnsDefaultBitmapImage()
    {
        IApplication applicationMock = Substitute.For<IApplication>();
        applicationMock.GetInitialFolderPath().Returns(@"C:\test");
        applicationMock.GetAboutInformation(Arg.Any<Assembly>()).Returns(
            new AboutInformation { Product = "PhotoManager", Version = "1.0" });

        ApplicationViewModel applicationViewModel = new(applicationMock);

        IImageData thumbnailImageData = Substitute.For<IImageData>();
        IImageData emptyEncodingImageData = Substitute.For<IImageData>();
        emptyEncodingImageData.ToByteArray(ImageEncodingFormat.Jpeg).Returns([]);

        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\test" };

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("test.heic")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC,
                FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default), ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithImageData(thumbnailImageData)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        applicationViewModel.SetAssets(@"C:\test", [asset]);

        applicationMock.LoadBitmapImageFromPath(asset.FullPath, asset.ImageRotation)
            .Returns(emptyEncodingImageData);

        Bitmap? image = applicationViewModel.LoadBitmapImageFromPath();

        Assert.That(image, Is.Null);
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
        Assert.That(_applicationViewModel!.IsCataloging, Is.False);
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
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.IsCataloging, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
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
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset,
                expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedCurrentAsset.Folder);
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

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
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
                expectedAppTitleAssetsCount,
                expectedAssets,
                expectedStatusMessage,
                expectedCurrentAsset,
                expectedCanGoToNextAsset);
        }
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset,
                currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }
}
