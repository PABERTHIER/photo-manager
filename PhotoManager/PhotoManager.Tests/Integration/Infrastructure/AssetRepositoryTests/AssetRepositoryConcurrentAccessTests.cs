using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryConcurrentAccessTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, userConfigurationService);

        _asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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
            FolderId = new Guid("68493435-e299-4bb5-9e02-214da41d0256"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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
        _asset3 = new()
        {
            FolderId = new Guid("f91b8c81-6938-431a-a689-d86c7c4db126"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11.heic",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2247285,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [Test]
    public void AssetRepository_ConcurrentAccess_WorksCorrectly()
    {
        Asset asset4 = new()
        {
            FolderId = new Guid("cf9293ad-d835-4126-8a76-17c7bbfe626e"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 2.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2247285,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "b92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        Asset asset5 = new()
        {
            FolderId = new Guid("1e3c2274-bc11-482a-a7b4-e86e5aeb17f1"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 3.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 2247285,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "c92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            string folderPath3 = Path.Combine(_dataDirectory!, "NewFolder3");
            string folderPath4 = Path.Combine(_dataDirectory!, "NewFolder4");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            _asset3 = _asset3!.WithFolder(folder2);
            byte[] assetData3 = [4, 5, 6];

            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            // DeleteAsset
            string folderPath5 = Path.Combine(_dataDirectory!, "NewFolder5");
            string folderPath6 = Path.Combine(_dataDirectory!, "NewFolder6");
            Folder folder5 = _testableAssetRepository!.AddFolder(folderPath5);
            Folder folder6 = _testableAssetRepository!.AddFolder(folderPath6);

            asset4 = asset4.WithFolder(folder5);
            byte[] assetData4 = [1, 2, 3];

            asset5 = asset5.WithFolder(folder6);
            byte[] assetData5 = [];

            _testableAssetRepository.AddAsset(asset4, assetData4);
            _testableAssetRepository.AddAsset(asset5, assetData5);
            // DeleteAsset

            bool folderExists1 = false;
            bool folderExists2 = false;

            Folder? folderByPath1 = null; 
            Folder? folderByPath2 = null;

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository.GetAssetsByPath(folderPath1),
                () => _testableAssetRepository.SaveCatalog(folder1),
                () => _testableAssetRepository.DeleteFolder(folder4),
                () => _testableAssetRepository.HasChanges(),
                () => _testableAssetRepository.ContainsThumbnail(folderPath2, _asset2.FileName),
                () => _testableAssetRepository.GetCataloguedAssets(),
                () => _testableAssetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _testableAssetRepository.GetSubFolders(folder1),
                () => _testableAssetRepository.BackupExists(),
                () => _testableAssetRepository.GetSyncAssetsConfiguration(),
                () => _testableAssetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _testableAssetRepository.DeleteAsset(folderPath5, asset4.FileName),
                () => _testableAssetRepository.LoadThumbnail(folderPath1, _asset2.FileName, 150, 150),
                () => _testableAssetRepository.GetAssetsCounter(),
                () => _testableAssetRepository.HasChanges(),
                () => _testableAssetRepository.AddAsset(_asset1, assetData1),
                () => _testableAssetRepository.SaveCatalog(folder1),
                () => _testableAssetRepository.GetSyncAssetsConfiguration(),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.ContainsThumbnail(folderPath1, _asset1.FileName),
                () => _testableAssetRepository.GetCataloguedAssetsByPath(folderPath1),
                () => _testableAssetRepository.GetFolders(),
                () => _testableAssetRepository.DeleteFolder(folder3),
                () => _testableAssetRepository.DeleteAsset(folderPath6, asset5.FileName),
                () => _testableAssetRepository.GetRecentTargetPaths(),
                () => _testableAssetRepository.LoadThumbnail(folderPath2, _asset3.FileName, 150, 150),
                () => _testableAssetRepository.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _testableAssetRepository.AddFolder(folderPath3),
                () => _testableAssetRepository.FolderHasThumbnails(folder2),
                () => _testableAssetRepository.SaveCatalog(null),
                () => _testableAssetRepository.AddAsset(_asset2, assetData2),
                () => _testableAssetRepository.IsAssetCatalogued(folderPath1, _asset1.FileName),
                () => folderExists1 = _testableAssetRepository.FolderExists(folderPath1),
                () => _testableAssetRepository.GetFolders(),
                () => folderByPath1 = _testableAssetRepository.GetFolderByPath(folderPath1),
                () => _testableAssetRepository.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _testableAssetRepository.GetCataloguedAssets(),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.AddAsset(_asset3, assetData3),
                () => _testableAssetRepository.GetSubFolders(folder2),
                () => _testableAssetRepository.AddFolder(folderPath4),
                () => _testableAssetRepository.GetRecentTargetPaths(),
                () => folderByPath2 = _testableAssetRepository.GetFolderByPath(folderPath2),
                () => _testableAssetRepository.IsAssetCatalogued(folderPath2, _asset2.FileName),
                () => _testableAssetRepository.FolderHasThumbnails(folder1),
                () => _testableAssetRepository.GetCataloguedAssetsByPath(folderPath2),
                () => _testableAssetRepository.BackupExists(),
                () => folderExists2 = _testableAssetRepository.FolderExists(folderPath2),
                _testableAssetRepository.WriteBackup,
                () => _testableAssetRepository.GetAssetsCounter(),
                () => _testableAssetRepository.GetAssetsByPath(folderPath2)
            );

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets.Any(x => x.FileName == _asset1.FileName), Is.True);
            Assert.That(cataloguedAssets.Any(x => x.FileName == _asset2.FileName), Is.True);
            Assert.That(cataloguedAssets.Any(x => x.FileName == _asset3.FileName), Is.True);

            Assert.That(thumbnails, Has.Count.EqualTo(4));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath5), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath6), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath5], Is.Empty);
            Assert.That(thumbnails[folderPath6], Is.Empty);

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath1][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData3));

            Folder[] folders = _testableAssetRepository!.GetFolders();
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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
