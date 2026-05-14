using PhotoManager.Persistence;
using PhotoManager.Persistence.Repositories;
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
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

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
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);
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

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);

        _assetRepository = new(_pathProviderServiceMock!, _imageProcessingServiceMock,
            _imageMetadataServiceMock, userConfigurationService, _persistenceContextMock, _testLogger);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" },
            FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_1_JPG,
                    Height = PixelHeightAsset.IMAGE_1_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                    Height = ThumbnailHeightAsset.IMAGE_1_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void AddAsset_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
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
                _assetPersistenceMock!.DidNotReceive().Upsert(Arg.Any<Asset>());
                _thumbnailPersistenceMock!.DidNotReceive().Upsert(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>());
                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            IOException expectedException = new("Failed to access blob storage");

            _assetPersistenceMock!.DeleteByFolderId(Arg.Any<Guid>()).Throws(expectedException);

            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() => _assetRepository!.DeleteFolder(folder));

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                Assert.That(assetsUpdatedEvents, Is.Empty);

                _assetPersistenceMock!.Received(1).DeleteByFolderId(folder.Id);
                _thumbnailPersistenceMock!.DidNotReceive().DeleteByFolderId(Arg.Any<Guid>());
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
    public void AddAsset_DisposedRepository_ThrowsObjectDisposedException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            ObjectDisposedException expectedException = new(nameof(AssetRepository));

            _assetRepository!.Dispose();

            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            Asset asset = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            ObjectDisposedException? exception = Assert.Throws<ObjectDisposedException>(() =>
                _assetRepository!.AddAsset(asset, assetData));

            Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _folderPersistenceMock!.DidNotReceive().Insert(Arg.Any<string>());
            _assetPersistenceMock!.DidNotReceive().Upsert(Arg.Any<Asset>());
            _thumbnailPersistenceMock!.DidNotReceive().Upsert(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
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

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        try
        {
            using (Assert.EnterMultipleScope())
            {
                IOException? exception = Assert.Throws<IOException>(() =>
                    new AssetRepository(_pathProviderServiceMock!, imageProcessingServiceMock, imageMetadataServiceMock,
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
    public void GetAssetsByPath_PartialThumbnails_ReturnsOnlyAssetsWithImageData()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);
        TestLogger<AssetRepository> testLogger = new();

        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\TestFolder" };
        byte[] validThumbnail = [0xFF, 0xD8, 0xFF, 0xE0];

        Asset asset1 = new()
        {
            Folder = folder,
            FolderId = folderId,
            FileName = "img1.jpg",
            ImageRotation = Rotation.Rotate0,
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
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "hash1",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        Asset asset2 = new()
        {
            Folder = folder,
            FolderId = folderId,
            FileName = "img2.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 100, Height = 100 },
                Thumbnail = new() { Width = 50, Height = 50 }
            },
            FileProperties = new()
            {
                Size = 2000,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "hash2",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

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
        imageProcessingServiceMock.LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                                  .Returns(new BitmapImage());

        IImageMetadataService imageMetadataServiceMock = Substitute.For<IImageMetadataService>();

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);

        AssetRepository assetRepository = new(_pathProviderServiceMock!, imageProcessingServiceMock,
            imageMetadataServiceMock, userConfigurationService, persistenceContextMock, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\TestFolder");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Has.Length.EqualTo(1));
                Assert.That(assets[0].FileName, Is.EqualTo("img1.jpg"));
                Assert.That(assets[0].ImageData, Is.Not.Null);

                imageProcessingServiceMock.Received(1).LoadBitmapThumbnailImage(validThumbnail, 50, 50);

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
    public void LoadThumbnail_ThumbnailExists_CallsLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Photos" };
        byte[] thumbnailData = [0xFF, 0xD8, 0xFF, 0xE0];

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId)
            .Returns(new Dictionary<string, byte[]> { { "photo.jpg", thumbnailData } });

        _imageProcessingServiceMock!.LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                                        .Returns(new BitmapImage());

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_pathProviderServiceMock!, _imageProcessingServiceMock!,
            _imageMetadataServiceMock!, userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            BitmapImage? result = assetRepository.LoadThumbnail(@"C:\Photos", "photo.jpg", 100, 100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);

                _imageProcessingServiceMock!.Received(1).LoadBitmapThumbnailImage(thumbnailData, 100, 100);

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

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_pathProviderServiceMock!, _imageProcessingServiceMock!,
            _imageMetadataServiceMock!, userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            BitmapImage? result = assetRepository.LoadThumbnail(@"C:\Photos", "missing.jpg", 100, 100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);

                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

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
        BitmapImage? result = _assetRepository!.LoadThumbnail(@"C:\NonExistent", "photo.jpg", 100, 100);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);

            _imageProcessingServiceMock!.DidNotReceive()
                .LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
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

        Asset asset = new()
        {
            Folder = folder,
            FolderId = folderId,
            FileName = "sunset.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 200, Height = 150 },
                Thumbnail = new() { Width = 80, Height = 60 }
            },
            FileProperties = new()
            {
                Size = 5000,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "sunsetHash",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([asset]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId)
            .Returns(new Dictionary<string, byte[]> { { "sunset.jpg", thumbnailData } });

        _imageProcessingServiceMock!.LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                                        .Returns(new BitmapImage());

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_pathProviderServiceMock!, _imageProcessingServiceMock!,
            _imageMetadataServiceMock!, userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\Gallery");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Has.Length.EqualTo(1));
                Assert.That(assets[0].FileName, Is.EqualTo("sunset.jpg"));
                Assert.That(assets[0].ImageData, Is.Not.Null);

                _imageProcessingServiceMock!.Received(1).LoadBitmapThumbnailImage(thumbnailData, 80, 60);

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
    public void GetAssetsByPath_NoThumbnails_DoesNotCallLoadBitmapThumbnailImage()
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = @"C:\Empty" };

        Asset asset = new()
        {
            Folder = folder,
            FolderId = folderId,
            FileName = "orphan.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 200, Height = 150 },
                Thumbnail = new() { Width = 80, Height = 60 }
            },
            FileProperties = new()
            {
                Size = 3000,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "orphanHash",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        _folderPersistenceMock!.GetAll().Returns([folder]);
        _assetPersistenceMock!.GetAll().Returns([asset]);

        _thumbnailPersistenceMock!.GetByFolderId(folderId).Returns([]);

        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        TestLogger<AssetRepository> testLogger = new();

        AssetRepository assetRepository = new(_pathProviderServiceMock!, _imageProcessingServiceMock!,
            _imageMetadataServiceMock!, userConfigurationService, _persistenceContextMock!, testLogger);

        try
        {
            Asset[] assets = assetRepository.GetAssetsByPath(@"C:\Empty");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assets, Is.Empty);

                _imageProcessingServiceMock!.DidNotReceive()
                    .LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

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
                .LoadBitmapThumbnailImage(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
            _thumbnailPersistenceMock!.DidNotReceive().GetByFolderId(Arg.Any<Guid>());

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
    }
}
