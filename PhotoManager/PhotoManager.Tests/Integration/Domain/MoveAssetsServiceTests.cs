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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource= _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(2));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(4));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(1));
            Assert.That(recentTargetPaths[0], Is.EqualTo(destinationFolder.Path));
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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, newSourceFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.True);

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, false);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.False);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Empty);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(2));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Empty);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(1));
            Assert.That(recentTargetPaths[0], Is.EqualTo(destinationFolder.Path));
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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(2));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.That(newDestinationFolder, Is.Not.Null);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(4));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == newDestinationFolder!.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == newDestinationFolder!.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(1));
            Assert.That(recentTargetPaths[0], Is.EqualTo(destinationFolder.Path));
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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, newSourceFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.True);

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, false);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.False);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Empty);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(2));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.That(newDestinationFolder, Is.Not.Null);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == newDestinationFolder!.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == newDestinationFolder!.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Empty);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(1));
            Assert.That(recentTargetPaths[0], Is.EqualTo(destinationFolder.Path));
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

            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.That(asset1, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            Asset[] assets = [asset1];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile);

            Assert.That(hasBeenMoved, Is.False);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            string expectedExceptionMessage = $"Access to the path '{destinationFilePath}' is denied.";
            string[] messages = [$"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {expectedExceptionMessage}"];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);
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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, newSourceFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2); // Asset will be in the dest and not in the source

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(sourceDirectory, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(destinationDirectory, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(1));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

            asset2 = asset2.WithFolder(new() { Id = Guid.NewGuid(), Path = sourceDirectory }); // Asset will be in the dest and not in the source

            Asset[] assets = [asset1, asset2];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, false));
            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{newSourceFilePath2}'."));

            Assert.That(File.Exists(newSourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(newSourceFilePath2), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(1));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);
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

            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            string fakeRecentTargetPath = Path.Combine(destinationDirectory, "fake");
            _assetRepository.SaveRecentTargetPaths([fakeRecentTargetPath, destinationDirectory]);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 6.jpg");
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(_dataDirectory!, "Image 1.jpg");
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);
            _assetRepository!.SaveCatalog(destinationFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1!.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths[0], Is.EqualTo(fakeRecentTargetPath));
            Assert.That(recentTargetPaths[1], Is.EqualTo(destinationFolder.Path));

            Asset[] assets = [asset1, asset2];

            bool hasBeenMoved = _moveAssetsService!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(_dataDirectory!);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(2));
            Assert.That(assetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(assetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset2.FileName), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(4));
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == sourceFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset1.FileName && x.FolderId == destinationFolder.Id), Is.True);
            Assert.That(assetsInDb.Any(x => x.FileName == asset2.FileName && x.FolderId == destinationFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset2.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Not.Empty);
            Assert.That(cataloguedAssetsInDestination, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset1.FileName), Is.True);
            Assert.That(cataloguedAssetsInDestination.Any(x => x.FileName == asset2.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Not.Empty);
            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths[0], Is.EqualTo(destinationFolder.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(fakeRecentTargetPath));
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, "Image 1.jpg");
            Assert.That(asset, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName), Is.True);

            bool hasBeenMoved = _moveAssetsService!.MoveAssets([asset], sourceFolder, preserveOriginalFile);
            Assert.That(hasBeenMoved, Is.True);

            Assert.That(File.Exists(destinationFilePath), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);

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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(newSourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(sourceDirectory);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset? asset = _assetCreationService!.CreateAsset(sourceDirectory, assetOldFileName);
            Assert.That(asset, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset!.FileName), Is.True);

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset.FileName), Is.False);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == asset.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            List<Asset> cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

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
                Metadata = asset.Metadata,
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{Path.Combine(sourceDirectory, assetNewFileName)}'."));

            Assert.That(File.Exists(newSourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetOldFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, assetNewFileName), Is.False);

            Folder? newDestinationFolder = _assetRepository.GetFolderByPath(destinationFolder.Path);
            Assert.That(newDestinationFolder, Is.Null);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == assetOldFileName && x.FolderId == sourceFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            cataloguedAssetsInDestination = _assetRepository!.GetCataloguedAssetsByPath(destinationFolder.Path);
            Assert.That(cataloguedAssetsInDestination, Is.Empty);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
            Assert.That(File.Exists(newDestinationFilePath), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, assetOldFileName);
            Assert.That(asset, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset!.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset!.FileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == asset.FileName && x.FolderId == sourceFolder.Id), Is.True);

            List<Asset> cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);

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
                Metadata = asset.Metadata
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets(assets, sourceFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{Path.Combine(destinationDirectory, assetNewFileName)}'."));

            Assert.That(File.Exists(destinationFilePath), Is.True);
            Assert.That(File.Exists(newDestinationFilePath), Is.False);

            assetsInSource = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetOldFileName), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == assetOldFileName && x.FolderId == sourceFolder.Id), Is.True);

            cataloguedAssetsInSource = _assetRepository!.GetCataloguedAssetsByPath(sourceFolder.Path);
            Assert.That(cataloguedAssetsInSource, Is.Not.Empty);
            Assert.That(cataloguedAssetsInSource, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            recentTargetPaths = _assetRepository.GetRecentTargetPaths();
            Assert.That(recentTargetPaths, Is.Empty);
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

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets!, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
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

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
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
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset? asset2 = null;
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2!, asset3];

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("asset cannot be null. (Parameter 'asset')"));
            Assert.That(exception?.ParamName, Is.EqualTo("asset"));
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
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();
            Folder? folder = null;

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = Guid.Empty,
                Folder = folder!,
                FileName = "NonExistentFile.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 0, Height = 0 },
                    Thumbnail = new() { Width = 0, Height = 0 }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2, asset3];

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("asset.Folder cannot be null. (Parameter 'Folder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(Folder)));
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
            Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            Asset[] assets = [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "NonExistentFile.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 0, Height = 0 },
                        Thumbnail = new() { Width = 0, Height = 0 }
                    },
                    Hash = string.Empty
                }
            ];

            Folder? destinationFolder = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.MoveAssets(assets, destinationFolder!, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("destinationFolder cannot be null. (Parameter 'destinationFolder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(destinationFolder)));
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

            Assert.That(File.Exists(sourceFilePath), Is.False);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset asset = new()
            {
                FolderId = sourceFolder.Id,
                Folder = sourceFolder,
                FileName = "NonExistentFile.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 0, Height = 0 },
                    Thumbnail = new() { Width = 0, Height = 0 }
                },
                Hash = string.Empty
            };

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.MoveAssets([asset], destinationFolder, preserveOriginalFile));
            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{sourceFilePath}'."));
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

            const string asset1FileName = "Image 6.jpg";
            const string asset2FileName = "Image 1.jpg";

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, destinationFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);

            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset1 = _assetCreationService!.CreateAsset(destinationDirectory, asset1FileName);
            Assert.That(asset1, Is.Not.Null);
            Asset? asset2 = _assetCreationService!.CreateAsset(destinationDirectory, asset2FileName);
            Assert.That(asset2, Is.Not.Null);

            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1.FileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2.FileName));

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb[0].FileName, Is.EqualTo(asset1.FileName));
            Assert.That(assetsInDb[1].FileName, Is.EqualTo(asset2.FileName));

            _moveAssetsService!.DeleteAssets([asset1, asset2]);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.False);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databasePath!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetIsValid_DeletesAssetAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath1 = Path.Combine(_dataDirectory!, "Image 6.jpg");
            string destinationFilePath1 = Path.Combine(destinationDirectory, "Image 6.jpg");

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, destinationFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);

            string sourceFilePath2 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string destinationFilePath2 = Path.Combine(destinationDirectory, "Image 1.jpg");

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            Folder sourceFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset? asset = _assetCreationService!.CreateAsset(destinationDirectory, "Image 6.jpg");
            Assert.That(asset, Is.Not.Null);
            _assetRepository!.SaveCatalog(sourceFolder);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset!.FileName));

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb[0].FileName, Is.EqualTo(asset.FileName));

            _moveAssetsService!.DeleteAssets([asset]);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Empty);
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

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
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

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
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
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset? asset2 = null;
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2!, asset3];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo("asset cannot be null. (Parameter 'asset')"));
            Assert.That(exception?.ParamName, Is.EqualTo("asset"));
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
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();
            Folder? folder = null;

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = Guid.Empty,
                Folder = folder!,
                FileName = "NonExistentFile.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 0, Height = 0 },
                    Thumbnail = new() { Width = 0, Height = 0 }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2, asset3];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo("asset.Folder cannot be null. (Parameter 'Folder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(Folder)));
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
            Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = "NonExistentFile.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 0, Height = 0 },
                    Thumbnail = new() { Width = 0, Height = 0 }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _moveAssetsService!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{asset.FullPath}'."));
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
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

            Assert.That(hasBeenCopied, Is.False);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

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

            Assert.That(hasBeenCopied, Is.False);
            Assert.That(File.Exists(destinationFilePath), Is.False);

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
            Assert.That(exception?.Message, Is.EqualTo($"The target file '{destinationDirectory}' is a directory, not a file."));
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);

            hasBeenCopied = _moveAssetsService!.CopyAsset(destinationFilePath, destinationFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);

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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);

            sourceFilePath = Path.Combine(destinationDirectory, "Image 1.jpg");
            destinationFilePath = Path.Combine(destinationDirectory, "Image 2.jpg");

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
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

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{nonExistentFilePath}'."));
            Assert.That(File.Exists(destinationFilePath), Is.False);
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

            Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{nonExistentFilePath}'."));
            Assert.That(File.Exists(destinationFilePath), Is.False);
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

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
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

            Assert.That(hasBeenCopied, Is.False);
            Assert.That(File.Exists(sourceFilePath), Is.False);
            Assert.That(File.Exists(destinationFilePath), Is.True);

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

            Assert.That(exception?.Message, Is.EqualTo("The value cannot be an empty string. (Parameter 'sourceFilePath')"));
            Assert.That(File.Exists(destinationFilePath), Is.False);
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

            Assert.That(hasBeenCopied, Is.False);

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

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'sourceFilePath')"));
            Assert.That(File.Exists(destinationFilePath), Is.False);
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

            Assert.That(hasBeenCopied, Is.False);

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

            Assert.That(hasBeenCopied, Is.False);

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
