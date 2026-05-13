using System.Reactive.Linq;
using ReactiveUnit = System.Reactive.Unit;

namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class CatalogAssetsServiceTests
{
    private TestLogger<CatalogAssetsService>? _testLogger;

    [SetUp]
    public void SetUp()
    {
        _testLogger = new TestLogger<CatalogAssetsService>();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void
        CatalogAssetsAsync_CancellationBeforeAnyFolderProcessed_SavesWithNullFolderAndThrowsOperationCanceledException()
    {
        const string testPath = "C:\\test\\photos";

        Folder testFolder = new()
        {
            Id = Guid.NewGuid(),
            Path = testPath
        };

        CancellationTokenSource cancellationTokenSource = new();

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.AssetsUpdated.Returns(Observable.Never<ReactiveUnit>());
        assetRepositoryMock.FolderExists(Arg.Any<string>()).Returns(false);
        assetRepositoryMock.AddFolder(Arg.Any<string>()).Returns(testFolder);

        // Cancel the token inside GetFoldersPath so that _currentFolderPath is still empty
        // when the OperationCanceledException is caught (covers the null branch of the ternary).
        assetRepositoryMock.GetFoldersPath().Returns(_ =>
        {
            cancellationTokenSource.Cancel();
            return [testPath];
        });


        Folder savedFolder = new() { Id = Guid.NewGuid(), Path = "folder" };

        IFileOperationsService fileOpsServiceMock = Substitute.For<IFileOperationsService>();
        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();
        IAssetCreationService assetCreationServiceMock = Substitute.For<IAssetCreationService>();

        AssetSettings assetSettings = new(
            AnalyseVideos: false,
            CorruptedMessage: "Corrupted",
            RotatedMessage: "Rotated",
            CatalogBatchSize: 1,
            CatalogCooldownMinutes: 0,
            CorruptedImageOrientation: 0,
            DefaultExifOrientation: 1,
            DetectThumbnails: false,
            SyncAssetsEveryXMinutes: false,
            ThumbnailMaxHeight: 150,
            ThumbnailMaxWidth: 200);

        IUserConfigurationService userConfigMock = Substitute.For<IUserConfigurationService>();
        userConfigMock.AssetSettings.Returns(assetSettings);
        userConfigMock.GetRootCatalogFolderPaths().Returns([testPath]);

        IAssetsComparator assetsComparatorMock = Substitute.For<IAssetsComparator>();

        using CatalogAssetsService service = new(
            assetRepositoryMock,
            fileOpsServiceMock,
            imageMetadataServiceMock,
            assetCreationServiceMock,
            userConfigMock,
            assetsComparatorMock,
            _testLogger!);

        List<CatalogChangeCallbackEventArgs> catalogChanges = [];

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await service.CatalogAssetsAsync(catalogChanges.Add, cancellationTokenSource.Token));

        Assert.That(savedFolder, Is.Null);

        Assert.That(catalogChanges, Has.Count.EqualTo(2));

        int increment = 0;

        CatalogAssetsAsyncAsserts.CheckCatalogChangesCancelled(catalogChanges, ref increment);
        CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

        _testLogger!.AssertLogExceptions([], typeof(CatalogAssetsService));
    }
}
