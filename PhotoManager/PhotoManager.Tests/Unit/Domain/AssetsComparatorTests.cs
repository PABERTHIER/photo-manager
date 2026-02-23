using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using FileSize = PhotoManager.Tests.Unit.Constants.FileSize;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

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
            FolderId = new("010233a2-8ea6-4cb0-86e4-156fef7cd772"),
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new() { Size = FileSize.IMAGE_1_JPG },
            ThumbnailCreationDateTime = _oldDateTime1,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = new("010233a2-8ea6-4cb0-86e4-156fef7cd772"),
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = FileNames.IMAGE_9_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new() { Size = FileSize.IMAGE_9_PNG },
            ThumbnailCreationDateTime = _oldDateTime2,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = FileNames.HOMER_GIF,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
            },
            FileProperties = new() { Size = FileSize.HOMER_GIF },
            ThumbnailCreationDateTime = _oldDateTime1,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.HOMER_GIF,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
            },
            FileProperties = new() { Size = FileSize.IMAGE_11_HEIC },
            ThumbnailCreationDateTime = _oldDateTime2,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [Test]
    public void GetNewFileNames_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file6.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file7.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file8.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file9.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNames_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file6.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file7.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file8.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file9.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file2.png", "file3.gif", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNames_NoNewFiles_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.That(newFileNames, Is.Empty);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = [];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.That(newFileNames, Is.Empty);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _assetsComparator!.GetNewFileNames(fileNames!, cataloguedAssets));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'first')"));
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets = [];

        string[] newFileNames = _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _assetsComparator!.GetNewFileNames(fileNames, cataloguedAssets!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    public void GetNewFileNamesToSync_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file6.jpg", "file7.png", "file8.gif", "file9.heic"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNamesToSync_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames =
            ["file1.jpg", "file4.heic", "file6.jpg", "file7.png", "file8.gif", "file9.heic"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file2.png", "file3.gif", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNamesToSync_NoNewFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(newFileNames, Is.Empty);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = [];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(newFileNames, Is.Empty);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetNewFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'first')"));
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = [];

        string[] newFileNames = _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(newFileNames, Is.Not.Empty);
        Assert.That(newFileNames, Is.EquivalentTo(["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"]));
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'second')"));
    }

    [Test]
    [TestCase(new[] { "image1.jpg", "video1.mp4", "file2.txt", "image2.png", "image3.gif", "image4.heic" },
        new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "file2.txt", "image2.png", "image1.jpg", "video1.mp4", "image3.gif", "image4.heic" },
        new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "image1.jpg", "video1.mp4", "image2.png", "image3.gif", "image4.heic" },
        new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new[] { "video1.mp4" })]
    [TestCase(new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" },
        new[] { "image1.jpg", "image2.png", "image3.gif", "image4.heic" }, new string[] { })]
    [TestCase(new[] { "video1.mp4", "video2.mov" }, new string[] { }, new[] { "video1.mp4", "video2.mov" })]
    [TestCase(new[] { "file1.txt", "file2.doc", "file3.pdf" }, new string[] { }, new string[] { })]
    [TestCase(new string[] { }, new string[] { }, new string[] { })]
    public void GetImageAndVideoNames_ReturnsImageAndVideoNamesArray(string[] fileNames, string[] expectedImageNames,
        string[] expectedVideoNames)
    {
        (string[] imageNames, string[] videoNames) = _assetsComparator!.GetImageAndVideoNames(fileNames);

        Assert.That(imageNames, Is.EquivalentTo(expectedImageNames));
        Assert.That(videoNames, Is.EquivalentTo(expectedVideoNames));
    }

    [Test]
    public void GetImageAndVideoNames_FileNamesIsNull_ThrowsNullReferenceException()
    {
        string[]? fileNames = null;

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _assetsComparator!.GetImageAndVideoNames(fileNames!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void
        GetUpdatedFileNames_ThumbnailCreationDateTimeBeforeFileCreationOrModificationDateTime_ReturnsArrayOfNamesOfAssetsUpdated()
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

        Assert.That(updatedFileNames, Is.Not.Empty);
        Assert.That(updatedFileNames, Is.EquivalentTo(expectedFileNames));
    }

    [Test]
    public void
        GetUpdatedFileNames_ThumbnailCreationDateTimeIsSameAsFileCreationOrModificationDateTime_ReturnsEmptyArray()
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

        Assert.That(updatedFileNames, Is.Empty);
    }

    [Test]
    public void GetUpdatedFileNames_ThumbnailCreationDateTimeAfterFileCreationOrModificationDateTime_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.That(updatedFileNames, Is.Empty);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        List<Asset> cataloguedAssets = [];

        string[] updatedFileNames = _assetsComparator!.GetUpdatedFileNames(cataloguedAssets);

        Assert.That(updatedFileNames, Is.Empty);
    }

    [Test]
    public void GetUpdatedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _assetsComparator!.GetUpdatedFileNames(cataloguedAssets!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    public void GetDeletedFileNames_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file6.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file7.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file8.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file9.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo(["file6.jpg", "file7.png", "file8.gif", "file9.heic"]));
    }

    [Test]
    public void GetDeletedFileNames_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file6.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file7.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file8.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file9.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo(["file6.jpg", "file7.png", "file8.gif", "file9.heic"]));
    }

    [Test]
    public void GetDeletedFileNames_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.That(deletedFileNames, Is.Empty);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = [];
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo([
            "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"
        ]));
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets =
        [
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file2.png",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file3.gif",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file4.heic",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            },
            new()
            {
                FolderId = Guid.Empty,
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "file5.mp4",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            }
        ];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetDeletedFileNames(fileNames!, cataloguedAssets));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'second')"));
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset> cataloguedAssets = [];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.That(deletedFileNames, Is.Empty);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        List<Asset>? cataloguedAssets = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetDeletedFileNames(fileNames, cataloguedAssets!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    public void GetDeletedFileNamesToSync_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo([
            "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"
        ]));
    }

    [Test]
    public void GetDeletedFileNamesToSync_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames =
            ["file1.jpg", "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo([
            "file6.jpg", "file7.png", "file8.gif", "file9.heic", "file10.mp4"
        ]));
    }

    [Test]
    public void GetDeletedFileNamesToSync_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(deletedFileNames, Is.Empty);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = [];
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(deletedFileNames, Is.Not.Empty);
        Assert.That(deletedFileNames, Is.EquivalentTo([
            "file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"
        ]));
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4"];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames!, destinationFileNames));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'second')"));
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[] destinationFileNames = [];

        string[] deletedFileNames = _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.That(deletedFileNames, Is.Empty);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames =
            ["file1.jpg", "file2.png", "file3.gif", "file4.heic", "file5.mp4", "toto.txt", "tutu.bat"];
        string[]? destinationFileNames = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _assetsComparator!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'first')"));
    }
}
