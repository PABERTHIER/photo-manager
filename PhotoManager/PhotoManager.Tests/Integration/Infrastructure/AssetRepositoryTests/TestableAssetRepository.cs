namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class TestableAssetRepository(IDatabase database, IStorageService storageService, IUserConfigurationService userConfigurationService) : AssetRepository(database, storageService, userConfigurationService)
{
    public Dictionary<string, Dictionary<string, byte[]>> GetThumbnails() => Thumbnails;
}
