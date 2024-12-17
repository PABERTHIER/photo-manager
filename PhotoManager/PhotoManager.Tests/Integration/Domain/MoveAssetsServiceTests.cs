namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class MoveAssetsServiceTests
{
    private string? _dataDirectory;
    private string? _databasePath;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private MoveAssetsService? _moveAssetsService;
    private UserConfigurationService? _userConfigurationService;
    private AssetRepository? _assetRepository;
    private Database? _database;
    private StorageService? _storageService;
    private AssetCreationService? _assetCreationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);
        _databasePath = Path.Combine(_dataDirectory, "DatabaseTests");

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        _storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        _assetCreationService = new (_assetRepository, _storageService, assetHashCalculatorService, _userConfigurationService);
        _moveAssetsService = new (_assetRepository, _storageService, _assetCreationService);
    }

    [Test]
    public void MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsTrue_MoveSucceeds()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource= _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.IsTrue(hasBeenMoved);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(2, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(4, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(2, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(1, recentTargetPaths.Count);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[0]);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsFalse_MoveSucceeds()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, "SourceToMove");
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string newSourceFilePath1 = Path.Combine(sourceDirectory, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string newSourceFilePath2 = Path.Combine(sourceDirectory, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(newSourceFilePath1));

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, newSourceFilePath2);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(newSourceFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, false);

            Assert.IsTrue(hasBeenMoved);
            Assert.IsFalse(File.Exists(newSourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));
            Assert.IsFalse(File.Exists(newSourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsEmpty(assetsInSource);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(2, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInSource);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(2, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(1, recentTargetPaths.Count);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[0]);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void MoveAssets_AssetsAreValidAndDestinationFolderNotCataloguedAndPreserveOriginalFilesIsTrue_MoveSucceeds()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = new() { FolderId = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.IsTrue(hasBeenMoved);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(2, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.IsNotNull(newDestinationFolder);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(4, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == newDestinationFolder!.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == newDestinationFolder!.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(2, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(1, recentTargetPaths.Count);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[0]);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void MoveAssets_AssetsAreValidAndDestinationFolderNotCataloguedAndPreserveOriginalFilesIsFalse_MoveSucceeds()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, "SourceToMove");
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string newSourceFilePath1 = Path.Combine(sourceDirectory, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string newSourceFilePath2 = Path.Combine(sourceDirectory, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(newSourceFilePath1));

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, newSourceFilePath2);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(newSourceFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = new() { FolderId = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, false);

            Assert.IsTrue(hasBeenMoved);
            Assert.IsFalse(File.Exists(newSourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));
            Assert.IsFalse(File.Exists(newSourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsEmpty(assetsInSource);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(2, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.IsNotNull(newDestinationFolder);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == newDestinationFolder!.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == newDestinationFolder!.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInSource);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(2, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(1, recentTargetPaths.Count);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[0]);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_AssetsAreValidButDirectoryIsInReadOnlyMode_ReturnFalseAndLogsItAndDoesNotMoveFile(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "NoMoveDirectory");
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            // Deny write access to the directory
            DirectoryHelper.DenyWriteAccess(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.IsNotNull(asset1);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset[] assets = [asset1];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile);

            Assert.IsFalse(hasBeenMoved);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            string expectedExceptionMessage = $"Access to the path '{destinationFilePath}' is denied.";
            string[] messages = [$"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {expectedExceptionMessage}"];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);

            // Allow to write access to the directory
            DirectoryHelper.AllowWriteAccess(destinationDirectory);

            Directory.Delete(destinationDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void MoveAssets_AssetIsInTheDestinationButNotInTheSourceAndPreserveOriginalFilesIsTrue_ThrowsFileNotFoundException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, "SourceToMove");
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string newSourceFilePath1 = Path.Combine(sourceDirectory, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string newSourceFilePath2 = Path.Combine(sourceDirectory, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(newSourceFilePath1));

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2); // Asset will be in the dest and not in the source

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(newSourceFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(destinationDirectory, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(1, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2!.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(1, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            asset2.Folder = new() { Path = sourceDirectory }; // Asset will be in the dest and not in the source

            Asset[] assets = [asset1, asset2];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, false));
            Assert.AreEqual($"File does not exist: '{newSourceFilePath2}'.", exception?.Message);

            Assert.IsTrue(File.Exists(newSourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsFalse(File.Exists(newSourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(1, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(1, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsTrueAndRecentTargetPathAlreadyAdded_MoveSucceeds()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");
            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsFalse(File.Exists(destinationFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            string fakeRecentTargetPath = Path.Combine(destinationDirectory, "fake");
            _assetRepository.SaveRecentTargetPaths([fakeRecentTargetPath, destinationDirectory]);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.IsNotNull(asset1);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.IsNotNull(asset2);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1!.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName));

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(2, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(fakeRecentTargetPath, recentTargetPaths[0]);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[1]);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.IsTrue(hasBeenMoved);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(2, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset2.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInDestination);
            Assert.AreEqual(2, assetsInDestination.Length);
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(assetsInDestination.Any(x => x.FileName == asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(4, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.FolderId));
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(2, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInDestination);
            Assert.AreEqual(2, cataloguedAssetsInDestination.Count);
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName));
            Assert.IsTrue(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsNotEmpty(recentTargetPaths);
            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(destinationFolder.Path, recentTargetPaths[0]);
            Assert.AreEqual(fakeRecentTargetPath, recentTargetPaths[1]);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_SameSourceAndDestination_ReturnsTrueAndLogsItAndDoesNotMoveFile(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, "Image 1.jpg");
            Assert.IsNotNull(asset);

            _assetRepository!.SaveCatalog(sourceFolder);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName));

            bool hasBeenMoved = _moveAssetsService!.MoveAssets([asset], sourceFolder, preserveOriginalFile);
            Assert.IsTrue(hasBeenMoved);

            Assert.IsTrue(File.Exists(destinationFilePath));
            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName));

            string[] messages = [$"Cannot copy '{destinationFilePath}' into '{destinationFilePath}' because the file already exists in the destination."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_DifferentFileNameBetweenSourceAndDestination_ThrowsFileNotFoundException(bool preserveOriginalFile)
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, "SourceToMove");
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string assetOldFileName = "Image 1.jpg";
            const string assetNewFileName = "Image 2.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, assetOldFileName);
            string newSourceFilePath = Path.Combine(sourceDirectory, assetOldFileName);
            string destinationFilePath = Path.Combine(destinationDirectory, assetNewFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, newSourceFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(newSourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = new() { FolderId = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset = _assetCreationService!.CreateAsset(sourceDirectory, assetOldFileName);
            Assert.IsNotNull(asset);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset!.FileName));

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName));

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset newAsset = new()
            {
                FolderId = asset.FolderId,
                Folder = asset.Folder,
                FileName = assetNewFileName,
                FileProperties = asset.FileProperties,
                Pixel = asset.Pixel,
                ImageRotation = asset.ImageRotation,
                Hash = asset.Hash,
                ImageData = asset.ImageData,
                ThumbnailCreationDateTime  = asset.ThumbnailCreationDateTime,
                IsAssetCorrupted = asset.IsAssetCorrupted,
                AssetCorruptedMessage = asset.AssetCorruptedMessage,
                IsAssetRotated = asset.IsAssetRotated,
                AssetRotatedMessage = asset.AssetRotatedMessage
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.AreEqual($"File does not exist: '{Path.Combine(sourceDirectory, assetNewFileName)}'.", exception?.Message);

            Assert.IsTrue(File.Exists(newSourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset.FileName));

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInDestination);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetOldFileName));
            Assert.IsFalse(_assetRepository!.ContainsThumbnail(destinationFolder.Path, assetNewFileName));

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.IsNull(newDestinationFolder);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == assetOldFileName && x.FolderId == sourceFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName));

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.IsEmpty(cataloguedAssetsInDestination);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);
        }
        finally
        {
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_DifferentFileNameAndSameSourceAndDestination_MovesFileAndReturnsTrue(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            const string assetOldFileName = "Image 1.jpg";
            const string assetNewFileName = "Image 2.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, assetOldFileName);
            string destinationFilePath = Path.Combine(destinationDirectory, assetOldFileName);
            string newDestinationFilePath = Path.Combine(destinationDirectory, assetNewFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));
            Assert.IsFalse(File.Exists(newDestinationFilePath));

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, assetOldFileName);
            Assert.IsNotNull(asset);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset!.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == asset.FileName && x.FolderId == sourceFolder.FolderId));

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName));

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);

            Asset newAsset = new()
            {
                FolderId = asset.FolderId,
                Folder = asset.Folder,
                FileName = assetNewFileName,
                FileProperties = asset.FileProperties,
                Pixel = asset.Pixel,
                ImageRotation = asset.ImageRotation,
                Hash = asset.Hash,
                ImageData = asset.ImageData,
                ThumbnailCreationDateTime  = asset.ThumbnailCreationDateTime,
                IsAssetCorrupted = asset.IsAssetCorrupted,
                AssetCorruptedMessage = asset.AssetCorruptedMessage,
                IsAssetRotated = asset.IsAssetRotated,
                AssetRotatedMessage = asset.AssetRotatedMessage
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, sourceFolder, preserveOriginalFile));

            Assert.AreEqual($"File does not exist: '{Path.Combine(destinationDirectory, assetNewFileName)}'.", exception?.Message);

            Assert.IsTrue(File.Exists(destinationFilePath));
            Assert.IsFalse(File.Exists(newDestinationFilePath));

            assetsInSource = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInSource);
            Assert.AreEqual(1, assetsInSource.Length);
            Assert.IsTrue(assetsInSource.Any(x => x.FileName == asset.FileName));

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetOldFileName));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.IsTrue(assetsInDb.Any(x => x.FileName == assetOldFileName && x.FolderId == sourceFolder.FolderId));

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.IsNotEmpty(cataloguedAssetsInSource);
            Assert.AreEqual(1, cataloguedAssetsInSource.Count);
            Assert.IsTrue(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName));

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.IsEmpty(recentTargetPaths);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_AssetsIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        try
        {
            Asset[]? assets = null;

            Folder destinationFolder = new() { Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets!, destinationFolder, preserveOriginalFile));

            Assert.AreEqual("assets cannot be null or empty. (Parameter 'assets')", exception?.Message);
            Assert.AreEqual(nameof(assets), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_AssetsIsEmpty_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        try
        {
            Asset[] assets = [];

            Folder destinationFolder = new() { Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.AreEqual("assets cannot be null or empty. (Parameter 'assets')", exception?.Message);
            Assert.AreEqual(nameof(assets), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_OneAssetIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        try
        {
            Asset asset1 = new() { FileName = "Image 1.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset? asset2 = null;
            Asset asset3 = new() { FileName = "Image 2.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset[] assets = [asset1, asset2!, asset3];

            Folder destinationFolder = new() { Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.AreEqual("asset cannot be null. (Parameter 'asset')", exception?.Message);
            Assert.AreEqual("asset", exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_OneAssetFolderIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        try
        {
            Folder? folder = null;

            Asset asset1 = new() { FileName = "Image 1.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset asset2 = new() { FileName = "NonExistentFile.jpg", Folder = folder!, Hash = string.Empty };
            Asset asset3 = new() { FileName = "Image 2.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset[] assets = [asset1, asset2, asset3];

            Folder destinationFolder = new() { Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.AreEqual("asset.Folder cannot be null. (Parameter 'Folder')", exception?.Message);
            Assert.AreEqual(nameof(Folder), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_DestinationFolderIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        try
        {
            Asset[] assets = [new() { FileName = "NonExistentFile.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty }];

            Folder? destinationFolder = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder!, preserveOriginalFile));

            Assert.AreEqual("destinationFolder cannot be null. (Parameter 'destinationFolder')", exception?.Message);
            Assert.AreEqual(nameof(destinationFolder), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_FileDoesNotExist_ThrowsFileNotFoundException(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToMove");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "NonExistentFile.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "NonExistentFile.jpg");

            Assert.IsFalse(File.Exists(sourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset asset = new()
            {
                FileName = "NonExistentFile.jpg",
                Folder = sourceFolder,
                FolderId = sourceFolder.FolderId,
                Hash = string.Empty
            };

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets([asset], destinationFolder, preserveOriginalFile));
            Assert.AreEqual($"File does not exist: '{sourceFilePath}'.", exception?.Message);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsAreValid_DeletesAssetsAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, destinationFilePath1);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath1));

            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath2));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Assert.IsTrue(File.Exists(sourceFilePath1));

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, "Image 6.jpg");
            Assert.IsNotNull(asset);
            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsNotEmpty(assetsInRepository);
            Assert.AreEqual(1, assetsInRepository.Length);
            Assert.AreEqual(asset!.FileName, assetsInRepository[0].FileName);

            Assert.IsTrue(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName));

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsNotEmpty(assetsInDb);
            Assert.AreEqual(1, assetsInDb.Count);
            Assert.AreEqual(asset.FileName, assetsInDb[0].FileName);

            _moveAssetsService!.DeleteAssets([asset]);

            Assert.IsFalse(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.IsEmpty(assetsInRepository);

            Assert.IsFalse(File.Exists(destinationFilePath1));
            Assert.IsTrue(File.Exists(destinationFilePath2));

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.IsEmpty(assetsInDb);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsIsNull_ThrowsArgumentNullException()
    {
        try
        {
            Asset[]? assets = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets!));

            Assert.AreEqual("assets cannot be null or empty. (Parameter 'assets')", exception?.Message);
            Assert.AreEqual(nameof(assets), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsIsEmpty_ThrowsArgumentNullException()
    {
        try
        {
            Asset[] assets = [];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.AreEqual("assets cannot be null or empty. (Parameter 'assets')", exception?.Message);
            Assert.AreEqual(nameof(assets), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_OneAssetIsNull_ThrowsArgumentNullException()
    {
        try
        {
            Asset asset1 = new() { FileName = "Image 1.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset? asset2 = null;
            Asset asset3 = new() { FileName = "Image 2.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset[] assets = [asset1, asset2!, asset3];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.AreEqual("asset cannot be null. (Parameter 'asset')", exception?.Message);
            Assert.AreEqual("asset", exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_OneAssetFolderIsNull_ThrowsArgumentNullException()
    {
        try
        {
            Folder? folder = null;

            Asset asset1 = new() { FileName = "Image 1.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset asset2 = new() { FileName = "NonExistentFile.jpg", Folder = folder!, Hash = string.Empty };
            Asset asset3 = new() { FileName = "Image 2.jpg", Folder = new() { Path = _dataDirectory! }, Hash = string.Empty };
            Asset[] assets = [asset1, asset2, asset3];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.AreEqual("asset.Folder cannot be null. (Parameter 'Folder')", exception?.Message);
            Assert.AreEqual(nameof(Folder), exception?.ParamName);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetDoesNotExists_ThrowsFileNotFoundException()
    {
        try
        {
            Folder folder = new() { Path = _dataDirectory! };
            Asset asset = new() { FileName = "NonExistentFile.jpg", Folder = folder, Hash = string.Empty };
            Asset[] assets = [asset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.AreEqual($"File does not exist: '{asset.FullPath}'.", exception?.Message);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_SourceAndDestinationAreValid_CopiesFileAndReturnsTrue()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_SourceAndDestinationAreValidButDirectoryIsInReadOnlyMode_ReturnFalseAndLogsItAndDoesNotCopyFile()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "NoCopyDirectory");
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            // Deny to write access to the directory
            DirectoryHelper.DenyWriteAccess(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsFalse(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsFalse(File.Exists(destinationFilePath));

            string expectedExceptionMessage = $"Access to the path '{destinationFilePath}' is denied.";
            string[] messages = [$"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {expectedExceptionMessage}"];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);

            // Allow to write access to the directory
            DirectoryHelper.AllowWriteAccess(destinationDirectory);

            Directory.Delete(destinationDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_SourceFilePathIsADirectory_ReturnFalseAndLogsItAndDoesNotCopyFile()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(_dataDirectory!, destinationFilePath); // Access denied when source file path is a directory -> UnauthorizedAccessException

            Assert.IsFalse(hasBeenCopied);
            Assert.IsFalse(File.Exists(destinationFilePath));

            string expectedExceptionMessage = $"Access to the path '{_dataDirectory}' is denied.";
            string[] messages = [$"Cannot copy '{_dataDirectory}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {expectedExceptionMessage}"];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            Directory.Delete(destinationDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_DestinationFilePathIsADirectory_DoesNotCopyAndThrowsIOException()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToSync");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            IOException? exception = Assert.Throws<IOException>(() => _moveAssetsService!.CopyAsset(sourceDirectory, destinationDirectory));
            Assert.AreEqual($"The target file '{destinationDirectory}' is a directory, not a file.", exception?.Message);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public void CopyAsset_SameSourceAndDestination_ReturnsTrueAndLogsItAndDoesNotCopyFile()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));

            hasBeenCopied = _moveAssetsService!.CopyAsset(destinationFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(destinationFilePath));

            string[] messages = [$"Cannot copy '{destinationFilePath}' into '{destinationFilePath}' because the file already exists in the destination."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_DifferentFileNameBetweenSourceAndDestination_CopiesFileAndReturnsTrue()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 2.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_DifferentFileNameAndSameSourceAndDestination_CopiesFileAndReturnsTrue()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));

            sourceFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");
            destinationFilePath = Path.Combine(destinationDirectory, "Image 2.jpg");

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_FileInSourceDoesNotExist_DoesNotCopyFileAndThrowsFileNotFoundException()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string nonExistentFilePath = Path.Combine(_dataDirectory!, "NonExistentFile.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image.jpg");

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.CopyAsset(nonExistentFilePath, destinationFilePath));

            Assert.AreEqual($"File does not exist: '{nonExistentFilePath}'.", exception?.Message);
            Assert.IsFalse(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_SourceDoesNotExist_DoesNotCopyFileAndThrowsDirectoryNotFoundException()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");
        string nonExistentSource = Path.Combine(_dataDirectory!, "toto");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string nonExistentFilePath = Path.Combine(nonExistentSource, "NonExistentFile.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image.jpg");

            DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _moveAssetsService!.CopyAsset(nonExistentFilePath, destinationFilePath));

            Assert.AreEqual($"Could not find a part of the path '{nonExistentFilePath}'.", exception?.Message);
            Assert.IsFalse(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_DestinationDoesNotExist_CopiesFileAndReturnsTrue()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsTrue(hasBeenCopied);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_FileInSourceDoesNotExistButExistsInTheDestination_ReturnsFalseAndLogsItAndDoesNotCopyFile()
    {
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string sourceFilePath = Path.Combine(_dataDirectory!, "NonExistentFile.jpg");
            string destinationFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsFalse(hasBeenCopied);
            Assert.IsFalse(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(destinationFilePath));

            string[] messages = [$"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the file already exists in the destination."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_SourceIsEmpty_ThrowsArgumentException()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            string sourceFilePath = string.Empty;
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath));

            Assert.AreEqual("The value cannot be an empty string. (Parameter 'sourceFilePath')", exception?.Message);
            Assert.IsFalse(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_DestinationIsEmpty_ReturnsFalseAndLogsItAndDoesNotCopyFile()
    {
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath = string.Empty;

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.IsFalse(hasBeenCopied);

            string[] messages = [$"Cannot copy '{sourceFilePath}' because the destination path is null or empty."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_SourceIsNull_ThrowsArgumentNullException()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            string? sourceFilePath = null;
            string destinationFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.CopyAsset(sourceFilePath!, destinationFilePath));

            Assert.AreEqual("Value cannot be null. (Parameter 'sourceFilePath')", exception?.Message);
            Assert.IsFalse(File.Exists(destinationFilePath));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void CopyAsset_SourceIsNullButFileExistsInDestination_ReturnsFalseAndLogsItAndDoesNotCopyFile()
    {
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string? sourceFilePath = null;
            string destinationFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath!, destinationFilePath);

            Assert.IsFalse(hasBeenCopied);

            string[] messages = [$"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the file already exists in the destination."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CopyAsset_DestinationIsNull_ReturnsFalseAndLogsItAndDoesNotCopyFile()
    {
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            string sourceFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string? destinationFilePath = null;

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath!);

            Assert.IsFalse(hasBeenCopied);

            string[] messages = [$"Cannot copy '{sourceFilePath}' because the destination path is null or empty."];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);
        }
        finally
        {
            Directory.Delete(_databasePath!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }
}
