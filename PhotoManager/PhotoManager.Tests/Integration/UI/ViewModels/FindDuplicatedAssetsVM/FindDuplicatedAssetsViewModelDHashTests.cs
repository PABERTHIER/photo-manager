using System.ComponentModel;

namespace PhotoManager.Tests.Integration.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelDHashTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;
    private Asset _asset5;
    private Asset _asset6;
    private Asset _asset7;
    private Asset _asset8;
    private Asset _asset9;
    private Asset _asset10;
    private Asset _asset11;
    private Asset _asset12;
    private Asset _asset13;
    private Asset _asset14;
    private Asset _asset15;
    private Asset _asset16;
    private Asset _asset17;
    private Asset _asset18;
    private Asset _asset19;
    private Asset _asset20;
    private Asset _asset21;
    private Asset _asset22;
    private Asset _asset23;
    private Asset _asset24;
    private Asset _asset25;
    private Asset _asset26;
    private Asset _asset27;
    private Asset _asset28;
    private Asset _asset29;
    private Asset _asset30;
    private Asset _asset31;
    private Asset _asset32;
    private Asset _asset33;
    private Asset _asset34;
    private Asset _asset35;
    private Asset _asset36;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Homer.gif",
            Pixel = new()
            {
                Asset = new() { Width = 320, Height = 320 },
                Thumbnail = new() { Width = 150, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 64123,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_180_deg.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 31535,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate180,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_270_deg.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 720, Height = 1280 },
                Thumbnail = new() { Width = 84, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 31535,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate270,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        _asset5 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_90_deg.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 720, Height = 1280 },
                Thumbnail = new() { Width = 84, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 31535,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate90,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        _asset6 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 2 duplicated.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30197,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset7 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 2.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30197,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset8 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 3.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30276,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset9 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 4.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30115,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset10 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 5.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30117,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset11 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 6.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30366,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset12 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 7.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29974,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset13 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 8.jpeg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 30514,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset14 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "9346989754624",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset15 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset16 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11_270.heic",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1423616,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate270,
            Hash = "0",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
        _asset17 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "IMAGE_WITH_UPPERCASE_NAME.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 40937,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset18 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset19 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "23831507941108244",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset20 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "9346989754624",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset21 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9_duplicate.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "9346989754624",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset22 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset23 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4526710,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1090783378",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset24 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1337.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4003090,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset25 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_BottomLeftPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1119873,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "27162885008392837",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset26 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_BottomPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 75 }
            },
            FileProperties = new()
            {
                Size = 2172866,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "27162885008392837",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset27 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_LeftPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 3024 },
                Thumbnail = new() { Width = 100, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1475892,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "9079767575972116",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset28 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_Original.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4526710,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1090783378",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset29 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_RightPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 3024 },
                Thumbnail = new() { Width = 100, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1437903,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "6879371593265376",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset30 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_TopPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 75 }
            },
            FileProperties = new()
            {
                Size = 737269,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "9079767575972116",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset31 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_TopRightPart.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 2016, Height = 1512 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 383508,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "6879371593265376",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset32 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "1336_4K_Original.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4526710,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1090783378",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset33 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_Original.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 4526710,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1090783378",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset34 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_ShitQuality.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 4032, Height = 3024 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 370819,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset35 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_1336_Small.JPG",
            Pixel = new()
            {
                Asset = new() { Width = 591, Height = 443 },
                Thumbnail = new() { Width = 200, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 53227,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset36 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Homer.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 320, Height = 180 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 6599,
                Creation = actualDate,
                Modification = actualDate
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "0",
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureFindDuplicatedAssetsViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, storageServiceMock.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, _userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (_application);
        _findDuplicatedAssetsViewModel = new (_application);
    }

    [Test]
    public async Task SetDuplicates_CataloguedAssetsAndDHashTypeAndAllDuplicatesSets_SetsDuplicates()
    {
        string rootDirectory = Path.Combine(_dataDirectory!);
        string duplicatesDirectory = Path.Combine(rootDirectory, "Duplicates");
        string directoryNewFolder1 = Path.Combine(duplicatesDirectory, "NewFolder1");
        string directoryNewFolder2 = Path.Combine(duplicatesDirectory, "NewFolder2");
        string directorySample1 = Path.Combine(duplicatesDirectory, "NotDuplicate", "Sample1");
        string directoryPart = Path.Combine(duplicatesDirectory, "Part");
        string directoryResolution = Path.Combine(duplicatesDirectory, "Resolution");
        string directoryThumbnail = Path.Combine(duplicatesDirectory, "Thumbnail");

        ConfigureFindDuplicatedAssetsViewModel(100, rootDirectory, 200, 150, true, false, false, true);

        string directoryOutputVideoFirstFrame = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(7));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(8));
            Assert.That(duplicatedAssetsSets[1], Has.Count.EqualTo(15));
            Assert.That(duplicatedAssetsSets[2], Has.Count.EqualTo(3));
            Assert.That(duplicatedAssetsSets[3], Has.Count.EqualTo(4));
            Assert.That(duplicatedAssetsSets[4], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[5], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[6], Has.Count.EqualTo(2));

            // First set
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset15.FileName));
            Assert.That(duplicatedAssetsSets[0][2].FileName, Is.EqualTo(_asset16.FileName));
            Assert.That(duplicatedAssetsSets[0][3].FileName, Is.EqualTo(_asset22.FileName));
            Assert.That(duplicatedAssetsSets[0][4].FileName, Is.EqualTo(_asset24.FileName));
            Assert.That(duplicatedAssetsSets[0][5].FileName, Is.EqualTo(_asset34.FileName));
            Assert.That(duplicatedAssetsSets[0][6].FileName, Is.EqualTo(_asset35.FileName));
            Assert.That(duplicatedAssetsSets[0][7].FileName, Is.EqualTo(_asset36.FileName));

            // Second set
            Assert.That(duplicatedAssetsSets[1][0].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(duplicatedAssetsSets[1][1].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(duplicatedAssetsSets[1][2].FileName, Is.EqualTo(_asset4.FileName));
            Assert.That(duplicatedAssetsSets[1][3].FileName, Is.EqualTo(_asset5.FileName));
            Assert.That(duplicatedAssetsSets[1][4].FileName, Is.EqualTo(_asset6.FileName));
            Assert.That(duplicatedAssetsSets[1][5].FileName, Is.EqualTo(_asset7.FileName));
            Assert.That(duplicatedAssetsSets[1][6].FileName, Is.EqualTo(_asset8.FileName));
            Assert.That(duplicatedAssetsSets[1][7].FileName, Is.EqualTo(_asset9.FileName));
            Assert.That(duplicatedAssetsSets[1][8].FileName, Is.EqualTo(_asset10.FileName));
            Assert.That(duplicatedAssetsSets[1][9].FileName, Is.EqualTo(_asset11.FileName));
            Assert.That(duplicatedAssetsSets[1][10].FileName, Is.EqualTo(_asset12.FileName));
            Assert.That(duplicatedAssetsSets[1][11].FileName, Is.EqualTo(_asset13.FileName));
            Assert.That(duplicatedAssetsSets[1][12].FileName, Is.EqualTo(_asset17.FileName));
            Assert.That(duplicatedAssetsSets[1][13].FileName, Is.EqualTo(_asset18.FileName));
            Assert.That(duplicatedAssetsSets[1][14].FileName, Is.EqualTo(_asset19.FileName));

            // Third set
            Assert.That(duplicatedAssetsSets[2][0].FileName, Is.EqualTo(_asset14.FileName));
            Assert.That(duplicatedAssetsSets[2][1].FileName, Is.EqualTo(_asset20.FileName));
            Assert.That(duplicatedAssetsSets[2][2].FileName, Is.EqualTo(_asset21.FileName));

            // Fourth set
            Assert.That(duplicatedAssetsSets[3][0].FileName, Is.EqualTo(_asset23.FileName));
            Assert.That(duplicatedAssetsSets[3][1].FileName, Is.EqualTo(_asset28.FileName));
            Assert.That(duplicatedAssetsSets[3][2].FileName, Is.EqualTo(_asset32.FileName));
            Assert.That(duplicatedAssetsSets[3][3].FileName, Is.EqualTo(_asset33.FileName));

            // Fifth set
            Assert.That(duplicatedAssetsSets[4][0].FileName, Is.EqualTo(_asset25.FileName));
            Assert.That(duplicatedAssetsSets[4][1].FileName, Is.EqualTo(_asset26.FileName));

            // Sixth set
            Assert.That(duplicatedAssetsSets[5][0].FileName, Is.EqualTo(_asset27.FileName));
            Assert.That(duplicatedAssetsSets[5][1].FileName, Is.EqualTo(_asset30.FileName));

            // Seventh set
            Assert.That(duplicatedAssetsSets[6][0].FileName, Is.EqualTo(_asset29.FileName));
            Assert.That(duplicatedAssetsSets[6][1].FileName, Is.EqualTo(_asset31.FileName));

            Folder? folder1 = _assetRepository!.GetFolderByPath(rootDirectory);
            Folder? folder2 = _assetRepository!.GetFolderByPath(directoryNewFolder1);
            Folder? folder3 = _assetRepository!.GetFolderByPath(directoryNewFolder2);
            Folder? folder4 = _assetRepository!.GetFolderByPath(directorySample1);
            Folder? folder5 = _assetRepository!.GetFolderByPath(directoryPart);
            Folder? folder6 = _assetRepository!.GetFolderByPath(directoryResolution);
            Folder? folder7 = _assetRepository!.GetFolderByPath(directoryThumbnail);
            Folder? folder8 = _assetRepository!.GetFolderByPath(directoryOutputVideoFirstFrame);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder4, Is.Not.Null);
            Assert.That(folder5, Is.Not.Null);
            Assert.That(folder6, Is.Not.Null);
            Assert.That(folder7, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder1!);
            _asset2 = _asset2.WithFolder(folder1!);
            _asset3 = _asset3.WithFolder(folder1!);
            _asset4 = _asset4.WithFolder(folder1!);
            _asset5 = _asset5.WithFolder(folder1!);
            _asset6 = _asset6.WithFolder(folder1!);
            _asset7 = _asset7.WithFolder(folder1!);
            _asset8 = _asset8.WithFolder(folder1!);
            _asset9 = _asset9.WithFolder(folder1!);
            _asset10 = _asset10.WithFolder(folder1!);
            _asset11 = _asset11.WithFolder(folder1!);
            _asset12 = _asset12.WithFolder(folder1!);
            _asset13 = _asset13.WithFolder(folder1!);
            _asset14 = _asset14.WithFolder(folder1!);
            _asset15 = _asset15.WithFolder(folder1!);
            _asset16 = _asset16.WithFolder(folder1!);
            _asset17 = _asset17.WithFolder(folder1!);

            _asset18 = _asset18.WithFolder(folder2!);

            _asset19 = _asset19.WithFolder(folder3!);
            _asset20 = _asset20.WithFolder(folder3!);
            _asset21 = _asset21.WithFolder(folder3!);
            _asset22 = _asset22.WithFolder(folder3!);

            _asset23 = _asset23.WithFolder(folder4!);
            _asset24 = _asset24.WithFolder(folder4!);

            _asset25 = _asset25.WithFolder(folder5!);
            _asset26 = _asset26.WithFolder(folder5!);
            _asset27 = _asset27.WithFolder(folder5!);
            _asset28 = _asset28.WithFolder(folder5!);
            _asset29 = _asset29.WithFolder(folder5!);
            _asset30 = _asset30.WithFolder(folder5!);
            _asset31 = _asset31.WithFolder(folder5!);

            _asset32 = _asset32.WithFolder(folder6!);

            _asset33 = _asset33.WithFolder(folder7!);
            _asset34 = _asset34.WithFolder(folder7!);
            _asset35 = _asset35.WithFolder(folder7!);

            _asset36 = _asset36.WithFolder(folder8!);

            DuplicatedSetViewModel duplicatedAssetSet1 = [];
            DuplicatedSetViewModel duplicatedAssetSet2 = [];
            DuplicatedSetViewModel duplicatedAssetSet3 = [];
            DuplicatedSetViewModel duplicatedAssetSet4 = [];
            DuplicatedSetViewModel duplicatedAssetSet5 = [];
            DuplicatedSetViewModel duplicatedAssetSet6 = [];
            DuplicatedSetViewModel duplicatedAssetSet7 = [];

            // First set
            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset15,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            DuplicatedAssetViewModel duplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset16,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel3);

            DuplicatedAssetViewModel duplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset22,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel4);

            DuplicatedAssetViewModel duplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset24,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel5);

            DuplicatedAssetViewModel duplicatedAssetViewModel6 = new (_application!)
            {
                Asset = _asset34,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel6);

            DuplicatedAssetViewModel duplicatedAssetViewModel7 = new (_application!)
            {
                Asset = _asset35,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel7);

            DuplicatedAssetViewModel duplicatedAssetViewModel8 = new (_application!)
            {
                Asset = _asset36,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel8);

            // Second set
            DuplicatedAssetViewModel duplicatedAssetViewModel9 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel9);

            DuplicatedAssetViewModel duplicatedAssetViewModel10 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel10);

            DuplicatedAssetViewModel duplicatedAssetViewModel11 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel11);

            DuplicatedAssetViewModel duplicatedAssetViewModel12 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel12);

            DuplicatedAssetViewModel duplicatedAssetViewModel13 = new (_application!)
            {
                Asset = _asset6,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel13);

            DuplicatedAssetViewModel duplicatedAssetViewModel14 = new (_application!)
            {
                Asset = _asset7,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel14);

            DuplicatedAssetViewModel duplicatedAssetViewModel15 = new (_application!)
            {
                Asset = _asset8,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel15);

            DuplicatedAssetViewModel duplicatedAssetViewModel16 = new (_application!)
            {
                Asset = _asset9,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel16);

            DuplicatedAssetViewModel duplicatedAssetViewModel17 = new (_application!)
            {
                Asset = _asset10,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel17);

            DuplicatedAssetViewModel duplicatedAssetViewModel18 = new (_application!)
            {
                Asset = _asset11,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel18);

            DuplicatedAssetViewModel duplicatedAssetViewModel19 = new (_application!)
            {
                Asset = _asset12,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel19);

            DuplicatedAssetViewModel duplicatedAssetViewModel20 = new (_application!)
            {
                Asset = _asset13,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel20);

            DuplicatedAssetViewModel duplicatedAssetViewModel21 = new (_application!)
            {
                Asset = _asset17,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel21);

            DuplicatedAssetViewModel duplicatedAssetViewModel22 = new (_application!)
            {
                Asset = _asset18,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel22);

            DuplicatedAssetViewModel duplicatedAssetViewModel23 = new (_application!)
            {
                Asset = _asset19,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel23);

            // Third set
            DuplicatedAssetViewModel duplicatedAssetViewModel24 = new (_application!)
            {
                Asset = _asset14,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel24);

            DuplicatedAssetViewModel duplicatedAssetViewModel25 = new (_application!)
            {
                Asset = _asset20,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel25);

            DuplicatedAssetViewModel duplicatedAssetViewModel26 = new (_application!)
            {
                Asset = _asset21,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel26);

            // Fourth set
            DuplicatedAssetViewModel duplicatedAssetViewModel27 = new (_application!)
            {
                Asset = _asset23,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel27);

            DuplicatedAssetViewModel duplicatedAssetViewModel28 = new (_application!)
            {
                Asset = _asset28,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel28);

            DuplicatedAssetViewModel duplicatedAssetViewModel29 = new (_application!)
            {
                Asset = _asset32,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel29);

            DuplicatedAssetViewModel duplicatedAssetViewModel30 = new (_application!)
            {
                Asset = _asset33,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel30);

            // Fifth set
            DuplicatedAssetViewModel duplicatedAssetViewModel31 = new (_application!)
            {
                Asset = _asset25,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel31);

            DuplicatedAssetViewModel duplicatedAssetViewModel32 = new (_application!)
            {
                Asset = _asset26,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel32);

            // Sixth set
            DuplicatedAssetViewModel duplicatedAssetViewModel33 = new (_application!)
            {
                Asset = _asset27,
                ParentViewModel = duplicatedAssetSet6
            };
            duplicatedAssetSet6.Add(duplicatedAssetViewModel33);

            DuplicatedAssetViewModel duplicatedAssetViewModel34 = new (_application!)
            {
                Asset = _asset30,
                ParentViewModel = duplicatedAssetSet6
            };
            duplicatedAssetSet6.Add(duplicatedAssetViewModel34);

            // Seventh set
            DuplicatedAssetViewModel duplicatedAssetViewModel35 = new (_application!)
            {
                Asset = _asset29,
                ParentViewModel = duplicatedAssetSet7
            };
            duplicatedAssetSet7.Add(duplicatedAssetViewModel35);

            DuplicatedAssetViewModel duplicatedAssetViewModel36 = new (_application!)
            {
                Asset = _asset31,
                ParentViewModel = duplicatedAssetSet7
            };
            duplicatedAssetSet7.Add(duplicatedAssetViewModel36);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [
                duplicatedAssetSet1,
                duplicatedAssetSet2,
                duplicatedAssetSet3,
                duplicatedAssetSet4,
                duplicatedAssetSet5,
                duplicatedAssetSet6,
                duplicatedAssetSet7
            ];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(149));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[23], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[27], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[29], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[32], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[33], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[35], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[36], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[38], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[39], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[40], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[41], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[43], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[44], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[45], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[46], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[47], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[49], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[51], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[52], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[53], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[55], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[56], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[57], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[58], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[59], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[61], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[62], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[63], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[64], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[65], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[66], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[67], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[68], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[69], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[70], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[71], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[72], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[73], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[74], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[75], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[76], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[77], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[78], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[79], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[80], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[81], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[82], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[83], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[84], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[85], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[86], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[87], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[88], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[89], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[90], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[91], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[92], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[93], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[94], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[95], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[96], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[97], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[98], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[99], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[100], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[101], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[102], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[103], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[104], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[105], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[106], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[107], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[108], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[109], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[110], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[111], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[112], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[113], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[114], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[115], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[116], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[117], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[118], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[119], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[120], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[121], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[122], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[123], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[124], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[125], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[126], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[127], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[128], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[129], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[130], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[131], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[132], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[133], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[134], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[135], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[136], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[137], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[138], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[139], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[140], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[141], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[142], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[143], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[144], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[145], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[146], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[147], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[148], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            // Rider (debug/coverage mode) seems to hold references longer, causing file locks (frame generated)
            // Forcing GC ensures cleanup before deletion
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(directoryOutputVideoFirstFrame, true);
        }
    }

    private (
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
        List<string> notifyApplicationVmPropertyChangedEvents,
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents = [];
        List<string> notifyApplicationVmPropertyChangedEvents = [];
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances = [];

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyFindDuplicatedAssetsVmPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        _applicationViewModel!.PropertyChanged += delegate(object? _, PropertyChangedEventArgs e)
        {
            notifyApplicationVmPropertyChangedEvents.Add(e.PropertyName!);
        };

        return (notifyFindDuplicatedAssetsVmPropertyChangedEvents, notifyApplicationVmPropertyChangedEvents, findDuplicatedAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset, Is.Null);
    }

    private static void CheckAfterChanges(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        AssertDuplicatedAssetSets(findDuplicatedAssetsViewModelInstance, expectedDuplicatedAssetSets);

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition, Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition, Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet, expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset, expectedCurrentDuplicatedAsset);
    }

    private static void AssertDuplicatedAssetSets(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets)
    {
        if (expectedDuplicatedAssetSets.Count > 0)
        {
            for (int i = 0; i < expectedDuplicatedAssetSets.Count; i++)
            {
                AssertDuplicatedAssetsSet(
                    findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets[i],
                    expectedDuplicatedAssetSets[i]);
            }
        }
        else
        {
            Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets, Is.Empty);
        }
    }

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet, DuplicatedSetViewModel expectedDuplicatedAssetSet)
    {
        if (expectedDuplicatedAssetSet.Count > 0)
        {
            AssertDuplicatedSet(duplicatedAssetSet, expectedDuplicatedAssetSet);

            for (int i = 0; i < expectedDuplicatedAssetSet.Count; i++)
            {
                AssertDuplicatedAsset(duplicatedAssetSet[i], expectedDuplicatedAssetSet[i]);
            }
        }
        else
        {
            Assert.That(duplicatedAssetSet, Is.Empty);
        }
    }

    private static void AssertDuplicatedSet(
        DuplicatedSetViewModel duplicatedSetViewModel,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));

        Assert.That(duplicatedSetViewModel.Visible, Is.EqualTo(expectedDuplicatedSetViewModel.Visible));
    }

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset, DuplicatedAssetViewModel? expectedDuplicatedAsset)
    {
        if (expectedDuplicatedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAsset!.Asset, expectedDuplicatedAsset.Asset);

            Assert.That(duplicatedAsset.Visible, Is.EqualTo(expectedDuplicatedAsset.Visible));

            if (expectedDuplicatedAsset.ParentViewModel.Count > 0)
            {
                AssertDuplicatedSet(duplicatedAsset.ParentViewModel, expectedDuplicatedAsset.ParentViewModel);

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    Assert.That(duplicatedAsset.ParentViewModel[i].Visible, Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].Visible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset, expectedDuplicatedAsset.ParentViewModel[i].Asset);
                }
            }
            else
            {
                Assert.That(duplicatedAsset.ParentViewModel, Is.Empty);
            }
        }
        else
        {
            Assert.That(duplicatedAsset, Is.Null);
        }
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
            asset,
            expectedAsset,
            expectedAsset.FullPath,
            expectedAsset.Folder.Path,
            expectedAsset.Folder);
        // Unlike below (Application, CatalogAssetsService), it is set here for assets in the current directory
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null); 
    }

    private static void CheckInstance(
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        int findDuplicatedAssetsViewModelInstancesCount = findDuplicatedAssetsViewModelInstances.Count;

        if (findDuplicatedAssetsViewModelInstancesCount > 1)
        {
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 1],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2]));
        }

        if (findDuplicatedAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                findDuplicatedAssetsViewModelInstances[0],
                expectedDuplicatedAssetSets,
                expectedDuplicatedAssetSetsPosition,
                expectedDuplicatedAssetPosition,
                expectedCurrentDuplicatedAssetSet,
                expectedCurrentDuplicatedAsset);
        }
    }
}
