namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private FindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;
    private Asset? asset3;
    private Asset? asset4;
    private Asset? asset5;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new (userConfigurationService);
        _findDuplicatedAssetsService = new (_assetRepository, _storageService, userConfigurationService);

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
        asset2 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset3 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1_duplicate.jpg",
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
        asset4 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 9_duplicate.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 6000,
            PixelHeight = 6120,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset5 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image_11.heic",
            FileSize = 2247285,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 3024,
            PixelHeight = 4032,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            Hash = "a92dd8dba1e47ee54dd166574e699ecaec57beb7be4bddded3735dceafe2eaacf21febd96b169eff511dc0c366e088902b4d5c661365e1fdc3dad12c1726df88",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = "The asset has been rotated",
            IsAssetRotated = true
        };
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        string destinationPath1 = Path.Combine(dataDirectory!, "Duplicates\\DestinationToCopy1");
        string destinationPath2 = Path.Combine(dataDirectory!, "Duplicates\\DestinationToCopy2");

        try
        {
            Directory.CreateDirectory(destinationPath1);
            Directory.CreateDirectory(destinationPath2);

            string sourcePath1 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");
            string sourcePath2 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");

            string asset1DestinationPath = Path.Combine(destinationPath1, asset1!.FileName);
            string asset2DestinationPath = Path.Combine(destinationPath2, asset2!.FileName);
            string asset3DestinationPath = Path.Combine(destinationPath2, asset3!.FileName);
            string asset4DestinationPath = Path.Combine(destinationPath2, asset4!.FileName);
            string asset5DestinationPath = Path.Combine(destinationPath2, asset5!.FileName);

            File.Copy(Path.Combine(sourcePath1, asset1!.FileName), asset1DestinationPath);
            File.Copy(Path.Combine(sourcePath2, asset2!.FileName), asset2DestinationPath);
            File.Copy(Path.Combine(sourcePath2, asset3!.FileName), asset3DestinationPath);
            File.Copy(Path.Combine(sourcePath2, asset4!.FileName), asset4DestinationPath);
            File.Copy(Path.Combine(sourcePath2, asset5!.FileName), asset5DestinationPath);

            DateTime oldDateTime1 = DateTime.Now.AddDays(-1);
            DateTime oldDateTime2 = DateTime.Now.AddDays(-5);

            File.SetLastWriteTime(asset1DestinationPath, oldDateTime1);
            File.SetLastWriteTime(asset2DestinationPath, oldDateTime1);
            File.SetLastWriteTime(asset3DestinationPath, oldDateTime2);
            File.SetLastWriteTime(asset4DestinationPath, oldDateTime2);
            File.SetLastWriteTime(asset5DestinationPath, oldDateTime1);

            Folder folder1 = new() { Path = destinationPath1 };
            Folder folder2 = new() { Path = destinationPath2 };

            asset1!.Folder = folder1;
            asset2!.Folder = folder2;
            asset3!.Folder = folder2;
            asset4!.Folder = folder2;
            asset5!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);
            _assetRepository.AddAsset(asset3!, assetData1);
            _assetRepository.AddAsset(asset4!, assetData2);
            _assetRepository.AddAsset(asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsNotEmpty(duplicatedAssets);
            Assert.AreEqual(2, duplicatedAssets.Count);

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];

            Assert.AreEqual(2, firstDuplicatedAssetsSet.Count);
            Assert.AreEqual(2, secondDuplicatedAssetsSet.Count);

            Asset? duplicatedAsset1 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset1.FileName);
            Asset? duplicatedAsset3 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset3.FileName);

            DateTime actualDate = DateTime.Now.Date;

            Assert.IsNotNull(duplicatedAsset1);
            Assert.IsNotNull(duplicatedAsset3);
            Assert.AreEqual(asset1.FileName, duplicatedAsset1!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset1!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime1.Date, duplicatedAsset1!.FileModificationDateTime.Date);
            Assert.AreEqual(asset3.FileName, duplicatedAsset3!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset3!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime2.Date, duplicatedAsset3!.FileModificationDateTime.Date);

            Asset? duplicatedAsset2 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset2.FileName);
            Asset? duplicatedAsset4 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset4.FileName);

            Assert.IsNotNull(duplicatedAsset2);
            Assert.IsNotNull(duplicatedAsset4);
            Assert.AreEqual(asset2.FileName, duplicatedAsset2!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset2!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime1.Date, duplicatedAsset2!.FileModificationDateTime.Date);
            Assert.AreEqual(asset4.FileName, duplicatedAsset4!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset4!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime2.Date, duplicatedAsset4!.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(destinationPath1, true);
            Directory.Delete(destinationPath2, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        string destinationPath = Path.Combine(dataDirectory!, "Duplicates\\DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            string hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960";
            string sourcePath = Path.Combine(dataDirectory!, "Duplicates\\NewFolder2");

            string asset2DestinationPath = Path.Combine(destinationPath, asset2!.FileName);
            string asset3DestinationPath = Path.Combine(destinationPath, asset3!.FileName);
            string asset4DestinationPath = Path.Combine(destinationPath, asset4!.FileName);
            string asset5DestinationPath = Path.Combine(destinationPath, asset5!.FileName);

            File.Copy(Path.Combine(sourcePath, asset2!.FileName), asset2DestinationPath);
            File.Copy(Path.Combine(sourcePath, asset3!.FileName), asset3DestinationPath);
            File.Copy(Path.Combine(sourcePath, asset4!.FileName), asset4DestinationPath);
            File.Copy(Path.Combine(sourcePath, asset5!.FileName), asset5DestinationPath);

            DateTime oldDateTime1 = DateTime.Now.AddDays(-1);
            DateTime oldDateTime2 = DateTime.Now.AddDays(-5);

            File.SetLastWriteTime(asset2DestinationPath, oldDateTime1);
            File.SetLastWriteTime(asset3DestinationPath, oldDateTime2);
            File.SetLastWriteTime(asset4DestinationPath, oldDateTime2);
            File.SetLastWriteTime(asset5DestinationPath, oldDateTime1);

            Folder folder = new() { Path = destinationPath };

            asset2!.Folder = folder;
            asset2.Hash = hash;
            asset3!.Folder = folder;
            asset3.Hash = hash;
            asset4!.Folder = folder;
            asset4.Hash = hash;
            asset5!.Folder = folder;
            asset5.Hash = hash;

            byte[] assetData = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset2!, assetData);
            _assetRepository.AddAsset(asset3!, assetData);
            _assetRepository.AddAsset(asset4!, assetData);
            _assetRepository.AddAsset(asset5!, assetData);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsNotEmpty(duplicatedAssets);
            Assert.AreEqual(1, duplicatedAssets.Count);

            List<Asset> duplicatedAssetsSet = duplicatedAssets[0];

            Assert.AreEqual(4, duplicatedAssetsSet.Count);

            Asset? duplicatedAsset2 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset2.FileName);
            Asset? duplicatedAsset3 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset3.FileName);
            Asset? duplicatedAsset4 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset4.FileName);
            Asset? duplicatedAsset5 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == asset5.FileName);

            DateTime actualDate = DateTime.Now.Date;

            Assert.IsNotNull(duplicatedAsset2);
            Assert.IsNotNull(duplicatedAsset3);
            Assert.IsNotNull(duplicatedAsset4);
            Assert.IsNotNull(duplicatedAsset5);

            Assert.AreEqual(asset2.FileName, duplicatedAsset2!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset2!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime1.Date, duplicatedAsset2!.FileModificationDateTime.Date);

            Assert.AreEqual(asset3.FileName, duplicatedAsset3!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset3!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime2.Date, duplicatedAsset3!.FileModificationDateTime.Date);

            Assert.AreEqual(asset4.FileName, duplicatedAsset4!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset4!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime2.Date, duplicatedAsset4!.FileModificationDateTime.Date);

            Assert.AreEqual(asset5.FileName, duplicatedAsset5!.FileName);
            Assert.AreEqual(actualDate, duplicatedAsset5!.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime1.Date, duplicatedAsset5!.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "Duplicates\\NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset3!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset3!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "NewFolder2");

            Folder folder1 = new() { Path = folderPath1 };
            Folder folder2 = new() { Path = folderPath2 };

            asset1!.Folder = folder1;
            asset2!.Folder = folder2;
            asset3!.Folder = folder2;
            asset4!.Folder = folder2;
            asset5!.Folder = folder2;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);
            _assetRepository.AddAsset(asset3!, assetData1);
            _assetRepository.AddAsset(asset4!, assetData2);
            _assetRepository.AddAsset(asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        try
        {
            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        try
        {
            string folderPath = Path.Combine(dataDirectory!, "NewFolder");

            Folder folder = new() { Path = folderPath };

            asset1!.Folder = folder;
            asset2!.Folder = folder;

            byte[] assetData1 = new byte[] { 1, 2, 3 };
            byte[] assetData2 = Array.Empty<byte>();

            _assetRepository!.AddAsset(asset1!, assetData1);
            _assetRepository.AddAsset(asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.IsEmpty(duplicatedAssets);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
