using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Domain.Services.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private FindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private TestableAssetRepository? _testableAssetRepository;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        _findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService, userConfigurationService,
            new TestLogger<FindDuplicatedAssetsService>());

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("876283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_1_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("886283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithRotation(ImageRotation.Rotate90)
            .WithPixels(PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("886283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_1_DUPLICATE_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
                ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG)
            .WithFileProperties(FileSize.IMAGE_1_DUPLICATE_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_1_DUPLICATE_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("886283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_9_DUPLICATE_PNG)
            .WithRotation(ImageRotation.Rotate90)
            .WithPixels(PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, PixelHeightAsset.IMAGE_9_DUPLICATE_PNG,
                ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG)
            .WithFileProperties(FileSize.IMAGE_9_DUPLICATE_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_DUPLICATE_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("886283c6-780e-4ad5-975c-be63044c087a"))
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(true, "The asset is corrupted")
            .WithRotated(true, "The asset has been rotated")
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        string sourcePath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
        string sourcePath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = sourcePath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = sourcePath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset2 = _asset2!.WithFolder(folder2);
        _asset3 = _asset3!.WithFolder(folder2);
        _asset4 = _asset4!.WithFolder(folder2);
        _asset5 = _asset5!.WithFolder(folder2);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);
        _testableAssetRepository.AddAsset(_asset3!, assetData1);
        _testableAssetRepository.AddAsset(_asset4!, assetData2);
        _testableAssetRepository.AddAsset(_asset5!, assetData1);

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

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        const string hash = Hashes.IMAGE_9_PNG;
        string sourcePath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

        _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset2!, assetData);
        _testableAssetRepository.AddAsset(_asset3!, assetData);
        _testableAssetRepository.AddAsset(_asset4!, assetData);
        _testableAssetRepository.AddAsset(_asset5!, assetData);

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

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_2);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset3 = _asset3!.WithFolder(folder2);

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1!, assetData);
        _testableAssetRepository.AddAsset(_asset3!, assetData);

        List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_1);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER_2);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset2 = _asset2!.WithFolder(folder2);
        _asset3 = _asset3!.WithFolder(folder2);
        _asset4 = _asset4!.WithFolder(folder2);
        _asset5 = _asset5!.WithFolder(folder2);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);
        _testableAssetRepository.AddAsset(_asset3!, assetData1);
        _testableAssetRepository.AddAsset(_asset4!, assetData2);
        _testableAssetRepository.AddAsset(_asset5!, assetData1);

        List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

        _asset1 = _asset1!.WithFolder(folder);
        _asset2 = _asset2!.WithFolder(folder);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);

        List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }
}
