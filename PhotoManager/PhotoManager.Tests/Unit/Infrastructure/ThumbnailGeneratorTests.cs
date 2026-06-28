namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ThumbnailGeneratorTests
{
    private TestLogger<ThumbnailGeneratorTests> _testLogger = new();

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void GenerateThumbnail_BmpFormat_ReturnsBmpBytes()
    {
        byte[] imageBytes = [1, 2, 3];
        byte[] expectedBytes = [4, 5, 6];
        IImageProcessingService imageProcessingService = Substitute.For<IImageProcessingService>();
        IImageData thumbnailImage = Substitute.For<IImageData>();
        imageProcessingService.LoadBitmapThumbnailImage(imageBytes, ImageRotation.Rotate90, 10, 20)
            .Returns(thumbnailImage);
        thumbnailImage.ToByteArray(ImageEncodingFormat.Bmp).Returns(expectedBytes);
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);

        byte[] result = thumbnailGenerator.GenerateThumbnail(imageBytes, ImageRotation.Rotate90, 10, 20,
            ImageEncodingFormat.Bmp);

        Assert.That(result, Is.EqualTo(expectedBytes));
        thumbnailImage.Received(1).ToByteArray(ImageEncodingFormat.Bmp);
        thumbnailImage.Received(1).Dispose();

        _testLogger.AssertLogExceptions([], typeof(ThumbnailGeneratorTests));
    }

    [Test]
    public void GenerateThumbnail_UnsupportedEncodingFormat_ThrowsArgumentOutOfRangeException()
    {
        byte[] imageBytes = [1, 2, 3];
        const ImageEncodingFormat unsupportedFormat = (ImageEncodingFormat)123;
        IImageProcessingService imageProcessingService = Substitute.For<IImageProcessingService>();
        IImageData thumbnailImage = Substitute.For<IImageData>();
        imageProcessingService.LoadBitmapThumbnailImage(imageBytes, ImageRotation.Rotate0, 10, 20)
            .Returns(thumbnailImage);
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            thumbnailGenerator.GenerateThumbnail(imageBytes, ImageRotation.Rotate0, 10, 20, unsupportedFormat));

        Assert.That(exception?.ParamName, Is.EqualTo("encodingFormat"));
        thumbnailImage.Received(1).Dispose();

        _testLogger.AssertLogExceptions([], typeof(ThumbnailGeneratorTests));
    }
}
