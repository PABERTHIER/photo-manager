using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class DirectoryComparerTests
{
    private string? dataDirectory;

    private IStorageService? _storageService;
    private IDirectoryComparer? _directoryComparer;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? asset1;
    private Asset? asset2;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _storageService = new StorageService(userConfigurationService);
        _directoryComparer = new DirectoryComparer(_storageService);
    }

    [SetUp]
    public void SetUp()
    {
        asset1 = new()
        {
            FolderId = new Guid("010233a2-8ea6-4cb0-86e4-156fef7cd772"),
            FileName = "Image 1.jpg",
            FileSize = 363888,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        asset2 = new()
        {
            FolderId = new Guid("010233a2-8ea6-4cb0-86e4-156fef7cd772"),
            FileName = "Image 9.png",
            FileSize = 4602393,
            ImageRotation = Rotation.Rotate90,
            PixelWidth = 1280,
            PixelHeight = 700,
            ThumbnailPixelWidth = 147,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeBeforeFileCreationOrModificationDateTime_ReturnsArrayOfNamesOfAssetsUpdated()
    {
        string[] expectedFileNames = { asset1!.FileName, asset2!.FileName };

        Folder folder = new() { Path = dataDirectory! };
        asset1!.Folder = folder;
        asset2!.Folder = folder;

        asset1.ThumbnailCreationDateTime = DateTime.Now.AddDays(-1);
        asset2.ThumbnailCreationDateTime = DateTime.Now.AddDays(-1);

        List<Asset> cataloguedAssets = new() { asset1!, asset2! };

        string[] updatedFileNames = _directoryComparer!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsNotEmpty(updatedFileNames);
        CollectionAssert.AreEquivalent(expectedFileNames, updatedFileNames);

        DateTime actualDate = DateTime.Now.Date;
        DateTime fileModificationDate = new DateTime(2023, 1, 7).Date;

        Assert.AreEqual(actualDate, asset1.FileCreationDateTime.Date);
        Assert.AreEqual(fileModificationDate, asset1.FileModificationDateTime.Date);

        Assert.AreEqual(actualDate, asset2.FileCreationDateTime.Date);
        Assert.AreEqual(fileModificationDate, asset2.FileModificationDateTime.Date);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeIsSameAsFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        Folder folder = new() { Path = dataDirectory! };
        asset1!.Folder = folder;
        asset2!.Folder = folder;

        List<Asset> cataloguedAssets = new() { asset1!, asset2! };

        string[] updatedFileNames = _directoryComparer!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeAfterFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        Folder folder = new() { Path = dataDirectory! };
        asset1!.Folder = folder;
        asset2!.Folder = folder;

        asset1.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);
        asset2.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);

        List<Asset> cataloguedAssets = new() { asset1!, asset2! };

        string[] updatedFileNames = _directoryComparer!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = new();

        string[] updatedFileNames = _directoryComparer!.GetUpdatedFileNames(cataloguedAssets!);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsNull_ThrowsNullReferenceException()
    {
        List<Asset>? cataloguedAssets = null;

        Assert.Throws<NullReferenceException>(() => _directoryComparer!.GetUpdatedFileNames(cataloguedAssets!));
    }
}
