namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceThumbnailTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private FindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();
        _configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");
        _configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new (userConfigurationService);
        _findDuplicatedAssetsService = new (_assetRepository!, _storageService!, userConfigurationService);

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
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
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
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
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
        _asset4 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9_duplicate.png",
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
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
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
    public void GetDuplicatesBetweenOriginalAndThumbnail_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        try
        {
            string sourcePath1 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");
            string sourcePath2 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = sourcePath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = sourcePath2 };

            const string hash1 = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";
            const string hash2 = "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0";
            const string hash3 = "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056";

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Not.Empty);
            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Asset? duplicatedAsset1 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset1.FileName);
            Asset? duplicatedAsset3 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);

            Assert.That(duplicatedAsset1, Is.Not.Null);
            Assert.That(duplicatedAsset3, Is.Not.Null);
            Assert.That(duplicatedAsset1!.FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));

            Asset? duplicatedAsset2 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
            Asset? duplicatedAsset4 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);

            Assert.That(duplicatedAsset2, Is.Not.Null);
            Assert.That(duplicatedAsset4, Is.Not.Null);
            Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatesBetweenOriginalAndThumbnail_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        try
        {
            const string hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960";

            string sourcePath = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

            Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1!, assetData);
            _assetRepository.AddAsset(_asset2!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);
            _assetRepository.AddAsset(_asset4!, assetData);
            _assetRepository.AddAsset(_asset5!, assetData);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Not.Empty);
            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));

            List<Asset> duplicatedAssetsSet = duplicatedAssets[0];

            Assert.That(duplicatedAssetsSet, Has.Count.EqualTo(4));

            Asset? duplicatedAsset2 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
            Asset? duplicatedAsset3 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);
            Asset? duplicatedAsset4 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);
            Asset? duplicatedAsset5 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset5.FileName);

            Assert.That(duplicatedAsset2, Is.Not.Null);
            Assert.That(duplicatedAsset3, Is.Not.Null);
            Assert.That(duplicatedAsset4, Is.Not.Null);
            Assert.That(duplicatedAsset5, Is.Not.Null);

            Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
            Assert.That(duplicatedAsset5!.FileName, Is.EqualTo(_asset5.FileName));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
    
    [Test]
    public void GetDuplicatesBetweenOriginalAndThumbnail_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            const string hash = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatesBetweenOriginalAndThumbnail_FilesDoNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            const string hash1 = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";
            const string hash2 = "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0";
            const string hash3 = "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056";

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NoAssets_ReturnsEmptyList()
    {
        try
        {
            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NoDuplicatedAssets_ReturnsEmptyList()
    {
        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            const string hash1 = "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0";
            const string hash2 = "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0";

            _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
