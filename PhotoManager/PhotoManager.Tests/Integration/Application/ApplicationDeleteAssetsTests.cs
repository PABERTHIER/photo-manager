﻿namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationDeleteAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private MoveAssetsService? _moveAssetsService;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
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

        _userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        _moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, _userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, _moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
    }

    [Test]
    public async Task DeleteAssets_AssetsAreValid_DeletesAssetsAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

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

            await _application!.CatalogAssetsAsync(_ => {});

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset1FileName));

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2FileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1FileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(2));
            Assert.That(assetsInDb[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInDb[1].FileName, Is.EqualTo(asset1FileName));

            _application!.DeleteAssets([assetsInRepository[0], assetsInRepository[1]]);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset2FileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, asset1FileName), Is.False);

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
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task DeleteAssets_AssetIsValid_DeletesAssetAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            const string assetFileName = "Image 1.jpg";

            string sourceFilePath2 = Path.Combine(_dataDirectory!, assetFileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, assetFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            await _application!.CatalogAssetsAsync(_ => {});

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(assetFileName));

            Folder? sourceFolder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetFileName), Is.True);

            List<Asset> assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Not.Empty);
            Assert.That(assetsInDb, Has.Count.EqualTo(1));
            Assert.That(assetsInDb[0].FileName, Is.EqualTo(assetFileName));

            _application!.DeleteAssets([assetsInRepository[0]]);

            Assert.That(_assetRepository!.ContainsThumbnail(sourceFolder.Path, assetFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Assert.That(File.Exists(destinationFilePath2), Is.False);

            assetsInDb = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
            Assert.That(assetsInDb, Is.Empty);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsIsNull_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Asset[]? assets = null;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets!));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsIsEmpty_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            Asset[] assets = [];

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteAssets_OneAssetIsNull_ThrowsArgumentNullException()
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

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo("asset cannot be null. (Parameter 'asset')"));
            Assert.That(exception?.ParamName, Is.EqualTo("asset"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteAssets_OneAssetFolderIsNull_ThrowsArgumentNullException()
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

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo("asset.Folder cannot be null. (Parameter 'Folder')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(Folder)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetDoesNotExists_ThrowsFileNotFoundException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

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

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _application!.DeleteAssets(assets));

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{asset.FullPath}'."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
