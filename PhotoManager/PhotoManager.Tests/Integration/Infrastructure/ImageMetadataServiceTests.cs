using System.Drawing.Imaging;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class ImageMetadataServiceTests
{
    private string? _dataDirectory;

    private ImageMetadataService? _imageMetadataService;
    private FileOperationsService? _fileOperationService;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new(configurationRootMock.Object);
        _fileOperationService = new(_userConfigurationService);
        _imageMetadataService = new(_fileOperationService);
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, 1)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, 6)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, 3)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, 8)]
    [TestCase(FileNames.IMAGE_8_JPEG, 1)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, 1)]
    [TestCase(FileNames.IMAGE_11_HEIC, 1)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC,
        1)] // HEIC files typically store the sensor orientation metadata without rotating the actual pixel data
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC,
        1)] // HEIC files typically store the sensor orientation metadata without rotating the actual pixel data
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC,
        1)] // HEIC files typically store the sensor orientation metadata without rotating the actual pixel data
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _imageMetadataService!.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsCorruptedImageOrientation(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _imageMetadataService!.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _imageMetadataService!.GetExifOrientation(
            invalidImageBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = _imageMetadataService!.GetExifOrientation(
            nullBuffer!,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = [];

        ushort orientation = _imageMetadataService!.GetExifOrientation(
            emptyBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidFormat_ReturnsCorruptedOrientationValue()
    {
        Bitmap image = new(10, 10);

        using (MemoryStream ms = new())
        {
            image.Save(ms, ImageFormat.Bmp); // Save as BMP to create an invalid format for JPEG
            byte[] buffer = ms.ToArray(); // Buffer with invalid Exif Metadata (Metadata null)

            ushort orientation = _imageMetadataService!.GetExifOrientation(
                buffer,
                _userConfigurationService!.AssetSettings.DefaultExifOrientation,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

            Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, 1)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, 6)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, 3)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, 8)]
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName,
        int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _imageMetadataService!.GetHeicExifOrientation(buffer,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _imageMetadataService!.GetHeicExifOrientation(invalidHeicBuffer,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _imageMetadataService!.GetHeicExifOrientation(nullBuffer!,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            _imageMetadataService!.GetHeicExifOrientation(emptyBuffer,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    public void UpdateAssetsFileProperties_SomeFilesExist_PopulatesAssetsFileProperties()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName3,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_9_PNG,
                        Height = ThumbnailHeightAsset.IMAGE_9_PNG
                    }
                },
                Hash = string.Empty
            };
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                        Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                    }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new()
                    {
                        Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG
                    },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG
                    }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.Zero);
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Size, Is.Zero);
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset3.FileProperties.Size, Is.Zero);
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Size, Is.Zero);
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.Zero);
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            _imageMetadataService!.UpdateAssetsFileProperties([asset1, asset2, asset3, asset4, asset5]);

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(FileSize.HOMER_GIF));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_9_PNG));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_11_HEIC));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(FileSize.NON_EXISTENT_IMAGE_JPG));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileProperties_AssetsIsEmpty_DoesNothing()
    {
        _imageMetadataService!.UpdateAssetsFileProperties([]);
    }

    [Test]
    public void UpdateAssetsFileProperties_AssetsIsNull_ThrowsNullReferenceException()
    {
        List<Asset>? assets = null;

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _imageMetadataService!.UpdateAssetsFileProperties(assets!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void UpdateAssetsFileProperties_FilePathIsNull_ThrowsArgumentNullException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            string? path = null;
            Folder folder = new() { Id = Guid.NewGuid(), Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName3,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_9_PNG,
                        Height = ThumbnailHeightAsset.IMAGE_9_PNG
                    }
                },
                Hash = string.Empty
            };
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                        Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                    }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new()
                    {
                        Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG
                    },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG
                    }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.Zero);
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.Zero);
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.Zero);
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.Zero);
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.Zero);
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _imageMetadataService!.UpdateAssetsFileProperties([asset1, asset2, asset3, asset4, asset5]));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            Assert.That(asset1.FileProperties.Size, Is.Zero);
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.Zero);
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.Zero);
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.Zero);
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.Zero);
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileProperties_OneAssetIsNull_ThrowsNullReferenceException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                Hash = string.Empty
            };
            Asset? asset3 = null;
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                        Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                    }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new()
                    {
                        Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG
                    },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG
                    }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.Zero);
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Size, Is.Zero);
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Size, Is.Zero);
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.Zero);
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
                _imageMetadataService!.UpdateAssetsFileProperties([asset1, asset2, asset3!, asset4, asset5]));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(FileSize.HOMER_GIF));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.Zero);
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.Zero);
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FileExists_PopulatesAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                Hash = string.Empty
            };

            Assert.That(asset.FileProperties.Size, Is.Zero);
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            _imageMetadataService!.UpdateAssetFileProperties(asset);

            Assert.That(asset.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FileDoesNotExist_DoesNotPopulateAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.NON_EXISTENT_IMAGE_JPG;

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };
            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new()
                    {
                        Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG
                    },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG
                    }
                },
                Hash = string.Empty
            };

            DateTime creationTime = default;
            DateTime modificationTime = default;

            Assert.That(asset.FileProperties.Size, Is.Zero);
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            _imageMetadataService!.UpdateAssetFileProperties(asset);

            Assert.That(asset.FileProperties.Size, Is.Zero);
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FilePathIsNull_ThrowsArgumentNullException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            string? path = null;
            Folder folder = new() { Id = Guid.NewGuid(), Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                Hash = string.Empty
            };

            Assert.That(asset.FileProperties.Size, Is.Zero);
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            ArgumentNullException? exception =
                Assert.Throws<ArgumentNullException>(() => _imageMetadataService!.UpdateAssetFileProperties(asset));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            Assert.That(asset.FileProperties.Size, Is.Zero);
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_AssetIsNull_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _imageMetadataService!.UpdateAssetFileProperties(asset!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }
}
