namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class AssetsComparatorTests
{
    private AssetsComparator? _assetsComparator;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    private readonly DateTime _oldDateTime1 = DateTime.Now.AddDays(-1);
    private readonly DateTime _oldDateTime2 = DateTime.Now.AddDays(-2);
    private readonly DateTime _oldDateTime3 = DateTime.Now.AddDays(-3);

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
            Folder = new() { Path = "" },
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new() { Size = 363888 },
            ThumbnailCreationDateTime = _oldDateTime1,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset2 = new()
        {
            FolderId = new Guid("010233a2-8ea6-4cb0-86e4-156fef7cd772"),
            Folder = new() { Path = "" },
            FileName = "Image 9.png",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 700 },
                Thumbnail = new() { Width = 147, Height = 150 }
            },
            FileProperties = new() { Size = 4602393 },
            ThumbnailCreationDateTime = _oldDateTime2,
            Hash = "f8d5cf6deda198be0f181dd7cabfe74cb14c43426c867f0ae855d9e844651e2d7ce4833c178912d5bc7be600cfdd18d5ba19f45988a0c6943b4476a90295e960",
            AssetCorruptedMessage = null,
            IsAssetCorrupted = false,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };
        _asset3 = new()
        {
            Folder = new() { Path = "" },
            FileName = "Homer.gif",
            Pixel = new()
            {
                Asset = new() { Width = 320, Height = 320 },
                Thumbnail = new() { Width = 150, Height = 150 }
            },
            FileProperties = new() { Size = 64123 },
            ThumbnailCreationDateTime = _oldDateTime1,
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4 = new()
        {
            Folder = new() { Path = "" },
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new() { Size = 1411940 },
            ThumbnailCreationDateTime = _oldDateTime2,
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
            new() { Folder = new() { Path = "" }, FileName = "file6.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file7.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file8.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file9.heic", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file6.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file7.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file8.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file9.heic", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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

        _asset1.FileProperties = new()
        {
            Creation = DateTime.Now,
            Modification = _oldDateTime2
        };
        _asset2.FileProperties = new()
        {
            Creation = _oldDateTime3,
            Modification = DateTime.Now
        };
        _asset3.FileProperties = new()
        {
            Creation = DateTime.Now,
            Modification = _oldDateTime2
        };
        _asset4.FileProperties = new()
        {
            Creation = _oldDateTime3,
            Modification = DateTime.Now
        };

        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsNotEmpty(updatedFileNames);
        CollectionAssert.AreEquivalent(expectedFileNames, updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeIsSameAsFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        _asset1!.FileProperties = new()
        {
            Creation = _oldDateTime2,
            Modification = _oldDateTime1
        };
        _asset2!.FileProperties = new()
        {
            Creation = _oldDateTime2,
            Modification = _oldDateTime3
        };
        _asset3!.FileProperties = new()
        {
            Creation = _oldDateTime1,
            Modification = _oldDateTime2
        };
        _asset4!.FileProperties = new()
        {
            Creation = _oldDateTime3,
            Modification = _oldDateTime2
        };

        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeAfterFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
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
            new() { Folder = new() { Path = "" }, FileName = "file6.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file7.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file8.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file9.heic", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file6.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file7.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file8.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file9.heic", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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
            new() { Folder = new() { Path = "" }, FileName = "file1.jpg", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file2.png", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file3.gif", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file4.heic", Hash = string.Empty },
            new() { Folder = new() { Path = "" }, FileName = "file5.mp4", Hash = string.Empty }
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
