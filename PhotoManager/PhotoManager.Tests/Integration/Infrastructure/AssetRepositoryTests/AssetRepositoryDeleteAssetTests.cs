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
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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

    [Test]
    public void DeleteAsset_FolderAndAssetExist_ReturnsDeletedAssetAndThumbnailsAndAssetAreDeletedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.False);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            Asset? assetDeleted1 = _testableAssetRepository!.DeleteAsset(folderPath1, _asset1.FileName);
            Asset? assetDeleted2 = _testableAssetRepository!.DeleteAsset(folderPath2, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted1, Is.Not.Null);
            Assert.That(assetDeleted1!.FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetDeleted1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assetDeleted1.FullPath, Is.EqualTo(_asset1!.FullPath));

            Assert.That(assetDeleted2, Is.Null);

            Assert.That(thumbnails, Is.Empty);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_AssetDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object,
            imageProcessingService, imageMetadataService, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            testableAssetRepository.AddFolder(folderPath);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset? assetDeleted = testableAssetRepository.DeleteAsset(folderPath, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted, Is.Null);

            Assert.That(thumbnails, Is.Empty);

            assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_DirectoryDoesNotExist_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset("non_existent_path", _asset1.FileName);

            Assert.That(assetDeleted, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Not.Empty);
            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_ThumbnailsDoNotExistInRepositoryButBinExists_ReturnsDeletedAssetAndThumbnailsAndAssetAreDeletedAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);
            const string fileName = FileNames.NON_EXISTENT_IMAGE_PNG;
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { fileName, [4, 5, 6]}
            };

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath, _asset1!.FileName);

            Assert.That(assetDeleted, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(fileName), Is.True);
            Assert.That(thumbnails[folderPath][fileName], Is.EqualTo(blobToWrite[fileName]));

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_DirectoryIsNull_ReturnsNullAndDoesNothingAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string? folderPath2 = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath2!, _asset1.FileName);

            Assert.That(assetDeleted, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Not.Empty);
            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FileNameIsNull_ThrowsArgumentNullExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string? assetFileName = null;

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.DeleteAsset(folderPath1, assetFileName!));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Not.Empty);
            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_FolderAndAssetDoNotExist_ReturnsNullAndHasChangesIsFalseAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Asset? assetDeleted = _testableAssetRepository!.DeleteAsset(folderPath1, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(assetDeleted, Is.Null);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteAsset_ConcurrentAccess_ReturnsDeletedAssetsAndAssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            Folder addedFolder1 = _testableAssetRepository!.AddFolder(folderPath1);
            _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(addedFolder1);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.True);
            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets.FirstOrDefault()?.FileName, Is.EqualTo(_asset1.FileName));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.False);
            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData));

            Asset? assetDeleted1 = null;
            Asset? assetDeleted2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () =>
                { assetDeleted1 = _testableAssetRepository!.DeleteAsset(folderPath1, _asset1.FileName); },
                () =>
                { assetDeleted2 = _testableAssetRepository!.DeleteAsset(folderPath2, FileNames.NON_EXISTENT_FILE_JPG); }
            );

            Assert.That(assetDeleted1, Is.Not.Null);
            Assert.That(assetDeleted1!.FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetDeleted1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assetDeleted1.FullPath, Is.EqualTo(_asset1!.FullPath));

            Assert.That(assetDeleted2, Is.Null);

            Assert.That(thumbnails, Is.Empty);

            assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);
            Assert.That(_testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
