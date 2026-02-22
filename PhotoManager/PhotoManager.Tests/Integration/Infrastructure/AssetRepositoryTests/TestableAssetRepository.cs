namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class TestableAssetRepository(
    IDatabase database,
    IPathProviderService pathProviderService,
    IImageProcessingService imageProcessingService,
    IImageMetadataService imageMetadataService,
    IUserConfigurationService userConfigurationService)
    : AssetRepository(
        database,
        pathProviderService,
        imageProcessingService,
        imageMetadataService,
        userConfigurationService)
{
    public Dictionary<string, Dictionary<string, byte[]>> GetThumbnails() => Thumbnails;
}
