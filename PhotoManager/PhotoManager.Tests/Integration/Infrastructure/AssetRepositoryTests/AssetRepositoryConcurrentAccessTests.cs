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
public class AssetRepositoryConcurrentAccessTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

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
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("f91b8c81-6938-431a-a689-d86c7c4db126"))
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(true, "The asset is corrupted")
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
    public void AssetRepository_ConcurrentAccess_WorksCorrectly()
    {
        Asset asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("cf9293ad-d835-4126-8a76-17c7bbfe626e"))
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileProperties(FileSize.IMAGE_2_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_2_JPG)
            .WithCorrupted(true, "The asset is corrupted")
            .WithRotated(true, "The asset has been rotated")
            .Build();
        Asset asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("1e3c2274-bc11-482a-a7b4-e86e5aeb17f1"))
            .WithFileName(FileNames.IMAGE_3_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG,
                ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG)
            .WithFileProperties(FileSize.IMAGE_3_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_3_JPG)
            .WithCorrupted(true, "The asset is corrupted")
            .WithRotated(true, "The asset has been rotated")
            .Build();

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            string folderPath3 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_3);
            string folderPath4 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_4);
            Folder folder1 = _assetRepository!.AddFolder(folderPath1);
            Folder folder2 = _assetRepository!.AddFolder(folderPath2);

            Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1);
            string filePath1 = Path.Combine(folderPath1, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder1);
            string filePath2 = Path.Combine(folderPath2, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder2);
            string filePath3 = Path.Combine(folderPath2, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            string[] recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            Asset[] cataloguedAssets = _assetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            // DeleteAsset
            string folderPath5 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_5);
            string folderPath6 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_6);
            Folder folder5 = _assetRepository!.AddFolder(folderPath5);
            Folder folder6 = _assetRepository!.AddFolder(folderPath6);

            asset4 = asset4.WithFolder(folder5);
            byte[] assetData4 = [1, 2, 3];

            asset5 = asset5.WithFolder(folder6);
            byte[] assetData5 = [];

            _assetRepository.AddAsset(asset4, assetData4);
            _assetRepository.AddAsset(asset5, assetData5);
            // DeleteAsset

            bool folderExists1 = false;
            bool folderExists2 = false;

            Folder? folderByPath1 = null;
            Folder? folderByPath2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository.GetAssetsByPath(folderPath1),
                () => _assetRepository.DeleteFolder(folder4),
                () => _assetRepository.GetCataloguedAssets(),
                () => _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _assetRepository.GetSubFolders(folder1),
                () => _assetRepository.BackupExists(),
                () => _assetRepository.GetSyncAssetsConfiguration(),
                () => _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository.DeleteAsset(folderPath5, asset4.FileName),
                () => _assetRepository.GetFoldersPath(),
                () => _assetRepository.GetAssetsCounter(),
                () => _assetRepository.AddAsset(_asset1, assetData1),
                () => _assetRepository.GetSyncAssetsConfiguration(),
                _assetRepository.WriteBackup,
                () => _assetRepository.GetCataloguedAssetsByPath(folderPath1),
                () => _assetRepository.GetFolders(),
                () => _assetRepository.DeleteFolder(folder3),
                () => _assetRepository.DeleteAsset(folderPath6, asset5.FileName),
                () => _assetRepository.GetRecentTargetPaths(),
                () => _assetRepository.GetFoldersPath(),
                () => _assetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository.AddFolder(folderPath3),
                () => _assetRepository.AddAsset(_asset2, assetData2),
                () => _assetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName),
                () => folderExists1 = _assetRepository.FolderExists(folderPath1),
                () => _assetRepository.GetFolders(),
                () => folderByPath1 = _assetRepository.GetFolderByPath(folderPath1),
                () => _assetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _assetRepository.GetCataloguedAssets(),
                _assetRepository.WriteBackup,
                () => _assetRepository.AddAsset(_asset3, assetData3),
                () => _assetRepository.GetSubFolders(folder2),
                () => _assetRepository.AddFolder(folderPath4),
                () => _assetRepository.GetRecentTargetPaths(),
                () => folderByPath2 = _assetRepository.GetFolderByPath(folderPath2),
                () => _assetRepository.IsAssetCatalogued(folderPath2, _asset2.FileName),
                () => _assetRepository.GetCataloguedAssetsByPath(folderPath2),
                () => _assetRepository.BackupExists(),
                () => folderExists2 = _assetRepository.FolderExists(folderPath2),
                _assetRepository.WriteBackup,
                () => _assetRepository.GetAssetsCounter(),
                () => _assetRepository.GetAssetsByPath(folderPath2)
            );

            using (Assert.EnterMultipleScope())
            {
                cataloguedAssets = _assetRepository!.GetCataloguedAssets();
                Assert.That(cataloguedAssets, Has.Length.EqualTo(3));
                Assert.That(cataloguedAssets.Any(x => x.FileName == _asset1.FileName), Is.True);
                Assert.That(cataloguedAssets.Any(x => x.FileName == _asset2.FileName), Is.True);
                Assert.That(cataloguedAssets.Any(x => x.FileName == _asset3.FileName), Is.True);

                Folder[] folders = _assetRepository!.GetFolders();
                Assert.That(folders, Has.Length.GreaterThanOrEqualTo(4));
                Assert.That(folders.Any(x => x.Path == folderPath1), Is.True);
                Assert.That(folders.Any(x => x.Path == folderPath2), Is.True);
                Assert.That(folders.Any(x => x.Path == folderPath5), Is.True);
                Assert.That(folders.Any(x => x.Path == folderPath6), Is.True);

                Assert.That(folderExists1, Is.True);
                Assert.That(folderExists2, Is.True);

                Assert.That(folderByPath1, Is.Not.Null);
                Assert.That(folderByPath2, Is.Not.Null);

                Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(7));
                Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[4], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[5], Is.EqualTo(Reactive.Unit.Default));
                Assert.That(assetsUpdatedEvents[6], Is.EqualTo(Reactive.Unit.Default));
            }

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
