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

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteAssetTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;

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
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, _testLogger);

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
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
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("68493435-e299-4bb5-9e02-214da41d0256"))
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithRotation(ImageRotation.Rotate90)
            .WithPixels(PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithCorrupted(false, null)
            .WithRotated(true, "The asset has been rotated")
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
    public void
        DeleteAssets_FolderAndAssetsExist_ReturnsDeletedAssetsAndAssetsAreDeletedAndAssetsUpdatedIsUpdatedOnce()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
        Folder addedFolder = _assetRepository!.AddFolder(folderPath);
        _asset1 = _asset1!.WithFolder(addedFolder);
        _asset2 = _asset2!.WithFolder(addedFolder);
        _assetRepository!.AddAsset(_asset1, [1, 2, 3]);
        _assetRepository!.AddAsset(_asset2, [4, 5, 6]);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            IReadOnlyList<Asset> deletedAssets =
                _assetRepository.DeleteAssets(folderPath, [_asset1.FileName, _asset2.FileName]);

            Asset[] assets = _assetRepository.GetCataloguedAssets();
            string[] expectedFileNames = [_asset1.FileName, _asset2.FileName];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deletedAssets, Has.Count.EqualTo(2));
                Assert.That(deletedAssets.Select(a => a.FileName), Is.EqualTo(expectedFileNames));
                Assert.That(assets, Is.Empty);
                Assert.That(_assetRepository.IsAssetCatalogued(folderPath, _asset1.FileName), Is.False);
                Assert.That(_assetRepository.IsAssetCatalogued(folderPath, _asset2.FileName), Is.False);
                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            }

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAssets_EmptyList_ReturnsEmptyAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            IReadOnlyList<Asset> deletedAssets =
                _assetRepository.DeleteAssets("non_existent_path", []);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deletedAssets, Is.Empty);
                Assert.That(assetsUpdatedEvents, Is.Empty);
            }

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAssets_DirectoryDoesNotExist_ReturnsEmptyAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            IReadOnlyList<Asset> deletedAssets =
                _assetRepository.DeleteAssets("non_existent_path", [FileNames.NON_EXISTENT_FILE_JPG]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deletedAssets, Is.Empty);
                Assert.That(assetsUpdatedEvents, Is.Empty);
            }

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void
        DeleteAsset_FolderAndAssetExist_ReturnsDeletedAssetAndThumbnailsAndAssetAreDeletedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Asset[] assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Asset? assetDeleted1 = _assetRepository!.DeleteAsset(folderPath1, _asset1.FileName);
            Asset? assetDeleted2 = _assetRepository!.DeleteAsset(folderPath2, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted1, Is.Not.Null);
            Assert.That(assetDeleted1!.FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetDeleted1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assetDeleted1.FullPath, Is.EqualTo(_asset1!.FullPath));

            Assert.That(assetDeleted2, Is.Null);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void
        DeleteAsset_AssetDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        AssetRepository assetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            assetRepository.AddFolder(folderPath);

            Asset[] assets = assetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Asset? assetDeleted = assetRepository.DeleteAsset(folderPath, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted, Is.Null);

            assets = assetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetRepository.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_DirectoryDoesNotExist_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Asset[] assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset? assetDeleted = _assetRepository!.DeleteAsset("non_existent_path", _asset1.FileName);

            Assert.That(assetDeleted, Is.Null);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Not.Empty);
            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_DirectoryIsNull_LogsItAndThrowsArgumentNullExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Value cannot be null. (Parameter 'key')";

            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string? folderPath2 = null;

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Asset[] assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _assetRepository!.DeleteAsset(folderPath2!, _asset1.FileName));

            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Not.Empty);
            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([new Exception(exceptionMessage)], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FileNameIsNull_LogsItAndThrowsInvalidOperationExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string exceptionMessage = "Must add values for the following parameters: $fileName";

            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string? assetFileName = null;

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
                Asset[] assets = _assetRepository!.GetCataloguedAssets();
                Assert.That(assets, Has.Length.EqualTo(1));
                Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

                InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
                    _assetRepository!.DeleteAsset(folderPath1, assetFileName!));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                assets = _assetRepository!.GetCataloguedAssets();
                Assert.That(assets, Is.Not.Empty);
                Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

                Exception expectedException = new(exceptionMessage);
                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FolderAndAssetDoNotExist_ReturnsNullAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);

            Asset[] assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Asset? assetDeleted = _assetRepository!.DeleteAsset(folderPath1, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted, Is.Null);


            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_ConcurrentAccess_ReturnsDeletedAssetsAndAssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            _assetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _assetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            Asset[] assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Asset? assetDeleted1 = null;
            Asset? assetDeleted2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () =>
                {
                    assetDeleted1 = _assetRepository!.DeleteAsset(folderPath1, _asset1.FileName);
                },
                () =>
                {
                    assetDeleted2 = _assetRepository!.DeleteAsset(folderPath2, FileNames.NON_EXISTENT_FILE_JPG);
                }
            );

            Assert.That(assetDeleted1, Is.Not.Null);
            Assert.That(assetDeleted1!.FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetDeleted1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assetDeleted1.FullPath, Is.EqualTo(_asset1!.FullPath));

            Assert.That(assetDeleted2, Is.Null);

            assets = _assetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(_assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
