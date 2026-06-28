using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Reactive = System.Reactive;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class AssetRepositoryTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IImageProcessingService? _imageProcessingServiceMock;
    private IImageMetadataService? _imageMetadataServiceMock;
    private IConfigurationRoot? _configurationRootMock;
    private IPersistenceContext? _persistenceContextMock;
    private IFolderPersistence? _folderPersistenceMock;
    private IAssetPersistence? _assetPersistenceMock;
    private IThumbnailPersistence? _thumbnailPersistenceMock;
    private IRecentPathsPersistence? _recentPathsPersistenceMock;
    private ISyncDefinitionsPersistence? _syncDefinitionsPersistenceMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();

        _folderPersistenceMock = Substitute.For<IFolderPersistence>();
        _assetPersistenceMock = Substitute.For<IAssetPersistence>();
        _thumbnailPersistenceMock = Substitute.For<IThumbnailPersistence>();
        _recentPathsPersistenceMock = Substitute.For<IRecentPathsPersistence>();
        _syncDefinitionsPersistenceMock = Substitute.For<ISyncDefinitionsPersistence>();

        _folderPersistenceMock.GetAll().Returns([]);
        _assetPersistenceMock.GetAll().Returns([]);
        _recentPathsPersistenceMock.GetAll().Returns([]);
        _syncDefinitionsPersistenceMock.GetAll().Returns(Array.Empty<SyncAssetsDirectoriesDefinition>());

        _persistenceContextMock = Substitute.For<IPersistenceContext>();
        _persistenceContextMock.Folders.Returns(_folderPersistenceMock);
        _persistenceContextMock.Assets.Returns(_assetPersistenceMock);
        _persistenceContextMock.Thumbnails.Returns(_thumbnailPersistenceMock);
        _persistenceContextMock.RecentPaths.Returns(_recentPathsPersistenceMock);
        _persistenceContextMock.SyncDefinitions.Returns(_syncDefinitionsPersistenceMock);

        _imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        _imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();

        _assetRepository = new(_imageProcessingServiceMock, _imageMetadataServiceMock, userConfigurationService,
            _persistenceContextMock, _testLogger);

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" })
            .WithFolderId(new("876283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_1_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void Constructor_ReadCatalogThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        TestLogger<AssetRepository> testLogger = new();

        IFolderPersistence folderPersistenceMock = Substitute.For<IFolderPersistence>();
        IOException expectedInnerException = new("DB corrupted");
        folderPersistenceMock.GetAll().Throws(expectedInnerException);

        IAssetPersistence assetPersistenceMock = Substitute.For<IAssetPersistence>();
        IThumbnailPersistence thumbnailPersistenceMock = Substitute.For<IThumbnailPersistence>();
        IRecentPathsPersistence recentPathsPersistenceMock = Substitute.For<IRecentPathsPersistence>();
        ISyncDefinitionsPersistence syncDefinitionsPersistenceMock = Substitute.For<ISyncDefinitionsPersistence>();

        IPersistenceContext persistenceContextMock = Substitute.For<IPersistenceContext>();
        persistenceContextMock.Folders.Returns(folderPersistenceMock);
        persistenceContextMock.Assets.Returns(assetPersistenceMock);
        persistenceContextMock.Thumbnails.Returns(thumbnailPersistenceMock);
        persistenceContextMock.RecentPaths.Returns(recentPathsPersistenceMock);
        persistenceContextMock.SyncDefinitions.Returns(syncDefinitionsPersistenceMock);

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        try
        {
            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() =>
                    new AssetRepository(imageProcessingServiceMock, imageMetadataServiceMock,
                        userConfigurationService, persistenceContextMock, testLogger));

                Assert.That(exception?.Message, Is.EqualTo(expectedInnerException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                testLogger.AssertLogExceptions([expectedInnerException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void Constructor_AssetFolderIdNotInPersistedFolders_AssetFolderIsNotOverridden()
    {
        Guid assetFolderId = Guid.NewGuid();
        Folder knownFolder = new() { Id = Guid.NewGuid(), Path = @"C:\KnownFolder" };
        Folder originalFolder = new() { Id = assetFolderId, Path = @"C:\OriginalPath" };

        Asset asset = AssetBuilder.Create()
            .WithFolder(originalFolder)
            .WithFileName("orphan.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(100, 100, 50, 50)
            .WithFileProperties(1000, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("orphanHash")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        IFolderPersistence folderPersistenceMock = Substitute.For<IFolderPersistence>();
        folderPersistenceMock.GetAll().Returns([knownFolder]);

        IAssetPersistence assetPersistenceMock = Substitute.For<IAssetPersistence>();
        assetPersistenceMock.GetAll().Returns([asset]);

        IThumbnailPersistence thumbnailPersistenceMock = Substitute.For<IThumbnailPersistence>();

        IRecentPathsPersistence recentPathsPersistenceMock = Substitute.For<IRecentPathsPersistence>();
        recentPathsPersistenceMock.GetAll().Returns([]);

        ISyncDefinitionsPersistence syncDefinitionsPersistenceMock = Substitute.For<ISyncDefinitionsPersistence>();
        syncDefinitionsPersistenceMock.GetAll().Returns(Array.Empty<SyncAssetsDirectoriesDefinition>());

        IPersistenceContext persistenceContextMock = Substitute.For<IPersistenceContext>();
        persistenceContextMock.Folders.Returns(folderPersistenceMock);
        persistenceContextMock.Assets.Returns(assetPersistenceMock);
        persistenceContextMock.Thumbnails.Returns(thumbnailPersistenceMock);
        persistenceContextMock.RecentPaths.Returns(recentPathsPersistenceMock);
        persistenceContextMock.SyncDefinitions.Returns(syncDefinitionsPersistenceMock);

        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_imageProcessingServiceMock!, imageMetadataServiceMock,
            userConfigurationService, persistenceContextMock, testLogger);

        try
        {
            Asset[] cataloguedAssets = assetRepository.GetCataloguedAssets();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cataloguedAssets, Has.Length.EqualTo(1));
                Assert.That(cataloguedAssets[0].FileName, Is.EqualTo("orphan.jpg"));
                Assert.That(cataloguedAssets[0].Folder, Is.SameAs(originalFolder));

                imageMetadataServiceMock.Received(1).UpdateAssetFileProperties(asset);

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void AddAsset_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            IOException expectedException = new("Database read error");

            _folderPersistenceMock!.Insert(Arg.Any<string>()).Throws(expectedException);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() =>
                    _assetRepository!.AddAsset(_asset1!, assetData));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _folderPersistenceMock!.Received(1).Insert(Arg.Any<string>());
                _persistenceContextMock!.DidNotReceive().UpsertAssetWithThumbnail(Arg.Any<Asset>(), Arg.Any<byte[]>());
                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(
                        Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_PersistenceThrowsException_LogsItAndDoesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            byte[] thumbnailData = [1, 2, 3];
            IOException expectedException = new("Asset persistence error");

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _assetRepository!.AddFolder(folderPath);
            _persistenceContextMock!
                .When(context => context.UpsertAssetWithThumbnail(
                    Arg.Is<Asset>(candidate => candidate.FolderId == asset.FolderId
                                               && candidate.FileName == asset.FileName),
                    thumbnailData))
                .Do(_ => throw expectedException);

            IOException? exception = Assert.Throws<IOException>(() => _assetRepository.AddAsset(asset, thumbnailData));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));
                Assert.That(_assetRepository.GetCataloguedAssets(), Is.Empty);
                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).UpsertAssetWithThumbnail(
                    Arg.Is<Asset>(candidate => candidate.FolderId == asset.FolderId
                                               && candidate.FileName == asset.FileName),
                    thumbnailData);
                _thumbnailPersistenceMock!.DidNotReceive().GetByFolderId(Arg.Any<Guid>());
                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(
                        Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderIdExistsWithDifferentPath_UsesTrackedFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string trackedPath = @"C:\TrackedFolder";
            const string assetPath = @"C:\OtherFolder";
            Folder trackedFolder = new() { Id = Guid.NewGuid(), Path = trackedPath };
            Asset asset = _asset1!.WithFolder(new() { Id = trackedFolder.Id, Path = assetPath });
            byte[] thumbnailData = [1, 2, 3];
            _folderPersistenceMock!.Insert(trackedPath).Returns(trackedFolder);
            _thumbnailPersistenceMock!.GetByFolderId(trackedFolder.Id).Returns([]);
            _assetRepository.AddFolder(trackedPath);
            assetsUpdatedEvents.Clear();

            _assetRepository.AddAsset(asset, thumbnailData);

            using (Assert.EnterMultipleScope())
            {
                Asset cataloguedAsset = _assetRepository.GetCataloguedAssets().Single();
                Assert.That(cataloguedAsset.Folder, Is.SameAs(trackedFolder));
                Assert.That(cataloguedAsset.Folder.Path, Is.EqualTo(trackedPath));
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));

                _persistenceContextMock!.Received(1).UpsertAssetWithThumbnail(
                    Arg.Is<Asset>(candidate => candidate.FolderId == trackedFolder.Id
                                               && candidate.Folder.Path == trackedPath),
                    thumbnailData);

                _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ReplacesExistingAsset_DisposesPreviousImageDataAndKeepsAssetCounter()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
        IImageData previousImageData = Substitute.For<IImageData>();
        IImageData currentImageData = Substitute.For<IImageData>();
        Asset previousAsset = CreateAssetWithImageData(folder, previousImageData);
        Asset currentAsset = CreateAssetWithImageData(folder, currentImageData);

        _folderPersistenceMock!.Insert(folderPath).Returns(folder);
        _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

        _assetRepository!.AddAsset(previousAsset, [1, 2, 3]);
        _assetRepository.AddAsset(currentAsset, [4, 5, 6]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_assetRepository.GetAssetsCounter(), Is.EqualTo(1));
            previousImageData.Received(1).Dispose();
            currentImageData.DidNotReceive().Dispose();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void AddAsset_SameReferenceAddedTwice_DoesNotDisposeImageDataAndKeepsAssetCounter()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
        IImageData imageData = Substitute.For<IImageData>();
        Asset asset = CreateAssetWithImageData(folder, imageData);

        _folderPersistenceMock!.Insert(folderPath).Returns(folder);
        _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

        _assetRepository!.AddAsset(asset, [1, 2, 3]);
        _assetRepository.AddAsset(asset, [4, 5, 6]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_assetRepository.GetAssetsCounter(), Is.EqualTo(1));
            Assert.That(asset.ImageData, Is.Not.Null);
            imageData.DidNotReceive().Dispose();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void AddAsset_ConcurrentSameFileName_RetriesAndFinalAssetCountIsOne()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

            const int threadCount = 50;
            ManualResetEventSlim barrier = new(false);

            Asset[] assets = new Asset[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                assets[i] = AssetBuilder.Create()
                    .WithFolder(folder)
                    .WithFileName(FileNames.IMAGE_1_JPG)
                    .WithRotation(ImageRotation.Rotate0)
                    .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                        ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
                    .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
                    .WithThumbnailCreationDateTime(DateTime.Now)
                    .WithHash(Hashes.IMAGE_1_JPG)
                    .WithCorrupted(false, null)
                    .WithRotated(false, null)
                    .Build();
            }

            Task[] tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int idx = i;
                tasks[i] = Task.Run(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    barrier.Wait();
                    _assetRepository!.AddAsset(assets[idx], [1, 2, 3]);
                });
            }

            barrier.Set();
            Task.WaitAll(tasks);
            barrier.Dispose();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_assetRepository!.GetAssetsCounter(), Is.EqualTo(1));
                Assert.That(_assetRepository.GetCataloguedAssets(), Has.Length.EqualTo(1));
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(threadCount));

                _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_DisposedRepository_ThrowsObjectDisposedException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            ObjectDisposedException expectedException = new(nameof(AssetRepository));

            _assetRepository!.Dispose();

            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            ObjectDisposedException? exception = Assert.Throws<ObjectDisposedException>(() =>
                _assetRepository!.AddAsset(asset, assetData));

            Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _folderPersistenceMock!.DidNotReceive().Insert(Arg.Any<string>());
            _persistenceContextMock!.DidNotReceive().UpsertAssetWithThumbnail(Arg.Any<Asset>(), Arg.Any<byte[]>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAssets_PersistenceThrowsException_LogsItAndDoesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string folderPath = @"C:\BatchFolder";
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            AssetWithThumbnail assetWithThumbnail = new(asset, [1, 2, 3]);
            IOException expectedException = new("Batch persistence error");

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _assetRepository.AddFolder(folderPath);
            _persistenceContextMock!
                .When(context => context.UpsertAssetsWithThumbnails(Arg.Any<IReadOnlyList<AssetWithThumbnail>>()))
                .Do(_ => throw expectedException);

            IOException? exception = Assert.Throws<IOException>(() =>
                _assetRepository.AddAssets([assetWithThumbnail]));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));
                Assert.That(_assetRepository.GetCataloguedAssets(), Is.Empty);
                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).UpsertAssetsWithThumbnails(
                    Arg.Any<IReadOnlyList<AssetWithThumbnail>>());
                _thumbnailPersistenceMock!.DidNotReceive().GetByFolderId(Arg.Any<Guid>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAssets_PersistenceThrowsException_LogsItAndDoesNotMutateRepository()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string folderPath = @"C:\BatchDeleteFolder";
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            byte[] thumbnailData = [1, 2, 3];
            IOException expectedException = new("Failed to delete asset batch");

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);
            _assetRepository.AddFolder(folderPath);
            _assetRepository.AddAsset(asset, thumbnailData);
            assetsUpdatedEvents.Clear();

            _persistenceContextMock!
                .When(context => context.DeleteAssetsWithThumbnails(folder.Id, Arg.Any<IReadOnlyList<string>>()))
                .Do(_ => throw expectedException);

            IOException? exception = Assert.Throws<IOException>(() =>
                _assetRepository.DeleteAssets(folderPath, [asset.FileName]));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));
                Assert.That(_assetRepository.GetCataloguedAssets(), Has.Length.EqualTo(1));
                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).DeleteAssetsWithThumbnails(
                    folder.Id, Arg.Any<IReadOnlyList<string>>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAssets_FolderExistsWithoutAssets_ReturnsEmptyAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string folderPath = @"C:\EmptyBatchDeleteFolder";
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _assetRepository.AddFolder(folderPath);

            IReadOnlyList<Asset> deletedAssets = _assetRepository.DeleteAssets(folderPath, ["missing.jpg"]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deletedAssets, Is.Empty);
                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).DeleteAssetsWithThumbnails(
                    folder.Id,
                    Arg.Is<IReadOnlyList<string>>(fileNames => fileNames.Count == 1 && fileNames[0] == "missing.jpg"));

                _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAssets_OneFileDoesNotExist_ReturnsOnlyDeletedAssetAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string folderPath = @"C:\PartialBatchDeleteFolder";
            const string missingFileName = "missing.jpg";
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);
            _assetRepository.AddFolder(folderPath);
            _assetRepository.AddAsset(asset, [1, 2, 3]);
            assetsUpdatedEvents.Clear();

            IReadOnlyList<Asset> deletedAssets =
                _assetRepository.DeleteAssets(folderPath, [asset.FileName, missingFileName]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deletedAssets, Has.Count.EqualTo(1));
                Assert.That(deletedAssets[0].FileName, Is.EqualTo(asset.FileName));
                Assert.That(_assetRepository.GetCataloguedAssets(), Is.Empty);
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

                _persistenceContextMock!.Received(1).DeleteAssetsWithThumbnails(
                    folder.Id,
                    Arg.Is<IReadOnlyList<string>>(fileNames => fileNames.Count == 2
                                                               && fileNames[0] == asset.FileName
                                                               && fileNames[1] == missingFileName));

                _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_AssetHasImageData_DisposesImageData()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
        IImageData imageData = Substitute.For<IImageData>();
        Asset asset = CreateAssetWithImageData(folder, imageData);

        _folderPersistenceMock!.Insert(folderPath).Returns(folder);
        _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

        _assetRepository!.AddAsset(asset, [1, 2, 3]);

        Asset? deletedAsset = _assetRepository.DeleteAsset(folderPath, asset.FileName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(deletedAsset, Is.SameAs(asset));
            Assert.That(deletedAsset!.ImageData, Is.Null);
            imageData.Received(1).Dispose();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void DeleteAsset_DatabaseThrowsException_DoesNotMutateRepository()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            byte[] thumbnailData = [1, 2, 3];
            IOException expectedException = new("Failed to delete asset");

            _folderPersistenceMock!.Insert(folderPath).Returns(folder);
            _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);
            _assetRepository.AddFolder(folderPath);
            _assetRepository.AddAsset(asset, thumbnailData);
            assetsUpdatedEvents.Clear();

            _persistenceContextMock!.When(context => context.DeleteAssetWithThumbnail(folder.Id, asset.FileName))
                                    .Do(_ => throw expectedException);

            IOException? exception = Assert.Throws<IOException>(() =>
                _assetRepository.DeleteAsset(folderPath, asset.FileName));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));
                Assert.That(_assetRepository.GetCataloguedAssets(), Has.Length.EqualTo(1));
                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).DeleteAssetWithThumbnail(folder.Id, asset.FileName);

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddFolder_ConcurrentSamePath_ReturnsAllSameFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string folderPath = @"C:\ConcurrentTestPath";
            int callCount = 0;
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _folderPersistenceMock!
                .Insert(Arg.Any<string>())
                .Returns(_ =>
                {
                    Interlocked.Increment(ref callCount);
                    Thread.SpinWait(1000);
                    return folder;
                });

            Folder[] folders = new Folder[200];

            Parallel.For(0, 200, i => folders[i] = _assetRepository!.AddFolder(folderPath));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(folders.All(r => r.Id == folder.Id), Is.True);
                Assert.That(callCount, Is.EqualTo(1));

                _folderPersistenceMock!.Received(1).Insert(folderPath);

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_AssetsHaveImageData_DisposesImageData()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
        IImageData imageData = Substitute.For<IImageData>();
        Asset asset = CreateAssetWithImageData(folder, imageData);

        _folderPersistenceMock!.Insert(folderPath).Returns(folder);
        _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

        _assetRepository!.AddAsset(asset, [1, 2, 3]);

        _assetRepository.DeleteFolder(folder);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(asset.ImageData, Is.Null);
            imageData.Received(1).Dispose();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void DeleteFolder_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            IOException expectedException = new("Failed to access blob storage");

            _persistenceContextMock!.When(context => context.DeleteFolderWithAssetsAndThumbnails(folder.Id))
                                    .Do(_ => throw expectedException);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() => _assetRepository!.DeleteFolder(folder));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _persistenceContextMock!.Received(1).DeleteFolderWithAssetsAndThumbnails(folder.Id);
                _assetPersistenceMock!.DidNotReceive().DeleteByFolderId(Arg.Any<Guid>());
                _thumbnailPersistenceMock!.DidNotReceive().Delete(Arg.Any<Guid>(), Arg.Any<string>());
                _folderPersistenceMock!.DidNotReceive().Delete(Arg.Any<Guid>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_CallsLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Photos" };
        byte[] thumbnailData = [0xFF, 0xD8, 0xFF, 0xE0];

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId)
            .Returns(new Dictionary<string, byte[]> { { "photo.jpg", thumbnailData } });

        _imageProcessingServiceMock!.LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(SkiaImageData.Empty());

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_imageProcessingServiceMock!, _imageMetadataServiceMock!,
            userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            IImageData? result = assetRepository.LoadThumbnail(@"C:\Photos", "photo.jpg", 100, 100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);

                _imageProcessingServiceMock!.Received(1)
                    .LoadBitmapThumbnailImage(thumbnailData, ImageRotation.Rotate0, 100, 100);

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void LoadThumbnail_ThumbnailMissing_DoesNotCallLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Photos" };

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId).Returns([]);

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_imageProcessingServiceMock!, _imageMetadataServiceMock!,
            userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            IImageData? result = assetRepository.LoadThumbnail(@"C:\Photos", "missing.jpg", 100, 100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);

                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(
                        Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void LoadThumbnail_FolderNotFound_DoesNotCallLoadBitmapThumbnailImage()
    {
        IImageData? result = _assetRepository!.LoadThumbnail(@"C:\NonExistent", "photo.jpg", 100, 100);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);

            _imageProcessingServiceMock!.DidNotReceive()
                .LoadBitmapThumbnailImage(
                    Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());
            _thumbnailPersistenceMock!.DidNotReceive().GetByFolderId(Arg.Any<Guid>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void GetAssetsByPath_WithThumbnails_CallsLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Gallery" };
        byte[] thumbnailData = [0xFF, 0xD8, 0xFF, 0xE0];

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("sunset.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(200, 150, 80, 60)
            .WithFileProperties(5000, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("sunsetHash")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([asset]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId)
            .Returns(new Dictionary<string, byte[]> { { "sunset.jpg", thumbnailData } });

        _imageProcessingServiceMock!.LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(SkiaImageData.Empty());

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_imageProcessingServiceMock!, _imageMetadataServiceMock!,
            userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\Gallery");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Has.Length.EqualTo(1));
                Assert.That(assets[0].FileName, Is.EqualTo("sunset.jpg"));
                Assert.That(assets[0].ImageData, Is.Not.Null);

                _imageProcessingServiceMock!.Received(1)
                    .LoadBitmapThumbnailImage(thumbnailData, ImageRotation.Rotate0, 80, 60);

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetAssetsByPath_PartialThumbnails_ReturnsOnlyAssetsWithImageData()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);
        TestLogger<AssetRepository> testLogger = new();

        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\TestFolder" };
        byte[] validThumbnail = [0xFF, 0xD8, 0xFF, 0xE0];

        Asset asset1 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("img1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(100, 100, 50, 50)
            .WithFileProperties(1000, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("hash1")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        Asset asset2 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("img2.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(100, 100, 50, 50)
            .WithFileProperties(2000, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("hash2")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        IFolderPersistence folderPersistenceMock = Substitute.For<IFolderPersistence>();
        folderPersistenceMock.GetAll().Returns([folder]);

        IAssetPersistence assetPersistenceMock = Substitute.For<IAssetPersistence>();
        assetPersistenceMock.GetAll().Returns([asset1, asset2]);

        IThumbnailPersistence thumbnailPersistenceMock = Substitute.For<IThumbnailPersistence>();
        thumbnailPersistenceMock.GetByFolderId(folderId)
            .Returns(new Dictionary<string, byte[]> { { "img1.jpg", validThumbnail } });

        IRecentPathsPersistence recentPathsPersistenceMock = Substitute.For<IRecentPathsPersistence>();
        recentPathsPersistenceMock.GetAll().Returns([]);

        ISyncDefinitionsPersistence syncDefinitionsPersistenceMock = Substitute.For<ISyncDefinitionsPersistence>();
        syncDefinitionsPersistenceMock.GetAll().Returns(Array.Empty<SyncAssetsDirectoriesDefinition>());

        IPersistenceContext persistenceContextMock = Substitute.For<IPersistenceContext>();
        persistenceContextMock.Folders.Returns(folderPersistenceMock);
        persistenceContextMock.Assets.Returns(assetPersistenceMock);
        persistenceContextMock.Thumbnails.Returns(thumbnailPersistenceMock);
        persistenceContextMock.RecentPaths.Returns(recentPathsPersistenceMock);
        persistenceContextMock.SyncDefinitions.Returns(syncDefinitionsPersistenceMock);

        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        imageProcessingServiceMock.LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(SkiaImageData.Empty());

        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();

        AssetRepository assetRepository = new(imageProcessingServiceMock, imageMetadataServiceMock,
            userConfigurationService, persistenceContextMock, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\TestFolder");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Has.Length.EqualTo(1));
                Assert.That(assets[0].FileName, Is.EqualTo("img1.jpg"));
                Assert.That(assets[0].ImageData, Is.Not.Null);

                imageProcessingServiceMock.Received(1)
                    .LoadBitmapThumbnailImage(validThumbnail, ImageRotation.Rotate0, 50, 50);

                Assert.That(assetsUpdatedEvents, Is.Empty);

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetAssetsByPath_NoThumbnails_DoesNotCallLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Empty" };

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("orphan.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(200, 150, 80, 60)
            .WithFileProperties(3000, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("orphanHash")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([asset]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId).Returns([]);

        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_imageProcessingServiceMock!, _imageMetadataServiceMock!,
            userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\Empty");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Is.Empty);

                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(
                        Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

                testLogger.AssertLogExceptions([], typeof(AssetRepository));
            }
        }
        finally
        {
            assetRepository.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderNotFound_DoesNotCallLoadBitmapThumbnailImage()
    {
        Asset[] assets = _assetRepository!.GetAssetsByPath(@"C:\NonExistent");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(assets, Is.Empty);

            _imageProcessingServiceMock!.DidNotReceive()
                .LoadBitmapThumbnailImage(
                    Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());
            _thumbnailPersistenceMock!.DidNotReceive().GetByFolderId(Arg.Any<Guid>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void GetRecentTargetPaths_PersistedRecentPaths_ReturnsInitializedRecentPaths()
    {
        string[] persistedRecentTargetPaths =
        [
            Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER),
            Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_2)
        ];
        _assetRepository!.Dispose();
        _recentPathsPersistenceMock!.GetAll().Returns(persistedRecentTargetPaths);
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();

        _assetRepository = new(_imageProcessingServiceMock!, _imageMetadataServiceMock!, userConfigurationService,
            _persistenceContextMock!, _testLogger!);

        string[] recentTargetPaths = _assetRepository.GetRecentTargetPaths();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(recentTargetPaths, Is.EqualTo(persistedRecentTargetPaths));
            Assert.That(recentTargetPaths, Is.Not.SameAs(persistedRecentTargetPaths));
            _recentPathsPersistenceMock.Received(2).GetAll();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void AssetsUpdated_SubscribeAfterDispose_CompletesObserverAndReturnedSubscriptionCanBeDisposed()
    {
        IObserver<Reactive.Unit> observer = Substitute.For<IObserver<Reactive.Unit>>();

        _assetRepository!.Dispose();

        IDisposable subscription = _assetRepository.AssetsUpdated.Subscribe(observer);
        subscription.Dispose();

        using (Assert.EnterMultipleScope())
        {
            observer.Received(1).OnCompleted();
            observer.DidNotReceive().OnNext(Arg.Any<Reactive.Unit>());
            observer.DidNotReceive().OnError(Arg.Any<Exception>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    [Test]
    public void Dispose_AssetsHaveImageData_DisposesImageData()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
        IImageData imageData = Substitute.For<IImageData>();
        Asset asset = CreateAssetWithImageData(folder, imageData);

        _folderPersistenceMock!.Insert(folderPath).Returns(folder);
        _thumbnailPersistenceMock!.GetByFolderId(folder.Id).Returns([]);

        _assetRepository!.AddAsset(asset, [1, 2, 3]);

        _assetRepository.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(asset.ImageData, Is.Null);
            imageData.Received(1).Dispose();

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }

    private Asset CreateAssetWithImageData(Folder folder, IImageData imageData)
    {
        Asset asset = _asset1!.WithFolder(folder);
        asset.ImageData = imageData;
        return asset;
    }
}
