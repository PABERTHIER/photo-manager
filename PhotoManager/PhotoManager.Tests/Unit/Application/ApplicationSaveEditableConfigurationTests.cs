namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationSaveEditableConfigurationTests
{
    [Test]
    public void SaveEditableConfiguration_ValidConfiguration_SavesEditableConfiguration()
    {
        IUserConfigurationService userConfigurationService = Substitute.For<IUserConfigurationService>();
        EditableUserConfiguration configuration = ConfigurationFactory.CreateEditableConfiguration();
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

}
