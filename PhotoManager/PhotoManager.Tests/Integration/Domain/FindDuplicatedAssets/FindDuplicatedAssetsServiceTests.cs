using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

[TestFixture]
public class FindDuplicatedAssetsServiceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsService? _findDuplicatedAssetsService;
    private AssetRepository? _assetRepository;
    private StorageService? _storageService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new(database, _storageServiceMock!.Object, userConfigurationService);
        _storageService = new(userConfigurationService);
        _findDuplicatedAssetsService = new(_assetRepository, _storageService, userConfigurationService);

        _asset1 = new()
        {
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new Guid("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        try
        {
            string sourcePath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
            string sourcePath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = sourcePath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = sourcePath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset2 = _asset2!.WithFolder(folder2);
            _asset3 = _asset3!.WithFolder(folder2);
            _asset4 = _asset4!.WithFolder(folder2);
            _asset5 = _asset5!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        try
        {
            const string hash = Hashes.IMAGE_9_PNG;
            string sourcePath = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

            Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset2!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);
            _assetRepository.AddAsset(_asset4!, assetData);
            _assetRepository.AddAsset(_asset5!, assetData);

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_2);

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset3 = _asset3!.WithFolder(folder2);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_2);

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset2 = _asset2!.WithFolder(folder2);
            _asset3 = _asset3!.WithFolder(folder2);
            _asset4 = _asset4!.WithFolder(folder2);
            _asset5 = _asset5!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        try
        {
            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);

            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _findDuplicatedAssetsService!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
