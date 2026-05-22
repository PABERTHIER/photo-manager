using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ImageMetadataServiceTests
{
    private string? _assetsDirectory;

    private ImageMetadataService? _imageMetadataService;
    private FileOperationsService? _fileOperationService;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<ImageMetadataService>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _assetsDirectory);

        _userConfigurationService = new(configurationRootMock);
        _fileOperationService = new(_userConfigurationService, new TestLogger<FileOperationsService>());
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _imageMetadataService = new(_fileOperationService!, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [TestCase((ushort)0, ImageRotation.Rotate0)]
    [TestCase((ushort)1, ImageRotation.Rotate0)]
    [TestCase((ushort)2, ImageRotation.Rotate0)]
    [TestCase((ushort)3, ImageRotation.Rotate180)]
    [TestCase((ushort)4, ImageRotation.Rotate180)]
    [TestCase((ushort)5, ImageRotation.Rotate90)]
    [TestCase((ushort)6, ImageRotation.Rotate90)]
    [TestCase((ushort)7, ImageRotation.Rotate270)]
    [TestCase((ushort)8, ImageRotation.Rotate270)]
    [TestCase((ushort)9, ImageRotation.Rotate0)]
    [TestCase((ushort)10, ImageRotation.Rotate0)]
    [TestCase((ushort)10000, ImageRotation.Rotate0)]
    [TestCase(ushort.MinValue, ImageRotation.Rotate0)]
    [TestCase(ushort.MaxValue, ImageRotation.Rotate0)]
    public void GetImageRotation_ValidExifOrientation_ReturnsCorrectRotationValue(ushort exifOrientation,
        ImageRotation expectedRotation)
    {
        ImageRotation rotation = _imageMetadataService!.GetImageRotation(exifOrientation);

        Assert.That(rotation, Is.EqualTo(expectedRotation));

        _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        // ReSharper disable once IntVariableOverflowInUncheckedContext
        ImageRotation rotation = _imageMetadataService!.GetImageRotation((ushort)exifOrientation);

        Assert.That(rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() =>
                _imageMetadataService!.GetImageRotation((ushort)exifOrientation!));

        Assert.That(exception?.Message, Is.EqualTo("Nullable object must have a value."));

        _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
    }
}
