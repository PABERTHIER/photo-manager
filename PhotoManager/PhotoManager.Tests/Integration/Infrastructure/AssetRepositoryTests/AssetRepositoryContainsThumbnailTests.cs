using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryContainsThumbnailTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _testableAssetRepository = new(_database, _storageServiceMock!.Object, userConfigurationService);

        asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    [Test]
    public void ContainsThumbnail_ThumbnailsExistInRepository_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ConcurrentAccess_ThumbnailsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            bool isContainingThumbnail1 = false;
            bool isContainingThumbnail2 = false;
            bool isContainingThumbnail3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => isContainingThumbnail1 = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName),
                () => isContainingThumbnail2 = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName),
                () => isContainingThumbnail3 = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName)
            );

            Assert.IsTrue(isContainingThumbnail1);
            Assert.IsTrue(isContainingThumbnail2);
            Assert.IsTrue(isContainingThumbnail3);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExistButAdded_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = new()
            {
                Path = folderPath,
                FolderId = Guid.NewGuid(),
            };

            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsTrue(isContainingThumbnail);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExist_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = new()
            {
                Path = folderPath,
                FolderId = Guid.NewGuid(),
            };

            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsEmpty(thumbnails[asset1!.Folder.Path]);
            Assert.IsFalse(isContainingThumbnail);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_FolderDoesNotExistAndThumbnailsDictionaryEntriesToKeepIs0_ReturnsFalse()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, _storageServiceMock!.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = new()
            {
                Path = folderPath,
                FolderId = Guid.NewGuid(),
            };

            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            testableAssetRepository.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(testableAssetRepository.HasChanges()); // Set by AddFolder method, called by AddAsset method

            Assert.IsEmpty(assetsUpdatedEvents);

            bool isContainingThumbnail = testableAssetRepository.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.IsFalse(isContainingThumbnail);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ThumbnailsDoNotExistInRepository_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1!.Folder.Path));
            Assert.IsEmpty(thumbnails[asset1!.Folder.Path]);
            Assert.IsFalse(isContainingThumbnail);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_ThumbnailsDoNotExistInRepositoryButBinExists_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { asset1!.FileName, new byte[] { 1, 2, 3 } },
                { "Image2.png", new byte[] { 4, 5, 6 } }
            };
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);

            _database!.WriteBlob(blobToWrite, asset1!.Folder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);
            Assert.IsFalse(thumbnails.ContainsKey(asset1!.Folder.Path));

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, asset1!.FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset1.Folder.Path));
            Assert.AreEqual(2, thumbnails[asset1.Folder.Path].Count);
            Assert.IsTrue(thumbnails[asset1.Folder.Path].ContainsKey(asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[asset1.Folder.Path][asset1.FileName]);

            Assert.IsTrue(isContainingThumbnail);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_DifferentFileName_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            string fileName = "toto";
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, fileName);

            Assert.IsFalse(isContainingThumbnail);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_NullFileName_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            string? fileName = null;
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(asset1!.Folder.Path, fileName!);

            Assert.IsFalse(isContainingThumbnail);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void ContainsThumbnail_NullDirectoryName_ThrowsArgumentNullException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");
            string? directoryName = null;
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            asset1!.Folder = folder;
            asset1!.FolderId = folder.FolderId;
            _testableAssetRepository!.AddAsset(asset1!, Array.Empty<byte>());

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _testableAssetRepository!.ContainsThumbnail(directoryName!, asset1!.FileName));

            Assert.AreEqual("Value cannot be null. (Parameter 'key')", exception?.Message);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
