﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddAssetTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 363888,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
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
            FileName = "Image 9.png",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 6000, Height = 6120 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4602393,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
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
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(_asset2!, assetData2);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);
            Assert.AreEqual(_asset2!.FileName, assets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(_asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][_asset2.FileName]);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderDoesNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData1); // Just to make the thumbnail exists for this path
            _testableAssetRepository.AddAsset(_asset2!, assetData2);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);
            Assert.AreEqual(_asset2!.FileName, assets[1].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(_asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][_asset2.FileName]);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, [1, 2, 3]},
                { _asset2!.FileName, [] }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][_asset2.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_ThumbnailsDictionaryEntriesToKeepIs0_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);
        TestableAssetRepository testableAssetRepository = new (_database!, _storageServiceMock!.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = testableAssetRepository.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.IsTrue(testableAssetRepository.HasChanges()); // Due to AddFolder()
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_FolderAndThumbnailsDoNotExist_AssetIsAddedAndAssetsUpdatedIsUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };
            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void AddAsset_AssetFolderIsNull_AssetIsNotAddedAndAssetsUpdatedIsNotUpdated()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData = [1, 2, 3];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.IsFalse(_testableAssetRepository.HasChanges());
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _testableAssetRepository.AddAsset(asset!, assetData));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
            Assert.IsFalse(_testableAssetRepository.HasChanges());
            Assert.IsEmpty(assets);
            Assert.IsEmpty(thumbnails);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);
            _asset1 = _asset1!.WithFolder(folder);
            byte[]? assetData = null;

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            _testableAssetRepository.AddAsset(_asset1!, assetData!);

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(1, assets.Count);
            Assert.AreEqual(_asset1.FileName, assets[0].FileName);

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.AreEqual(1, thumbnails[_asset1!.Folder.Path].Count);
            Assert.IsTrue(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName));
            Assert.AreEqual(null, thumbnails[_asset1!.Folder.Path][_asset1.FileName]);

            Assert.AreEqual(1, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
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
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath);
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder2);
            byte[] assetData2 = [];

            List<Asset> assets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assets);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository.AddAsset(_asset1!, assetData1),
                () => _testableAssetRepository.AddAsset(_asset2!, assetData2)
            );

            Assert.IsTrue(_testableAssetRepository.HasChanges());

            Assert.AreEqual(2, assets.Count);
            Assert.IsTrue(assets.Any(x => x.FileName == _asset1.FileName));
            Assert.IsTrue(assets.Any(x => x.FileName == _asset2.FileName));

            Assert.AreEqual(1, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(_asset1!.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(_asset2!.Folder.Path));
            Assert.AreEqual(2, thumbnails[folderPath].Count);
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset1.FileName));
            Assert.IsTrue(thumbnails[folderPath].ContainsKey(_asset2.FileName));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, thumbnails[folderPath][_asset1.FileName]);
            Assert.AreEqual(Array.Empty<byte>(), thumbnails[folderPath][_asset2.FileName]);

            Assert.AreEqual(2, assetsUpdatedEvents.Count);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[0]);
            Assert.AreEqual(Reactive.Unit.Default, assetsUpdatedEvents[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
