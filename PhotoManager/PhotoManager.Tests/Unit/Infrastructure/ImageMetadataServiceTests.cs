using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ImageMetadataServiceTests
{
    private string? _dataDirectory;

    private ImageMetadataService? _imageMetadataService;
    private FileOperationsService? _fileOperationService;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new(configurationRootMock.Object);
        _fileOperationService = new(_userConfigurationService);
        _imageMetadataService = new(_fileOperationService);
    }

    [Test]
    [TestCase((ushort)0, Rotation.Rotate0)]
    [TestCase((ushort)1, Rotation.Rotate0)]
    [TestCase((ushort)2, Rotation.Rotate0)]
    [TestCase((ushort)3, Rotation.Rotate180)]
    [TestCase((ushort)4, Rotation.Rotate180)]
    [TestCase((ushort)5, Rotation.Rotate90)]
    [TestCase((ushort)6, Rotation.Rotate90)]
    [TestCase((ushort)7, Rotation.Rotate270)]
    [TestCase((ushort)8, Rotation.Rotate270)]
    [TestCase((ushort)9, Rotation.Rotate0)]
    [TestCase((ushort)10, Rotation.Rotate0)]
    [TestCase((ushort)10000, Rotation.Rotate0)]
    [TestCase(ushort.MinValue, Rotation.Rotate0)]
    [TestCase(ushort.MaxValue, Rotation.Rotate0)]
    public void GetImageRotation_ValidExifOrientation_ReturnsCorrectRotationValue(ushort exifOrientation,
        Rotation expectedRotation)
    {
        Rotation rotation = _imageMetadataService!.GetImageRotation(exifOrientation);

        Assert.That(rotation, Is.EqualTo(expectedRotation));
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        // ReSharper disable once IntVariableOverflowInUncheckedContext
        Rotation rotation = _imageMetadataService!.GetImageRotation((ushort)exifOrientation);

        Assert.That(rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() =>
                _imageMetadataService!.GetImageRotation((ushort)exifOrientation!));

        Assert.That(exception?.Message, Is.EqualTo("Nullable object must have a value."));
    }
}
