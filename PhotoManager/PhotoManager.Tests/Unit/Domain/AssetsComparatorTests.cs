namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class AssetsComparatorTests
{
    private AssetsComparator? _assetsComparator;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsComparator = new();
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
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
        _asset2 = new()
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
        _asset3 = new()
        {
            FileName = "Homer.gif",
            FileSize = 64123,
            PixelHeight = 320,
            PixelWidth = 320,
            ThumbnailPixelWidth = 150,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4 = new()
        {
            FileName = "Image_11.heic",
            FileSize = 1411940,
            PixelHeight = 4032,
            PixelWidth = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
    }

    [Test]
    public void GetNewFileNames_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file6.jpg" },
            new Asset { FileName = "file7.png" },
            new Asset { FileName = "file8.gif" },
            new Asset { FileName = "file9.heic" }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file6.jpg" },
            new Asset { FileName = "file7.png" },
            new Asset { FileName = "file8.gif" },
            new Asset { FileName = "file9.heic" }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file2.png", "file3.gif", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_NoNewFiles_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = [];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNames(fileNames!, cataloguedAssets));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets = [];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void GetNewFileNamesToSync_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file6.jpg", "file7.png", "file8.gif", "file9.heic"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file4.heic", "file6.jpg", "file7.png", "file8.gif", "file9.heic"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file2.png", "file3.gif", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_NoNewFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = [];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = [];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    [TestCase(new[] { "image1.jpg", "video1.mp4", "file2.txt", "image2.png", "image3.gif", "image4.heic" }, new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "file2.txt", "image2.png", "image1.jpg", "video1.mp4", "image3.gif", "image4.heic" }, new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "image1.jpg", "video1.mp4", "image2.png", "image3.gif", "image4.heic" }, new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new string[] { })]
    [TestCase(new[] { "video1.mp4", "video2.mov" }, new string[] { }, new[] { "video1.mp4", "video2.mov" })]
    [TestCase(new[] { "file1.txt", "file2.doc", "file3.pdf" }, new string[] { }, new string[] { })]
    [TestCase(new string[] { }, new string[] { }, new string[] { })]
    public void GetImageAndVideoNames_ReturnsImageAndVideoNamesArray(string[] fileNames, string[] expectedImageNames, string[] expectedVideoNames)
    {
        (string[] imageNames, string[] videoNames) = _assetsComparator!.GetImageAndVideoNames(fileNames);

        CollectionAssert.AreEquivalent(expectedImageNames, imageNames);
        CollectionAssert.AreEquivalent(expectedVideoNames, videoNames);
    }

    [Test]
    public void GetImageAndVideoNames_FileNamesIsNull_ThrowsNullReferenceException()
    {
        string[]? fileNames = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _assetsComparator!.GetImageAndVideoNames(fileNames!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeBeforeFileCreationOrModificationDateTime_ReturnsArrayOfNamesOfAssetsUpdated()
    {
        string[] expectedFileNames = [_asset1!.FileName, _asset2!.FileName, _asset3!.FileName, _asset4!.FileName];

        DateTime oldDateTime1 = DateTime.Now.AddDays(-1);
        DateTime oldDateTime2 = DateTime.Now.AddDays(-2);

        _asset1.ThumbnailCreationDateTime = oldDateTime1;
        _asset2.ThumbnailCreationDateTime = oldDateTime2;
        _asset3.ThumbnailCreationDateTime = oldDateTime1;
        _asset4.ThumbnailCreationDateTime = oldDateTime2;

        _asset1.FileCreationDateTime = DateTime.Now;
        _asset1.FileModificationDateTime = DateTime.Now.AddDays(-2);
        _asset2.FileCreationDateTime = DateTime.Now.AddDays(-3);
        _asset2.FileModificationDateTime = DateTime.Now;
        _asset3.FileCreationDateTime = DateTime.Now;
        _asset3.FileModificationDateTime = DateTime.Now.AddDays(-2);
        _asset4.FileCreationDateTime = DateTime.Now.AddDays(-3);
        _asset4.FileModificationDateTime = DateTime.Now;

        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsNotEmpty(updatedFileNames);
        CollectionAssert.AreEquivalent(expectedFileNames, updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeIsSameAsFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeAfterFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        _asset1!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);
        _asset2!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);
        _asset3!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);
        _asset4!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);

        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = [];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetUpdatedFileNames(cataloguedAssets!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNames_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file6.jpg" },
            new Asset { FileName = "file7.png" },
            new Asset { FileName = "file8.gif" },
            new Asset { FileName = "file9.heic" }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file6.jpg", "file7.png", "file8.gif", "file9.heic" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file6.jpg" },
            new Asset { FileName = "file7.png" },
            new Asset { FileName = "file8.gif" },
            new Asset { FileName = "file9.heic" }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file6.jpg", "file7.png", "file8.gif", "file9.heic" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = [];
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets =
        [
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.gif" },
            new Asset { FileName = "file4.heic" },
            new Asset { FileName = "file5.mp4" }
        ];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNames(fileNames!, cataloguedAssets));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets = [];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNamesToSync_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = [];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new[] { "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = [];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }
}
