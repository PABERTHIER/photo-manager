using System.Reactive.Linq;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
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
        CatalogAssetsAsync_CancellationDuringSyncAndSaveFails_LogsErrorAndThrowsOperationCanceledException()
    {
        const string testPath = "C:\\test\\photos";
        const string testFileName = "test.jpg";

        Folder testFolder = new()
        {
            Id = Guid.NewGuid(),
            Path = testPath
        };

        Asset testAsset = new()
        {
            FolderId = testFolder.Id,
            Folder = testFolder,
            FileName = testFileName,
            Pixel = new()
            {
                Asset = new() { Width = 100, Height = 100 },
                Thumbnail = new() { Width = 50, Height = 50 }
            },
            FileProperties = new()
            {
                Size = 1000,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            Hash = Hashes.IMAGE_1_JPG,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.AssetsUpdated.Returns(Observable.Never<ReactiveUnit>());
        assetRepositoryMock.FolderExists(Arg.Any<string>()).Returns(false, true);
        assetRepositoryMock.AddFolder(Arg.Any<string>()).Returns(testFolder);
        assetRepositoryMock.GetFoldersPath().Returns([testPath]);
        assetRepositoryMock.GetFolderByPath(Arg.Any<string>()).Returns(testFolder);
        assetRepositoryMock.GetCataloguedAssetsByPath(Arg.Any<string>()).Returns([]);
        assetRepositoryMock.HasChanges().Returns(true);
        assetRepositoryMock.IsBlobFileExists(Arg.Any<string>()).Returns(true);
        assetRepositoryMock.BackupExists().Returns(false);

        int saveCatalogCallCount = 0;
        IOException saveException = new("Disk full");

        assetRepositoryMock.When(x => x.SaveCatalog(Arg.Any<Folder?>()))
            .Do(_ =>
            {
                saveCatalogCallCount++;

                if (saveCatalogCallCount >= 2)
                {
                    throw saveException;
                }
            });

        IFileOperationsService fileOpsServiceMock = Substitute.For<IFileOperationsService>();
        fileOpsServiceMock.FolderExists(Arg.Any<string>()).Returns(true);
        fileOpsServiceMock.GetFileNames(Arg.Any<string>()).Returns([testFileName]);

        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        IAssetCreationService assetCreationServiceMock = Substitute.For<IAssetCreationService>();
        assetCreationServiceMock.CreateAsset(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(testAsset);

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

        string[] imageNames = [testFileName];
        string[] emptyNames = [];

        IAssetsComparator assetsComparatorMock = Substitute.For<IAssetsComparator>();
        assetsComparatorMock.GetImageAndVideoNames(Arg.Any<string[]>()).Returns((imageNames, emptyNames));
        assetsComparatorMock.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns(imageNames);
        assetsComparatorMock.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns(emptyNames);
        assetsComparatorMock.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns(emptyNames);

        using CatalogAssetsService service = new(
            assetRepositoryMock,
            fileOpsServiceMock,
            imageMetadataServiceMock,
            assetCreationServiceMock,
            userConfigMock,
            assetsComparatorMock,
            _testLogger!);

        CancellationTokenSource cancellationTokenSource = new();
        List<CatalogChangeCallbackEventArgs> catalogChanges = [];

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await service.CatalogAssetsAsync(e =>
            {
                catalogChanges.Add(e);

                if (e.Reason == CatalogChangeReason.AssetCreated)
                {
                    cancellationTokenSource.Cancel();
                }
            }, cancellationTokenSource.Token));

        Assert.That(saveCatalogCallCount, Is.EqualTo(2));

        Assert.That(catalogChanges, Has.Count.EqualTo(4));

        int increment = 0;
        Folder[] foldersInRepository = [testFolder];

        CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository,
            testPath, ref increment);
        CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, testPath, [testAsset], testAsset,
            testFolder, ref increment);
        CatalogAssetsAsyncAsserts.CheckCatalogChangesCancelled(catalogChanges, ref increment);
        CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

        _testLogger!.AssertLogExceptions([saveException], typeof(CatalogAssetsService));
    }

    [Test]
    public void
        CatalogAssetsAsync_CancellationBeforeAnyFolderProcessedAndHasChanges_SavesWithNullFolderAndThrowsOperationCanceledException()
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

        assetRepositoryMock.HasChanges().Returns(true);

        Folder? savedFolder = new() { Id = Guid.NewGuid(), Path = "sentinel" };
        assetRepositoryMock.When(x => x.SaveCatalog(Arg.Any<Folder?>()))
            .Do(x => savedFolder = (Folder?)x[0]);

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
