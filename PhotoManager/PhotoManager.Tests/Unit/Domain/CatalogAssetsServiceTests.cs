namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class CatalogAssetsServiceTests
{
    private string? _assetsDirectory;
    private TestLogger<CatalogAssetsService> _testLogger = new();
    private string AssetsDirectory => _assetsDirectory!;

    [SetUp]
    public void SetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "CatalogAssetsServiceTests");
        Directory.CreateDirectory(_assetsDirectory);
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_assetsDirectory))
        {
            Directory.Delete(_assetsDirectory, true);
        }

        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public async Task CatalogAssetsAsync_NewVideosAndBatchLimit_CatalogsFirstCreatedVideoOnly()
    {
        string[] videoFileNames = ["broken.mp4", "created.mp4", "skipped.mp4"];
        Folder folder = CreateFolder();
        Asset createdAsset = CreateAsset(folder, "created.mp4");
        IAssetRepository assetRepository = CreateAssetRepository(folder, []);
        IFileOperationsService fileOperationsService = CreateFileOperationsService(videoFileNames);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 1);
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService);
        CatalogChangeRecorder catalogChanges = [];

        assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([], videoFileNames));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>())
            .Returns(call => call.ArgAt<string[]>(0).Length == 0 ? [] : videoFileNames);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetCreationService.CreateAsset(AssetsDirectory, "broken.mp4", true, true).Returns((Asset?)null);
        assetCreationService.CreateAsset(AssetsDirectory, "created.mp4", true, true).Returns(createdAsset);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(catalogChanges.Add);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(catalogChanges.Count(change => change.Reason == CatalogChangeReason.AssetCreated),
                    Is.EqualTo(1));
                Assert.That(catalogChanges.Single(change => change.Reason == CatalogChangeReason.AssetCreated).Asset,
                    Is.SameAs(createdAsset));

                assetCreationService.DidNotReceive().CreateAsset(AssetsDirectory, "skipped.mp4", true, true);

                _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
            }
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_UpdatedVideosAndBatchLimit_UpdatesFirstCreatedVideoOnly()
    {
        string[] videoFileNames = ["broken.mp4", "updated.mp4", "skipped.mp4"];
        Folder folder = CreateFolder();
        Asset brokenAsset = CreateAsset(folder, "broken.mp4");
        Asset oldUpdatedAsset = CreateAsset(folder, "updated.mp4");
        Asset updatedAsset = CreateAsset(folder, "updated.mp4", "updated-hash");
        Asset skippedAsset = CreateAsset(folder, "skipped.mp4");
        IAssetRepository assetRepository = CreateAssetRepository(folder, [brokenAsset, oldUpdatedAsset, skippedAsset]);
        IFileOperationsService fileOperationsService = CreateFileOperationsService(videoFileNames);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 1);
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService);
        CatalogChangeRecorder catalogChanges = [];

        assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([], videoFileNames));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns(videoFileNames);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetRepository.DeleteAsset(AssetsDirectory, "broken.mp4").Returns(brokenAsset);
        assetRepository.DeleteAsset(AssetsDirectory, "updated.mp4").Returns(oldUpdatedAsset);
        assetCreationService.CreateAsset(AssetsDirectory, "broken.mp4", true, true).Returns((Asset?)null);
        assetCreationService.CreateAsset(AssetsDirectory, "updated.mp4", true, true).Returns(updatedAsset);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(catalogChanges.Add);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(catalogChanges.Count(change => change.Reason == CatalogChangeReason.AssetUpdated),
                    Is.EqualTo(1));
                Assert.That(catalogChanges.Single(change => change.Reason == CatalogChangeReason.AssetUpdated).Asset,
                    Is.SameAs(updatedAsset));

                assetRepository.DidNotReceive().DeleteAsset(AssetsDirectory, "skipped.mp4");
                assetCreationService.DidNotReceive().CreateAsset(AssetsDirectory, "skipped.mp4", true, true);

                _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
            }
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public void CatalogAssetsAsync_DeletedAssetCallbackCancels_ThrowsOperationCanceledException()
    {
        const string firstDeletedFileName = "deleted-1.jpg";
        const string secondDeletedFileName = "deleted-2.jpg";
        Folder folder = CreateFolder();
        Asset firstDeletedAsset = CreateAsset(folder, firstDeletedFileName);
        Asset secondDeletedAsset = CreateAsset(folder, secondDeletedFileName);
        IAssetRepository assetRepository = CreateAssetRepository(folder, [firstDeletedAsset, secondDeletedAsset]);
        IFileOperationsService fileOperationsService = CreateFileOperationsService([]);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 10);
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService);
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        Action cancel = cancellationTokenSource.Cancel;
        CatalogChangeRecorder catalogChanges = [];

        try
        {
            assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([], []));
            assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
            assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
            assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>())
                .Returns([firstDeletedFileName, secondDeletedFileName]);
            assetRepository.DeleteAssets(AssetsDirectory, Arg.Any<IReadOnlyList<string>>())
                .Returns([firstDeletedAsset, secondDeletedAsset]);

            OperationCanceledException? exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
                // ReSharper disable once AccessToDisposedClosure
                await catalogAssetsService.CatalogAssetsAsync(change =>
                {
                    catalogChanges.Add(change);

                    if (change.Reason == CatalogChangeReason.AssetDeleted)
                    {
                        cancel();
                    }
                }, cancellationToken));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo("The operation was canceled."));

                Assert.That(catalogChanges.Count(change => change.Reason == CatalogChangeReason.AssetDeleted),
                    Is.EqualTo(1));
                Assert.That(catalogChanges.Any(change => change.Reason == CatalogChangeReason.CatalogProcessCancelled),
                    Is.True);
                Assert.That(catalogChanges.Last().Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessEnded));

                assetRepository.Received(1).DeleteAssets(
                    AssetsDirectory,
                    Arg.Is<IReadOnlyList<string>>(fileNames => fileNames.Count == 2
                                                               && fileNames[0] == firstDeletedFileName
                                                               && fileNames[1] == secondDeletedFileName));

                _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
            }
        }
        finally
        {
            cancellationTokenSource.Dispose();
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_DeletedAssetsAboveBatchLimit_DeletesOnlyRemainingBatch()
    {
        const string firstDeletedFileName = "deleted-1.jpg";
        const string secondDeletedFileName = "deleted-2.jpg";
        Folder folder = CreateFolder();
        Asset firstDeletedAsset = CreateAsset(folder, firstDeletedFileName);
        Asset secondDeletedAsset = CreateAsset(folder, secondDeletedFileName);
        IAssetRepository assetRepository = CreateAssetRepository(folder, [firstDeletedAsset, secondDeletedAsset]);
        IFileOperationsService fileOperationsService = CreateFileOperationsService([]);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 1);
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService);
        CatalogChangeRecorder catalogChanges = [];

        assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([], []));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>())
            .Returns([firstDeletedFileName, secondDeletedFileName]);
        assetRepository.DeleteAssets(AssetsDirectory, Arg.Any<IReadOnlyList<string>>()).Returns([firstDeletedAsset]);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(catalogChanges.Add);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(catalogChanges.Count(change => change.Reason == CatalogChangeReason.AssetDeleted),
                    Is.EqualTo(1));
                Assert.That(catalogChanges.Single(change => change.Reason == CatalogChangeReason.AssetDeleted).Asset,
                    Is.SameAs(firstDeletedAsset));

                assetRepository.Received(1).DeleteAssets(
                    AssetsDirectory,
                    Arg.Is<IReadOnlyList<string>>(fileNames => fileNames.Count == 1
                                                               && fileNames[0] == firstDeletedFileName));
                assetRepository.DidNotReceive().DeleteAsset(AssetsDirectory, secondDeletedFileName);

                _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
            }
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_ExistingFolder_UpdatesFilePropertiesFromEnumeratedFileInfos()
    {
        const string fileName = "existing.jpg";
        Folder folder = CreateFolder();
        Asset existingAsset = CreateAsset(folder, fileName);
        IAssetRepository assetRepository = CreateAssetRepository(folder, [existingAsset]);
        IFileOperationsService fileOperationsService = CreateFileOperationsService([fileName]);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 10);
        IImageMetadataService imageMetadataService = Substitute.For<IImageMetadataService>();
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService, imageMetadataService);

        assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([fileName], []));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(_ => { });

            imageMetadataService.Received(1).UpdateAssetsFileProperties(
                Arg.Is<List<Asset>>(assets => assets.Count == 1 && assets[0] == existingAsset),
                Arg.Is<IReadOnlyDictionary<string, FileProperties>>(filePropertiesByName =>
                    ContainsFileProperties(filePropertiesByName, fileName, 3)));

            _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_FileInfoCannotBeRead_SkipsFileAndLogs()
    {
        const string fileName = "missing.jpg";
        Folder folder = CreateFolder();
        IAssetRepository assetRepository = CreateAssetRepository(folder, []);
        IFileOperationsService fileOperationsService = Substitute.For<IFileOperationsService>();
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 10);
        IImageMetadataService imageMetadataService = Substitute.For<IImageMetadataService>();
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService, imageMetadataService);
        FileInfo missingFileInfo = new(Path.Combine(AssetsDirectory, fileName));

        fileOperationsService.FolderExists(AssetsDirectory).Returns(true);
        fileOperationsService.GetFileInfos(AssetsDirectory).Returns([missingFileInfo]);
        assetsComparator.GetImageAndVideoNames(Arg.Is<string[]>(fileNames => fileNames.Length == 0)).Returns(([], []));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(_ => { });

            imageMetadataService.Received(1).UpdateAssetsFileProperties(
                Arg.Any<List<Asset>>(),
                Arg.Is<IReadOnlyDictionary<string, FileProperties>>(filePropertiesByName =>
                    filePropertiesByName.Count == 0));
            assetsComparator.Received(1).GetImageAndVideoNames(Arg.Is<string[]>(fileNames => fileNames.Length == 0));

            _testLogger.AssertLogInfos(
                [$"Skipped file metadata snapshot for {missingFileInfo.FullName}."],
                typeof(CatalogAssetsService));
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_UpdatedImageCannotBeRecreatedAndDeleteReturnsNull_DoesNotRaiseDeleteChange()
    {
        const string updatedFileName = "updated.jpg";
        string[] imageFileNames = [updatedFileName];
        Folder folder = CreateFolder();
        Asset existingAsset = CreateAsset(folder, updatedFileName);
        IAssetRepository assetRepository = CreateAssetRepository(folder, [existingAsset]);
        IFileOperationsService fileOperationsService = CreateFileOperationsService(imageFileNames);
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 10);
        CatalogAssetsService catalogAssetsService = CreateService(assetRepository, fileOperationsService,
            assetCreationService, assetsComparator, userConfigurationService);
        CatalogChangeRecorder catalogChanges = [];

        assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>())
            .Returns(call => call.ArgAt<string[]>(0).Contains(updatedFileName) ? ([updatedFileName], []) : ([], []));
        assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns(imageFileNames);
        assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
        fileOperationsService.GetFileBytes(Path.Combine(AssetsDirectory, updatedFileName)).Returns([1, 2, 3]);
        assetCreationService.CreateAssetWithThumbnail(AssetsDirectory, updatedFileName, [1, 2, 3], false, true)
            .Returns((AssetWithThumbnail?)null);
        assetRepository.DeleteAsset(AssetsDirectory, updatedFileName).Returns((Asset?)null);

        try
        {
            await catalogAssetsService.CatalogAssetsAsync(catalogChanges.Add);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(catalogChanges.Any(change => change.Reason == CatalogChangeReason.AssetDeleted), Is.False);
                Assert.That(catalogChanges.Any(change => change.Reason == CatalogChangeReason.AssetUpdated), Is.False);

                assetRepository.Received(1).DeleteAsset(AssetsDirectory, updatedFileName);

                _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
            }
        }
        finally
        {
            catalogAssetsService.Dispose();
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_RootChangesAndReturns_RevisitsPreviouslyStaleSubfolder()
    {
        string firstRootPath = Path.Combine(AssetsDirectory, "first-root");
        string configuredFirstRootPath = $"{firstRootPath}{Path.DirectorySeparatorChar}";
        string firstSubfolderPath = Path.Combine(firstRootPath, "subfolder");
        string secondRootPath = Path.Combine(AssetsDirectory, "second-root");
        Dictionary<string, Folder> foldersByPath = new(StringComparer.Ordinal);
        string? storedAssetsDirectory = null;
        int rootCallIndex = 0;
        IAssetRepository assetRepository = Substitute.For<IAssetRepository>();
        IFileOperationsService fileOperationsService = Substitute.For<IFileOperationsService>();
        IAssetCreationService assetCreationService = Substitute.For<IAssetCreationService>();
        IAssetsComparator assetsComparator = Substitute.For<IAssetsComparator>();
        IUserConfigurationService userConfigurationService = CreateUserConfigurationService(catalogBatchSize: 10);
        CatalogAssetsService? catalogAssetsService = null;

        try
        {
            Directory.CreateDirectory(firstSubfolderPath);
            Directory.CreateDirectory(secondRootPath);

            IObservable<System.Reactive.Unit> assetsUpdated = CreateAssetsUpdatedObservable();

            assetRepository.AssetsUpdated.Returns(assetsUpdated);
            assetRepository.BackupExists().Returns(true);
            assetRepository.FolderExists(Arg.Any<string>())
                .Returns(call => foldersByPath.ContainsKey(call.ArgAt<string>(0)));
            assetRepository.AddFolder(Arg.Any<string>()).Returns(call =>
            {
                string path = call.ArgAt<string>(0);
                Folder folder = new() { Id = Guid.NewGuid(), Path = path };
                foldersByPath[path] = folder;

                return folder;
            });
            assetRepository.GetStoredAssetsDirectory().Returns(_ => storedAssetsDirectory);
            assetRepository
                .When(repository => repository.StoreAssetsDirectory(Arg.Any<string>()))
                .Do(call =>
                {
                    storedAssetsDirectory = call.ArgAt<string>(0);
                });
            assetRepository.GetFoldersPath().Returns(_ => [.. foldersByPath.Keys]);
            assetRepository.GetFolderByPath(Arg.Any<string>())
                .Returns(call => foldersByPath.GetValueOrDefault(call.ArgAt<string>(0)));
            assetRepository.GetCataloguedAssetsByPath(Arg.Any<string>()).Returns([]);
            assetRepository
                .When(repository => repository.DeleteFolder(Arg.Any<Folder>()))
                .Do(call => foldersByPath.Remove(call.Arg<Folder>().Path));

            fileOperationsService.FolderExists(Arg.Any<string>())
                .Returns(call => Directory.Exists(call.ArgAt<string>(0)));
            fileOperationsService.GetFileInfos(Arg.Any<string>()).Returns([]);

            assetsComparator.GetImageAndVideoNames(Arg.Any<string[]>()).Returns(([], []));
            assetsComparator.GetNewFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);
            assetsComparator.GetUpdatedFileNames(Arg.Any<List<Asset>>()).Returns([]);
            assetsComparator.GetDeletedFileNames(Arg.Any<string[]>(), Arg.Any<List<Asset>>()).Returns([]);

            userConfigurationService.GetRootCatalogFolderPaths().Returns(_ =>
            {
                string path = rootCallIndex++ switch
                {
                    0 => configuredFirstRootPath,
                    1 => secondRootPath,
                    _ => configuredFirstRootPath
                };

                return [path];
            });

            catalogAssetsService = CreateService(
                assetRepository,
                fileOperationsService,
                assetCreationService,
                assetsComparator,
                userConfigurationService);

            await catalogAssetsService.CatalogAssetsAsync(_ => { });
            await catalogAssetsService.CatalogAssetsAsync(_ => { });
            await catalogAssetsService.CatalogAssetsAsync(_ => { });

            fileOperationsService.Received(2).GetFileInfos(firstSubfolderPath);

            _testLogger.AssertLogExceptions([], typeof(CatalogAssetsService));
        }
        finally
        {
            catalogAssetsService?.Dispose();
        }
    }

    private CatalogAssetsService CreateService(IAssetRepository assetRepository,
        IFileOperationsService fileOperationsService, IAssetCreationService assetCreationService,
        IAssetsComparator assetsComparator, IUserConfigurationService userConfigurationService,
        IImageMetadataService? imageMetadataService = null)
    {
        imageMetadataService ??= Substitute.For<IImageMetadataService>();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            assetRepository);

        return new(assetRepository, fileOperationsService, imageMetadataService, assetCreationService,
            userConfigurationService, assetsComparator, catalogFolderPipeline, _testLogger);
    }

    private IAssetRepository CreateAssetRepository(Folder folder, Asset[] cataloguedAssets)
    {
        IObservable<System.Reactive.Unit> assetsUpdated = CreateAssetsUpdatedObservable();
        IAssetRepository assetRepository = Substitute.For<IAssetRepository>();
        assetRepository.AssetsUpdated.Returns(assetsUpdated);
        assetRepository.FolderExists(AssetsDirectory).Returns(true);
        assetRepository.GetStoredAssetsDirectory().Returns(AssetsDirectory);
        assetRepository.GetFoldersPath().Returns([AssetsDirectory]);
        assetRepository.GetFolderByPath(AssetsDirectory).Returns(folder);
        assetRepository.GetCataloguedAssetsByPath(AssetsDirectory).Returns(_ => [.. cataloguedAssets]);
        assetRepository.BackupExists().Returns(true);

        return assetRepository;
    }

    private static IObservable<System.Reactive.Unit> CreateAssetsUpdatedObservable()
    {
        IObservable<System.Reactive.Unit> assetsUpdated = Substitute.For<IObservable<System.Reactive.Unit>>();
        IDisposable subscription = Substitute.For<IDisposable>();
        assetsUpdated.Subscribe(Arg.Any<IObserver<System.Reactive.Unit>>()).Returns(subscription);

        return assetsUpdated;
    }

    private IFileOperationsService CreateFileOperationsService(string[] fileNames)
    {
        IFileOperationsService fileOperationsService = Substitute.For<IFileOperationsService>();
        fileOperationsService.FolderExists(AssetsDirectory).Returns(true);
        fileOperationsService.GetFileInfos(AssetsDirectory).Returns(_ => CreateFileInfos(AssetsDirectory, fileNames));

        return fileOperationsService;
    }

    private static FileInfo[] CreateFileInfos(string directory, string[] fileNames)
    {
        FileInfo[] fileInfos = new FileInfo[fileNames.Length];

        for (int i = 0; i < fileNames.Length; i++)
        {
            string filePath = Path.Combine(directory, fileNames[i]);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(filePath, [1, 2, 3]);
            fileInfos[i] = new(filePath);
        }

        return fileInfos;
    }

    private static bool ContainsFileProperties(IReadOnlyDictionary<string, FileProperties> filePropertiesByName,
        string fileName, long expectedSize)
    {
        return filePropertiesByName.TryGetValue(fileName, out FileProperties fileProperties)
               && fileProperties.Size == expectedSize;
    }

    private IUserConfigurationService CreateUserConfigurationService(int catalogBatchSize)
    {
        IUserConfigurationService userConfigurationService = Substitute.For<IUserConfigurationService>();
        AssetSettings assetSettings = new(true, "The asset is corrupted", "The asset has been rotated",
            catalogBatchSize, 0, 0, 1, false, false, 150, 200);
        userConfigurationService.AssetSettings.Returns(assetSettings);
        userConfigurationService.PipelineSettings.Returns(new PipelineSettings(4, 0, 16, 32, 64, 50));
        userConfigurationService.GetRootCatalogFolderPaths().Returns([AssetsDirectory]);

        return userConfigurationService;
    }

    private Folder CreateFolder() => new()
    {
        Id = Guid.NewGuid(),
        Path = AssetsDirectory
    };

    private static Asset CreateAsset(Folder folder, string fileName, string? hash = null) =>
        AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithFileProperties(1, DateTime.UnixEpoch, DateTime.UnixEpoch)
            .WithPixels(100, 100, 50, 50)
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(hash ?? fileName)
            .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
}
