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
public class AssetRepositoryAddAssetTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
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
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
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
        _asset2 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            FileName = FileNames.IMAGE_9_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [Test]
    public void AddAsset_FolderAndThumbnailsExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(_asset2!, assetData2);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails.ContainsKey(_asset2!.Folder.Path), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

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
    public void AddAsset_FolderDoesNotExist_AssetIsAddedAndFolderIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(_asset2!, assetData2);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails.ContainsKey(_asset2!.Folder.Path), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(folderPath);
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Path, Is.EqualTo(folderPath));
            Assert.That(folder.Name, Is.EqualTo("NewFolder"));

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
    public void AddAsset_ThumbnailDoesNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

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
    public void AddAsset_ThumbnailDoesNotExistButBinExists_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData1);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

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
    public void AddAsset_ThumbnailsDictionaryEntriesToKeepIs0_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
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
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = testableAssetRepository.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(testableAssetRepository.HasChanges(), Is.True); // Due to AddFolder()
            Assert.That(assets, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderAndThumbnailsDoNotExist_AssetIsAddedAndFolderIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            Folder? folderFromRepository = _testableAssetRepository!.GetFolderByPath(folderPath);
            Assert.That(folderFromRepository, Is.Not.Null);
            Assert.That(folderFromRepository.Path, Is.EqualTo(folderPath));
            Assert.That(folderFromRepository.Name, Is.EqualTo("NewFolder"));

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
    public void AddAsset_AssetFolderIsDefault_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);
            Assert.That(assets, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetIsNull_ThrowsNullReferenceExceptionAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Asset? asset = null;
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _testableAssetRepository.AddAsset(asset!, assetData));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
            Assert.That(_testableAssetRepository.HasChanges(), Is.False);
            Assert.That(assets, Is.Empty);
            Assert.That(thumbnails, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailDataIsNull_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[]? assetData = null;

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.Null);

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
    public void AddAsset_ConcurrentAccess_AssetsAreHandledSafelyAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository.AddAsset(_asset1!, assetData1),
                () => _testableAssetRepository.AddAsset(_asset2!, assetData2)
            );

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assets, Has.Count.EqualTo(2));
            Assert.That(assets.Any(x => x.FileName == _asset1.FileName), Is.True);
            Assert.That(assets.Any(x => x.FileName == _asset2.FileName), Is.True);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails.ContainsKey(_asset2!.Folder.Path), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

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
