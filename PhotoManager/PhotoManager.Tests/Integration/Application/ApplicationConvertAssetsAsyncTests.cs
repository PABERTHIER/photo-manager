using SkiaSharp;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationConvertAssetsAsyncTests
{
    private const string CONVERTED_FOLDER_NAME = "Converted";
    private const string WORKING_DIRECTORY_NAME = "ApplicationConvertAssetsAsyncTestsWorkingDirectory";

    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private string? _workingDirectory;
    private string? _convertedDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private TestLogger<AssetConversionService> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
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

        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);

        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, true);
        }

        Directory.CreateDirectory(_workingDirectory!);
    }

    [Test]
    public void ConvertAssetsAsync_NullRequest_ThrowsNullReferenceException()
    {
        ConfigureApplication();

        AssetConversionRequest? request = null;
        List<AssetConversionProgressEventArgs> progressEvents = [];

        NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _application!.ConvertAssetsAsync(request!, progressEvents.Add));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        Assert.That(progressEvents, Is.Empty);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_NullCallback_ThrowsArgumentNullException()
    {
        ConfigureApplication();

        AssetConversionProgressCallback? callback = null;
        string sourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);

        ArgumentNullException? exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _application!.ConvertAssetsAsync(request, callback!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'callback')"));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_NoFilePaths_ThrowsArgumentException()
    {
        ConfigureApplication();

        AssetConversionRequest request = new([], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _application!.ConvertAssetsAsync(request, progressEvents.Add));

        Assert.That(exception?.Message, Is.EqualTo("At least one file must be selected. (Parameter 'request')"));

        Assert.That(progressEvents, Is.Empty);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogErrors(["Cannot convert assets because no file has been selected."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_EmptyFilePath_ReturnsFailureAndWritesNoFile()
    {
        ConfigureApplication();

        const string expectedMessage = "The source path is empty.";

        AssetConversionRequest request = new([string.Empty], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(string.Empty));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogErrors(["Cannot convert an asset because the source path is null or empty."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_FileDoesNotExist_ReturnsFailureAndWritesNoFile()
    {
        ConfigureApplication();

        string sourcePath = Path.Combine(_workingDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);
        string expectedMessage = $"File not found: {sourcePath}";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        Assert.That(File.Exists(sourcePath), Is.False);

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogErrors([$"Cannot convert '{sourcePath}' because the file does not exist."],
            typeof(AssetConversionService));
    }

    [Test]
    [TestCase(AssetConversionTargetFormat.Jpeg, ".jpg")]
    [TestCase(AssetConversionTargetFormat.Png, ".png")]
    public async Task ConvertAssetsAsync_UnsupportedSourceFormat_ReturnsFailureAndWritesNoFile(
        AssetConversionTargetFormat targetFormat, string targetExtension)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(FileNames.HOMER_GIF);
        string expectedMessage = $"{FileNames.HOMER_GIF} cannot be converted to {targetExtension}.";

        AssetConversionRequest request = new([sourcePath], targetFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_UnknownTargetFormat_ReturnsFailureAndWritesNoFile()
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        const string expectedMessage = $"Could not convert {FileNames.IMAGE_9_PNG}.";

        const AssetConversionTargetFormat targetFormat = (AssetConversionTargetFormat)999;
        ArgumentOutOfRangeException expectedException =
            new(nameof(targetFormat), targetFormat, "Unknown target format.");
        AssetConversionRequest request = new([sourcePath], targetFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogExceptions([expectedException], typeof(AssetConversionService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_9_PNG, AssetConversionTargetFormat.Jpeg, "Image 9.jpg", SKEncodedImageFormat.Jpeg)]
    [TestCase(FileNames.IMAGE_8_JPEG, AssetConversionTargetFormat.Png, "Image 8.png", SKEncodedImageFormat.Png)]
    [TestCase(FileNames.IMAGE_1_JPG, AssetConversionTargetFormat.Png, "Image 1.png", SKEncodedImageFormat.Png)]
    [TestCase(FileNames.IMAGE_11_HEIC, AssetConversionTargetFormat.Jpeg, "Image_11.jpg", SKEncodedImageFormat.Jpeg)]
    [TestCase(FileNames.IMAGE_11_HEIC, AssetConversionTargetFormat.Png, "Image_11.png", SKEncodedImageFormat.Png)]
    public async Task ConvertAssetsAsync_SupportedConversion_WritesValidConvertedFile(
        string fileName, AssetConversionTargetFormat targetFormat, string convertedFileName,
        SKEncodedImageFormat expectedFormat)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(fileName);
        string destinationPath = Path.Combine(_convertedDirectory!, convertedFileName);
        long sourceLengthBefore = new FileInfo(sourcePath).Length;
        string expectedMessage = $"{fileName} converted.";

        AssetConversionRequest request = new([sourcePath], targetFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);
        Assert.That(File.Exists(destinationPath), Is.False);

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.EqualTo(1));
        Assert.That(result.FailedCount, Is.Zero);

        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(destinationPath));
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(Directory.Exists(_convertedDirectory!), Is.True);
        ImageHelper.AssertValidImageOfFormat(destinationPath, expectedFormat);

        // The conversion is non-destructive: the source file is left untouched.
        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(new FileInfo(sourcePath).Length, Is.EqualTo(sourceLengthBefore));

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_ConvertedDirectoryAlreadyExists_WritesConvertedFile()
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        string destinationPath = Path.Combine(_convertedDirectory!, "Image 9.jpg");
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        Directory.CreateDirectory(_convertedDirectory!);
        Assert.That(File.Exists(destinationPath), Is.False);

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.EqualTo(1));
        Assert.That(result.FailedCount, Is.Zero);

        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(destinationPath));
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        ImageHelper.AssertValidImageOfFormat(destinationPath, SKEncodedImageFormat.Jpeg);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, AssetConversionTargetFormat.Jpeg, ".jpg")]
    [TestCase(FileNames.IMAGE_9_PNG, AssetConversionTargetFormat.Png, ".png")]
    public async Task ConvertAssetsAsync_SameFormat_ReturnsFailureAndWritesNoFile(string fileName,
        AssetConversionTargetFormat targetFormat, string fileExtension)
    {
        ConfigureApplication();

        string sourcePath = CopyToWorkingDirectory(fileName);
        string expectedMessage = $"{fileName} cannot be converted to {fileExtension}.";

        AssetConversionRequest request = new([sourcePath], targetFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.ConvertedCount, Is.Zero);
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.False);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.Null);
        Assert.That(result.Items[0].Message, Is.EqualTo(expectedMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d));

        Assert.That(File.Exists(sourcePath), Is.True);
        Assert.That(Directory.Exists(_convertedDirectory!), Is.False);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_MultipleFilesToJpeg_ConvertsSupportedAndSkipsSameFormat()
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

        AssetConversionRequest request =
            new([heicSourcePath, jpgSourcePath, pngSourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(3));
        Assert.That(result.ConvertedCount, Is.EqualTo(2));
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(heicSourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(heicDestinationPath));
        Assert.That(result.Items[0].Message, Is.EqualTo(heicMessage));

        Assert.That(result.Items[1].Success, Is.False);
        Assert.That(result.Items[1].SourcePath, Is.EqualTo(jpgSourcePath));
        Assert.That(result.Items[1].DestinationPath, Is.Null);
        Assert.That(result.Items[1].Message, Is.EqualTo(jpgMessage));

        Assert.That(result.Items[2].Success, Is.True);
        Assert.That(result.Items[2].SourcePath, Is.EqualTo(pngSourcePath));
        Assert.That(result.Items[2].DestinationPath, Is.EqualTo(pngDestinationPath));
        Assert.That(result.Items[2].Message, Is.EqualTo(pngMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(3));

        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[0].Message, Is.EqualTo(heicMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d / 3));

        Assert.That(progressEvents[1].CompletedCount, Is.EqualTo(2));
        Assert.That(progressEvents[1].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[1].Message, Is.EqualTo(jpgMessage));
        Assert.That(progressEvents[1].ProgressPercentage, Is.EqualTo(200d / 3));

        Assert.That(progressEvents[2].CompletedCount, Is.EqualTo(3));
        Assert.That(progressEvents[2].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[2].Message, Is.EqualTo(pngMessage));
        Assert.That(progressEvents[2].ProgressPercentage, Is.EqualTo(100d));

        // Only the two supported files produced a real, decodable JPEG; the same-format JPG produced nothing.
        ImageHelper.AssertValidImageOfFormat(heicDestinationPath, SKEncodedImageFormat.Jpeg);
        ImageHelper.AssertValidImageOfFormat(pngDestinationPath, SKEncodedImageFormat.Jpeg);
        Assert.That(File.Exists(jpgDestinationPath), Is.False);

        _testLogger.AssertLogErrors([jpgMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_MultipleFilesToPng_ConvertsSupportedAndSkipsSameFormat()
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

        AssetConversionRequest request =
            new([heicSourcePath, jpgSourcePath, pngSourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result = await _application!.ConvertAssetsAsync(request, progressEvents.Add);

        Assert.That(result.Items, Has.Count.EqualTo(3));
        Assert.That(result.ConvertedCount, Is.EqualTo(2));
        Assert.That(result.FailedCount, Is.EqualTo(1));

        Assert.That(result.Items[0].Success, Is.True);
        Assert.That(result.Items[0].SourcePath, Is.EqualTo(heicSourcePath));
        Assert.That(result.Items[0].DestinationPath, Is.EqualTo(heicDestinationPath));
        Assert.That(result.Items[0].Message, Is.EqualTo(heicMessage));

        Assert.That(result.Items[1].Success, Is.True);
        Assert.That(result.Items[1].SourcePath, Is.EqualTo(jpgSourcePath));
        Assert.That(result.Items[1].DestinationPath, Is.EqualTo(jpgDestinationPath));
        Assert.That(result.Items[1].Message, Is.EqualTo(jpgMessage));

        Assert.That(result.Items[2].Success, Is.False);
        Assert.That(result.Items[2].SourcePath, Is.EqualTo(pngSourcePath));
        Assert.That(result.Items[2].DestinationPath, Is.Null);
        Assert.That(result.Items[2].Message, Is.EqualTo(pngMessage));

        Assert.That(progressEvents, Has.Count.EqualTo(3));

        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[0].Message, Is.EqualTo(heicMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(100d / 3));

        Assert.That(progressEvents[1].CompletedCount, Is.EqualTo(2));
        Assert.That(progressEvents[1].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[1].Message, Is.EqualTo(jpgMessage));
        Assert.That(progressEvents[1].ProgressPercentage, Is.EqualTo(200d / 3));

        Assert.That(progressEvents[2].CompletedCount, Is.EqualTo(3));
        Assert.That(progressEvents[2].TotalCount, Is.EqualTo(3));
        Assert.That(progressEvents[2].Message, Is.EqualTo(pngMessage));
        Assert.That(progressEvents[2].ProgressPercentage, Is.EqualTo(100d));

        // Only the two supported files produced a real, decodable PNG; the same-format PNG produced nothing.
        ImageHelper.AssertValidImageOfFormat(heicDestinationPath, SKEncodedImageFormat.Png);
        ImageHelper.AssertValidImageOfFormat(jpgDestinationPath, SKEncodedImageFormat.Png);
        Assert.That(File.Exists(pngDestinationPath), Is.False);

        _testLogger.AssertLogErrors([pngMessage], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_CancellationRequested_ThrowsAndWritesOnlyFirstFile()
    {
        ConfigureApplication();

        string firstSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_9_PNG);
        string secondSourcePath = CopyToWorkingDirectory(FileNames.IMAGE_10_PORTRAIT_PNG);
        string firstDestinationPath = Path.Combine(_convertedDirectory!, "Image 9.jpg");
        string secondDestinationPath = Path.Combine(_convertedDirectory!, "Image 10 portrait.jpg");
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        CancellationTokenSource cancellationTokenSource = new();
        AssetConversionRequest request =
            new([firstSourcePath, secondSourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _application!.ConvertAssetsAsync(
                request,
                e =>
                {
                    progressEvents.Add(e);
                    cancellationTokenSource.Cancel();
                },
                cancellationTokenSource.Token));

        Assert.That(exception?.Message, Is.EqualTo("The operation was canceled."));

        Assert.That(progressEvents, Has.Count.EqualTo(1));
        Assert.That(progressEvents[0].CompletedCount, Is.EqualTo(1));
        Assert.That(progressEvents[0].TotalCount, Is.EqualTo(2));
        Assert.That(progressEvents[0].Message, Is.EqualTo(expectedMessage));
        Assert.That(progressEvents[0].ProgressPercentage, Is.EqualTo(50d));

        // The first file was converted before the cancellation was observed; the second was never processed.
        ImageHelper.AssertValidImageOfFormat(firstDestinationPath, SKEncodedImageFormat.Jpeg);
        Assert.That(File.Exists(secondDestinationPath), Is.False);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public void ProgressPercentage_ZeroTotalCount_ReturnsZero()
    {
        ConfigureApplication();

        AssetConversionProgressEventArgs progressEventArgs = new()
        {
            CompletedCount = 0,
            TotalCount = 0,
            Message = "No images."
        };

        Assert.That(progressEventArgs.ProgressPercentage, Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
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
