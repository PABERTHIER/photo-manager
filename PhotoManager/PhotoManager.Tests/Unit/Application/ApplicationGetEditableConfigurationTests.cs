namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationGetEditableConfigurationTests
{
    [Test]
    public void GetEditableConfiguration_ValidConfiguration_ReturnsEditableConfiguration()
    {
        IUserConfigurationService userConfigurationService = Substitute.For<IUserConfigurationService>();
        EditableUserConfiguration configuration = ConfigurationFactory.CreateEditableConfiguration();
        userConfigurationService.GetEditableConfiguration().Returns(configuration);
        PhotoManager.Application.Application application = CreateApplication(userConfigurationService);

        EditableUserConfiguration result = application.GetEditableConfiguration();

        Assert.That(result, Is.SameAs(configuration));
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
