namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class AssetsComparatorTests
{
    private AssetsComparator? _assetsComparator;

    private Asset? _asset1;
    private Asset? _asset2;

    [OneTimeSetUp]
    public void OneTimeSetup()
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
    }

    [Test]
    public void GetNewFileNames_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_NoNewFiles_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = Array.Empty<string>();
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNames(fileNames!, cataloguedAssets));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new();

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void GetNewFileNamesToSync_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file4.jpg", "file5.png" };

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file4.jpg", "file5.png" };

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_NoNewFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = Array.Empty<string>();
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = Array.Empty<string>();

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    [TestCase(new string[] { "image1.jpg", "video1.mp4", "file2.txt", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "file2.txt", "image2.png", "image1.jpg", "video1.mp4" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "image1.jpg", "video1.mp4", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "image1.jpg", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { })]
    [TestCase(new string[] { "video1.mp4", "video2.mov" }, new string[] { }, new string[] { "video1.mp4", "video2.mov" })]
    [TestCase(new string[] { "file1.txt", "file2.doc", "file3.pdf" }, new string[] { }, new string[] { })]
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
        string[] expectedFileNames = [_asset1!.FileName, _asset2!.FileName];

        DateTime oldDateTime1 = DateTime.Now.AddDays(-1);
        DateTime oldDateTime2 = DateTime.Now.AddDays(-2);

        _asset1.ThumbnailCreationDateTime = oldDateTime1;
        _asset2.ThumbnailCreationDateTime = oldDateTime2;

        _asset1.FileCreationDateTime = DateTime.Now;
        _asset1.FileModificationDateTime = DateTime.Now.AddDays(-2);
        _asset2.FileCreationDateTime = DateTime.Now.AddDays(-3);
        _asset2.FileModificationDateTime = DateTime.Now;

        List<Asset> cataloguedAssets = [_asset1!, _asset2!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsNotEmpty(updatedFileNames);
        CollectionAssert.AreEquivalent(expectedFileNames, updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeIsSameAsFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = [_asset1!, _asset2!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.IsEmpty(updatedFileNames);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeAfterFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        _asset1!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);
        _asset2!.ThumbnailCreationDateTime = DateTime.Now.AddDays(1);

        List<Asset> cataloguedAssets = [_asset1!, _asset2!];

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
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = Array.Empty<string>();
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNames(fileNames!, cataloguedAssets));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new();

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNamesToSync_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file4.jpg", "file5.png" };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file4.jpg", "file5.png" };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = Array.Empty<string>();
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.AreEqual("Value cannot be null. (Parameter 'second')", exception?.Message);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = Array.Empty<string>();

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.AreEqual("Value cannot be null. (Parameter 'first')", exception?.Message);
    }
}
