using SkiaSharp;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.UI.ViewModels;

[TestFixture]
[NonParallelizable]
public class ConvertAssetsViewModelTests
{
    private const string CONVERTED_FOLDER_NAME = "Converted";
    private const string WORKING_DIRECTORY_NAME = "ConvertAssetsViewModelTestsWorkingDirectory";

    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private string? _workingDirectory;
    private string? _convertedDirectory;

    private ConvertAssetsViewModel? _convertAssetsViewModel;
    private TestableAssetRepository? _testableAssetRepository;
    private TestLogger<AssetConversionService> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();

        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
        _workingDirectory = Path.Combine(_assetsDirectory, WORKING_DIRECTORY_NAME);
        _convertedDirectory = Path.Combine(_workingDirectory, CONVERTED_FOLDER_NAME);
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);

        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, true);
        }

        _testLogger.LoggingAssertTearDown();
    }

    private void ConfigureApplication()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _assetsDirectory!);

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
        ImageMagickThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            catalogFolderPipeline, new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());

        _testLogger = new();
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            _testLogger);

        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);

        _convertAssetsViewModel = new(application);

        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, true);
        }

        Directory.CreateDirectory(_workingDirectory!);
    }

    [Test]
    public async Task ConvertAsync_NoSelectedFiles_ReturnsNullAndDoesNotConvert()
    {
        ConfigureApplication();

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        AssetConversionResult? result = await _convertAssetsViewModel!.ConvertAsync();

        Assert.That(result, Is.Null);

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, [], "JPG", false, 0d, string.Empty,
            "Select at least one image before converting.", false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));

        CheckInstance(convertAssetsViewModelInstances, [], "JPG", false, 0d, string.Empty,
            "Select at least one image before converting.", false);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAsync_EmptySelectedFilePath_ReturnsFailureAndWritesNoFile()
    {
        ConfigureApplication();

        const string expectedMessage = "The source path is empty.";

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([string.Empty]);

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(string.Empty));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, [string.Empty], "JPG", false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, [string.Empty], "JPG", false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        _testLogger.AssertLogErrors(["Cannot convert an asset because the source path is null or empty."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAsync_SelectedFileDoesNotExist_ReturnsFailureAndWritesNoFile()
    {
        ConfigureApplication();

        string sourcePath = Path.Combine(_workingDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);
        string expectedMessage = $"File not found: {sourcePath}";

        Assert.That(File.Exists(sourcePath), Is.False);

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([sourcePath]);

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, [sourcePath], "JPG", false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, [sourcePath], "JPG", false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        _testLogger.AssertLogErrors([$"Cannot convert '{sourcePath}' because the file does not exist."],
            typeof(AssetConversionService));
    }

    [Test]
    [TestCase("JPG", ".jpg")]
    [TestCase("PNG", ".png")]
    public async Task ConvertAsync_UnsupportedSourceFormat_ReturnsFailureAndWritesNoFile(string targetFormat,
        string targetExtension)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(FileNames.HOMER_GIF);
        string expectedMessage = $"{FileNames.HOMER_GIF} cannot be converted to {targetExtension}.";

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([sourcePath]);
        _convertAssetsViewModel.SelectedTargetFormat = targetFormat;

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        AssertNotifications(notifyPropertyChangedEvents,
            SingleFileNotifications(targetFormatChanged: targetFormat == "PNG"));

        CheckInstance(convertAssetsViewModelInstances, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_9_PNG, "JPG", "Image 9.jpg", SKEncodedImageFormat.Jpeg)]
    [TestCase(FileNames.IMAGE_8_JPEG, "PNG", "Image 8.png", SKEncodedImageFormat.Png)]
    [TestCase(FileNames.IMAGE_1_JPG, "PNG", "Image 1.png", SKEncodedImageFormat.Png)]
    [TestCase(FileNames.IMAGE_11_HEIC, "JPG", "Image_11.jpg", SKEncodedImageFormat.Jpeg)]
    [TestCase(FileNames.IMAGE_11_HEIC, "PNG", "Image_11.png", SKEncodedImageFormat.Png)]
    public async Task ConvertAsync_SupportedConversion_WritesValidConvertedFileAndUpdatesStateFromUiThread(
        string fileName, string targetFormat, string convertedFileName, SKEncodedImageFormat expectedFormat)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(fileName);
        string destinationPath = Path.Combine(_convertedDirectory!, convertedFileName);
        long sourceLengthBefore = new FileInfo(sourcePath).Length;
        string expectedMessage = $"{fileName} converted.";

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([sourcePath]);
        _convertAssetsViewModel.SelectedTargetFormat = targetFormat;

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);
        Assert.That(File.Exists(destinationPath), Is.False);

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.EqualTo(1));
        Assert.That(result.FailedCount, Is.Zero);

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(destinationPath));
        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.True);
        ImageHelper.AssertValidImageOfFormat(destinationPath, expectedFormat);

        // The conversion is non-destructive: the source file is left untouched.
        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(new FileInfo(sourcePath).Length, Is.EqualTo(sourceLengthBefore));

        // Real progress flowed through NotifyProgress on the UI thread (the mocked unit fixture cannot assert this).
        CheckAfterChanges(_convertAssetsViewModel, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 1. Failed: 0.", true);

        AssertNotifications(notifyPropertyChangedEvents,
            SingleFileNotifications(targetFormatChanged: targetFormat == "PNG"));

        CheckInstance(convertAssetsViewModelInstances, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 1. Failed: 0.", true);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAsync_ConvertedDirectoryAlreadyExists_WritesConvertedFile()
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        string destinationPath = Path.Combine(_convertedDirectory!, "Image 9.jpg");
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        Directory.CreateDirectory(_convertedDirectory!);
        Assert.That(File.Exists(destinationPath), Is.False);

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([sourcePath]);

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.EqualTo(1));
        Assert.That(result.FailedCount, Is.Zero);

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(destinationPath));
        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        ImageHelper.AssertValidImageOfFormat(destinationPath, SKEncodedImageFormat.Jpeg);

        CheckAfterChanges(_convertAssetsViewModel, [sourcePath], "JPG", false, 100d, expectedMessage,
            "Converted: 1. Failed: 0.", true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, [sourcePath], "JPG", false, 100d, expectedMessage,
            "Converted: 1. Failed: 0.", true);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, "JPG", ".jpg")]
    [TestCase(FileNames.IMAGE_9_PNG, "PNG", ".png")]
    public async Task ConvertAsync_SameFormat_ReturnsFailureAndWritesNoFile(string fileName, string targetFormat,
        string fileExtension)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(fileName);
        string expectedMessage = $"{fileName} cannot be converted to {fileExtension}.";

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths([sourcePath]);
        _convertAssetsViewModel.SelectedTargetFormat = targetFormat;

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        AssertNotifications(notifyPropertyChangedEvents,
            SingleFileNotifications(targetFormatChanged: targetFormat == "PNG"));

        CheckInstance(convertAssetsViewModelInstances, [sourcePath], targetFormat, false, 100d, expectedMessage,
            "Converted: 0. Failed: 1.", true);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAsync_MultipleFilesToJpeg_ConvertsSupportedAndSkipsSameFormat()
    {
        ConfigureApplication();

        string heicSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_11_HEIC);
        string jpgSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_1_JPG);
        string pngSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);

        string heicDestinationPath = Path.Combine(_convertedDirectory!, "Image_11.jpg");
        string jpgDestinationPath = Path.Combine(_convertedDirectory!, "Image 1.jpg");
        string pngDestinationPath = Path.Combine(_convertedDirectory!, "Image 9.jpg");

        const string heicMessage = $"{FileNames.IMAGE_11_HEIC} converted.";
        const string jpgMessage = $"{FileNames.IMAGE_1_JPG} cannot be converted to .jpg.";
        const string pngMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        string[] selectedFilePaths = [heicSourcePath, jpgSourcePath, pngSourcePath];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(selectedFilePaths);

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(3));
        Assert.That(result.ConvertedCount, Is.EqualTo(2));
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(heicSourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(heicDestinationPath));
        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].Message, Is.EqualTo(heicMessage));

        Assert.That(result.Items[1].SourcePath, Is.EqualTo(jpgSourcePath));
        Assert.That(result.Items[1].DestinationPath, Is.Null);
        Assert.That(result.Items[1].Success, Is.False);
        Assert.That(result.Items[1].Message, Is.EqualTo(jpgMessage));

        Assert.That(result.Items[2].SourcePath, Is.EqualTo(pngSourcePath));
        Assert.That(result.Items[2].DestinationPath, Is.EqualTo(pngDestinationPath));
        Assert.That(result.Items[2].Success, Is.True);
        Assert.That(result.Items[2].Message, Is.EqualTo(pngMessage));

        ImageHelper.AssertValidImageOfFormat(heicDestinationPath, SKEncodedImageFormat.Jpeg);
        ImageHelper.AssertValidImageOfFormat(pngDestinationPath, SKEncodedImageFormat.Jpeg);
        Assert.That(File.Exists(jpgDestinationPath), Is.False);

        // The last processed file (the converted PNG) drives the final progress state.
        CheckAfterChanges(_convertAssetsViewModel, selectedFilePaths, "JPG", false, 100d, pngMessage,
            "Converted: 2. Failed: 1.", true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, selectedFilePaths, "JPG", false, 100d, pngMessage,
            "Converted: 2. Failed: 1.", true);

        _testLogger.AssertLogErrors([jpgMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAsync_MultipleFilesToPng_ConvertsSupportedAndSkipsSameFormat()
    {
        ConfigureApplication();

        string heicSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_11_HEIC);
        string jpgSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_1_JPG);
        string pngSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);

        string heicDestinationPath = Path.Combine(_convertedDirectory!, "Image_11.png");
        string jpgDestinationPath = Path.Combine(_convertedDirectory!, "Image 1.png");
        string pngDestinationPath = Path.Combine(_convertedDirectory!, "Image 9.png");

        const string heicMessage = $"{FileNames.IMAGE_11_HEIC} converted.";
        const string jpgMessage = $"{FileNames.IMAGE_1_JPG} converted.";
        const string pngMessage = $"{FileNames.IMAGE_9_PNG} cannot be converted to .png.";

        string[] selectedFilePaths = [heicSourcePath, jpgSourcePath, pngSourcePath];

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(selectedFilePaths);
        _convertAssetsViewModel.SelectedTargetFormat = "PNG";

        AssetConversionResult? result =
            await AvaloniaTestSetup.RunOnUiThreadAsync(() => _convertAssetsViewModel.ConvertAsync());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Has.Count.EqualTo(3));
        Assert.That(result.ConvertedCount, Is.EqualTo(2));
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].SourcePath, Is.EqualTo(heicSourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(heicDestinationPath));
        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].Message, Is.EqualTo(heicMessage));

        Assert.That(result.Items[1].SourcePath, Is.EqualTo(jpgSourcePath));
        Assert.That(result.Items[1].DestinationPath, Is.EqualTo(jpgDestinationPath));
        Assert.That(result.Items[1].Success, Is.True);
        Assert.That(result.Items[1].Message, Is.EqualTo(jpgMessage));

        Assert.That(result.Items[2].SourcePath, Is.EqualTo(pngSourcePath));
        Assert.That(result.Items[2].DestinationPath, Is.Null);
        Assert.That(result.Items[2].Success, Is.False);
        Assert.That(result.Items[2].Message, Is.EqualTo(pngMessage));

        ImageHelper.AssertValidImageOfFormat(heicDestinationPath, SKEncodedImageFormat.Png);
        ImageHelper.AssertValidImageOfFormat(jpgDestinationPath, SKEncodedImageFormat.Png);
        Assert.That(File.Exists(pngDestinationPath), Is.False);

        CheckAfterChanges(_convertAssetsViewModel, selectedFilePaths, "PNG", false, 100d, pngMessage,
            "Converted: 2. Failed: 1.", true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedTargetFormat)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, selectedFilePaths, "PNG", false, 100d, pngMessage,
            "Converted: 2. Failed: 1.", true);

        _testLogger.AssertLogErrors([pngMessage], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAsync_CancellationRequestedDuringConversion_ThrowsAndWritesOnlyFirstFile()
    {
        ConfigureApplication();

        string firstSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        string secondSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_10_PORTRAIT_PNG);
        string firstDestinationPath = Path.Combine(_convertedDirectory!, "Image 9.jpg");
        string secondDestinationPath = Path.Combine(_convertedDirectory!, "Image 10 portrait.jpg");
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        string[] selectedFilePaths = [firstSourcePath, secondSourcePath];
        CancellationTokenSource cancellationTokenSource = new();

        (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
            = NotifyPropertyChangedEvents();

        CheckBeforeChanges();

        _convertAssetsViewModel!.SetSelectedFilePaths(selectedFilePaths);

        // NotifyProgress blocks the conversion worker until the UI-thread handler completes, so cancelling on the first
        // progress notification deterministically lets the first file finish and stops the second from being processed.
        _convertAssetsViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ConvertAssetsViewModel.ProgressPercentage)
                && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        };

        OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
                _convertAssetsViewModel.ConvertAsync(cancellationTokenSource.Token)));

        Assert.That(exception, Is.Not.Null);

        // The first file was converted before the cancellation was observed; the second was never processed.
        ImageHelper.AssertValidImageOfFormat(firstDestinationPath, SKEncodedImageFormat.Jpeg);
        Assert.That(File.Exists(secondDestinationPath), Is.False);

        // The converting state was reset by the finally block even though the operation was cancelled.
        CheckAfterChanges(_convertAssetsViewModel, selectedFilePaths, "JPG", false, 50d, expectedMessage,
            string.Empty, true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo(nameof(ConvertAssetsViewModel.SelectedFilePaths)));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo(nameof(ConvertAssetsViewModel.SummaryMessage)));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressPercentage)));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo(nameof(ConvertAssetsViewModel.ProgressMessage)));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo(nameof(ConvertAssetsViewModel.IsConverting)));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo(nameof(ConvertAssetsViewModel.CanConvert)));

        CheckInstance(convertAssetsViewModelInstances, selectedFilePaths, "JPG", false, 50d, expectedMessage,
            string.Empty, true);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    private (List<string> notifyPropertyChangedEvents, List<ConvertAssetsViewModel> convertAssetsViewModelInstances)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<ConvertAssetsViewModel> convertAssetsViewModelInstances = [];
        object eventsLock = new();

        _convertAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            lock (eventsLock)
            {
                notifyPropertyChangedEvents.Add(e.PropertyName!);
                convertAssetsViewModelInstances.Add((ConvertAssetsViewModel)sender!);
            }
        };

        return (notifyPropertyChangedEvents, convertAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        string[] expectedTargetFormats = ["JPG", "PNG"];

        Assert.That(_convertAssetsViewModel!.SelectedFilePaths, Is.Empty);
        Assert.That(_convertAssetsViewModel.TargetFormats, Is.EqualTo(expectedTargetFormats));
        Assert.That(_convertAssetsViewModel.SelectedTargetFormat, Is.EqualTo("JPG"));
        Assert.That(_convertAssetsViewModel.IsConverting, Is.False);
        Assert.That(_convertAssetsViewModel.ProgressPercentage, Is.Zero);
        Assert.That(_convertAssetsViewModel.ProgressMessage, Is.EqualTo(string.Empty));
        Assert.That(_convertAssetsViewModel.SummaryMessage, Is.EqualTo(string.Empty));
        Assert.That(_convertAssetsViewModel.CanConvert, Is.False);
    }

    private static void CheckAfterChanges(
        ConvertAssetsViewModel convertAssetsViewModelInstance,
        string[] selectedFilePaths,
        string selectedTargetFormat,
        bool isConverting,
        double progressPercentage,
        string progressMessage,
        string summaryMessage,
        bool canConvert)
    {
        string[] expectedTargetFormats = ["JPG", "PNG"];

        Assert.That(convertAssetsViewModelInstance.SelectedFilePaths, Is.EqualTo(selectedFilePaths));
        Assert.That(convertAssetsViewModelInstance.TargetFormats, Is.EqualTo(expectedTargetFormats));
        Assert.That(convertAssetsViewModelInstance.SelectedTargetFormat, Is.EqualTo(selectedTargetFormat));
        Assert.That(convertAssetsViewModelInstance.IsConverting, Is.EqualTo(isConverting));
        Assert.That(convertAssetsViewModelInstance.ProgressPercentage, Is.EqualTo(progressPercentage));
        Assert.That(convertAssetsViewModelInstance.ProgressMessage, Is.EqualTo(progressMessage));
        Assert.That(convertAssetsViewModelInstance.SummaryMessage, Is.EqualTo(summaryMessage));
        Assert.That(convertAssetsViewModelInstance.CanConvert, Is.EqualTo(canConvert));
    }

    private static void CheckInstance(
        List<ConvertAssetsViewModel> convertAssetsViewModelInstances,
        string[] selectedFilePaths,
        string selectedTargetFormat,
        bool isConverting,
        double progressPercentage,
        string progressMessage,
        string summaryMessage,
        bool canConvert)
    {
        int convertAssetsViewModelInstancesCount = convertAssetsViewModelInstances.Count;

        if (convertAssetsViewModelInstancesCount > 1)
        {
            Assert.That(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 2],
                Is.EqualTo(convertAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 1],
                Is.EqualTo(convertAssetsViewModelInstances[convertAssetsViewModelInstancesCount - 2]));
        }

        if (convertAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                convertAssetsViewModelInstances[0],
                selectedFilePaths,
                selectedTargetFormat,
                isConverting,
                progressPercentage,
                progressMessage,
                summaryMessage,
                canConvert);
        }
    }

    private static void AssertNotifications(List<string> notifyPropertyChangedEvents, string[] expected)
    {
        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(expected.Length));

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(notifyPropertyChangedEvents[i], Is.EqualTo(expected[i]));
        }
    }

    // The notification order of a single-file conversion is identical whether the item succeeds or fails, because the
    // service raises exactly one progress callback (CompletedCount 1 of 1, 100%) with a distinct message either way.
    // Switching the target to PNG inserts a SelectedTargetFormat notification right after the selection is set.
    private static string[] SingleFileNotifications(bool targetFormatChanged)
    {
        List<string> expected =
        [
            nameof(ConvertAssetsViewModel.SummaryMessage),
            nameof(ConvertAssetsViewModel.CanConvert),
            nameof(ConvertAssetsViewModel.SelectedFilePaths),
            nameof(ConvertAssetsViewModel.IsConverting),
            nameof(ConvertAssetsViewModel.CanConvert),
            nameof(ConvertAssetsViewModel.ProgressMessage),
            nameof(ConvertAssetsViewModel.SummaryMessage),
            nameof(ConvertAssetsViewModel.ProgressPercentage),
            nameof(ConvertAssetsViewModel.ProgressMessage),
            nameof(ConvertAssetsViewModel.SummaryMessage),
            nameof(ConvertAssetsViewModel.IsConverting),
            nameof(ConvertAssetsViewModel.CanConvert)
        ];

        if (targetFormatChanged)
        {
            expected.Insert(3, nameof(ConvertAssetsViewModel.SelectedTargetFormat));
        }

        return [.. expected];
    }

    private string CopyToWorkingDirectory(string fileName)
    {
        string destinationPath = Path.Combine(_workingDirectory!, fileName);

        Assert.That(File.Exists(destinationPath), Is.False);

        File.Copy(Path.Combine(_assetsDirectory!, fileName), destinationPath);

        Assert.That(File.Exists(destinationPath), Is.True);

        return destinationPath;
    }
}
