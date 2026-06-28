namespace PhotoManager.Tests.Unit.Domain.Services;

[TestFixture]
public class SyncAssetsServiceTests
{
    [Test]
    public async Task ExecuteAsync_ExceptionThrownDuringSync_ReturnsResultWithExceptionMessage()
    {
        const string sourceDirectory = "C:\\Source";
        const string destinationDirectory = "C:\\Destination";
        const string exceptionMessage = "Disk read error.";

        SyncAssetsConfiguration syncAssetsConfiguration = new();

        syncAssetsConfiguration.Definitions.Add(new()
        {
            SourceDirectory = sourceDirectory,
            DestinationDirectory = destinationDirectory
        });

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.GetSyncAssetsConfiguration().Returns(syncAssetsConfiguration);

        IFileOperationsService fileOperationsServiceMock = Substitute.For<IFileOperationsService>();
        fileOperationsServiceMock.FolderExists(sourceDirectory).Returns(true);
        fileOperationsServiceMock.FolderExists(destinationDirectory).Returns(true);
        fileOperationsServiceMock.GetFileNames(sourceDirectory).Throws(new IOException(exceptionMessage));

        IAssetsComparator assetsComparatorMock = Substitute.For<IAssetsComparator>();
        IMoveAssetsService moveAssetsServiceMock = Substitute.For<IMoveAssetsService>();

        SyncAssetsService service = new(assetRepositoryMock, fileOperationsServiceMock, assetsComparatorMock,
            moveAssetsServiceMock);

        List<ProcessStatusChangedCallbackEventArgs> statusChanges = [];

        List<SyncAssetsResult> result = await service.ExecuteAsync(statusChanges.Add);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].SourceDirectory, Is.EqualTo(sourceDirectory));
        Assert.That(result[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
        Assert.That(result[0].SyncedImages, Is.Zero);
        Assert.That(result[0].Message, Is.EqualTo(exceptionMessage));

        Assert.That(statusChanges, Is.Empty);
    }
}
