namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class TestableAssetRepository : AssetRepository
{
    public Dictionary<string, Dictionary<string, byte[]>> GetThumbnails() => Thumbnails;

    public TestableAssetRepository(IDatabase database, IStorageService storageService, IUserConfigurationService userConfigurationService) : base(database, storageService, userConfigurationService)
    {
    }
}
