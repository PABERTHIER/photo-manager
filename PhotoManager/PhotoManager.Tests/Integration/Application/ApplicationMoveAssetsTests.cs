using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationMoveAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private MoveAssetsService? _moveAssetsService;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = new(configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(_database, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new(_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        _moveAssetsService = new(_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(_assetRepository, storageService, assetsComparator, _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_assetRepository, storageService, _userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, _moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
    }

    [Test]
    public async Task MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsTrue_MoveSucceeds()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.ASSETS_TESTS);
        string sourceDirectory = Path.Combine(assetsDirectory, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(assetsDirectory, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder? destinationFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(destinationFolder, Is.Not.Null);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInSource[1].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];
            Asset asset2 = assetsInSource[1];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

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

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
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
            Directory.Delete(assetsDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsFalse_MoveSucceeds()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.ASSETS_TESTS);
        string sourceDirectory = Path.Combine(assetsDirectory, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(assetsDirectory, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder? destinationFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(destinationFolder, Is.Not.Null);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInSource[1].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];
            Asset asset2 = assetsInSource[1];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

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

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, false);

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
            Directory.Delete(assetsDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task MoveAssets_AssetsAreValidAndDestinationFolderNotCataloguedAndPreserveOriginalFilesIsTrue_MoveSucceeds()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, sourceDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInSource[1].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];
            Asset asset2 = assetsInSource[1];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

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

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
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
            Directory.Delete(sourceDirectory, true);
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task MoveAssets_AssetsAreValidAndDestinationFolderNotCataloguedAndPreserveOriginalFilesIsFalse_MoveSucceeds()
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, sourceDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInSource[1].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];
            Asset asset2 = assetsInSource[1];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

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

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, false);

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
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task MoveAssets_AssetIsValidButDirectoryIsInReadOnlyMode_ReturnFalseAndLogsItAndDoesNotMoveFile(bool preserveOriginalFile)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.ASSETS_TESTS);
        string sourceDirectory = Path.Combine(assetsDirectory, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(assetsDirectory, Directories.NO_MOVE_DIRECTORY);
        LoggingAssertsService loggingAssertsService = new();

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            // Deny write access to the directory
            DirectoryHelper.DenyWriteAccess(destinationDirectory);

            const string assetFileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath = Path.Combine(_dataDirectory!, assetFileName);
            string newSourceFilePath = Path.Combine(sourceDirectory, assetFileName);
            string destinationFilePath = Path.Combine(destinationDirectory, assetFileName);

            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, newSourceFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(newSourceFilePath), Is.True);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder? destinationFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(destinationFolder, Is.Not.Null);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(assetFileName));
            Asset asset = assetsInSource[0];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);

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

            Asset[] assets = [asset];

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, preserveOriginalFile);

            Assert.That(hasBeenMoved, Is.False);
            Assert.That(File.Exists(newSourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            string expectedExceptionMessage = $"Access to the path '{destinationFilePath}' is denied.";
            string[] messages = [$"Cannot copy '{newSourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {expectedExceptionMessage}"];
            Type typeOfService = typeof(MoveAssetsService);

            loggingAssertsService.AssertLogInfos(messages, typeOfService);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource.Any(x => x.FileName == asset.FileName), Is.True);

            assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(destinationFolder.Path, asset.FileName), Is.False);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb.Any(x => x.FileName == asset.FileName && x.FolderId == sourceFolder.Id), Is.True);

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
            Directory.Delete(_databaseDirectory!, true);

            // Allow to write access to the directory
            DirectoryHelper.AllowWriteAccess(destinationDirectory);

            Directory.Delete(assetsDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task MoveAssets_AssetIsInTheDestinationButNotInTheSourceAndPreserveOriginalFilesIsTrue_ThrowsFileNotFoundException()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.ASSETS_TESTS);
        string sourceDirectory = Path.Combine(assetsDirectory, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(assetsDirectory, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder? destinationFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(destinationFolder, Is.Not.Null);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Not.Empty);
            Assert.That(assetsInDestination, Has.Length.EqualTo(1));
            Assert.That(assetsInDestination[0].FileName, Is.EqualTo(asset2FileName));
            Asset asset2 = assetsInDestination[0];

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.False);

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

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _application!.MoveAssets(assets, destinationFolder, false));
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
            Directory.Delete(assetsDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task MoveAssets_AssetsAreValidAndPreserveOriginalFilesIsTrueAndRecentTargetPathAlreadyAdded_MoveSucceeds()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.ASSETS_TESTS);
        string sourceDirectory = Path.Combine(assetsDirectory, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(assetsDirectory, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, asset1FileName);
            string newSourceFilePath1 = Path.Combine(sourceDirectory, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, asset2FileName);
            string newSourceFilePath2 = Path.Combine(sourceDirectory, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

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

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder? destinationFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(destinationFolder, Is.Not.Null);

            string fakeRecentTargetPath = Path.Combine(destinationDirectory, Directories.FAKE);
            _assetRepository.SaveRecentTargetPaths([fakeRecentTargetPath, destinationDirectory]);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(2));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInSource[1].FileName, Is.EqualTo(asset1FileName));
            Asset asset1 = assetsInSource[0];
            Asset asset2 = assetsInSource[1];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1.FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2.FileName), Is.True);

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

            bool hasBeenMoved = _application!.MoveAssets(assets, destinationFolder, true);

            Assert.That(hasBeenMoved, Is.True);
            Assert.That(File.Exists(newSourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);
            Assert.That(File.Exists(newSourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
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
            Directory.Delete(assetsDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task MoveAssets_SameSourceAndDestination_ReturnsTrueAndLogsItAndDoesNotMoveFile(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);
        LoggingAssertsService loggingAssertsService = new();

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string destinationFilePath = Path.Combine(destinationDirectory, FileNames.IMAGE_1_JPG);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Asset asset = assetsInRepository[0];

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);

            bool hasBeenMoved = _application!.MoveAssets([asset], sourceFolder, preserveOriginalFile);
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
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task MoveAssets_DifferentFileNameBetweenSourceAndDestination_ThrowsFileNotFoundException(bool preserveOriginalFile)
    {
        string sourceDirectory = Path.Combine(_dataDirectory!, Directories.SOURCE_TO_MOVE);
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, sourceDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            const string assetOldFileName = FileNames.IMAGE_1_JPG;
            const string assetNewFileName = FileNames.IMAGE_2_JPG;

            string sourceFilePath = Path.Combine(_dataDirectory!, assetOldFileName);
            string newSourceFilePath = Path.Combine(sourceDirectory, assetOldFileName);
            string destinationFilePath = Path.Combine(destinationDirectory, assetNewFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, newSourceFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(newSourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(sourceDirectory);
            Assert.That(sourceFolder, Is.Not.Null);
            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = destinationDirectory };

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(sourceDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(assetOldFileName));
            Asset asset = assetsInSource[0];

            Asset[] assetsInDestination = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInDestination, Is.Empty);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);
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
                ThumbnailCreationDateTime = asset.ThumbnailCreationDateTime,
                Metadata = asset.Metadata,
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _application!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

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
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task MoveAssets_DifferentFileNameAndSameSourceAndDestination_MovesFileAndReturnsTrue(bool preserveOriginalFile)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            const string assetOldFileName = FileNames.IMAGE_1_JPG;
            const string assetNewFileName = FileNames.IMAGE_2_JPG;

            string sourceFilePath = Path.Combine(_dataDirectory!, assetOldFileName);
            string destinationFilePath = Path.Combine(destinationDirectory, assetOldFileName);
            string newDestinationFilePath = Path.Combine(destinationDirectory, assetNewFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath, destinationFilePath);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath), Is.True);
            Assert.That(File.Exists(destinationFilePath), Is.True);
            Assert.That(File.Exists(newDestinationFilePath), Is.False);

            await _application!.CatalogAssetsAsync(_ => { });

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            Asset[] assetsInSource = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInSource, Is.Not.Empty);
            Assert.That(assetsInSource, Has.Length.EqualTo(1));
            Assert.That(assetsInSource[0].FileName, Is.EqualTo(assetOldFileName));
            Asset asset = assetsInSource[0];

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset.FileName), Is.True);

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
                ThumbnailCreationDateTime = asset.ThumbnailCreationDateTime,
                Metadata = asset.Metadata
            };
            Asset[] assets = [newAsset];

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _application!.MoveAssets(assets, sourceFolder, preserveOriginalFile));

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
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_AssetsIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Asset[]? assets = null;

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.MoveAssets(assets!, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_AssetsIsEmpty_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Asset[] assets = [];

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_OneAssetIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = FileNames.IMAGE_1_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
                },
                Hash = string.Empty
            };
            Asset? asset2 = null;
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = FileNames.IMAGE_2_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2!, asset3];

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("asset cannot be null. (Parameter 'asset')"));
            Assert.That(exception?.ParamName, Is.EqualTo("asset"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_OneAssetFolderIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Guid folderId1 = Guid.NewGuid();
            Guid folderId2 = Guid.NewGuid();
            Folder? folder = null;

            Asset asset1 = new()
            {
                FolderId = folderId1,
                Folder = new() { Id = folderId1, Path = _dataDirectory! },
                FileName = FileNames.IMAGE_1_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = Guid.Empty,
                Folder = folder!,
                FileName = FileNames.NON_EXISTENT_FILE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_FILE_JPG, Height = PixelHeightAsset.NON_EXISTENT_FILE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_FILE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_FILE_JPG }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folderId2,
                Folder = new() { Id = folderId2, Path = _dataDirectory! },
                FileName = FileNames.IMAGE_2_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
                },
                Hash = string.Empty
            };

            Asset[] assets = [asset1, asset2, asset3];

            Folder destinationFolder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.MoveAssets(assets, destinationFolder, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("asset.Folder cannot be null. (Parameter 'Folder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(Folder)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_DestinationFolderIsNull_ThrowsArgumentNullException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };

            Asset[] assets = [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = FileNames.NON_EXISTENT_FILE_JPG,
                    Pixel = new()
                    {
                        Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_FILE_JPG, Height = PixelHeightAsset.NON_EXISTENT_FILE_JPG },
                        Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_FILE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_FILE_JPG }
                    },
                    Hash = string.Empty
                }
            ];

            Folder? destinationFolder = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.MoveAssets(assets, destinationFolder!, preserveOriginalFile));

            Assert.That(exception?.Message, Is.EqualTo("destinationFolder cannot be null. (Parameter 'destinationFolder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(destinationFolder)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MoveAssets_FileDoesNotExist_ThrowsFileNotFoundException(bool preserveOriginalFile)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_MOVE);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            string sourceFilePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_FILE_JPG);
            string destinationFilePath = Path.Combine(destinationDirectory, FileNames.NON_EXISTENT_FILE_JPG);

            Assert.That(File.Exists(sourceFilePath), Is.False);
            Assert.That(File.Exists(destinationFilePath), Is.False);

            Folder sourceFolder = _assetRepository!.AddFolder(_dataDirectory!);
            Folder destinationFolder = _assetRepository!.AddFolder(destinationDirectory);

            Asset asset = new()
            {
                FolderId = sourceFolder.Id,
                Folder = sourceFolder,
                FileName = FileNames.NON_EXISTENT_FILE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_FILE_JPG, Height = PixelHeightAsset.NON_EXISTENT_FILE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_FILE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_FILE_JPG }
                },
                Hash = string.Empty
            };

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _application!.MoveAssets([asset], destinationFolder, preserveOriginalFile));
            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{sourceFilePath}'."));
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
