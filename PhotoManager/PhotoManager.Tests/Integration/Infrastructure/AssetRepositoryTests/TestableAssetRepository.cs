using Microsoft.Extensions.Logging;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class TestableAssetRepository(
    IDatabase database,
    IPathProviderService pathProviderService,
    IImageProcessingService imageProcessingService,
    IImageMetadataService imageMetadataService,
    IUserConfigurationService userConfigurationService,
    ILogger<AssetRepository> logger)
    : AssetRepository(
        database,
        pathProviderService,
        imageProcessingService,
        imageMetadataService,
        userConfigurationService,
        logger)
{
    public Dictionary<string, Dictionary<string, byte[]>> GetThumbnails() => Thumbnails;
}
