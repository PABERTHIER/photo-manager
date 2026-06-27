namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationSaveEditableConfigurationTests
{
    [Test]
    public void SaveEditableConfiguration_ValidConfiguration_SavesEditableConfiguration()
    {
        IUserConfigurationService userConfigurationService = Substitute.For<IUserConfigurationService>();
        EditableUserConfiguration configuration = CreateEditableConfiguration();
        PhotoManager.Application.Application application = CreateApplication(userConfigurationService);

        application.SaveEditableConfiguration(configuration);

        userConfigurationService.Received(1).SaveEditableConfiguration(configuration);
    }

    private static PhotoManager.Application.Application CreateApplication(
        IUserConfigurationService userConfigurationService)
    {
        return new(
            Substitute.For<IAssetRepository>(),
            Substitute.For<ISyncAssetsService>(),
            Substitute.For<ICatalogAssetsService>(),
            Substitute.For<IMoveAssetsService>(),
            Substitute.For<IFindDuplicatedAssetsService>(),
            userConfigurationService,
            Substitute.For<IFileOperationsService>(),
            Substitute.For<IImageProcessingService>(),
            Substitute.For<IAssetConversionService>());
    }

    private static EditableUserConfiguration CreateEditableConfiguration()
    {
        return new(
            new(true, "Corrupted", "Rotated", 42, 3, 999, 1, true, true, 320, 640),
            new(6, true, true, true),
            new(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets"),
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
                "Frames"),
            new(5, 1, 10, 20, 30, 25),
            new(4, 12),
            new("Dark"));
    }
}
