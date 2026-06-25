namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationConvertAssetsAsyncTests
{
    [Test]
    public async Task ConvertAssetsAsync_ValidFormat_ReturnsConversionResult()
    {
        IAssetConversionService assetConversionService = Substitute.For<IAssetConversionService>();
        PhotoManager.Application.Application application = CreateApplication(assetConversionService);

        AssetConversionRequest request = new([@"C:\Photos\Image.png"], AssetConversionTargetFormat.Jpeg);
        AssetConversionProgressCallback callback = Callback;
        AssetConversionResult expectedResult = new([new(@"C:\Photos\Image.png", @"C:\Photos\Converted\Image.jpg",
            true, "Image.png converted.")]);

        using (CancellationTokenSource cancellationTokenSource = new())
        {
            assetConversionService.ConvertAssetsAsync(request, callback, cancellationTokenSource.Token)
                                  .Returns(Task.FromResult(expectedResult));

            AssetConversionResult result =
                await application.ConvertAssetsAsync(request, callback, cancellationTokenSource.Token);

            Assert.That(result, Is.SameAs(expectedResult));
        }

        static void Callback(AssetConversionProgressEventArgs _)
        {
        }
    }

    private static PhotoManager.Application.Application CreateApplication(
        IAssetConversionService assetConversionService)
    {
        return new(
            Substitute.For<IAssetRepository>(),
            Substitute.For<ISyncAssetsService>(),
            Substitute.For<ICatalogAssetsService>(),
            Substitute.For<IMoveAssetsService>(),
            Substitute.For<IFindDuplicatedAssetsService>(),
            Substitute.For<IUserConfigurationService>(),
            Substitute.For<IFileOperationsService>(),
            Substitute.For<IImageProcessingService>(),
            assetConversionService);
    }
}
