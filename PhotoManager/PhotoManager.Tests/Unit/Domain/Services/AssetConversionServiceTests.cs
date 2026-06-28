using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Domain.Services;

[TestFixture]
public class AssetConversionServiceTests
{
    private string? _assetsDirectory;

    private IFileOperationsService? _fileOperationsService;
    private IImageProcessingService? _imageProcessingService;
    private AssetConversionService? _assetConversionService;
    private TestLogger<AssetConversionService> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _fileOperationsService = Substitute.For<IFileOperationsService>();
        _imageProcessingService = Substitute.For<IImageProcessingService>();
        _testLogger = new();
        _assetConversionService = new(_fileOperationsService, _imageProcessingService, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void ConvertAssetsAsync_NullRequest_ThrowsNullReferenceException()
    {
        AssetConversionRequest? request = null;
        List<AssetConversionProgressEventArgs> progressEvents = [];

        NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _assetConversionService!.ConvertAssetsAsync(request!, progressEvents.Add));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        Assert.That(progressEvents, Is.Empty);

        _fileOperationsService!.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_NullCallback_ThrowsArgumentNullException()
    {
        AssetConversionProgressCallback? callback = null;
        AssetConversionRequest request =
            new([Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG)], AssetConversionTargetFormat.Jpeg);

        ArgumentNullException? exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _assetConversionService!.ConvertAssetsAsync(request, callback!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'callback')"));

        _fileOperationsService!.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_NoFilePaths_ThrowsArgumentException()
    {
        AssetConversionRequest request = new([], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add));

        Assert.That(exception?.Message, Is.EqualTo("At least one file must be selected. (Parameter 'request')"));

        Assert.That(progressEvents, Is.Empty);

        _fileOperationsService!.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors(["Cannot convert assets because no file has been selected."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_EmptyFilePath_ReturnsFailure()
    {
        const string expectedMessage = "The source path is empty.";

        AssetConversionRequest request = new([string.Empty], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService!.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors(["Cannot convert an asset because the source path is null or empty."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_FileDoesNotExist_ReturnsFailure()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);
        string expectedMessage = $"File not found: {sourcePath}";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(false);

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors([$"Cannot convert '{sourcePath}' because the file does not exist."],
            typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_UnsupportedConversion_ReturnsFailure()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.HOMER_GIF);
        const string expectedMessage = $"{FileNames.HOMER_GIF} cannot be converted to .png.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _imageProcessingService!.DidNotReceiveWithAnyArgs().ConvertImage(null!, default);
        _fileOperationsService.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_ConversionThrows_ReturnsFailure()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);
        const string expectedMessage = $"Could not convert {FileNames.IMAGE_9_PNG}.";

        InvalidOperationException expectedException = new("Image conversion failed.");
        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(Path.Combine(_assetsDirectory!, "Converted")).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Jpeg).Throws(expectedException);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogExceptions([expectedException], typeof(AssetConversionService));
    }

    [Test]
    public void ConvertAssetsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        string firstSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);
        string secondSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_10_PORTRAIT_PNG);
        string destinationPath = Path.Combine(_assetsDirectory!, "Converted");
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";
        byte[] convertedBytes = [0x01];

        CancellationTokenSource cancellationTokenSource = new();
        AssetConversionRequest request =
            new([firstSourcePath, secondSourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(firstSourcePath).Returns(true);
        _fileOperationsService.FileExists(secondSourcePath).Returns(true);
        _fileOperationsService.FolderExists(destinationPath).Returns(true);
        _imageProcessingService!.ConvertImage(firstSourcePath, ImageEncodingFormat.Jpeg).Returns(convertedBytes);
        _imageProcessingService.ConvertImage(secondSourcePath, ImageEncodingFormat.Jpeg).Returns([0x02]);

        OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _assetConversionService!.ConvertAssetsAsync(
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

        _fileOperationsService.Received(1).WriteFileBytes(Path.Combine(destinationPath, "Image 9.jpg"), convertedBytes);
        _imageProcessingService.DidNotReceive().ConvertImage(secondSourcePath, ImageEncodingFormat.Jpeg);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_UnknownTargetFormat_ReturnsFailure()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);
        const string expectedMessage = $"Could not convert {FileNames.IMAGE_9_PNG}.";

        const AssetConversionTargetFormat targetFormat = (AssetConversionTargetFormat)999;
        ArgumentOutOfRangeException expectedException =
            new(nameof(targetFormat), targetFormat, "Unknown target format.");
        AssetConversionRequest request = new([sourcePath], targetFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.DidNotReceiveWithAnyArgs().WriteFileBytes(null!, null!);

        _testLogger.AssertLogExceptions([expectedException], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_PngToJpeg_ConvertsFile()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);
        string convertedDirectory = Path.Combine(_assetsDirectory!, "Converted");
        string destinationPath = Path.Combine(convertedDirectory, "Image 9.jpg");
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        const string expectedMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(convertedDirectory).Returns(false);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Jpeg).Returns(convertedBytes);

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.Received(1).CreateDirectory(convertedDirectory);
        _fileOperationsService.Received(1).WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_JpegToPng_ConvertsFile()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        string convertedDirectory = Path.Combine(_assetsDirectory!, "Converted");
        string destinationPath = Path.Combine(convertedDirectory, "Image 8.png");
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        const string expectedMessage = $"{FileNames.IMAGE_8_JPEG} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(convertedDirectory).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Png).Returns(convertedBytes);

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.DidNotReceive().CreateDirectory(convertedDirectory);
        _fileOperationsService.Received(1).WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_HeicToJpeg_ConvertsFile()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        string destinationPath = Path.Combine(_assetsDirectory!, "Converted", "Image_11.jpg");
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        const string expectedMessage = $"{FileNames.IMAGE_11_HEIC} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(Path.Combine(_assetsDirectory!, "Converted")).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Jpeg).Returns(convertedBytes);

        AssetConversionResult result =
            await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.Received(1).WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_HeicToPng_ConvertsFile()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        string destinationPath = Path.Combine(_assetsDirectory!, "Converted", "Image_11.png");
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        const string expectedMessage = $"{FileNames.IMAGE_11_HEIC} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(Path.Combine(_assetsDirectory!, "Converted")).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Png).Returns(convertedBytes);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.Received(1).WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_JpgToPng_ConvertsFile()
    {
        string sourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        string destinationPath = Path.Combine(_assetsDirectory!, "Converted", "Image 1.png");
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        const string expectedMessage = $"{FileNames.IMAGE_1_JPG} converted.";

        AssetConversionRequest request = new([sourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(Path.Combine(_assetsDirectory!, "Converted")).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, ImageEncodingFormat.Png).Returns(convertedBytes);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.Received(1).WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }

    [TestCase(FileNames.IMAGE_1_JPG, ".jpg", AssetConversionTargetFormat.Jpeg, ImageEncodingFormat.Jpeg)]
    [TestCase(FileNames.IMAGE_9_PNG, ".png", AssetConversionTargetFormat.Png, ImageEncodingFormat.Png)]
    public async Task ConvertAssetsAsync_SameFormat_DoesNotConvertFile(string fileName, string fileExtension,
        AssetConversionTargetFormat conversionFormat, ImageEncodingFormat encodingFormat)
    {
        string sourcePath = Path.Combine(_assetsDirectory!, fileName);
        string destinationPath = Path.Combine(_assetsDirectory!, "Converted", fileName);
        byte[] convertedBytes = [0x01, 0x02, 0x03];
        string expectedMessage = $"{fileName} cannot be converted to {fileExtension}.";

        AssetConversionRequest request = new([sourcePath], conversionFormat);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(sourcePath).Returns(true);
        _fileOperationsService.FolderExists(Path.Combine(_assetsDirectory!, "Converted")).Returns(true);
        _imageProcessingService!.ConvertImage(sourcePath, encodingFormat).Returns(convertedBytes);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _fileOperationsService.DidNotReceive().WriteFileBytes(destinationPath, convertedBytes);

        _testLogger.AssertLogErrors([expectedMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_MultipleFilesToJpeg_ConvertsSupportedAndSkipsSameFormat()
    {
        string heicSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        string jpgSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        string pngSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);

        string convertedDirectory = Path.Combine(_assetsDirectory!, "Converted");
        string heicDestinationPath = Path.Combine(convertedDirectory, "Image_11.jpg");
        string pngDestinationPath = Path.Combine(convertedDirectory, "Image 9.jpg");

        byte[] heicConvertedBytes = [0x01, 0x02, 0x03];
        byte[] pngConvertedBytes = [0x04, 0x05, 0x06];

        const string heicMessage = $"{FileNames.IMAGE_11_HEIC} converted.";
        const string jpgMessage = $"{FileNames.IMAGE_1_JPG} cannot be converted to .jpg.";
        const string pngMessage = $"{FileNames.IMAGE_9_PNG} converted.";

        AssetConversionRequest request =
            new([heicSourcePath, jpgSourcePath, pngSourcePath], AssetConversionTargetFormat.Jpeg);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(heicSourcePath).Returns(true);
        _fileOperationsService.FileExists(jpgSourcePath).Returns(true);
        _fileOperationsService.FileExists(pngSourcePath).Returns(true);
        _fileOperationsService.FolderExists(convertedDirectory).Returns(true);
        _imageProcessingService!.ConvertImage(heicSourcePath, ImageEncodingFormat.Jpeg).Returns(heicConvertedBytes);
        _imageProcessingService.ConvertImage(pngSourcePath, ImageEncodingFormat.Jpeg).Returns(pngConvertedBytes);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _imageProcessingService.DidNotReceive().ConvertImage(jpgSourcePath, ImageEncodingFormat.Jpeg);
        _fileOperationsService.DidNotReceive().CreateDirectory(convertedDirectory);
        _fileOperationsService.Received(1).WriteFileBytes(heicDestinationPath, heicConvertedBytes);
        _fileOperationsService.Received(1).WriteFileBytes(pngDestinationPath, pngConvertedBytes);
        _fileOperationsService.ReceivedWithAnyArgs(2).WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors([jpgMessage], typeof(AssetConversionService));
    }

    [Test]
    public async Task ConvertAssetsAsync_MultipleFilesToPng_ConvertsSupportedAndSkipsSameFormat()
    {
        string heicSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        string jpgSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        string pngSourcePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_9_PNG);

        string convertedDirectory = Path.Combine(_assetsDirectory!, "Converted");
        string heicDestinationPath = Path.Combine(convertedDirectory, "Image_11.png");
        string jpgDestinationPath = Path.Combine(convertedDirectory, "Image 1.png");

        byte[] heicConvertedBytes = [0x01, 0x02, 0x03];
        byte[] jpgConvertedBytes = [0x04, 0x05, 0x06];

        const string heicMessage = $"{FileNames.IMAGE_11_HEIC} converted.";
        const string jpgMessage = $"{FileNames.IMAGE_1_JPG} converted.";
        const string pngMessage = $"{FileNames.IMAGE_9_PNG} cannot be converted to .png.";

        AssetConversionRequest request =
            new([heicSourcePath, jpgSourcePath, pngSourcePath], AssetConversionTargetFormat.Png);
        List<AssetConversionProgressEventArgs> progressEvents = [];

        _fileOperationsService!.FileExists(heicSourcePath).Returns(true);
        _fileOperationsService.FileExists(jpgSourcePath).Returns(true);
        _fileOperationsService.FileExists(pngSourcePath).Returns(true);
        _fileOperationsService.FolderExists(convertedDirectory).Returns(true);
        _imageProcessingService!.ConvertImage(heicSourcePath, ImageEncodingFormat.Png).Returns(heicConvertedBytes);
        _imageProcessingService.ConvertImage(jpgSourcePath, ImageEncodingFormat.Png).Returns(jpgConvertedBytes);

        AssetConversionResult result = await _assetConversionService!.ConvertAssetsAsync(request, progressEvents.Add);

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

        _imageProcessingService.DidNotReceive().ConvertImage(pngSourcePath, ImageEncodingFormat.Png);
        _fileOperationsService.DidNotReceive().CreateDirectory(convertedDirectory);
        _fileOperationsService.Received(1).WriteFileBytes(heicDestinationPath, heicConvertedBytes);
        _fileOperationsService.Received(1).WriteFileBytes(jpgDestinationPath, jpgConvertedBytes);
        _fileOperationsService.ReceivedWithAnyArgs(2).WriteFileBytes(null!, null!);

        _testLogger.AssertLogErrors([pngMessage], typeof(AssetConversionService));
    }

    [Test]
    public void ProgressPercentage_ZeroTotalCount_ReturnsZero()
    {
        AssetConversionProgressEventArgs progressEventArgs = new()
        {
            CompletedCount = 0,
            TotalCount = 0,
            Message = "No images."
        };

        Assert.That(progressEventArgs.ProgressPercentage, Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(AssetConversionService));
    }
}
