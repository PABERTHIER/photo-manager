using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetTotalFilesCountTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private SyncAssetsService? _syncAssetsService;
    private CatalogAssetsService? _catalogAssetsService;
    private MoveAssetsService? _moveAssetsService;
    private FindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private UserConfigurationService? _userConfigurationService;
    private StorageService? _storageService;
    private Database? _database;
    private AssetsComparator? _assetsComparator;
    private AssetHashCalculatorService? _assetHashCalculatorService;

    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_databasePath);

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [SetUp]
    public void SetUp()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!);

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        _storageService = new (_userConfigurationService);
        _assetHashCalculatorService = new (_userConfigurationService);
        _assetsComparator = new (_storageService);
        _catalogAssetsService = new (_testableAssetRepository, _assetHashCalculatorService, _storageService, _userConfigurationService, _assetsComparator);
        _moveAssetsService = new (_testableAssetRepository, _storageService, _catalogAssetsService);

        _syncAssetsService = new (_testableAssetRepository, _storageService, _assetsComparator, _moveAssetsService);
        _findDuplicatedAssetsService = new (_testableAssetRepository, _storageService, _userConfigurationService);
        _application = new (_testableAssetRepository, _syncAssetsService, _catalogAssetsService, _moveAssetsService, _findDuplicatedAssetsService, _userConfigurationService, _storageService);
    }

    [Test]
    public void GetTotalFilesCount_RootDirectory_ReturnsTotalFilesCount()
    {
        try
        {
            int totalFilesCount = _application!.GetTotalFilesCount();
            Assert.AreEqual(67, totalFilesCount);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
